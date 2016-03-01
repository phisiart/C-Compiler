using System;
using CodeGeneration;

namespace AST {

    /// <summary>
    /// Compile-time constant. Cannot get the address.
    /// </summary>
    public abstract class ConstExpr : Expr {
        protected ConstExpr(ExprType type, Env env)
            : base(type) {
            this.Env = env;
        }

        public override sealed Env Env { get; }

        public override sealed Boolean IsConstExpr => true;

        public override sealed Boolean IsLValue => false;

        public override void CGenAddress(Env env, CGenState state) {
            throw new InvalidOperationException("Cannot get the address of a constant");
        }
    }

    public sealed class ConstLong : ConstExpr {
        public ConstLong(Int32 value, Env env)
            : base(new LongType(true), env) {
            this.Value = value;
        }
        public readonly Int32 Value;

        public override String ToString() => $"{this.Value}";

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }

        //[Obsolete]
        //public override void CGenPush(Env env, CGenState state) =>
        //    state.CGenPushLong(this.Value);
    }

    public sealed class ConstULong : ConstExpr {
        public ConstULong(UInt32 value, Env env)
            : base(new ULongType(true), env) {
            this.Value = value;
        }
        public readonly UInt32 Value;

        public override String ToString() => $"{this.Value}u";

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL((Int32) this.Value, Reg.EAX);
            return Reg.EAX;
        }

        //[Obsolete]
        //public override void CGenPush(Env env, CGenState state) =>
        //    state.CGenPushLong((Int32) this.Value);
    }

    public sealed class ConstShort : ConstExpr {
        public ConstShort(Int16 value, Env env)
            : base(new ShortType(true), env) {
            this.Value = value;
        }

        public readonly Int16 Value;

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed class ConstUShort : ConstExpr {
        public ConstUShort(UInt16 value, Env env)
            : base(new UShortType(true), env) {
            this.Value = value;
        }

        public readonly UInt16 Value;

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed class ConstChar : ConstExpr {
        public ConstChar(SByte value, Env env)
            : base(new CharType(true), env) {
            this.Value = value;
        }

        public readonly SByte Value;

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed class ConstUChar : ConstExpr {
        public ConstUChar(Byte value, Env env)
            : base(new CharType(true), env) {
            this.Value = value;
        }

        public readonly Byte Value;

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed class ConstPtr : ConstExpr {
        public ConstPtr(UInt32 value, ExprType type, Env env)
            : base(type, env) {
            this.Value = value;
        }
        public readonly UInt32 Value;

        public override String ToString() => $"({this.Type} *)0x{this.Value.ToString("X8")}";

        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL((Int32) this.Value, Reg.EAX);
            return Reg.EAX;
        }

        //[Obsolete]
        //public override void CGenPush(Env env, CGenState state) =>
        //    state.CGenPushLong((Int32) this.Value);
        
    }

    public sealed class ConstFloat : ConstExpr {
        public ConstFloat(Single value, Env env)
            : base(new FloatType(true), env) {
            this.Value = value;
        }
        public readonly Single Value;

        public override String ToString() => $"{this.Value}f";

        /// <summary>
        /// flds addr
        /// </summary>
        public override Reg CGenValue(Env env, CGenState state) {
            byte[] bytes = BitConverter.GetBytes(this.Value);
            Int32 intval = BitConverter.ToInt32(bytes, 0);
            String name = state.CGenLongConst(intval);
            state.FLDS(name);
            return Reg.ST0;
        }
    }

    public sealed class ConstDouble : ConstExpr {
        public ConstDouble(Double value, Env env)
            : base(new DoubleType(true), env) {
            this.Value = value;
        }
        public readonly Double Value;

        public override String ToString() => $"{this.Value}";

        /// <summary>
        /// fldl addr
        /// </summary>
        public override Reg CGenValue(Env env, CGenState state) {
            byte[] bytes = BitConverter.GetBytes(this.Value);
            Int32 firstInt = BitConverter.ToInt32(bytes, 0);
            Int32 secondInt = BitConverter.ToInt32(bytes, 4);
            String name = state.CGenLongLongConst(firstInt, secondInt);
            state.FLDL(name);
            return Reg.ST0;
        }
    }

    public sealed class ConstStringLiteral : ConstExpr {
        public ConstStringLiteral(String value, Env env)
            : base(new PointerType(new CharType(true), true), env) {
            this.Value = value;
        }
        public readonly String Value;

        public override String ToString() => $"\"{this.Value}\"";

        public override Reg CGenValue(Env env, CGenState state) {
            String name = state.CGenString(this.Value);
            state.LEA(name, Reg.EAX);
            return Reg.EAX;
        }
    }

}
