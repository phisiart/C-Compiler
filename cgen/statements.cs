using System;
using System.Collections.Generic;

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
    public class Stmt {
        public virtual void CGenStmt(Env env, CGenState state) {
            throw new NotImplementedException();
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
//		public GotoStmt(Env _env, Stmt _parent, string _label) : base(_parent) {
//			stmt_label = _label;
//		}
		public GotoStmt(string _label) {
			stmt_label = _label;
		}
		public readonly string stmt_label;
	}

	/// <summary>
	/// Labeled Statement
	/// </summary>
	public class LabeledStmt : Stmt {
//		public LabeledStmt(ref Env _env, Stmt _parent, string _label, SyntaxTree.Statement _stmt) : base(_parent) {
//			stmt_label = _label;
//			stmt_stmt = _stmt.Semant(ref _env, this);
//		}
		public LabeledStmt(string _label, Stmt _stmt) {
			stmt_label = _label;
			stmt_stmt = _stmt;
		}
		public readonly string stmt_label;
		public readonly Stmt stmt_stmt;
	}

	/// <summary>
	/// Continue Statement
	/// </summary>
	public class ContStmt : Stmt {
		public ContStmt() {}
	}

	/// <summary>
	/// Break Statement
	/// </summary>
	public class BreakStmt : Stmt {
		public BreakStmt() {}
	}

	/// <summary>
	/// Expression Statement
	/// </summary>
    public class ExprStmt : Stmt {
        public ExprStmt(Expr expr) {
            stmt_expr = expr;
        }
        public readonly Expr stmt_expr;

		public override void CGenStmt(Env env, CGenState state) {
			stmt_expr.CGenValue(env, state);
		}
    }

    public class CompoundStmt : Stmt {
        public CompoundStmt(List<Tuple<Env, Decln>> declns, List<Tuple<Env, Stmt>> stmts) {
            stmt_declns = declns;
            stmt_stmts = stmts;
        }
//
//		public CompoundStmt(ref Env env, List<SyntaxTree.Declaration> declns, List<SyntaxTree.Statement> stmts) {
//			// a '{' opens an inner scope
//			env = env.InScope();
//
//			stmt_declns = new List<Tuple<Env, Decln>>();
//			foreach (SyntaxTree.Declaration decln in declns) {
//				Tuple<Env, List<Tuple<Env, Decln>>> r_decln = decln.GetDeclns(env);
//				env = r_decln.Item1;
//				stmt_declns.AddRange(r_decln.Item2);
//			}
//
//			stmt_stmts = new List<Tuple<Env, Stmt>>();
//			foreach (SyntaxTree.Statement stmt in stmts) {
//				Stmt stmt_stmt = stmt.Semant(ref env, this);
//				stmt_stmts.Add(Tuple.Create(env, stmt_stmt));
//			}
//
//			// a '}' closes an inner scope
//			env = env.OutScope();
//		}

        public readonly List<Tuple<Env, Decln>> stmt_declns;
        public readonly List<Tuple<Env, Stmt>> stmt_stmts;

        public override void CGenStmt(Env env, CGenState state) {
            foreach (Tuple<Env, Decln> decln in stmt_declns) {
                decln.Item2.CGenExternDecln(decln.Item1, state);
            }
            foreach (Tuple<Env, Stmt> stmt in stmt_stmts) {
                stmt.Item2.CGenStmt(stmt.Item1, state);
            }
        }


    }

    public class ReturnStmt : Stmt {
        public ReturnStmt(Expr expr) {
            stmt_expr = expr;
        }
        public readonly Expr stmt_expr;

        public override void CGenStmt(Env env, CGenState state) {

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
    public class WhileStmt : Stmt {
        public WhileStmt(Expr _cond, Stmt _body) {
            while_cond = _cond;
            while_body = _body;
        }
        public readonly Expr while_cond;
        public readonly Stmt while_body;
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
    public class DoWhileStmt : Stmt {
        public DoWhileStmt(Stmt _body, Expr _expr) {
            do_body = _body;
            do_expr = _expr;
        }
        public readonly Stmt do_body;
        public readonly Expr do_expr;
    }

    public class ForStmt : Stmt {
        public ForStmt(Expr _init, Expr _cond, Expr _loop, Stmt _body) {
            for_init = _init;
            for_cond = _cond;
            for_loop = _loop;
            for_body = _body;
        }
        public readonly Expr for_init;
        public readonly Expr for_cond;
        public readonly Expr for_loop;
        public readonly Stmt for_body;
    }

	/// <summary>
	/// Switch Statement
	/// </summary>
	public class SwitchStmt : Stmt {
		public SwitchStmt(Expr _expr, Stmt _stmt) {
			switch_expr = _expr;
			switch_stmt = _stmt;
		}
		public readonly Expr switch_expr;
		public readonly Stmt switch_stmt;
	}

	/// <summary>
	/// Case Statement
	/// </summary>
	public class CaseStmt : Stmt {
		public CaseStmt(Int32 _value, Stmt _stmt) {
			case_value = _value;
			case_stmt = _stmt;
		}
		public readonly Int32 case_value;
		public readonly Stmt case_stmt;
	}

	public class DefaultStmt : Stmt {
		public DefaultStmt(Stmt _stmt) {
			default_stmt = _stmt;
		}
		public readonly Stmt default_stmt;
	}

	/// <summary>
	/// If Statement
	/// </summary>
	public class IfStmt : Stmt {
		public IfStmt(Expr _cond, Stmt _stmt) {
			if_cond = _cond;
			if_stmt = _stmt;
		}
		public readonly Expr if_cond;
		public readonly Stmt if_stmt;
	}

	/// <summary>
	/// If-else Statement
	/// </summary>
	public class IfElseStmt : Stmt {
		public IfElseStmt(Expr _cond, Stmt _true_stmt, Stmt _false_stmt) {
			if_cond = _cond;
			if_true_stmt = _true_stmt;
			if_false_stmt = _false_stmt;
		}
		public readonly Expr if_cond;
		public readonly Stmt if_true_stmt;
		public readonly Stmt if_false_stmt;
	}

}