using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// SyntaxTree.Statement {
/// 	Statement child1, child2;
/// 	Semant(Env env, Stmt parent) {
/// 		(env, new Stmt(env, parent, child1, child2))
/// 	}
/// }
/// 
/// Stmt {
/// 	Stmt child1, child2;
/// 	Stmt(env, parent, _child1, _child2) {
/// 		
/// 	}
/// }
/// </summary>
namespace AST {
    public abstract class Stmt {
        public enum Kind {
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
            IF_ELSE,
        }
        public abstract Kind kind {
            get;
        }
        public virtual void CGenStmt(Env env, CGenState state) {
            throw new NotImplementedException();
        }

        public Reg CGenExprStmt(Env env, Expr expr, CGenState state) {
            Int32 stack_size = state.StackSize;
            Reg ret = expr.CGenValue(env, state);
            state.CGenForceStackSizeTo(stack_size);
            return ret;
        }

        public void CGenTest(Env env, Reg ret, CGenState state) {
            // test cond
            switch (ret) {
                case Reg.EAX:
                    state.TESTL(Reg.EAX, Reg.EAX);
                    break;

                case Reg.ST0:
                    /// Compare expr with 0.0
                    /// < see cref = "BinaryArithmeticComp.OperateFloat(CGenState)" />
                    state.FLDZ();
                    state.FUCOMIP();
                    state.FSTP(Reg.ST0);
                    break;

                default:
                    throw new InvalidProgramException();
            }
        }

        //		public Stmt(Stmt _parent) {
        //			stmt_parent = _parent;
        //		}
        //
        //		public Stmt() : this(null) {
        //			// this(null);
        //		}

        //		public readonly Stmt stmt_parent;
    }

    /// <summary>
    /// Goto Statement
    /// </summary>
    public class GotoStmt : Stmt {

        public override Kind kind => Kind.GOTO;

        //		public GotoStmt(Env _env, Stmt _parent, String _label) : base(_parent) {
        //			stmt_label = _label;
        //		}
        public GotoStmt(String label) {
            this.label = label;
        }
        public readonly String label;
    }

    /// <summary>
    /// Labeled Statement
    /// </summary>
    public class LabeledStmt : Stmt {
        //		public LabeledStmt(ref Env _env, Stmt _parent, String _label, SyntaxTree.Statement _stmt) : base(_parent) {
        //			stmt_label = _label;
        //			stmt_stmt = _stmt.Semant(ref _env, this);
        //		}
        public override Kind kind => Kind.LABELED;
        public LabeledStmt(String _label, Stmt _stmt) {
            stmt_label = _label;
            stmt_stmt = _stmt;
        }
        public readonly String stmt_label;
        public readonly Stmt stmt_stmt;
    }

