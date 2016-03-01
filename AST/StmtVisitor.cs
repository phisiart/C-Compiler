using System;
using System.Collections.Generic;

namespace AST {
    public abstract class StmtVisitor {
        public virtual void Visit(Stmt stmt) {}
        public virtual void Visit(GotoStmt stmt) {}
        public virtual void Visit(LabeledStmt stmt) {}
        public virtual void Visit(ContStmt stmt) {}
        public virtual void Visit(BreakStmt stmt) {}
        public virtual void Visit(ExprStmt stmt) {}
        public virtual void Visit(CompoundStmt stmt) {}
        public virtual void Visit(ReturnStmt stmt) {}
        public virtual void Visit(WhileStmt stmt) {}
        public virtual void Visit(DoWhileStmt stmt) {}
        public virtual void Visit(ForStmt stmt) {}
        public virtual void Visit(SwitchStmt stmt) {}
        public virtual void Visit(CaseStmt stmt) {}
        public virtual void Visit(DefaultStmt stmt) {}
        public virtual void Visit(IfStmt stmt) {}
        public virtual void Visit(IfElseStmt stmt) {}
    }

    public class CaseLabelsGrabber : StmtVisitor {
        private readonly List<Int32> _labels = new List<Int32>();
        public IReadOnlyList<Int32> Labels => this._labels;

        public static IReadOnlyList<Int32> GrabLabels(SwitchStmt stmt) {
            CaseLabelsGrabber grabber = new CaseLabelsGrabber();
            stmt.stmt.Accept(grabber);
            return grabber.Labels;
        }

        public override void Visit(Stmt stmt) {
            throw new InvalidOperationException("Cannot visit abstract Stmt");
        }

        public override void Visit(GotoStmt stmt) {}

        public override void Visit(LabeledStmt stmt) =>
            stmt.stmt.Accept(this);

        public override void Visit(ContStmt stmt) {}

        public override void Visit(BreakStmt stmt) {}

        public override void Visit(ExprStmt stmt) {}

        public override void Visit(CompoundStmt stmt) =>
            stmt.stmts.ForEach(_ => _.Item2.Accept(this));

        public override void Visit(ReturnStmt stmt) {}

        public override void Visit(WhileStmt stmt) =>
            stmt.body.Accept(this);

        public override void Visit(DoWhileStmt stmt) =>
            stmt.body.Accept(this);

        public override void Visit(ForStmt stmt) =>
            stmt.Body.Accept(this);

        public override void Visit(SwitchStmt stmt) {
            // Must ignore this.
        }

        public override void Visit(CaseStmt stmt) {
            // Record the Value.
            this._labels.Add(stmt.value);
            stmt.stmt.Accept(this);
        }

        public override void Visit(DefaultStmt stmt) =>
            stmt.stmt.Accept(this);

        public override void Visit(IfStmt stmt) =>
            stmt.stmt.Accept(this);

        public override void Visit(IfElseStmt stmt) {
            stmt.true_stmt.Accept(this);
            stmt.false_stmt.Accept(this);
        }
    }

    public class GotoLabelsGrabber : StmtVisitor {
        private readonly List<String> _labels = new List<String>();
        public IReadOnlyList<String> Labels => this._labels;

        public static IReadOnlyList<String> GrabLabels(Stmt stmt) {
            GotoLabelsGrabber grabber = new GotoLabelsGrabber();
            stmt.Accept(grabber);
            return grabber.Labels;
        }

        public override void Visit(Stmt stmt) {
            throw new InvalidOperationException("Cannot visit abstract Stmt");
        }

        public override void Visit(GotoStmt stmt) { }

        public override void Visit(LabeledStmt stmt) {
            this._labels.Add(stmt.label);
            stmt.stmt.Accept(this);
        }

        public override void Visit(ContStmt stmt) { }

        public override void Visit(BreakStmt stmt) { }

        public override void Visit(ExprStmt stmt) { }

        public override void Visit(CompoundStmt stmt) =>
            stmt.stmts.ForEach(_ => _.Item2.Accept(this));

        public override void Visit(ReturnStmt stmt) { }

        public override void Visit(WhileStmt stmt) =>
            stmt.body.Accept(this);

        public override void Visit(DoWhileStmt stmt) =>
            stmt.body.Accept(this);

        public override void Visit(ForStmt stmt) =>
            stmt.Body.Accept(this);

        public override void Visit(SwitchStmt stmt) {
            stmt.stmt.Accept(this);
        }

        public override void Visit(CaseStmt stmt) {
            stmt.stmt.Accept(this);
        }

        public override void Visit(DefaultStmt stmt) =>
            stmt.stmt.Accept(this);

        public override void Visit(IfStmt stmt) =>
            stmt.stmt.Accept(this);

        public override void Visit(IfElseStmt stmt) {
            stmt.true_stmt.Accept(this);
            stmt.false_stmt.Accept(this);
        }
    }
}

