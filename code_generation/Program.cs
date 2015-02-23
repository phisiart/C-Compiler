using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Reg {
    public static readonly String EAX = "%eax";
    public static readonly String ECX = "%ecx";
    public static readonly String EDX = "%edx";
    public static readonly String EBX = "%ebx";

    public static readonly String EBP = "%ebp";
    public static readonly String ESP = "%esp";
}

public class CGenState {
    private enum Status {
        NONE,
        TEXT,
        DATA,
    }
    
    public CGenState() {
        os = new System.IO.StringWriter();
        status = Status.NONE;
    }

    public void TEXT() {
        if (status != Status.TEXT) {
            os.WriteLine("    .text");
            status = Status.TEXT;
        }
    }

    public void GLOBL(String name) {
        os.WriteLine("    .globl " + name);
    }

    public void CGenFuncName(String name) {
        os.WriteLine(name + ":");
        stack_size = 0;
    }

    public void PUSHL(String reg) {
        os.WriteLine("    push " + reg);
    }

    public void MOVL(String from, String to) {
        os.WriteLine("    movl " + from + ", " + to);
    }

    public void CGenExpandStack(int size, String comment = "") {
        if (size > stack_size) {
            SUBL(size - stack_size, Reg.ESP, comment);
            stack_size = size;
        }
    }

    public void LEAVE() {
        os.WriteLine("    leave # pop the whole frame and restore %ebp");
    }

    public void RET() {
        os.WriteLine("    ret # pop the old %eip and return");
    }

    public void NEWLINE() {
        os.WriteLine();
    }

    public void COMMENT(String comment) {
        os.WriteLine("    # " + comment);
    }

    public void SUBL(int suber, String subee, String comment = "") {
        os.Write("    subl $" + suber + ", " + subee);
        if (comment == "") {
            os.WriteLine();
        } else {
            os.WriteLine(" # " + comment);
        }
    }

    public override String ToString() {
        return os.ToString();
    }


    private System.IO.StringWriter os;
    private Status status;
    private int stack_size;
    
}

namespace code_generation {
    class Program {
        static void Main(string[] args) {

        }
    }
}
