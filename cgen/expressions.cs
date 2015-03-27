using System;
using System.Collections.Generic;

namespace AST {
    // Expr 
    // ========================================================================
    public class Expr {
        public Expr(ExprType _type) {
            type = _type;
        }
        public virtual Boolean IsConstExpr() { return false; }
        public virtual Reg CGenValue(Env env, CGenState state) {
            throw new NotImplementedException();
        }
        public virtual void CGenAddress(Env env, CGenState state) {
            throw new NotImplementedException();
        }
        public virtual void CGenPush(Env env, CGenState state) {
            throw new NotImplementedException();
        }
        public readonly ExprType type;
    }

    public class EmptyExpr : Expr {
        public EmptyExpr() : base(new TVoid()) { }
        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(0, Reg.EAX);
            return Reg.EAX;
        }
        public override void CGenAddress(Env env, CGenState state) {
            state.MOVL(0, Reg.EAX);
        }
    }

    public class Variable : Expr {
        public Variable(ExprType _type, String _name)
            : base(_type) {
            name = _name;
        }
        public readonly String name;

        public override void CGenAddress(Env env, CGenState state) {
            Env.Entry entry = env.Find(name);
            switch (entry.entry_loc) {
            case Env.EntryLoc.FRAME:
                break;
            case Env.EntryLoc.STACK:
                break;
            case Env.EntryLoc.GLOBAL:
                switch (entry.entry_type.expr_type) {
                case ExprType.EnumExprType.FUNCTION:
                    state.LEA(name);
                    break;
                case ExprType.EnumExprType.CHAR:
                case ExprType.EnumExprType.DOUBLE:
                case ExprType.EnumExprType.ERROR:
                case ExprType.EnumExprType.FLOAT:
                
                case ExprType.EnumExprType.LONG:
                case ExprType.EnumExprType.POINTER:
                case ExprType.EnumExprType.SHORT:
                case ExprType.EnumExprType.STRUCT:
                case ExprType.EnumExprType.UCHAR:
                case ExprType.EnumExprType.ULONG:
                case ExprType.EnumExprType.UNION:
                case ExprType.EnumExprType.USHORT:
                case ExprType.EnumExprType.VOID:
                default:
                    throw new NotImplementedException();
                }
                break;
            case Env.EntryLoc.ENUM:
            case Env.EntryLoc.NOT_FOUND:
            case Env.EntryLoc.TYPEDEF:
            default:
                throw new InvalidOperationException("Error: cannot get the address of " + entry.entry_loc);
            }
        }

        public override void CGenPush(Env env, CGenState state) {
            //state.COMMENT("push " + name);
            Env.Entry entry = env.Find(name);
            switch (entry.entry_loc) {
            case Env.EntryLoc.ENUM:
                // enum constant : just an integer
                state.PUSHL(entry.entry_offset);
                break;
            case Env.EntryLoc.FRAME:
                switch (entry.entry_type.expr_type) {
                case ExprType.EnumExprType.LONG:
                case ExprType.EnumExprType.ULONG:
                case ExprType.EnumExprType.POINTER:
                    state.LOAD(entry.entry_offset, Reg.EBP, Reg.EAX);
                    state.PUSHL(Reg.EAX);
                    break;

                case ExprType.EnumExprType.FLOAT:
                case ExprType.EnumExprType.DOUBLE:
                case ExprType.EnumExprType.STRUCT:
                case ExprType.EnumExprType.UNION:
                    throw new NotImplementedException();

                case ExprType.EnumExprType.VOID:
                case ExprType.EnumExprType.FUNCTION:
                case ExprType.EnumExprType.CHAR:
                case ExprType.EnumExprType.UCHAR:
                case ExprType.EnumExprType.SHORT:
                case ExprType.EnumExprType.USHORT:
                case ExprType.EnumExprType.ERROR:
                default:
                    throw new InvalidOperationException("Error: cannot push type " + entry.entry_type.expr_type);
                }
                break;

            case Env.EntryLoc.GLOBAL:
                throw new NotImplementedException();

            case Env.EntryLoc.STACK:
                switch (entry.entry_type.expr_type) {
                case ExprType.EnumExprType.LONG:
                case ExprType.EnumExprType.ULONG:
                case ExprType.EnumExprType.POINTER:
                    state.LOAD(-entry.entry_offset, Reg.EBP, Reg.EAX);
                    state.PUSHL(Reg.EAX);
                    break;

                case ExprType.EnumExprType.FLOAT:
                case ExprType.EnumExprType.DOUBLE:
                case ExprType.EnumExprType.STRUCT:
                case ExprType.EnumExprType.UNION:
                    throw new NotImplementedException();

                case ExprType.EnumExprType.VOID:
                case ExprType.EnumExprType.FUNCTION:
                case ExprType.EnumExprType.CHAR:
                case ExprType.EnumExprType.UCHAR:
                case ExprType.EnumExprType.SHORT:
                case ExprType.EnumExprType.USHORT:
                case ExprType.EnumExprType.ERROR:
                default:
                    throw new InvalidOperationException("Error: cannot push type " + entry.entry_type.expr_type + " from stack");
                }
                break;
            case Env.EntryLoc.TYPEDEF:
            case Env.EntryLoc.NOT_FOUND:
            default:
                throw new InvalidOperationException();
            }
        }

		public override Reg CGenValue(Env env, CGenState state) {
			Env.Entry entry = env.Find(name);
			switch (entry.entry_loc) {
			case Env.EntryLoc.ENUM:
				// enum constant : just an integer
				state.MOVL(entry.entry_offset, Reg.EAX);
				return Reg.EAX;

			case Env.EntryLoc.FRAME:
				switch (entry.entry_type.expr_type) {
				case ExprType.EnumExprType.LONG:
				case ExprType.EnumExprType.ULONG:
				case ExprType.EnumExprType.POINTER:
					state.LOAD(entry.entry_offset, Reg.EBP, Reg.EAX);
					return Reg.EAX;

				case ExprType.EnumExprType.FLOAT:
				case ExprType.EnumExprType.DOUBLE:
				case ExprType.EnumExprType.STRUCT:
				case ExprType.EnumExprType.UNION:
					throw new NotImplementedException();

				case ExprType.EnumExprType.VOID:
				case ExprType.EnumExprType.FUNCTION:
				case ExprType.EnumExprType.CHAR:
				case ExprType.EnumExprType.UCHAR:
				case ExprType.EnumExprType.SHORT:
				case ExprType.EnumExprType.USHORT:
				case ExprType.EnumExprType.ERROR:
				default:
					throw new InvalidOperationException("Error: cannot push type " + entry.entry_type.expr_type);
				}

			case Env.EntryLoc.GLOBAL:
				throw new NotImplementedException();

			case Env.EntryLoc.STACK:
				switch (entry.entry_type.expr_type) {
				case ExprType.EnumExprType.LONG:
				case ExprType.EnumExprType.ULONG:
				case ExprType.EnumExprType.POINTER:
					state.LOAD(-entry.entry_offset, Reg.EBP, Reg.EAX);
					return Reg.EAX;

				case ExprType.EnumExprType.FLOAT:
				case ExprType.EnumExprType.DOUBLE:
				case ExprType.EnumExprType.STRUCT:
				case ExprType.EnumExprType.UNION:
					throw new NotImplementedException();

				case ExprType.EnumExprType.VOID:
				case ExprType.EnumExprType.FUNCTION:
				case ExprType.EnumExprType.CHAR:
				case ExprType.EnumExprType.UCHAR:
				case ExprType.EnumExprType.SHORT:
				case ExprType.EnumExprType.USHORT:
				case ExprType.EnumExprType.ERROR:
				default:
					throw new InvalidOperationException("Error: cannot push type " + entry.entry_type.expr_type + " from stack");
				}

			case Env.EntryLoc.TYPEDEF:
			case Env.EntryLoc.NOT_FOUND:
			default:
				throw new InvalidOperationException();
			}
		}
    }

    public class Constant : Expr {
        public Constant(ExprType _type)
            : base(_type) { }
        public override Boolean IsConstExpr() { return true; }
		public override void CGenAddress(Env env, CGenState state) {
			throw new InvalidOperationException("Error: cannot get the address of a constant");
		}
    }

    public class ConstLong : Constant {
        public ConstLong(Int32 _value)
            : base(new TLong(true)) {
            value = _value;
        }

        public override String ToString() {
            return "Int32(" + value + ")";
        }
        public readonly Int32 value;


        public override Reg CGenValue(Env env, CGenState state) {
            state.MOVL(value, Reg.EAX);
            return Reg.EAX;
        }

		public override void CGenPush(Env env, CGenState state) {
			state.PUSHL(value);
		}
    }

    public class ConstULong : Constant {
        public ConstULong(UInt32 _value)
            : base(new TULong(true)) {
            value = _value;
        }

        public override String ToString() {
            return "uint(" + value + ")";
        }
        public readonly UInt32 value;

		public override Reg CGenValue(Env env, CGenState state) {
			state.MOVL((Int32)value, Reg.EAX);
			return Reg.EAX;
		}

		public override void CGenPush(Env env, CGenState state) {
			state.PUSHL((Int32)value);
		}
    }

    public class ConstPtr : Constant {
        public ConstPtr(UInt32 _value, ExprType _type)
            : base(_type) {
            value = _value;
        }

        public override String ToString() {
            return this.type.ToString() + "(" + value + ")";
        }
        public readonly UInt32 value;

		public override Reg CGenValue(Env env, CGenState state) {
			state.MOVL((Int32)value, Reg.EAX);
			return Reg.EAX;
		}

		public override void CGenPush(Env env, CGenState state) {
			state.PUSHL((Int32)value);
		}
    }

    public class ConstFloat : Constant {
        public ConstFloat(Single _value)
            : base(new TFloat(true)) {
            value = _value;
        }
        public override String ToString() {
            return "float(" + value + ")";
        }
        public readonly Single value;

		public override Reg CGenValue(Env env, CGenState state) {
			byte[] bytes = BitConverter.GetBytes(value);
			Int32 intval = BitConverter.ToInt32(bytes, 0);
			String name = state.CGenLongConst(intval);
			state.MOVL(name, Reg.XMM0);
			return Reg.XMM0;
		}
    }

    public class ConstDouble : Constant {
        public ConstDouble(Double _value)
            : base(new TDouble(true)) {
            value = _value;
        }
        public override String ToString() {
            return "double(" + value + ")";
        }
        public readonly Double value;
    }

    public class ConstStringLiteral : Constant {
        public ConstStringLiteral(String _value)
            : base(new TPointer(new TChar(true), true)) {
            value = _value;
        }
        public readonly String value;
    }

    public class AssignmentList : Expr {
        public AssignmentList(List<Expr> _exprs, ExprType _type)
            : base(_type) {
            exprs = _exprs;
        }
        public readonly List<Expr> exprs;

		public override Reg CGenValue(Env env, CGenState state) {
			Reg reg = Reg.EAX;
			foreach (Expr expr in exprs) {
				reg = expr.CGenValue(env, state);
			}
			return reg;
		}
    }

    public class Assignment : Expr {
        public Assignment(Expr _lvalue, Expr _rvalue, ExprType _type)
            : base(_type) {
            lvalue = _lvalue;
            rvalue = _rvalue;
        }
		public readonly Expr lvalue;
		public readonly Expr rvalue;
    }

	public class ConditionalExpr : Expr {
		public ConditionalExpr(Expr _cond, Expr _true_expr, Expr _false_expr, ExprType _type)
			: base(_type) {
			cond_cond = _cond;
			cond_true_expr = _true_expr;
			cond_false_expr = _false_expr;
		}
		public readonly Expr cond_cond;
		public readonly Expr cond_true_expr;
		public readonly Expr cond_false_expr;
	}

    public class FunctionCall : Expr {
        public FunctionCall(Expr _function, TFunction _func_type, List<Expr> _arguments, ExprType _type)
            : base(_type) {
            call_func = _function;
            call_func_type = _func_type;
            call_args = _arguments;
        }
        public readonly Expr       call_func;
        public readonly TFunction  call_func_type;
        public readonly List<Expr> call_args;

        public override void CGenAddress(Env env, CGenState state) {
            throw new Exception("Error: cannot get the address of a function call.");
        }

        public override Reg CGenValue(Env env, CGenState state) {
            
            // Push the arguments onto the stack in reverse order
            for (Int32 i = call_args.Count; i --> 0;) {
                Expr arg = call_args[i];
                arg.CGenPush(env, state);
            }

            // Get function address
            call_func.CGenAddress(env, state);

            state.CALL("*%eax");

            return Reg.EAX;
        }
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

    public class Equal : Expr {
        public Equal(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class GEqual : Expr {
        public GEqual(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class Greater : Expr {
        public Greater(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class LEqual : Expr {
        public LEqual(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class Less : Expr {
        public Less(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }
    
	public class Add : Expr {
		public Add(Expr _lhs, Expr _rhs, ExprType _type)
			: base(_type) {
			add_lhs = _lhs;
			add_rhs = _rhs;
		}
		public readonly Expr add_lhs;
		public readonly Expr add_rhs;

		public static AST.Expr GetPointerAddition(AST.Expr ptr, AST.Expr offset) {
			if (ptr.type.expr_type != AST.ExprType.EnumExprType.POINTER) {
				throw new InvalidOperationException("Error: expect a pointer");
			}
			if (offset.type.expr_type != AST.ExprType.EnumExprType.LONG) {
				throw new InvalidOperationException("Error: expect an integer");
			}

			if (ptr.IsConstExpr() && offset.IsConstExpr()) {
				Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
				Int32 _scale = ((AST.TPointer)(ptr.type)).referenced_type.SizeOf;
				Int32 _offset = ((AST.ConstLong)offset).value;
				return new AST.ConstPtr((UInt32)(_base + _scale * _offset), ptr.type);
			}

			return AST.TypeCast.ToPointer(
				new AST.Add(
					AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile)),
					new AST.Multiply(
						offset,
						new AST.ConstLong(((AST.TPointer)(ptr.type)).referenced_type.SizeOf),
						new AST.TLong(offset.type.is_const, offset.type.is_volatile)
					),
					new AST.TLong(offset.type.is_const, offset.type.is_volatile)
				),
				ptr.type
			);
		}

		public static Tuple<Env, Expr> MakeAdd(Env env, Expr lhs, Expr rhs) {
			if (lhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
				if (!rhs.type.IsIntegral()) {
					throw new InvalidOperationException("Error: must add an integral to a pointer");
				}
				rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));

				// lhs = base, rhs = offset
				return new Tuple<AST.Env, AST.Expr>(env, GetPointerAddition(lhs, rhs));

			} else if (rhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
				if (!lhs.type.IsIntegral()) {
					throw new InvalidOperationException("Error: must add an integral to a pointer");
				}
				lhs = AST.TypeCast.MakeCast(lhs, new AST.TLong(lhs.type.is_const, rhs.type.is_volatile));

				// rhs = base, lhs = offset
				return new Tuple<AST.Env, AST.Expr>(env, GetPointerAddition(rhs, lhs));

			} else {
				return SyntaxTree.Expression.GetArithmeticBinOpExpr(
					env,
					lhs,
					rhs,
					(x, y) => x + y,
					(x, y) => x + y,
					(x, y) => x + y,
					(x, y) => x + y,
					(_lhs, _rhs, _type) => new AST.Add(_lhs, _rhs, _type)
				);
			}
		}
	}

	public class Sub : Expr {
		public Sub(Expr _lhs, Expr _rhs, ExprType _type)
			: base(_type) {
			sub_lhs = _lhs;
			sub_rhs = _rhs;
		}
		public readonly Expr sub_lhs;
		public readonly Expr sub_rhs;

		public static AST.Expr GetPointerSubtraction(AST.Expr ptr, AST.Expr offset) {
			if (ptr.type.expr_type != AST.ExprType.EnumExprType.POINTER) {
				throw new InvalidOperationException("Error: expect a pointer");
			}
			if (offset.type.expr_type != AST.ExprType.EnumExprType.LONG) {
				throw new InvalidOperationException("Error: expect an integer");
			}

			if (ptr.IsConstExpr() && offset.IsConstExpr()) {
				Int32 _base = (Int32)((AST.ConstPtr)ptr).value;
				Int32 _scale = ((AST.TPointer)(ptr.type)).referenced_type.SizeOf;
				Int32 _offset = ((AST.ConstLong)offset).value;
				return new AST.ConstPtr((UInt32)(_base - _scale * _offset), ptr.type);
			}

			return AST.TypeCast.ToPointer(
				new AST.Sub(
					AST.TypeCast.FromPointer(ptr, new AST.TLong(ptr.type.is_const, ptr.type.is_volatile)),
					new AST.Multiply(
						offset,
						new AST.ConstLong(((AST.TPointer)(ptr.type)).referenced_type.SizeOf),
						new AST.TLong(offset.type.is_const, offset.type.is_volatile)
					),
					new AST.TLong(offset.type.is_const, offset.type.is_volatile)
				),
				ptr.type
			);
		}

		public static Tuple<Env, Expr> MakeSub(Env env, Expr lhs, Expr rhs) {
			if (lhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
				if (rhs.type.expr_type == AST.ExprType.EnumExprType.POINTER) {
					// both operands are pointers

					AST.TPointer lhs_type = (AST.TPointer)(lhs.type);
					AST.TPointer rhs_type = (AST.TPointer)(rhs.type);
					if (!lhs_type.referenced_type.EqualType(rhs_type.referenced_type)) {
						throw new InvalidOperationException("Error: the two pointers points to different types");
					}

					Int32 scale = lhs_type.referenced_type.SizeOf;

					if (lhs.IsConstExpr() && rhs.IsConstExpr()) {
						return new Tuple<AST.Env, AST.Expr>(
							env,
							new AST.ConstLong(
								(Int32)(((AST.ConstPtr)lhs).value - ((AST.ConstPtr)rhs).value) / scale
							)
						);

					} else {
						return new Tuple<AST.Env, AST.Expr>(
							env,
							new AST.Divide(
								// long(lhs) - long(rhs)
								new AST.Sub(
									AST.TypeCast.MakeCast(lhs, new AST.TLong()),
									AST.TypeCast.MakeCast(rhs, new AST.TLong()),
									new AST.TLong()
								),
								// / scale
								new AST.ConstLong(scale),
								new AST.TLong()
							)
						);
					}

				} else {
					// pointer - integral

					if (!rhs.type.IsIntegral()) {
						throw new InvalidOperationException("Error: expected an integral");
					}

					rhs = AST.TypeCast.MakeCast(rhs, new AST.TLong(rhs.type.is_const, rhs.type.is_volatile));

					return new Tuple<AST.Env, AST.Expr>(env, GetPointerSubtraction(lhs, rhs));
				}

			} else {
				// lhs is not a pointer.

				// we need usual arithmetic cast
				return SyntaxTree.Expression.GetArithmeticBinOpExpr(
					env,
					lhs,
					rhs,
					(x, y) => x - y,
					(x, y) => x - y,
					(x, y) => x - y,
					(x, y) => x - y,
					(_lhs, _rhs, _type) => new AST.Sub(_lhs, _rhs, _type)
				);

			}
		}
	}

    public class Multiply : Expr {
        public Multiply(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            mult_lhs = _lhs;
            mult_rhs = _rhs;
        }
        public readonly Expr mult_lhs;
		public readonly Expr mult_rhs;

		public static Tuple<Env, Expr> MakeMultiply(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetArithmeticBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => x * y,
				(x, y) => x * y,
				(x, y) => x * y,
				(x, y) => x * y,
				(_lhs, _rhs, _type) => new Multiply(_lhs, _rhs, _type)
			);
		}
    }

    public class Divide : Expr {
        public Divide(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
			div_lhs = _lhs;
			div_rhs = _rhs;
        }
		public readonly Expr div_lhs;
        public readonly Expr div_rhs;

		public static Tuple<Env, Expr> MakeDivide(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetArithmeticBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => x / y,
				(x, y) => x / y,
				(x, y) => x / y,
				(x, y) => x / y,
				(_lhs, _rhs, _type) => new Divide(lhs, rhs, _type)
			);
		}
    }

    public class Modulo : Expr {
        public Modulo(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            mod_lhs = _lhs;
            mod_rhs = _rhs;
        }
		public readonly Expr mod_lhs;
		public readonly Expr mod_rhs;

		public static Tuple<Env, Expr> MakeModulo(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetIntegralBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => x % y,
				(x, y) => x % y,
				(_lhs, _rhs, _type) => new Modulo(_lhs, _rhs, _type)
			);
		}
    }

    public class LShift : Expr {
        public LShift(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lshift_lhs = _lhs;
            lshift_rhs = _rhs;
        }
		public readonly Expr lshift_lhs;
		public readonly Expr lshift_rhs;

		public static Tuple<Env, Expr> MakeLShift(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetIntegralBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => (UInt32)((Int32)x << (Int32)y),
				(x, y) => x << y,
				(_lhs, _rhs, type) => new LShift(lhs, rhs, type)
			);
		}
    }

    public class RShift : Expr {
        public RShift(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            rshift_lhs = _lhs;
            rshift_rhs = _rhs;
        }
        public readonly Expr rshift_lhs;
        public readonly Expr rshift_rhs;

		public static Tuple<Env, Expr> MakeRShift(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetIntegralBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => (UInt32)((Int32)x >> (Int32)y),
				(x, y) => x >> y,
				(_lhs, _rhs, _type) => new RShift(_lhs, _rhs, _type)
			);
		}
    }

    public class Xor : Expr {
        public Xor(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            xor_lhs = _lhs;
            xor_rhs = _rhs;
        }
        public readonly Expr xor_lhs;
        public readonly Expr xor_rhs;

		public static Tuple<Env, Expr> MakeXor(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetIntegralBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => x ^ y,
				(x, y) => x ^ y,
				(_lhs, _rhs, _type) => new Xor(_lhs, _rhs, _type)
			);
		}
    }

    public class BitwiseOr : Expr {
        public BitwiseOr(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            or_lhs = _lhs;
            or_rhs = _rhs;
        }
        public readonly Expr or_lhs;
        public readonly Expr or_rhs;

		public static Tuple<Env, Expr> MakeOr(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetIntegralBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => x | y,
				(x, y) => x | y,
				(_lhs, _rhs, _type) => new AST.BitwiseOr(_lhs, _rhs, _type)
			);
		}

		public override Reg CGenValue(Env env, CGenState state) {
			Reg reg_lhs = or_lhs.CGenValue(env, state);
			if (reg_lhs != Reg.EAX) {
				throw new InvalidOperationException("Why not %eax?");
			}
			state.PUSHL(reg_lhs);

			Reg reg_rhs = or_rhs.CGenValue(env, state);
			if (reg_rhs != Reg.EAX) {
				throw new InvalidOperationException("Why not %eax?");
			}
			state.POPL(Reg.EBX);

			state.ORL(Reg.EBX, Reg.EAX);

			return Reg.EAX;
		}
    }

    public class BitwiseAnd : Expr {
        public BitwiseAnd(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;

		public static Tuple<Env, Expr> MakeBitwiseAnd(Env env, Expr lhs, Expr rhs) {
			return SyntaxTree.Expression.GetIntegralBinOpExpr(
				env,
				lhs,
				rhs,
				(x, y) => x & y,
				(x, y) => x & y,
				(_lhs, _rhs, _type) => new BitwiseAnd(_lhs, _rhs, _type)
			);
		}
    }

    public class LogicalAnd : Expr {
        public LogicalAnd(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class LogicalOr : Expr {
        public LogicalOr(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }

        public readonly Expr lhs;
        public readonly Expr rhs;
    }

    public class NotEqual : Expr {
        public NotEqual(Expr _lhs, Expr _rhs, ExprType _type)
            : base(_type) {
            lhs = _lhs;
            rhs = _rhs;
        }
        public readonly Expr lhs;
        public readonly Expr rhs;
    }
}