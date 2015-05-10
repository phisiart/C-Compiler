using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Reg {
    EAX,
    ECX,
    EDX,
    EBX,

    EBP,
    ESP,

    AL,
    AX,

	ST0,
}

public enum CGenReturn {
	EAX,
	ST0,
}

public class CGenState {
    private enum Status {
        NONE,
        TEXT,
        DATA,
    }

    public static Dictionary<Reg, String> reg_strs = new Dictionary<Reg, String>() {
        { Reg.EAX, "%eax" },
        { Reg.ECX, "%ecx" },
        { Reg.EDX, "%edx" },
        { Reg.EBX, "%ebx" },
        { Reg.EBP, "%ebp" },
        { Reg.ESP, "%esp" },
		{ Reg.ST0, "%st(0)" },
    };

    public static String StrReg(Reg reg) {
        return reg_strs[reg];
    }

    public CGenState() {
        os = new System.IO.StringWriter();
		rodata = new System.IO.StringWriter();
		rodata.WriteLine("    section .rodata");

		rodata_idx = 0;
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

	/// <summary>
	/// FLDS: load float to FPU stack.
	/// </summary>
	public void FLDS(String addr) {
		os.WriteLine("    flds " + addr);
	}

	public void FLDS(Int32 imm, Reg from) {
		FLDS(imm.ToString() + "(" + StrReg(from));
	}

	/// <summary>
	/// FLDL: load double to FPU stack.
	/// </summary>
	/// <param name="addr">Address.</param>
	public void FLDL(String addr) {
		os.WriteLine("    fldl " + addr);
	}

	public void FLDL(Int32 imm, Reg from) {
		FLDL(imm.ToString() + "(" + StrReg(from));
	}

    /// <summary>
    /// FSTS: store float from FPU stack.
    /// </summary>
    /// <param name="addr"></param>
    public void FSTS(String addr) {
        os.WriteLine("    fsts " + addr);
    }

    public void FSTS(Int32 imm, Reg to) {
        FSTS(imm.ToString() + "(" + StrReg(to));
    }

    /// <summary>
    /// FSTL: store double from FPU stack.
    /// </summary>
    /// <param name="addr"></param>
    public void FSTL(String addr) {
        os.WriteLine("    fstl " + addr);
    }

    public void FSTL(Int32 imm, Reg to) {
        FSTL(imm.ToString() + "(" + StrReg(to));
    }

    // PUSHL
    // =====
    public void PUSHL(String reg) {
        os.WriteLine("    pushl " + reg);
    }

    public void PUSHL(Reg reg) {
        PUSHL(StrReg(reg));
    }

    public void PUSHL(Int32 imm) {
        PUSHL("$" + imm.ToString());
    }

    //public void PUSHL(Int32 offset, Reg reg) {
    //    PUSHL(offset.ToString() + "(" + StrReg(reg) + ")");
    //}

    // POPL
    // ====
    public void POPL(String addr) {
        os.WriteLine("    popl " + addr);
    }

    public void POPL(Reg reg) {
        POPL(StrReg(reg));
    }

    /// <summary>
    /// MOVL: move a 4-byte long
    /// </summary>
    public void MOVL(String from, String to) {
        os.WriteLine("    movl " + from + ", " + to);
    }

	public void MOVL(String from, Reg to) {
        MOVL(from, StrReg(to));
	}

    public void MOVL(Int32 imm, String to) {
        MOVL("$" + imm.ToString(), to);
    }

    public void MOVL(Int32 imm, Reg to) {
        MOVL("$" + imm.ToString(), StrReg(to));
    }

    public void MOVL(Reg from, Reg to) {
        MOVL(StrReg(from), StrReg(to));
    }

    public void MOVL(Reg from, Int32 offset, Reg to) {
        MOVL(StrReg(from), offset.ToString() + "(" + StrReg(to) + ")");
    }

	public void MOVL(Int32 offset, Reg from, Reg to) {
		MOVL(offset.ToString() + "(" + StrReg(from) + ")", StrReg(to));
	}

    /// <summary>
    /// MOVZBL: move a byte and zero-extend to a 4-byte long
    /// </summary>
	public void MOVZBL(String from, String to) {
		os.WriteLine("    movzbl " + from + ", " + to);
	}

    public void MOVZBL(String from, Reg to) {
        MOVZBL(from, StrReg(to));
    }

    public void MOVZBL(Int32 offset, Reg from, Reg to) {
        MOVZBL(offset.ToString() + StrReg(from), StrReg(to));
    }

    /// <summary>
    /// MOVSBL: move a byte and sign-extend to a 4-byte long
    /// </summary>
	public void MOVSBL(String from, String to) {
		os.WriteLine("    movsbl " + from + ", " + to);
	}

    public void MOVSBL(String from, Reg to) {
        MOVSBL(from, StrReg(to));
    }

    public void MOVSBL(Int32 offset, Reg from, Reg to) {
        MOVSBL(offset.ToString() + StrReg(from), StrReg(to));
    }

    /// <summary>
    /// MOVB: move a byte
    /// </summary>
	public void MOVB(String from, String to) {
		os.WriteLine("    movb " + from + ", " + to);
	}

    public void MOVB(Reg from, Int32 imm, Reg to) {
        MOVB(StrReg(from), imm.ToString() + "(" + StrReg(to) + ")");
    }

    /// <summary>
    /// MOVW: move a 2-byte word
    /// </summary>
    public void MOVW(String from, String to) {
        os.WriteLine("    movw " + from + ", " + to);
    }

    public void MOVW(Reg from, Int32 imm, Reg to) {
        MOVW(StrReg(from), imm.ToString() + "(" + StrReg(to) + ")");
    }

    /// <summary>
    /// MOVZWL: move a 2-byte word and zero-extend to a 4-byte long
    /// </summary>
	public void MOVZWL(String from, String to) {
		os.WriteLine("    movzwl " + from + ", " + to);
	}

    public void MOVZWL(String from, Reg to) {
        MOVZWL(from, StrReg(to));
    }
    
    public void MOVZWL(Int32 offset, Reg from, Reg to) {
        MOVZWL(offset.ToString() + StrReg(from), StrReg(to));
    }

    /// <summary>
    /// MOVSWL: move a 2-byte word and sign-extend to a 4-byte long
    /// </summary>
	public void MOVSWL(String from, String to) {
		os.WriteLine("    movswl " + from + ", " + to);
	}

    public void MOVSWL(String from, Reg to) {
        MOVSWL(from, StrReg(to));
    }

	public void MOVSWL(Int32 offset, Reg from, Reg to) {
		MOVSWL(offset.ToString() + StrReg(from), StrReg(to));
	}

    // LEA
    // ===
    // 
    public void LEA(String addr) {
        os.WriteLine("    lea " + addr);
    }

    // CALL
    // ====
    // 
    public void CALL(String addr) {
        os.WriteLine("    call " + addr);
    }

    // CGenExpandStack
    // ===============
    // 
    public void CGenExpandStack(Int32 size, String comment = "") {
        if (size > stack_size) {
            SUBL(size - stack_size, StrReg(Reg.ESP), comment);
            stack_size = size;
        }
    }

    public void LEAVE() {
        //os.WriteLine("    leave # pop frame, restore %ebp");
        os.WriteLine("    leave");
    }

    public void RET() {
        //os.WriteLine("    ret # pop old %eip, jump");
        os.WriteLine("    ret");
    }

    public void NEWLINE() {
        os.WriteLine();
    }

    public void COMMENT(String comment) {
        os.WriteLine("    # " + comment);
    }

    /// <summary>
    /// SUBL: subtract long
    /// </summary>
    public void SUBL(String er, String ee, String comment = "") {
        os.Write("    subl " + er + ", " + ee);
        if (comment == "") {
            os.WriteLine();
        } else {
            os.WriteLine(" # " + comment);
        }
    }

    public void SUBL(Int32 er, String ee, String comment = "") {
        SUBL(er.ToString(), ee, comment);
    }

    public void SUBL(Int32 er, Reg ee, String comment = "") {
        SUBL(er.ToString(), StrReg(ee), comment);
    }

    public void SUBL(Reg er, Reg ee, String comment = "") {
        SUBL(StrReg(er), StrReg(ee), comment);
    }

    public override String ToString() {
		return os.ToString() + rodata.ToString();
    }

    /// <summary>
    /// ANDL er, ee
    /// ee = er & ee
    /// </summary>
    public void ANDL(String er, String ee) {
        os.WriteLine("    andl " + er + ", " + ee);
    }

    public void ANDL(Reg er, Reg ee) {
        ANDL(StrReg(er), StrReg(ee));
    }
    
    /// <summary>
    /// ORL er, ee
    ///     ee = ee | er
    /// </summary>
	public void ORL(String er, String ee, String comment = "") {
		os.Write("    orl " + er + ", " + ee);
		if (comment == "") {
			os.WriteLine();
		} else {
			os.WriteLine(" # " + comment);
		}
	}

	public void ORL(Reg er, Reg ee, String comment = "") {
		ORL(StrReg(er), StrReg(ee), comment);
	}

	public String CGenLongConst(Int32 val) {
		String name = ".LC" + rodata_idx.ToString();
		rodata.WriteLine("    .align 4");
		rodata.WriteLine(name + ":");
		rodata.WriteLine("    .long " + val.ToString());
		rodata_idx++;
		return name;
	}

	public String CGenLongLongConst(Int32 lo, Int32 hi) {
		String name = ".LC" + rodata_idx.ToString();
		rodata.WriteLine("    .align 8");
		rodata.WriteLine(name + ":");
		rodata.WriteLine("    .long " + lo.ToString());
		rodata.WriteLine("    .long " + hi.ToString());
		rodata_idx++;
		return name;
	}

	public String CGenString(String str) {
		String name = ".LC" + rodata_idx.ToString();
		rodata.WriteLine(name + ":");
		rodata.WriteLine("    .string \"" + str + "\"");
		rodata_idx++;
		return name;
	}

    private System.IO.StringWriter os;
	private System.IO.StringWriter rodata;
	private Int32 rodata_idx;

    private Status status;
    private Int32 stack_size;
    
}
