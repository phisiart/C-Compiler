using System;

static class Utils {

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