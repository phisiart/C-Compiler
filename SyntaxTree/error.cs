using System;

public class Log {
    public static void SemantError(string msg) {
        Console.Error.WriteLine(msg);
        Environment.Exit(1);
    }
}