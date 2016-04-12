using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ABT {
    public abstract partial class ExprType {
        public virtual Int32 Precedence => 0;

        public abstract String Decl(String name, Int32 precedence);

        public String Decl(String name) => this.Decl(name, 0);

        public String Decl() => this.Decl("");
    }

    public partial class VoidType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}void {name}".TrimEnd(' ');
    }

    public partial class CharType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}char {name}".TrimEnd(' ');
    }

    public partial class UCharType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}unsigned char {name}".TrimEnd(' ');
    }

    public partial class ShortType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}short {name}".TrimEnd(' ');
    }

    public partial class UShortType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}unsigned short {name}".TrimEnd(' ');
    }

    public partial class LongType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}long {name}".TrimEnd(' ');
    }

    public partial class ULongType {
        public override String Decl(String name, Int32 precedence) =>
            $"{DumpQualifiers()}unsigned long {name}".TrimEnd(' ');
    }

    public partial class FloatType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}float {name}".TrimEnd(' ');
    }

    public partial class DoubleType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}double {name}".TrimEnd(' ');
    }

    public partial class PointerType {
        public override String Decl(String name, Int32 precedence) {
            if (precedence > this.Precedence) {
                name = $"({name})";
            }
            return this.RefType.Decl($"*{this.DumpQualifiers()}{name}", this.Precedence);
        }
    }

    public partial class IncompleteArrayType {
        public override String Decl(String name, Int32 precedence) {
            if (precedence > this.Precedence) {
                name = $"({name})";
            }
            return this.ElemType.Decl($"{name}[]", this.Precedence);
        }
    }

    public partial class ArrayType {
        public override String Decl(String name, Int32 precedence) {
            if (precedence > this.Precedence) {
                name = $"({name})";
            }
            return this.ElemType.Decl($"{name}[{this.NumElems}]", this.Precedence);
        }
    }

    public partial class StructOrUnionType {
        public override String Decl(String name, Int32 precedence) =>
            $"{this.DumpQualifiers()}{this._layout.TypeName} {name}".TrimEnd(' ');
    }

    public partial class FunctionType {
        public override String Decl(String name, Int32 precedence) {
            if (precedence > this.Precedence) {
                name = $"({name})";
            }

            String str = "";
            if (this.Args.Count == 0) {
                if (this.HasVarArgs) {
                    str = "(...)";
                } else {
                    str = "(void)";
                }
            } else {
                str = this.Args[0].type.Decl();
                for (Int32 i = 1; i < this.Args.Count; ++i) {
                    str += $", {this.Args[i].type.Decl()}";
                }
                if (this.HasVarArgs) {
                    str += ", ...";
                }
                str = $"({str})";
            }

            return this.ReturnType.Decl($"{name}{str}", this.Precedence);
        }
    }
}
