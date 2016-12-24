namespace AST {
    public abstract class UnaryExprOperator : Expr {
        protected UnaryExprOperator(Expr expr) {
            this.Expr = expr;
        }

        public Expr Expr { get; }
    }

    /// <summary>
    /// Postfix increment: x++
    /// </summary>
    public sealed partial class PostIncrement : UnaryExprOperator {
        private PostIncrement(Expr expr)
            : base(expr) { }

        public static Expr Create(Expr expr) =>
            new PostIncrement(expr);
    }

    /// <summary>
    /// Postfix decrement: x--
    /// </summary>
    public sealed partial class PostDecrement : UnaryExprOperator {
        private PostDecrement(Expr expr)
            : base(expr) { }

        public static Expr Create(Expr expr) =>
            new PostDecrement(expr);
    }

    /// <summary>
    /// sizeof {type}
    /// </summary>
    [Checked]
    public sealed partial class SizeofType : Expr {
        private SizeofType(TypeName typeName) {
            this.TypeName = typeName;
        }

        public TypeName TypeName { get; }

        public static Expr Create(TypeName typeName) =>
            new SizeofType(typeName);
    }

    /// <summary>
    /// sizeof {expr}
    /// </summary>
    [Checked]
    public sealed partial class SizeofExpr : UnaryExprOperator {
        private SizeofExpr(Expr expr)
            : base(expr) { }

        public static Expr Create(Expr expr) =>
            new SizeofExpr(expr);
    }

    /// <summary>
    /// Prefix increment: ++x
    /// </summary>
    [Checked]
    public sealed partial class PreIncrement : UnaryExprOperator {
        private PreIncrement(Expr expr)
            : base(expr) { }

        public static Expr Create(Expr expr) =>
            new PreIncrement(expr);
    }

    /// <summary>
    /// Prefix decrement: --x
    /// </summary>
    [Checked]
    public sealed partial class PreDecrement : UnaryExprOperator {
        private PreDecrement(Expr expr)
            : base(expr) { }

        public static Expr Create(Expr expr) =>
            new PreDecrement(expr);
    }

    /// <summary>
    /// &amp;{expr}
    /// </summary>
    [Checked]
    public sealed partial class Reference : UnaryExprOperator {
        private Reference(Expr expr)
            : base(expr) { }

        public static Expr Create(Expr expr) =>
            new Reference(expr);
    }

    /// <summary>
    /// Dereference: *Expr
    /// 
    /// Note that Expr might have an **incomplete** Type.
    /// We need to search the environment
    /// </summary>
    [Checked]
    public sealed partial class Dereference : UnaryExprOperator {
        private Dereference(Expr expr)
            : base(expr) { }

        public static Expr Create(Expr expr) =>
            new Dereference(expr);
    }

    /// <summary>
    /// Merely a check on arithmetic Type.
    /// </summary>
    [Checked]
    public sealed partial class Positive : UnaryExprOperator {
        private Positive(Expr expr)
            : base(expr) { }
        
        public static Expr Create(Expr expr) =>
            new Positive(expr);
    }

    /// <summary>
    /// Negative: requires arithmetic Type.
    /// </summary>
    [Checked]
    public sealed partial class Negative : UnaryExprOperator {
        private Negative(Expr expr)
            : base(expr) { }
        
        public static Expr Create(Expr expr) =>
            new Negative(expr);
    }

    /// <summary>
    /// Bitwise not: requires integral.
    /// </summary>
    [Checked]
    public sealed partial class BitwiseNot : UnaryExprOperator {
        private BitwiseNot(Expr expr)
            : base(expr) { }
        
        public static Expr Create(Expr expr) =>
            new BitwiseNot(expr);
    }

    /// <summary>
    /// Logical not
    /// </summary>
    [Checked]
    public sealed partial class LogicalNot : UnaryExprOperator {
        private LogicalNot(Expr expr)
            : base(expr) { }
        
        public static Expr Create(Expr expr) =>
            new LogicalNot(expr);
    }

    /// <summary>
    /// User-specified explicit Type cast
    /// </summary>
    [Checked]
    public sealed partial class TypeCast : Expr {
        private TypeCast(TypeName typeName, Expr expr) {
            this.TypeName = typeName;
            this.Expr = expr;
        }

        public TypeName TypeName { get; }

        public Expr Expr { get; }

        public static Expr Create(TypeName typeName, Expr expr) =>
            new TypeCast(typeName, expr);
    }
}