using System;
using System.Text;

namespace LexicalAnalysis {
    public static class Utils {

        // Quote : Char -> String
        // ======================
        // Wraps a character in quotes for an error message, escaping it if it is not printable.
        //
        public static String Quote(Char ch) => Quote(ch.ToString());

        // Quote : String -> String
        // ========================
        // Wraps a piece of source text in quotes for an error message.
        // Long text is truncated, and unprintable characters are escaped.
        //
        public static String Quote(String text) {
            const Int32 maxLength = 20;
            if (text.Length > maxLength) {
                text = text.Substring(0, maxLength) + "...";
            }
            var builder = new StringBuilder();
            foreach (Char ch in text) {
                builder.Append(Char.IsControl(ch) ? $"\\x{(Int32)ch:x2}" : ch.ToString());
            }
            return $"'{builder}'";
        }

        // IsEscapeChar : Char -> Boolean
        // ==============================
        // 
        public static Boolean IsEscapeChar(Char ch) {
            switch (ch) {
                case 'a':
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                case 'v':
                case '\'':
                case '\"':
                case '\\':
                case '?':
                    return true;
                default:
                    return false;
            }
        }

        // IsHexDigit : Char -> Boolean
        // ============================
        // 
        public static Boolean IsHexDigit(Char ch) {
            return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
        }

        // IsOctDigit : Char -> Boolean
        // ============================
        // 
        public static Boolean IsOctDigit(Char ch) {
            return ch >= '0' && ch <= '7';
        }

        // GetHexDigit : Char -> Int64
        // ===========================
        // 
        public static Int64 GetHexDigit(Char ch) {
            if (ch >= '0' && ch <= '9') {
                return ch - '0';
            }
            if (ch >= 'a' && ch <= 'f') {
                return ch - 'a' + 0xA;
            }
            if (ch >= 'A' && ch <= 'F') {
                return ch - 'A' + 0xA;
            }
            throw new Exception("GetHexDigit: Character is not a hex digit. You should first call IsHexDigit(ch) for a check.");
        }

        // IsSpace : Char -> Boolean
        // =========================
        // 
        public static Boolean IsSpace(Char ch) {
            return (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\f' || ch == '\v');
        }
    }
}