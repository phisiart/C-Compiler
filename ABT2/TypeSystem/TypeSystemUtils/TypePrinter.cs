using System;
using System.Text;
using ABT2.Environment;

namespace ABT2.TypeSystem {
    public static partial class TypeSystemUtils {
        
        public sealed class TypePrinter : IExprTypeVisitor {
            public TypePrinter(TypeQuals typeQuals, Env env) {
                this.TypeQuals = typeQuals;
                this.Kind = NameKind.START;
                this.Name = "";
                this.Env = env;
            }

            private void printTypeQuals(StringBuilder builder) {
                String typeQualsStr = this.TypeQuals.ToString();

                if (typeQualsStr.Length != 0) {
                    builder.Append(typeQualsStr);
                    builder.Append(" ");
                }
            }

            public void VisitArithmeticType(IArithmeticType type) {
                var builder = new StringBuilder();

                this.printTypeQuals(builder);

                builder.Append(type.ToString());

                if (this.Name != "") {
                    builder.Append(" ");
                    builder.Append(this.Name);
                }

                this.Name = builder.ToString();
            }

            public void VisitSignedChar(TSChar type) {
                VisitArithmeticType(type);
            }

            public void VisitUnsignedChar(TUChar type) {
                VisitArithmeticType(type);
            }

            public void VisitSignedShort(TSShort type) {
                VisitArithmeticType(type);
            }

            public void VisitUnsignedShort(TUShort type) {
                VisitArithmeticType(type);
            }

            public void VisitSignedInt(TSInt type) {
                VisitArithmeticType(type);
            }

            public void VisitUnsignedInt(TUInt type) {
                VisitArithmeticType(type);
            }

            public void VisitSignedLong(TSLong type) {
                VisitArithmeticType(type);
            }

            public void VisitUnsignedLong(TULong type) {
                VisitArithmeticType(type);
            }

            public void VisitFloat(TFloat type) {
                VisitArithmeticType(type);
            }

            public void VisitDouble(TDouble type) {
                VisitArithmeticType(type);
            }

            public void VisitPointer(TPointer type) {
                if (this.Kind == NameKind.FINISHED) {
                    throw new InvalidOperationException("invalid status");
                }

                if (this.TypeQuals.ToString() == "" || this.Name == "") {
                    this.Name = $"*{this.TypeQuals.ToString()}{this.Name}";
                } else {
                    this.Name = $"*{this.TypeQuals.ToString()} {this.Name}";
                }

                this.Kind = NameKind.LEFT;

                this.TypeQuals = type.ElemQualType.TypeQuals;
                type.ElemQualType.Type.Visit(this);
            }

            public void VisitStructOrUnion(StructOrUnionType type) {
                if (this.Kind == NameKind.FINISHED) {
                    throw new InvalidOperationException("invalid status");
                }

                var builder = new StringBuilder();

                this.printTypeQuals(builder);

                if (type.Kind == StructOrUnionKind.Struct) {
                    builder.Append("struct ");
                } else if (type.Kind == StructOrUnionKind.Union) {
                    builder.Append("union ");
                } else {
                    throw new InvalidOperationException("invalid kind: must be struct or union");
                }

                builder.Append("{");

                type.GetLayoutOpt(this.Env).ForEach(layout => {
                    foreach (var member in layout.Members) {
                        var printer = new TypePrinter(member.QualType.TypeQuals, this.Env);
                        printer.Name = member.Name;
                        member.QualType.Type.Visit(printer);
                        builder.Append($" {printer.Name};");
                    }
                });

                builder.Append(" }");

                this.Kind = NameKind.FINISHED;

                if (this.Name == "") {
                    this.Name = builder.ToString();
                } else {
                    this.Name = $"{builder.ToString()} {this.Name}";
                }
            }

            /// <summary>
            /// For a function, the qualifiers are not used.
            /// We should directly use the qualifiers of the return type.
            /// </summary>
            public void VisitFunction(FunctionType type) {
                if (this.Kind == NameKind.FINISHED) {
                    throw new InvalidOperationException("invalid status");
                }

                // operator precedence
                if (this.Kind == NameKind.LEFT) {
                    this.Name = $"({this.Name})";
                }

                this.Name += "(";

                if (type.Args.IsEmpty) {
                    if (type.HasVarArgs) {
                        this.Name += "...";
                    } else {
                        this.Name += "void";
                    }

                } else {
                    var argStrings = type.Args.ConvertAll(arg => arg.QualType.ToString());
                    this.Name += String.Join(", ", argStrings);

                    if (type.HasVarArgs) {
                        this.Name += ", ...";
                    }

                }

                this.Name += ")";

                this.Kind = NameKind.RIGHT;

                this.TypeQuals = type.ReturnQualType.TypeQuals;
                type.ReturnQualType.Type.Visit(this);
            }

            public void VisitArray(ArrayType type) {
                if (this.Kind == NameKind.FINISHED) {
                    throw new InvalidOperationException("invalid status");
                }

                if (this.Kind == NameKind.LEFT) {
                    this.Name = $"({this.Name})";
                }

                this.Name = $"{this.Name}[{type.NumElems}]";
                this.Kind = NameKind.RIGHT;

                this.TypeQuals = type.ElemQualType.TypeQuals;
                type.ElemQualType.Type.Visit(this);
            }

            /// <summary>
            /// For an array, the qualifiers are not used.
            /// We should directly use the qualifiers of the element type.
            /// </summary>
            public void VisitIncompleteArray(IncompleteArrayType type) {
                if (this.Kind == NameKind.FINISHED) {
                    throw new InvalidOperationException("invalid status");
                }

                if (this.Kind == NameKind.LEFT) {
                    this.Name = $"({this.Name})";
                }

                this.Name = $"{this.Name}[]";
                this.Kind = NameKind.RIGHT;

                this.TypeQuals = type.ElemQualType.TypeQuals;
                type.ElemQualType.Type.Visit(this);
            }

            public enum NameKind {
                START,
                RIGHT,
                LEFT,
                FINISHED
            }

            public NameKind Kind { get; set; }

            public String Name { get; set; }

            public TypeQuals TypeQuals { get; set; }

            public Env Env { get; }
        }
    }
}
