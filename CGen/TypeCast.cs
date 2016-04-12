using System;
using CodeGeneration;

namespace ABT {
    public sealed partial class TypeCast {
        public override Reg CGenValue(CGenState state) {
            Reg ret = this.Expr.CGenValue(state);
            switch (this.Kind) {
                case TypeCastType.DOUBLE_TO_FLOAT:
                case TypeCastType.FLOAT_TO_DOUBLE:
                case TypeCastType.PRESERVE_INT16:
                case TypeCastType.PRESERVE_INT8:
                case TypeCastType.NOP:
                    return ret;

                case TypeCastType.DOUBLE_TO_INT32:
                case TypeCastType.FLOAT_TO_INT32:
                    state.CGenConvertFloatToLong();
                    return Reg.EAX;

                case TypeCastType.INT32_TO_DOUBLE:
                case TypeCastType.INT32_TO_FLOAT:
                    state.CGenConvertLongToFloat();
                    return Reg.ST0;

                case TypeCastType.INT16_TO_INT32:
                    state.MOVSWL(Reg.AX, Reg.EAX);
                    return ret;

                case TypeCastType.INT8_TO_INT16:
                case TypeCastType.INT8_TO_INT32:
                    state.MOVSBL(Reg.AL, Reg.EAX);
                    return ret;

                case TypeCastType.UINT16_TO_UINT32:
                    state.MOVZWL(Reg.AX, Reg.EAX);
                    return ret;

                case TypeCastType.UINT8_TO_UINT16:
                case TypeCastType.UINT8_TO_UINT32:
                    state.MOVZBL(Reg.AL, Reg.EAX);
                    return ret;

                default:
                    throw new InvalidProgramException();
            }
        }

        public override void CGenAddress(CGenState state) {
            throw new InvalidOperationException("Cannot get the address of a cast expression.");
        }
    }
}