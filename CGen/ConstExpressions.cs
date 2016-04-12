using System;
using CodeGeneration;

namespace ABT {
    public abstract partial class ConstExpr {
        public override sealed void CGenAddress(CGenState state) {
            throw new InvalidOperationException("Cannot get the address of a constant");
        }
    }

    public sealed partial class ConstLong {
        public override Reg CGenValue(CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class ConstULong {
        public override Reg CGenValue(CGenState state) {
            state.MOVL((Int32)this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class ConstShort {
        public override Reg CGenValue(CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class ConstUShort {
        public override Reg CGenValue(CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class ConstChar {
        public override Reg CGenValue(CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class ConstUChar {
        public override Reg CGenValue(CGenState state) {
            state.MOVL(this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class ConstPtr {
        public override Reg CGenValue(CGenState state) {
            state.MOVL((Int32)this.Value, Reg.EAX);
            return Reg.EAX;
        }
    }

    public sealed partial class ConstFloat {
        /// <summary>
        /// flds addr
        /// </summary>
        public override Reg CGenValue(CGenState state) {
            byte[] bytes = BitConverter.GetBytes(this.Value);
            Int32 intval = BitConverter.ToInt32(bytes, 0);
            String name = state.CGenLongConst(intval);
            state.FLDS(name);
            return Reg.ST0;
        }
    }

    public sealed partial class ConstDouble {
        /// <summary>
        /// fldl addr
        /// </summary>
        public override Reg CGenValue(CGenState state) {
            byte[] bytes = BitConverter.GetBytes(this.Value);
            Int32 firstInt = BitConverter.ToInt32(bytes, 0);
            Int32 secondInt = BitConverter.ToInt32(bytes, 4);
            String name = state.CGenLongLongConst(firstInt, secondInt);
            state.FLDL(name);
            return Reg.ST0;
        }
    }

    public sealed partial class ConstStringLiteral {
        public override Reg CGenValue(CGenState state) {
            String name = state.CGenString(this.Value);
            state.LEA(name, Reg.EAX);
            return Reg.EAX;
        }
    }
}