using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeGeneration;

namespace AST {
    // Expr 
    // ========================================================================

    /// <summary>
    /// The cdecl calling convention:
    /// 1. arguments are passed on the stack, right to left.
    /// 2. int values and pointer values are returned in %eax.
    /// 3. floats are returned in %st(0).
    /// 4. when calling a function, %st(0) ~ %st(7) are all free.
    /// 5. functions are free to use %eax, %ecx, %edx, because caller needs to save them.
    /// 6. stack must be aligned to 4 bytes (before gcc 4.5, for gcc 4.5+, aligned to 16 bytes).
    /// </summary>

    public abstract partial class Expr {
        protected Expr() { }

        /// <summary>
        /// Whether the Value is known at compile time.
        /// </summary>
        public virtual Boolean IsConstExpr => false;

        /// <summary>
        /// Whether the expression refers to an object (that can be assigned to).
        /// </summary>
        public abstract Boolean IsLValue { get; }

        public abstract Env Env { get; }

        public abstract ExprType Type { get; }
    }

    public sealed partial class Variable : Expr {
        public Variable(ExprType type, String name, Env env) {
            this.Name = name;
            this.Env = env;
            this.Type = type;
        }

        public String Name { get; }

        public override Env Env { get; }

        public override ExprType Type { get; }

        public override Boolean IsLValue => !(Type is FunctionType);
    }

    public sealed partial class AssignList : Expr {
        public AssignList(ImmutableList<Expr> exprs) {
            if (exprs.Count == 0) {
                throw new InvalidOperationException("Need at least one expression.");
            }
            this.Exprs = exprs;
        }

        public ImmutableList<Expr> Exprs { get; }

        public override Env Env => this.Exprs.Last().Env;

        public override Boolean IsLValue => false;

        public override ExprType Type => this.Exprs.Last().Type;
    }

    public sealed partial class Assign : Expr {
        public Assign(Expr left, Expr right) {
            this.Left = left;
            this.Right = right;

            if (!this.Left.IsLValue) {
                throw new InvalidOperationException("Can only assign to lvalue.");
            }
        }

        public Expr Left { get; }

        public Expr Right { get; }

        public override Env Env => this.Right.Env;

        public override Boolean IsLValue => false;

        public override ExprType Type => this.Left.Type.GetQualifiedType(false, false);
    }

    public sealed partial class ConditionalExpr : Expr {
        public ConditionalExpr(Expr cond, Expr trueExpr, Expr falseExpr, ExprType type) {
            this.Cond = cond;
            this.TrueExpr = trueExpr;
            this.FalseExpr = falseExpr;
            this.Type = type;
        }

        public readonly Expr Cond;

        public readonly Expr TrueExpr;

        public readonly Expr FalseExpr;

        public override Boolean IsLValue => false;

        public override ExprType Type { get; }

        public override Env Env => this.FalseExpr.Env;
    }
        
    public sealed partial class FuncCall : Expr {
        public FuncCall(Expr func, FunctionType funcType, List<Expr> args) {
            this.Func = func;
            this.FuncType = funcType;
            this.Args = args;
        }

        public Expr Func { get; }

        public FunctionType FuncType { get; }

        public IReadOnlyList<Expr> Args { get; }

        public override ExprType Type => this.FuncType.ReturnType;

        public override Env Env => this.Args.Any() ? this.Args.Last().Env : this.Func.Env;

        public override Boolean IsLValue => false;
    }

    /// <summary>
    /// Expr.name: Expr must be a struct or union.
    /// </summary>
    public sealed partial class Attribute : Expr {
        public Attribute(Expr expr, String name, ExprType type) {
            this.Expr = expr;
            this.Name = name;
            this.Type = type;
        }

        public Expr Expr { get; }

        public String Name { get; }

        public override Env Env => this.Expr.Env;

        public override ExprType Type { get; }

        // You might want to think of some special case like this.
        // struct EvilStruct {
        //     int a[10];
        // } evil;
        // evil.a <--- is this an lvalue?
        // Yes, it is. It cannot be assigned, but that's because of the wrong Type.
        public override Boolean IsLValue => this.Expr.IsLValue;
    }

    /// <summary>
    /// &amp;Expr: get the address of Expr.
    /// </summary>
    public sealed partial class Reference : Expr {
        public Reference(Expr expr) {
            this.Expr = expr;
            this.Type = new PointerType(expr.Type);
        }

        public Expr Expr { get; }

        public override Env Env => this.Expr.Env;

        public override ExprType Type { get; }

        // You might want to think of some special case like this.
        // int *a;
        // &(*a) = 3; // Is this okay?
        // But this should lead to an error: lvalue required.
        // The 'reference' operator only gets the 'current address'.
        public override Boolean IsLValue => false;
    }

    /// <summary>
    /// *Expr: Expr must be a pointer.
    /// 
    /// Arrays and functions are implicitly converted to pointers.
    /// 
    /// This is an lvalue, so it has an address.
    /// </summary>
    public sealed partial class Dereference : Expr {
        public Dereference(Expr expr, ExprType type) {
            this.Expr = expr;
            this.Type = type;
        }

        public Expr Expr { get; }

        public override Env Env => this.Expr.Env;

        public override Boolean IsLValue => true;

        public override ExprType Type { get; }
    }
}