using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {
    public class Assignment : Expr {
        public Assignment(Expr _lvalue, Expr _rvalue, ExprType _type)
            : base(_type) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        protected Expr lvalue;
        protected Expr rvalue;
    }

    public class FunctionCall : Expr {
        public FunctionCall(Expr _function, List<Expr> _arguments, ExprType _type)
            : base(_type) {
            function = _function;
            arguments = _arguments;
        }
        protected Expr function;
        protected List<Expr> arguments;
    }

    public class Attribute : Expr {
        public Attribute(Expr _expr, String _attrib_name, ExprType _type)
            : base(_type) {
            expr = _expr;
            attrib_name = _attrib_name;
        }
        protected Expr expr;
        protected String attrib_name;
    }

    public class PostIncrement : Expr {
        public PostIncrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class PostDecrement : Expr {
        public PostDecrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class PreIncrement : Expr {
        public PreIncrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class PreDecrement : Expr {
        public PreDecrement(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class Reference : Expr {
        public Reference(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class Dereference : Expr {
        public Dereference(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class Negative : Expr {
        public Negative(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class BitwiseNot : Expr {
        public BitwiseNot(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }

    public class LogicalNot : Expr {
        public LogicalNot(Expr _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expr expr;
    }


    //public class TypeCast : Expression {
    //    public TypeCast(Expression _expr, ExprType _type)
    //        : base(_type) {
    //        expr = _expr;
    //    }
    //    protected Expression expr;
    //}

    public class Multiply : Expr {
        public Multiply(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expr lhs;
        protected Expr rhs;
    }

    public class Divide : Expr {
        public Divide(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expr lhs;
        protected Expr rhs;
    }

    public class Modulo : Expr {
        public Modulo(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expr lhs;
        protected Expr rhs;
    }
}