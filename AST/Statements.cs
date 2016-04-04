using System;
using System.Collections.Generic;
using System.Linq;
using CodeGeneration;

namespace AST {
    public enum StmtKind {
        GOTO,
        LABELED,
        CONT,
        BREAK,
        EXPR,
        COMPOUND,
        RETURN,
        WHILE,
        DO,
        FOR,
        SWITCH,
        CASE,
        DEFAULT,
        IF,
        IF_ELSE
    }

    public abstract class Stmt {
        //protected Stmt(Env env) {
        //    this.Env = env;
        //}

        public abstract StmtKind Kind { get; }

        public Env Env { get; }

        public abstract void CGenStmt(Env env, CGenState state);

        public abstract void Accept(StmtVisitor visitor);

        public Reg CGenExprStmt(Env env, Expr expr, CGenState state) {
            Int32 stack_size = state.StackSize;
            Reg ret = expr.CGenValue(env, state);
            state.CGenForceStackSizeTo(stack_size);
            return ret;
        }

        public void CGenTest(Env env, Reg ret, CGenState state) {
            // test Cond
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    break;

                case Reg.ST0:
                    /// Compare Expr with 0.0
                    /// < see cref = "BinaryComparisonOp.OperateFloat(CGenState)" />
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    break;

                default:
                    throw new InvalidProgramException();
            }
        }

    }

    /// <summary>
    /// Goto Statement
    /// </summary>
    public class GotoStmt : Stmt {
        public override StmtKind Kind => StmtKind.GOTO;

        public GotoStmt(String label) {
            this.Label = label;
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);

        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.GotoLabel(this.Label);
            state.JMP(label);
        }

        public readonly String Label;
    }

    /// <summary>
    /// Labeled Statement
    /// </summary>
    public class LabeledStmt : Stmt {
        public override StmtKind Kind => StmtKind.LABELED;
        public LabeledStmt(String label, Stmt stmt) {
            this.label = label;
            this.stmt = stmt;
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);

        public override void CGenStmt(Env env, CGenState state) {
            state.CGenLabel(state.GotoLabel(this.label));
            state.CGenForceStackSizeTo(state.StackSize);
            this.stmt.CGenStmt(env, state);
        }

        public readonly String label;
        public readonly Stmt stmt;
    }

    /// <summary>
    /// Continue Statement
    /// </summary>
    public class ContStmt : Stmt {
        public override StmtKind Kind => StmtKind.CONT;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.ContinueLabel;
            state.JMP(label);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);

    }

    /// <summary>
    /// Break Statement
    /// </summary>
    public class BreakStmt : Stmt {
        public override StmtKind Kind => StmtKind.BREAK;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.BreakLabel;
            state.JMP(label);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// Expression Statement
    /// </summary>
    public class ExprStmt : Stmt {
        public override StmtKind Kind => StmtKind.EXPR;
        public ExprStmt(Option<Expr> expr) {
            this.expr = expr;
        }
        public readonly Option<Expr> expr;

        public override void CGenStmt(Env env, CGenState state) {
            if (this.expr.IsSome) {
                Int32 stack_size = state.StackSize;
                this.expr.Value.CGenValue(env, state);
                state.CGenForceStackSizeTo(stack_size);
            }
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    public class CompoundStmt : Stmt {
        public override StmtKind Kind => StmtKind.COMPOUND;
        public CompoundStmt(List<Tuple<Env, Decln>> declns, List<Tuple<Env, Stmt>> stmts) {
            this.declns = declns;
            this.stmts = stmts;
        }

        public readonly List<Tuple<Env, Decln>> declns;
        public readonly List<Tuple<Env, Stmt>> stmts;

        public override void CGenStmt(Env env, CGenState state) {
            foreach (Tuple<Env, Decln> decln in this.declns) {
                decln.Item2.CGenDecln(decln.Item1, state);
            }
            foreach (Tuple<Env, Stmt> stmt in this.stmts) {
                stmt.Item2.CGenStmt(stmt.Item1, state);
            }
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    public class ReturnStmt : Stmt {
        public override StmtKind Kind => StmtKind.RETURN;
        public ReturnStmt(Option<Expr> expr) {
            this.expr = expr;
        }
        public readonly Option<Expr> expr;

        public override void CGenStmt(Env env, CGenState state) {
            ExprType ret_type = env.GetCurrentFunction().ReturnType;

            Int32 stack_size = state.StackSize;

            if (this.expr.IsSome) {
                // Evaluate the Value.
                this.expr.Value.CGenValue(env, state);

                // If the function returns a struct, copy it to the address given by 8(%ebp).
                if (this.expr.Value.Type is StructOrUnionType) {
                    state.MOVL(Reg.EAX, Reg.ESI);
                    state.MOVL(2 * ExprType.SIZEOF_POINTER, Reg.EBP, Reg.EDI);
                    state.MOVL(this.expr.Value.Type.SizeOf, Reg.ECX);
                    state.CGenMemCpy();
                    state.MOVL(2 * ExprType.SIZEOF_POINTER, Reg.EBP, Reg.EAX);
                }

                // Restore stack size.
                state.CGenForceStackSizeTo(stack_size);
            }
            // Jump to end of the function.
            state.JMP(state.ReturnLabel);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// While Statement
    /// 
    /// while (Cond) {
    ///     Body
    /// }
    /// 
    /// Cond must be of scalar Type
    /// </summary>
    // +--> start: continue:
    // |        test Cond
    // |        jz finish --+
    // |        Body        |
    // +------- jmp start   |
    //      finish: <-------+
    // 
    public class WhileStmt : Stmt {
        public override StmtKind Kind => StmtKind.WHILE;
        public WhileStmt(Expr cond, Stmt body) {
            if (!cond.Type.IsScalar) {
                throw new InvalidProgramException();
            }
            this.cond = cond;
            this.body = body;
        }
        public readonly Expr cond;
        public readonly Stmt body;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 start_label = state.RequestLabel();
            Int32 finish_label = state.RequestLabel();

            // start:
            state.CGenLabel(start_label);

            // test Cond
            Reg ret = CGenExprStmt(env, this.cond, state);
            CGenTest(env, ret, state);

            // jz finish
            state.JZ(finish_label);

            // Body
            state.InLoop(start_label, finish_label);
            this.body.CGenStmt(env, state);
            state.OutLabels();

            // jmp start
            state.JMP(start_label);

            // finish:
            state.CGenLabel(finish_label);

        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// Do-while Stmt
    /// 
    /// do {
    ///     Body
    /// } while (Cond);
    /// 
    /// Cond must be of scalar Type
    /// </summary>
    // +--> start:
    // |        Body
    // |    continue:
    // |        test Cond
    // +------- jnz start
    //      finish:
    public class DoWhileStmt : Stmt {
        public override StmtKind Kind => StmtKind.DO;
        public DoWhileStmt(Stmt body, Expr cond) {
            this.body = body;
            this.cond = cond;
        }
        public readonly Stmt body;
        public readonly Expr cond;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 start_label = state.RequestLabel();
            Int32 finish_label = state.RequestLabel();
            Int32 continue_label = state.RequestLabel();

            // start:
            state.CGenLabel(start_label);

            // Body
            state.InLoop(continue_label, finish_label);
            this.body.CGenStmt(env, state);
            state.OutLabels();

            state.CGenLabel(continue_label);

            // test Cond
            Reg ret = CGenExprStmt(env, this.cond, state);
            CGenTest(env, ret, state);

            state.JNZ(start_label);

            state.CGenLabel(finish_label);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// for (Init; Cond; Loop) {
    ///     Body
    /// }
    /// 
    /// Cond must be scalar
    /// </summary>
    // 
    //          Init
    // +--> start:
    // |        test Cond
    // |        jz finish --+
    // |        Body        |
    // |    continue:       |
    // |        Loop        |
    // +------- jmp start   |
    //      finish: <-------+
    // 
    public sealed class ForStmt : Stmt {
        public override StmtKind Kind => StmtKind.FOR;

        public ForStmt(Option<Expr> init, Option<Expr> cond, Option<Expr> loop, Stmt body) {
            this.Init = init;
            this.Cond = cond;
            this.Loop = loop;
            this.Body = body;
        }

        public readonly Option<Expr> Init;
        public readonly Option<Expr> Cond;
        public readonly Option<Expr> Loop;
        public readonly Stmt Body;

        public override void CGenStmt(Env env, CGenState state) {
            // Init
            this.Init.Map(_ => CGenExprStmt(env, _, state));

            Int32 start_label = state.RequestLabel();
            Int32 finish_label = state.RequestLabel();
            Int32 continue_label = state.RequestLabel();

            // start:
            state.CGenLabel(start_label);

            // test cont
            this.Cond.Map(_ => {
                Reg ret = CGenExprStmt(env, _, state);
                CGenTest(env, ret, state);
                return ret;
            });
            
            // jz finish
            state.JZ(finish_label);

            // Body
            state.InLoop(continue_label, finish_label);
            this.Body.CGenStmt(env, state);
            state.OutLabels();

            // continue:
            state.CGenLabel(continue_label);

            // Loop
            this.Loop.Map(_ => CGenExprStmt(env, _, state));

            // jmp start
            state.JMP(start_label);

            // finish:
            state.CGenLabel(finish_label);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// Switch Statement
    /// </summary>
    //
    //     cmp Cond, value1
    //     je case1
    //     cmp Cond, value2
    //     je case2
    //     ...
    //     cmp Cond, value_n
    //     je case_n
    //     jmp default # if no default, then default = finish
    //     
    // case1:
    //     stmt
    // case2:
    //     stmt
    // ...
    // case_n:
    //     stmt
    // finish:
    public class SwitchStmt : Stmt {
        public override StmtKind Kind => StmtKind.SWITCH;
        public SwitchStmt(Expr expr, Stmt stmt) {
            this.expr = expr;
            this.stmt = stmt;
        }
        public readonly Expr expr;
        public readonly Stmt stmt;

        public override void CGenStmt(Env env, CGenState state) {

            // Inside a switch statement, the initializations are ignored,
            // but the stack size should be changed.
            List<Tuple<Env, Decln>> declns;
            List<Tuple<Env, Stmt>> stmts;
            switch (this.stmt.Kind) {
                case StmtKind.COMPOUND:
                    declns = ((CompoundStmt) this.stmt).declns;
                    stmts = ((CompoundStmt) this.stmt).stmts;
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Track all case values.
            IReadOnlyList<Int32> values = CaseLabelsGrabber.GrabLabels(this);

            // Make sure there are no duplicates.
            if (values.Distinct().Count() != values.Count) {
                throw new InvalidOperationException("case labels not unique.");
            }
            // Request labels for these values.
            Dictionary<Int32, Int32> value_to_label = values.ToDictionary(value => value, value => state.RequestLabel());

            Int32 label_finish = state.RequestLabel();

            Int32 num_default_stmts = stmts.Count(_ => _.Item2.Kind == StmtKind.DEFAULT);
            if (num_default_stmts > 1) {
                throw new InvalidOperationException("duplicate defaults.");
            }
            Int32 label_default =
                num_default_stmts == 1 ?
                state.RequestLabel() :
                label_finish;

            Int32 saved_stack_size = state.StackSize;
            Int32 stack_size =
                declns.Any() ?
                declns.Last().Item1.StackSize :
                saved_stack_size;

            // 1. Evaluate Expr.
            CGenExprStmt(env, this.expr, state);

            // 2. Expand stack.
            state.CGenForceStackSizeTo(stack_size);

            // 3. Make the Jump list.
            foreach (KeyValuePair<Int32, Int32> value_label_pair in value_to_label) {
                state.CMPL(value_label_pair.Key, Reg.EAX);
                state.JZ(value_label_pair.Value);
            }
            state.JMP(label_default);

            // 4. List all the statements.
            state.InSwitch(label_finish, label_default, value_to_label);
            foreach (Tuple<Env, Stmt> env_stmt_pair in stmts) {
                env_stmt_pair.Item2.CGenStmt(env_stmt_pair.Item1, state);
            }
            state.OutLabels();

            // 5. finish:
            state.CGenLabel(label_finish);
            
            // 6. Restore stack size.
            state.CGenForceStackSizeTo(saved_stack_size);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// Case Statement
    /// </summary>
    public class CaseStmt : Stmt {
        public override StmtKind Kind => StmtKind.CASE;
        public CaseStmt(Int32 value, Stmt stmt) {
            this.value = value;
            this.stmt = stmt;
        }
        public readonly Int32 value;
        public readonly Stmt stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.CaseLabel(this.value);
            state.CGenLabel(label);
            this.stmt.CGenStmt(env, state);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    public class DefaultStmt : Stmt {
        public override StmtKind Kind => StmtKind.DEFAULT;
        public DefaultStmt(Stmt stmt) {
            this.stmt = stmt;
        }
        public readonly Stmt stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.DefaultLabel;
            state.CGenLabel(label);
            this.stmt.CGenStmt(env, state);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// If Statement: if (Cond) stmt;
    /// If Cond is non-zero, stmt is executed.
    /// 
    /// Cond must be arithmetic or pointer Type.
    /// </summary>
    //          test Cond
    // +------- jz finish
    // |        Body
    // +--> finish:
    public class IfStmt : Stmt {
        public override StmtKind Kind => StmtKind.IF;
        public IfStmt(Expr cond, Stmt stmt) {
            this.cond = cond;
            this.stmt = stmt;
        }
        public readonly Expr cond;
        public readonly Stmt stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Reg ret = CGenExprStmt(env, this.cond, state);

            Int32 finish_label = state.RequestLabel();

            CGenTest(env, ret, state);

            state.JZ(finish_label);

            this.stmt.CGenStmt(env, state);

            state.CGenLabel(finish_label);
        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

    /// <summary>
    /// If-else Statement
    /// if (Cond) {
    ///     true_stmt
    /// } else {
    ///     false_stmt
    /// }
    /// </summary>
    ///
    //          test Cond
    // +------- jz false
    // |        true_stmt
    // |        jmp finish --+
    // +--> false:           |
    //          false_stmt   |
    //      finish: <--------+
    // 
    public class IfElseStmt : Stmt {
        public override StmtKind Kind => StmtKind.IF_ELSE;
        public IfElseStmt(Expr cond, Stmt true_stmt, Stmt false_stmt) {
            this.cond = cond;
            this.true_stmt = true_stmt;
            this.false_stmt = false_stmt;
        }
        public readonly Expr cond;
        public readonly Stmt true_stmt;
        public readonly Stmt false_stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Reg ret = CGenExprStmt(env, this.cond, state);

            CGenTest(env, ret, state);

            Int32 false_label = state.RequestLabel();
            Int32 finish_label = state.RequestLabel();

            state.JZ(false_label);

            this.true_stmt.CGenStmt(env, state);

            state.JMP(finish_label);

            state.CGenLabel(false_label);

            this.false_stmt.CGenStmt(env, state);

            state.CGenLabel(finish_label);

        }

        public override void Accept(StmtVisitor visitor) =>
            visitor.Visit(this);
    }

}