    /// <summary>
    /// Continue Statement
    /// </summary>
    public class ContStmt : Stmt {
        public override Kind kind => Kind.CONT;
        public ContStmt() { }
        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.ContinueLabel;
            state.JMP(label);
        }
    }

    /// <summary>
    /// Break Statement
    /// </summary>
    public class BreakStmt : Stmt {
        public override Kind kind => Kind.BREAK;
        public BreakStmt() { }
        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.BreakLabel;
            state.JMP(label);
        }
    }

    /// <summary>
    /// Expression Statement
    /// </summary>
    public class ExprStmt : Stmt {
        public override Kind kind => Kind.EXPR;
        public ExprStmt(Expr expr) {
            stmt_expr = expr;
        }
        public readonly Expr stmt_expr;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 stack_size = state.StackSize;
            stmt_expr.CGenValue(env, state);
            state.CGenForceStackSizeTo(stack_size);
        }
    }

    public class CompoundStmt : Stmt {
        public override Kind kind => Kind.COMPOUND;
        public CompoundStmt(List<Tuple<Env, Decln>> declns, List<Tuple<Env, Stmt>> stmts) {
            this.declns = declns;
            this.stmts = stmts;
        }

        public readonly List<Tuple<Env, Decln>> declns;
        public readonly List<Tuple<Env, Stmt>> stmts;

        public override void CGenStmt(Env env, CGenState state) {
            foreach (Tuple<Env, Decln> decln in declns) {
                decln.Item2.CGenDecln(decln.Item1, state);
            }
            foreach (Tuple<Env, Stmt> stmt in stmts) {
                stmt.Item2.CGenStmt(stmt.Item1, state);
            }
        }
    }

    public class ReturnStmt : Stmt {
        public override Kind kind => Kind.RETURN;
        public ReturnStmt(Expr expr) {
            this.expr = expr;
        }
        public readonly Expr expr;

        // TODO: return struct
        public override void CGenStmt(Env env, CGenState state) {
            ExprType ret_type = env.GetCurrentFunction().ret_type;

            Int32 stack_size = state.StackSize;
            Reg ret = expr.CGenValue(env, state);
            state.CGenForceStackSizeTo(stack_size);

            switch (ret_type.kind) {
                case ExprType.Kind.CHAR:
                case ExprType.Kind.DOUBLE:
                case ExprType.Kind.FLOAT:
                case ExprType.Kind.LONG:
                case ExprType.Kind.POINTER:
                case ExprType.Kind.SHORT:
                case ExprType.Kind.UCHAR:
                case ExprType.Kind.ULONG:
                case ExprType.Kind.USHORT:
                    state.JMP(state.ReturnLabel);
                    return;

                case ExprType.Kind.STRUCT_OR_UNION:
                case ExprType.Kind.VOID:
                default:
                    throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// While Statement
    /// 
    /// while (cond) {
    ///     body
    /// }
    /// 
    /// cond must be of scalar type
    /// </summary>
    // +--> start: continue:
    // |        test cond
    // |        jz finish --+
    // |        body        |
    // +------- jmp start   |
    //      finish: <-------+
    // 
    public class WhileStmt : Stmt {
        public override Kind kind => Kind.WHILE;
        public WhileStmt(Expr cond, Stmt body) {
            if (!cond.type.IsScalar) {
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

            // test cond
            Reg ret = CGenExprStmt(env, cond, state);
            CGenTest(env, ret, state);

            // jz finish
            state.JZ(finish_label);

            // body
            state.InLoop(start_label, finish_label);
            body.CGenStmt(env, state);
            state.OutLabels();

            // jmp start
            state.JMP(start_label);

            // finish:
            state.CGenLabel(finish_label);

        }
    }

    /// <summary>
    /// Do-while Stmt
    /// 
    /// do {
    ///     body
    /// } while (cond);
    /// 
    /// cond must be of scalar type
    /// </summary>
    // +--> start:
    // |        body
    // |    continue:
    // |        test cond
    // +------- jnz start
    //      finish:
    public class DoWhileStmt : Stmt {
        public override Kind kind => Kind.DO;
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

            // body
            state.InLoop(continue_label, finish_label);
            body.CGenStmt(env, state);
            state.OutLabels();

            state.CGenLabel(continue_label);

            // test cond
            Reg ret = CGenExprStmt(env, cond, state);
            CGenTest(env, ret, state);

            state.JNZ(start_label);

            state.CGenLabel(finish_label);
        }
    }

    /// <summary>
    /// for (init; cond; loop) {
    ///     body
    /// }
    /// 
    /// cond must be scalar
    /// </summary>
    // 
    //          init
    // +--> start:
    // |        test cond
    // |        jz finish --+
    // |        body        |
    // |    continue:       |
    // |        loop        |
    // +------- jmp start   |
    //      finish: <-------+
    // 
    public class ForStmt : Stmt {
        public override Kind kind => Kind.FOR;
        public ForStmt(Option<Expr> init, Option<Expr> cond, Option<Expr> loop, Stmt body) {
            this.init = init;
            this.cond = cond;
            this.loop = loop;
            this.body = body;
        }
        public readonly Option<Expr> init;
        public readonly Option<Expr> cond;
        public readonly Option<Expr> loop;
        public readonly Stmt body;

        public override void CGenStmt(Env env, CGenState state) {
            // init
            init.Map(_ => CGenExprStmt(env, _, state));

            Int32 start_label = state.RequestLabel();
            Int32 finish_label = state.RequestLabel();
            Int32 continue_label = state.RequestLabel();

            // start:
            state.CGenLabel(start_label);

            // test cont
            cond.Map(_ => {
                Reg ret = CGenExprStmt(env, _, state);
                CGenTest(env, ret, state);
                return ret;
            });
            
            // jz finish
            state.JZ(finish_label);

            // body
            state.InLoop(continue_label, finish_label);
            body.CGenStmt(env, state);
            state.OutLabels();

            // continue:
            state.CGenLabel(continue_label);

            // loop
            loop.Map(_ => CGenExprStmt(env, _, state));

            // jmp start
            state.JMP(start_label);

            // finish:
            state.CGenLabel(finish_label);
        }
    }

    /// <summary>
    /// Switch Statement
    /// </summary>
    //
    //     cmp cond, value1
    //     je case1
    //     cmp cond, value2
    //     je case2
    //     ...
    //     cmp cond, value_n
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
        public override Kind kind => Kind.SWITCH;
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
            switch (stmt.kind) {
                case Kind.COMPOUND:
                    declns = ((CompoundStmt)stmt).declns;
                    stmts = ((CompoundStmt)stmt).stmts;
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Track all case values.
            List<Int32> values =
                stmts
                .FindAll(_ => _.Item2.kind == Kind.CASE)
                .Select(_ => ((CaseStmt)_.Item2).value)
                .ToList();
            // Make sure there are no duplicates.
            if (values.Distinct().Count() != values.Count) {
                throw new InvalidOperationException("case labels not unique.");
            }
            // Request labels for these values.
            Dictionary<Int32, Int32> value_to_label = values.ToDictionary(value => value, value => state.RequestLabel());

            Int32 label_finish = state.RequestLabel();

            Int32 num_default_stmts = stmts.Count(_ => _.Item2.kind == Kind.DEFAULT);
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

            // 1. Evaluate expr.
            CGenExprStmt(env, expr, state);

            // 2. Expand stack.
            state.CGenForceStackSizeTo(stack_size);

            // 3. Make the Jump list.
            foreach (KeyValuePair<Int32, Int32> value_label_pair in value_to_label) {
                state.CMPL(Reg.EAX, value_label_pair.Key);
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
    }

    /// <summary>
    /// Case Statement
    /// </summary>
    public class CaseStmt : Stmt {
        public override Kind kind => Kind.CASE;
        public CaseStmt(Int32 value, Stmt stmt) {
            this.value = value;
            this.stmt = stmt;
        }
        public readonly Int32 value;
        public readonly Stmt stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.CaseLabel(value);
            state.CGenLabel(label);
            stmt.CGenStmt(env, state);
        }
    }

    public class DefaultStmt : Stmt {
        public override Kind kind => Kind.DEFAULT;
        public DefaultStmt(Stmt stmt) {
            this.stmt = stmt;
        }
        public readonly Stmt stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Int32 label = state.DefaultLabel;
            state.CGenLabel(label);
            stmt.CGenStmt(env, state);
        }
    }

    /// <summary>
    /// If Statement: if (cond) stmt;
    /// If cond is non-zero, stmt is executed.
    /// 
    /// cond must be arithmetic or pointer type.
    /// </summary>
    //          test cond
    // +------- jz finish
    // |        body
    // +--> finish:
    public class IfStmt : Stmt {
        public override Kind kind => Kind.IF;
        public IfStmt(Expr cond, Stmt stmt) {
            this.cond = cond;
            this.stmt = stmt;
        }
        public readonly Expr cond;
        public readonly Stmt stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Reg ret = CGenExprStmt(env, cond, state);

            Int32 finish_label = state.RequestLabel();

            CGenTest(env, ret, state);

            state.JZ(finish_label);

            stmt.CGenStmt(env, state);

            state.CGenLabel(finish_label);
        }
    }

    /// <summary>
    /// If-else Statement
    /// if (cond) {
    ///     true_stmt
    /// } else {
    ///     false_stmt
    /// }
    /// </summary>
    ///
    //          test cond
    // +------- jz false
    // |        true_stmt
    // |        jmp finish --+
    // +--> false:           |
    //          false_stmt   |
    //      finish: <--------+
    // 
    public class IfElseStmt : Stmt {
        public override Kind kind => Kind.IF_ELSE;
        public IfElseStmt(Expr cond, Stmt true_stmt, Stmt false_stmt) {
            this.cond = cond;
            this.true_stmt = true_stmt;
            this.false_stmt = false_stmt;
        }
        public readonly Expr cond;
        public readonly Stmt true_stmt;
        public readonly Stmt false_stmt;

        public override void CGenStmt(Env env, CGenState state) {
            Reg ret = CGenExprStmt(env, cond, state);

            CGenTest(env, ret, state);

            Int32 false_label = state.RequestLabel();
            Int32 finish_label = state.RequestLabel();

            state.JZ(false_label);

            true_stmt.CGenStmt(env, state);

            state.JMP(finish_label);

            state.CGenLabel(false_label);

            false_stmt.CGenStmt(env, state);

            state.CGenLabel(finish_label);

        }
    }

}