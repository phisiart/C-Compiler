using System;

public partial class Utils {
    public static Int64 RoundUp(Int64 value, Int64 alignment) {
        return (value + alignment - 1) & ~(alignment - 1);
    }
}
