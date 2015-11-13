using System;
using System.Diagnostics;
using CodeGeneration;

namespace AST {
    public class TypeCast : Expr {
        public enum Kind {
            NOP,
            INT8_TO_INT16,
            INT8_TO_INT32,

            INT16_TO_INT32,

            INT32_TO_FLOAT,
            INT32_TO_DOUBLE,

            PRESERVE_INT8,
            PRESERVE_INT16,

            UINT8_TO_UINT16,
            UINT8_TO_UINT32,

            UINT16_TO_UINT32,

            FLOAT_TO_INT32,
            FLOAT_TO_DOUBLE,

            DOUBLE_TO_INT32,
            DOUBLE_TO_FLOAT
        }

        public readonly Expr expr;
        public readonly Kind kind;

        // Note: typecast might introduce environment changes.
        public override Env Env { get; }

        public TypeCast(Kind kind, Expr expr, ExprType type, Env env)
            : base(type) {
            this.expr = expr;
            this.kind = kind;
            this.Env = env;
        }

        public TypeCast(Kind kind, Expr expr, ExprType type)
            : this(kind, expr, type, expr.Env) { }

        public override Reg CGenValue(Env env, CGenState state) {
            Reg ret = this.expr.CGenValue(env, state);
            switch (this.kind) {
                case Kind.DOUBLE_TO_FLOAT:
                case Kind.FLOAT_TO_DOUBLE:
                case Kind.PRESERVE_INT16:
                case Kind.PRESERVE_INT8:
                case Kind.NOP:
                    return ret;

                case Kind.DOUBLE_TO_INT32:
                case Kind.FLOAT_TO_INT32:
                    state.CGenConvertFloatToLong();
                    return Reg.EAX;

                case Kind.INT32_TO_DOUBLE:
                case Kind.INT32_TO_FLOAT:
                    state.CGenConvertLongToFloat();
                    return Reg.ST0;

                case Kind.INT16_TO_INT32:
                    state.MOVSWL(Reg.AX, Reg.EAX);
                    return ret;

                case Kind.INT8_TO_INT16:
                case Kind.INT8_TO_INT32:
                    state.MOVSBL(Reg.AL, Reg.EAX);
                    return ret;

                case Kind.UINT16_TO_UINT32:
                    state.MOVZWL(Reg.AX, Reg.EAX);
                    return ret;

                case Kind.UINT8_TO_UINT16:
                case Kind.UINT8_TO_UINT32:
                    state.MOVZBL(Reg.AL, Reg.EAX);
                    return ret;

                default:
                    throw new InvalidProgramException();
            }
        }

        public static Boolean EqualType(ExprType t1, ExprType t2) {
            return t1.EqualType(t2);
        }

