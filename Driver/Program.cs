using System;
using System.IO;
using LexicalAnalysis;

namespace Driver {
    class Program {
        private const Int32 ExitSuccess = 0;
        private const Int32 ExitCompilationFailed = 1;
        private const Int32 ExitUsage = 2;

        private const String Usage = "usage: C-Compiler [-o <output.s>] <source.c>";

        static Int32 Main(String[] args) {
            String sourceFile = null;
            String outputFile = null;

            for (Int32 i = 0; i < args.Length; ++i) {
                switch (args[i]) {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return ExitSuccess;
                    case "-o":
                        if (i + 1 == args.Length) {
                            return UsageError("missing file name after '-o'");
                        }
                        outputFile = args[++i];
                        break;
                    default:
                        if (args[i].StartsWith("-") && args[i] != "-") {
                            return UsageError($"unknown option '{args[i]}'");
                        }
                        if (sourceFile != null) {
                            return UsageError("only one source file can be compiled at a time");
                        }
                        sourceFile = args[i];
                        break;
                }
            }

            if (sourceFile == null) {
                return UsageError("no input file");
            }

            String source;
            try {
                source = sourceFile == "-" ? Console.In.ReadToEnd() : File.ReadAllText(sourceFile);
            } catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) {
                Console.Error.WriteLine($"error: cannot read '{sourceFile}': {e.Message}");
                return ExitCompilationFailed;
            }

            String fileName = sourceFile == "-" ? "<stdin>" : sourceFile;
            Compiler compiler;
            try {
                compiler = Compiler.FromSource(source);
            } catch (CompilationError e) {
                Console.Error.WriteLine(e.Format(fileName));
                return ExitCompilationFailed;
            } catch (Exception e) {
                // Semantic analysis and code generation don't track source locations yet.
                Console.Error.WriteLine($"{fileName}: error: {e.Message}");
                return ExitCompilationFailed;
            }

            if (outputFile == null) {
                Console.WriteLine(compiler.Assembly);
            } else {
                File.WriteAllText(outputFile, compiler.Assembly + System.Environment.NewLine);
            }
            return ExitSuccess;
        }

        private static Int32 UsageError(String message) {
            Console.Error.WriteLine($"error: {message}");
            Console.Error.WriteLine(Usage);
            return ExitUsage;
        }
    }
}
