using System;

public class Log {
    public static void SemantError(String msg) {
        Console.Error.WriteLine(msg);
        Environment.Exit(1);
    }
}