        /// <summary>
        /// From:
        ///     char, short, long
        /// To:
        ///     char, uchar, short, ushort, long, ulong, float double
        /// </summary>
        public static Expr SignedIntegralToArith(Expr expr, ExprType type) {
            ExprType.Kind from = expr.Type.kind;
            ExprType.Kind to = type.kind;

            Env env = expr.Env;

            switch (from) {
                case ExprType.Kind.CHAR:
                    switch (to) {
                        case ExprType.Kind.SHORT:
                        case ExprType.Kind.USHORT:
                            return new TypeCast(Kind.INT8_TO_INT16, expr, type);

                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                            return new TypeCast(Kind.INT8_TO_INT32, expr, type);

                        case ExprType.Kind.UCHAR:
                            return new TypeCast(Kind.NOP, expr, type);

                        case ExprType.Kind.FLOAT:
                            // char -> long -> float
                            return new TypeCast(Kind.INT32_TO_FLOAT, new TypeCast(Kind.INT8_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.DOUBLE:
                            // char -> long -> double
                            return new TypeCast(Kind.INT32_TO_DOUBLE, new TypeCast(Kind.INT8_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.VOID:
                        case ExprType.Kind.POINTER:
                        case ExprType.Kind.FUNCTION:
                        case ExprType.Kind.ARRAY:
                        case ExprType.Kind.INCOMPLETE_ARRAY:
                        case ExprType.Kind.STRUCT_OR_UNION:
                        case ExprType.Kind.CHAR:
                        default:
                            throw new InvalidProgramException($"Cannot cast from {from} to {to}");
                    }

                case ExprType.Kind.SHORT:
                    switch (to) {
                        case ExprType.Kind.CHAR:
                        case ExprType.Kind.UCHAR:
                            return new TypeCast(Kind.PRESERVE_INT8, expr, type);

                        case ExprType.Kind.USHORT:
                            return new TypeCast(Kind.NOP, expr, type);

                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                            return new TypeCast(Kind.INT16_TO_INT32, expr, type);

                        case ExprType.Kind.FLOAT:
                            // short -> long -> float
                            return new TypeCast(Kind.INT32_TO_FLOAT, new TypeCast(Kind.INT16_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.DOUBLE:
                            // short -> long -> double
                            return new TypeCast(Kind.INT32_TO_DOUBLE, new TypeCast(Kind.INT16_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.VOID:
                        case ExprType.Kind.SHORT:
                        case ExprType.Kind.POINTER:
                        case ExprType.Kind.FUNCTION:
                        case ExprType.Kind.ARRAY:
                        case ExprType.Kind.INCOMPLETE_ARRAY:
                        case ExprType.Kind.STRUCT_OR_UNION:
                        default:
                            throw new InvalidProgramException($"Cannot cast from {from} to {to}");
                    }

                case ExprType.Kind.LONG:
                    switch (to) {
                        case ExprType.Kind.CHAR:
                            if (expr.IsConstExpr) {
                                return new ConstLong((SByte)((ConstLong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT8, expr, type);

                        case ExprType.Kind.UCHAR:
                            if (expr.IsConstExpr) {
                                return new ConstULong((Byte)((ConstLong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT8, expr, type);

                        case ExprType.Kind.SHORT:
                            if (expr.IsConstExpr) {
                                return new ConstLong((Int16)((ConstLong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT16, expr, type);

                        case ExprType.Kind.USHORT:
                            if (expr.IsConstExpr) {
                                return new ConstULong((UInt16)((ConstLong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT16, expr, type);

                        case ExprType.Kind.ULONG:
                            if (expr.IsConstExpr) {
                                return new ConstULong((UInt32)((ConstLong)expr).value, env);
                            }
                            return new TypeCast(Kind.NOP, expr, type);

                        case ExprType.Kind.FLOAT:
                            if (expr.IsConstExpr) {
                                return new ConstFloat(((ConstLong)expr).value, env);
                            }
                            return new TypeCast(Kind.INT32_TO_FLOAT, expr, type);

                        case ExprType.Kind.DOUBLE:
                            if (expr.IsConstExpr) {
                                return new ConstDouble(((ConstLong)expr).value, env);
                            }
                            return new TypeCast(Kind.INT32_TO_DOUBLE, expr, type);

                        case ExprType.Kind.VOID:
                        case ExprType.Kind.LONG:
                        case ExprType.Kind.POINTER:
                        case ExprType.Kind.FUNCTION:
                        case ExprType.Kind.ARRAY:
                        case ExprType.Kind.INCOMPLETE_ARRAY:
                        case ExprType.Kind.STRUCT_OR_UNION:
                        default:
                            throw new InvalidProgramException($"Cannot cast from {from} to {to}");
                    }

                default:
                    throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// From:
        ///     uchar, ushort, ulong
        /// To:
        ///     char, uchar, short, ushort, long, ulong, float, double
        /// </summary>
        /// <remarks>
        /// Aaccording to MSDN "Conversions from Unsigned Integral Types",
        ///   unsigned long converts directly to double.
        /// However, I just treat unsigned long as long.
        /// </remarks>
        public static Expr UnsignedIntegralToArith(Expr expr, ExprType type) {
            ExprType.Kind from = expr.Type.kind;
            ExprType.Kind to = type.kind;

            Env env = expr.Env;

            switch (from) {
                case ExprType.Kind.UCHAR:
                    switch (to) {
                        case ExprType.Kind.CHAR:
                            return new TypeCast(Kind.NOP, expr, type);

                        case ExprType.Kind.SHORT:
                        case ExprType.Kind.USHORT:
                            return new TypeCast(Kind.UINT8_TO_UINT16, expr, type);

                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                            return new TypeCast(Kind.UINT8_TO_UINT32, expr, type);

                        case ExprType.Kind.FLOAT:
                            // uchar -> ulong -> long -> float
                            return new TypeCast(Kind.INT32_TO_FLOAT, new TypeCast(Kind.UINT8_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.DOUBLE:
                            // uchar -> ulong -> long -> double
                            return new TypeCast(Kind.INT32_TO_DOUBLE, new TypeCast(Kind.UINT8_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                case ExprType.Kind.USHORT:
                    switch (to) {
                        case ExprType.Kind.CHAR:
                        case ExprType.Kind.UCHAR:
                            return new TypeCast(Kind.PRESERVE_INT8, expr, type);

                        case ExprType.Kind.USHORT:
                            return new TypeCast(Kind.NOP, expr, type);

                        case ExprType.Kind.LONG:
                        case ExprType.Kind.ULONG:
                            return new TypeCast(Kind.UINT16_TO_UINT32, expr, type);

                        case ExprType.Kind.FLOAT:
                            // ushort -> ulong -> long -> float
                            return new TypeCast(Kind.INT32_TO_FLOAT, new TypeCast(Kind.UINT16_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.DOUBLE:
                            // ushort -> ulong -> long -> double
                            return new TypeCast(Kind.INT32_TO_DOUBLE, new TypeCast(Kind.UINT16_TO_UINT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                case ExprType.Kind.ULONG:
                    switch (to) {
                        case ExprType.Kind.CHAR:
                            if (expr.IsConstExpr) {
                                return new ConstLong((SByte)((ConstULong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT8, expr, type);

                        case ExprType.Kind.UCHAR:
                            if (expr.IsConstExpr) {
                                return new ConstULong((Byte)((ConstULong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT8, expr, type);

                        case ExprType.Kind.SHORT:
                            if (expr.IsConstExpr) {
                                return new ConstLong((Int16)((ConstULong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT16, expr, type);

                        case ExprType.Kind.USHORT:
                            if (expr.IsConstExpr) {
                                return new ConstULong((UInt16)((ConstULong)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT16, expr, type);

                        case ExprType.Kind.LONG:
                            if (expr.IsConstExpr) {
                                return new ConstLong((Int32)((ConstULong)expr).value, env);
                            }
                            return new TypeCast(Kind.NOP, expr, type);

                        case ExprType.Kind.FLOAT:
                            if (expr.IsConstExpr) {
                                return new ConstFloat(((ConstULong)expr).value, env);
                            }
                            return new TypeCast(Kind.INT32_TO_FLOAT, expr, type);

                        case ExprType.Kind.DOUBLE:
                            if (expr.IsConstExpr) {
                                return new ConstDouble(((ConstULong)expr).value, env);
                            }
                            return new TypeCast(Kind.INT32_TO_DOUBLE, expr, type);

                        default:
                            Debug.Assert(false);
                            return null;
                    }

                default:
                    Debug.Assert(false);
                    return null;
            }
        }

        /// <summary>
        /// From:
        ///     float, double
        /// To:
        ///     char, uchar, short, ushort, long, ulong, float, double
        /// </summary>
        /// <remarks>
        /// According to MSDN "Conversions from Floating-Point Types",
        ///   float cannot convert to unsigned char.
        /// I don't know why, but I follow it.
        /// </remarks>
        public static Expr FloatToArith(Expr expr, ExprType type) {

            ExprType.Kind from = expr.Type.kind;
            ExprType.Kind to = type.kind;
            Env env = expr.Env;

            switch (from) {
                case ExprType.Kind.FLOAT:
                    switch (to) {
                        case ExprType.Kind.CHAR:
                            if (expr.IsConstExpr) {
                                return new ConstLong((SByte)((ConstFloat)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT8, new TypeCast(Kind.FLOAT_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.SHORT:
                            if (expr.IsConstExpr) {
                                return new ConstLong((Int16)((ConstFloat)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT16, new TypeCast(Kind.FLOAT_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.USHORT:
                            if (expr.IsConstExpr) {
                                return new ConstULong((UInt16)((ConstFloat)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT16, new TypeCast(Kind.FLOAT_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.LONG:
                            if (expr.IsConstExpr) {
                                return new ConstLong((Int32)((ConstFloat)expr).value, env);
                            }
                            return new TypeCast(Kind.FLOAT_TO_INT32, expr, type);

                        case ExprType.Kind.ULONG:
                            if (expr.IsConstExpr) {
                                return new ConstULong((UInt32)((ConstFloat)expr).value, env);
                            }
                            return new TypeCast(Kind.FLOAT_TO_INT32, expr, type);

                        case ExprType.Kind.DOUBLE:
                            if (expr.IsConstExpr) {
                                return new ConstDouble(((ConstFloat)expr).value, env);
                            }
                            return new TypeCast(Kind.FLOAT_TO_DOUBLE, expr, type);

                        default:
                            throw new InvalidProgramException();
                    }

                case ExprType.Kind.DOUBLE:
                    switch (to) {
                        case ExprType.Kind.CHAR:
                            // double -> float -> char
                            if (expr.IsConstExpr) {
                                return new ConstLong((SByte)((ConstDouble)expr).value, env);
                            }
                            return FloatToArith(FloatToArith(expr, new TFloat(type.is_const, type.is_volatile)), new TChar(type.is_const, type.is_volatile));

                        case ExprType.Kind.SHORT:
                            // double -> float -> short
                            if (expr.IsConstExpr) {
                                return new ConstLong((Int16)((ConstDouble)expr).value, env);
                            }
                            return FloatToArith(FloatToArith(expr, new TFloat(type.is_const, type.is_volatile)), new TShort(type.is_const, type.is_volatile));

                        case ExprType.Kind.LONG:
                            // double -> float -> short
                            if (expr.IsConstExpr) {
                                return new ConstLong((Int32)((ConstDouble)expr).value, env);
                            }
                            return new TypeCast(Kind.DOUBLE_TO_INT32, expr, type);

                        case ExprType.Kind.ULONG:
                            if (expr.IsConstExpr) {
                                return new ConstULong((UInt32)((ConstDouble)expr).value, env);
                            }
                            return new TypeCast(Kind.DOUBLE_TO_INT32, expr, type);

                        case ExprType.Kind.USHORT:
                            // double -> long -> ushort
                            if (expr.IsConstExpr) {
                                return new ConstULong((UInt16)((ConstDouble)expr).value, env);
                            }
                            return new TypeCast(Kind.PRESERVE_INT16, new TypeCast(Kind.DOUBLE_TO_INT32, expr, new TLong(type.is_const, type.is_volatile)), type);

                        case ExprType.Kind.FLOAT:
                            if (expr.IsConstExpr) {
                                return new ConstFloat((Single)((ConstDouble)expr).value, env);
                            }
                            return new TypeCast(Kind.DOUBLE_TO_FLOAT, expr, type);

                        default:
                            throw new InvalidProgramException();
                    }

                default:
                    throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// From:
        ///     pointer
        /// To:
        ///     pointer, integral
        /// </summary>
        public static Expr FromPointer(Expr expr, ExprType type, Env env) {
            ExprType.Kind from = expr.Type.kind;
            ExprType.Kind to = type.kind;

            if (from != ExprType.Kind.POINTER) {
                throw new InvalidOperationException("Expected a pointer.");
            }

            // if we are casting to another pointer, do a nop
            if (to == ExprType.Kind.POINTER) {
                if (expr.IsConstExpr) {
                    return new ConstPtr(((ConstPtr)expr).value, type, env);
                }
                return new TypeCast(Kind.NOP, expr, type, env);
            }

            // if we are casting to an integral
            if (type.IsIntegral) {
                // pointer -> ulong -> whatever integral
                if (expr.IsConstExpr) {
                    expr = new ConstULong(((ConstPtr)expr).value, env);
                } else {
                    expr = new TypeCast(Kind.NOP, expr, new TULong(type.is_const, type.is_volatile), env);
                }
                return MakeCast(expr, type, env);
            }

            throw new InvalidOperationException("Casting from a pointer to an unsupported type.");
        }

        /// <summary>
        /// From:
        ///     pointer, integral
        /// To:
        ///     pointer
        /// </summary>
        public static Expr ToPointer(Expr expr, ExprType type, Env env) {
            ExprType.Kind from = expr.Type.kind;
            ExprType.Kind to = type.kind;

            if (to != ExprType.Kind.POINTER) {
                throw new InvalidOperationException("Error: expected casting to pointer.");
            }

            if (from == ExprType.Kind.POINTER) {
                if (expr.IsConstExpr) {
                    return new ConstPtr(((ConstPtr)expr).value, type, env);
                }
                return new TypeCast(Kind.NOP, expr, type, env);
            }

            if (expr.Type.IsIntegral) {
                // if we are casting from an integral

                // whatever integral -> ulong
                switch (expr.Type.kind) {
                    case ExprType.Kind.CHAR:
                    case ExprType.Kind.SHORT:
                    case ExprType.Kind.LONG:
                        expr = SignedIntegralToArith(expr, new TULong(type.is_const, type.is_volatile));
                        break;
                    case ExprType.Kind.UCHAR:
                    case ExprType.Kind.USHORT:
                    case ExprType.Kind.ULONG:
                        expr = UnsignedIntegralToArith(expr, new TULong(type.is_const, type.is_volatile));
                        break;
                    default:
                        break;
                }

                // ulong -> pointer
                if (expr.IsConstExpr) {
                    return new ConstPtr(((ConstULong)expr).value, type, env);
                }
                return new TypeCast(Kind.NOP, expr, type, env);
            }
            if (expr.Type is TFunction) {
                if (!expr.Type.EqualType((type as TPointer).ref_t)) {
                    throw new InvalidOperationException("Casting from an incompatible function.");
                }

                // TODO: only allow compatible type?
                return new TypeCast(Kind.NOP, expr, type, env);

            }
            if (expr.Type is TArray) {

                // TODO: allow any pointer type to cast to?
                return new TypeCast(Kind.NOP, expr, type, env);
            }

            throw new InvalidOperationException("Error: casting from an unsupported type to pointer.");
        }

        // MakeCast
        // ========
        // input: Expr, type
        // output: TypeCast
        // converts Expr to type
        // 
        public static Expr MakeCast(Expr expr, ExprType type, Env env) {

            // if two types are equal, return Expr
            if (EqualType(expr.Type, type)) {
                return expr;
            }

            // from pointer
            if (expr.Type.kind == ExprType.Kind.POINTER) {
                return FromPointer(expr, type, env);
            }

            // to pointer
            if (type.kind == ExprType.Kind.POINTER) {
                return ToPointer(expr, type, env);
            }

            switch (expr.Type.kind) {
                // from signed integral
                case ExprType.Kind.CHAR:
                case ExprType.Kind.SHORT:
                case ExprType.Kind.LONG:
                    return SignedIntegralToArith(expr, type);

                // from unsigned integral
                case ExprType.Kind.UCHAR:
                case ExprType.Kind.USHORT:
                case ExprType.Kind.ULONG:
                    return UnsignedIntegralToArith(expr, type);

                // from float
                case ExprType.Kind.FLOAT:
                case ExprType.Kind.DOUBLE:
                    return FloatToArith(expr, type);

                case ExprType.Kind.VOID:
                case ExprType.Kind.POINTER:
                case ExprType.Kind.FUNCTION:
                case ExprType.Kind.ARRAY:
                case ExprType.Kind.INCOMPLETE_ARRAY:
                case ExprType.Kind.STRUCT_OR_UNION:
                default:
                    throw new InvalidOperationException("Error: expression type not supported for casting from.");
            }

        }

        public static Expr MakeCast(Expr expr, ExprType type) =>
            MakeCast(expr, type, expr.Env);

        // UsualArithmeticConversion
        // =========================
        // input: e1, e2
        // output: tuple<e1', e2', enumexprtype>
        // performs the usual arithmetic conversion on e1 & e2
        // 
        // possible return type: double, float, ulong, long
        // 
        public static Tuple<Expr, Expr, ExprType.Kind> UsualArithmeticConversion(Expr e1, Expr e2) {
            ExprType t1 = e1.Type;
            ExprType t2 = e2.Type;

            Boolean c1 = t1.is_const;
            Boolean v1 = t1.is_volatile;
            Boolean c2 = t2.is_const;
            Boolean v2 = t2.is_volatile;
            // 1. if either Expr is double: both are converted to double
            if (t1.kind == ExprType.Kind.DOUBLE || t2.kind == ExprType.Kind.DOUBLE) {
                return new Tuple<Expr, Expr, ExprType.Kind>(MakeCast(e1, new TDouble(c1, v1)), MakeCast(e2, new TDouble(c2, v2)), ExprType.Kind.DOUBLE);
            }

            // 2. if either Expr is float: both are converted to float
            if (t1.kind == ExprType.Kind.FLOAT || t2.kind == ExprType.Kind.FLOAT) {
                return new Tuple<Expr, Expr, ExprType.Kind>(MakeCast(e1, new TFloat(c1, v1)), MakeCast(e2, new TFloat(c2, v2)), ExprType.Kind.FLOAT);
            }

            // 3. if either Expr is unsigned long: both are converted to unsigned long
            if (t1.kind == ExprType.Kind.ULONG || t2.kind == ExprType.Kind.ULONG) {
                return new Tuple<Expr, Expr, ExprType.Kind>(MakeCast(e1, new TULong(c1, v1)), MakeCast(e2, new TULong(c2, v2)), ExprType.Kind.ULONG);
            }

            // 4. both are converted to long
            return new Tuple<Expr, Expr, ExprType.Kind>(MakeCast(e1, new TLong(c1, v1)), MakeCast(e2, new TLong(c2, v2)), ExprType.Kind.LONG);

        }

        // UsualScalarConversion
        // =====================
        // input: e1, e2
        // output: tuple<e1', e2', enumexprtype>
        // first, convert pointers to ulongs, then do usual arithmetic conversion
        // 
        // possible return type: double, float, ulong, long
        // 
        public static Tuple<Expr, Expr, ExprType.Kind> UsualScalarConversion(Expr e1, Expr e2) {
            if (e1.Type.kind == ExprType.Kind.POINTER) {
                e1 = FromPointer(e1, new TULong(e1.Type.is_const, e1.Type.is_volatile), e2.Env);
            }
            if (e2.Type.kind == ExprType.Kind.POINTER) {
                e2 = FromPointer(e2, new TULong(e2.Type.is_const, e2.Type.is_volatile), e2.Env);
            }
            return UsualArithmeticConversion(e1, e2);
        }

        public static Tuple<Expr, ExprType.Kind> IntegralPromotion(Expr expr) {
            if (!expr.Type.IsIntegral) {
                throw new InvalidProgramException();
            }

            switch (expr.Type.kind) {
                case ExprType.Kind.CHAR:
                case ExprType.Kind.SHORT:
                case ExprType.Kind.LONG:
                    return Tuple.Create(MakeCast(expr, new TLong(expr.Type.is_const, expr.Type.is_volatile)), ExprType.Kind.LONG);

                case ExprType.Kind.UCHAR:
                case ExprType.Kind.USHORT:
                case ExprType.Kind.ULONG:
                    return Tuple.Create(MakeCast(expr, new TULong(expr.Type.is_const, expr.Type.is_volatile)), ExprType.Kind.ULONG);

                case ExprType.Kind.VOID:
                case ExprType.Kind.FLOAT:
                case ExprType.Kind.DOUBLE:
                case ExprType.Kind.POINTER:
                case ExprType.Kind.FUNCTION:
                case ExprType.Kind.ARRAY:
                case ExprType.Kind.INCOMPLETE_ARRAY:
                case ExprType.Kind.STRUCT_OR_UNION:
                default:
                    throw new InvalidProgramException();
            }
        }

    }
}