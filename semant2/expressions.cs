using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST {
    public class Assignment : Expression {
        public Assignment(Expression _lvalue, Expression _rvalue, ExprType _type)
            : base(_type) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
        protected Expression lvalue;
        protected Expression rvalue;
    }

    public class FunctionCall : Expression {
        public FunctionCall(Expression _function, List<Expression> _arguments, ExprType _type)
            : base(_type) {
            function = _function;
            arguments = _arguments;
        }
        protected Expression function;
        protected List<Expression> arguments;
    }

    public class Attribute : Expression {
        public Attribute(Expression _expr, String _attrib_name, ExprType _type)
            : base(_type) {
            expr = _expr;
            attrib_name = _attrib_name;
        }
        protected Expression expr;
        protected String attrib_name;
    }

    public class PostIncrement : Expression {
        public PostIncrement(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class PostDecrement : Expression {
        public PostDecrement(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class PreIncrement : Expression {
        public PreIncrement(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class PreDecrement : Expression {
        public PreDecrement(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class Reference : Expression {
        public Reference(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class Dereference : Expression {
        public Dereference(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class Negative : Expression {
        public Negative(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class BitwiseNot : Expression {
        public BitwiseNot(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }

    public class LogicalNot : Expression {
        public LogicalNot(Expression _expr, ExprType _type)
            : base(_type) {
            expr = _expr;
        }
        protected Expression expr;
    }


    //public class TypeCast : Expression {
    //    public TypeCast(Expression _expr, ExprType _type)
    //        : base(_type) {
    //        expr = _expr;
    //    }
    //    protected Expression expr;
    //}

    public class Multiply : Expression {
        public Multiply(Expression _lhs, Expression _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expression lhs;
        protected Expression rhs;
    }

    public class Divide : Expression {
        public Divide(Expression _lhs, Expression _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expression lhs;
        protected Expression rhs;
    }

    public class Modulo : Expression {
        public Modulo(Expression _lhs, Expression _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        protected Expression lhs;
        protected Expression rhs;
    }
}