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
    EDI,
    ESI,

    AL,
    AX,
    BL,
    BX,
    CL,

    ST0,

    STACK,
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
        [Reg.EAX] = "%eax",
        [Reg.ECX] = "%ecx",
        [Reg.EDX] = "%edx",
        [Reg.EBX] = "%ebx",
        [Reg.EBP] = "%ebp",
        [Reg.ESP] = "%esp",
        [Reg.EDI] = "%edi",
        [Reg.ESI] = "%esi",
        [Reg.AL] = "%al",
        [Reg.AX] = "%ax",
        [Reg.BL] = "%bl",
        [Reg.BX] = "%bx",
        [Reg.CL] = "%cl",
        [Reg.ST0] = "%st(0)",
    };

    public static String StrReg(Reg reg) {
        return reg_strs[reg];
    }

    public CGenState() {
        os = new System.IO.StringWriter();
        rodata = new System.IO.StringWriter();
        rodata.WriteLine("    section .rodata");

        rodata_idx = 0;
        label_idx = 2;
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
    /// FCHS: %st(0) = -%st(0)
    /// </summary>
    public void FCHS() => os.WriteLine("    fchs");

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
    /// FLD1: push 1.0 to FPU stack.
    /// </summary>
    public void FLD1() => os.WriteLine("    fld1");

    /// <summary>
    /// FLD0: push 0.0 to FPU stack.
    /// </summary>
    public void FLDZ() => os.WriteLine("    fldz");

    /// <summary>
    /// FSTS: store float from FPU stack.
    /// </summary>
    /// <param name="addr"></param>
    public void FSTS(String addr) {
        os.WriteLine("    fsts " + addr);
    }

    public void FSTS(Int32 imm, Reg to) {
        FSTS($"{imm}({StrReg(to)})");
    }

    /// <summary>
    /// FSTPS: pop float from FPU stack, and store to {addr}.
    /// </summary>
    public void FSTPS(String addr) {
        os.WriteLine($"    fstps {addr}");
    }

    public void FSTPS(Int32 imm, Reg to) {
        FSTPS($"{imm}({StrReg(to)})");
    }

    /// <summary>
    /// FSTL: store double from FPU stack.
    /// </summary>
    public void FSTL(String addr) {
        os.WriteLine("    fstl " + addr);
    }

    public void FSTL(Int32 imm, Reg to) {
        FSTL(imm.ToString() + "(" + StrReg(to));
    }

    /// <summary>
    /// FSTPL: pop from FPU and store *double*.
    /// </summary>
    public void FSTPL(String addr) {
        os.WriteLine($"    fstpl {addr}");
    }

    public void FSTPL(Int32 imm, Reg to) => FSTPL($"{imm}({StrReg(to)})");

    /// <summary>
    /// FSTP: copy %st(0) to dst, then pop %st(0).
    /// </summary>
    public void FSTP(String dst) {
        os.WriteLine($"    fstp {dst}");
    }

    public void FSTP(Reg dst) => FSTP(StrReg(dst));

    /// <summary>
    /// FADD: calculate %st(op1) + %st(op2) and rewrite %st(op2).
    /// </summary>
    public void FADD(Int32 op1, Int32 op2) =>
        os.WriteLine($"    fadd %st({op1}), %st({op2})");

    /// <summary>
    /// FADDP: pop operands from %st(0) and %st(1),
    ///        push addition result back to %st(0).
    /// </summary>
    public void FADDP() => os.WriteLine("    faddp");

    /// <summary>
    /// FADD: calculate %st(op1) + %st(op2) and rewrite %st(op2).
    /// </summary>
    public void FSUB(Int32 op1, Int32 op2) =>
        os.WriteLine($"    fsub %st({op1}), %st({op2})");

    /// <summary>
    /// FSUBP: pop operands from %st(0) and %st(1),
    ///        push %st(0) / %st(1) back to %st(0).
    /// </summary>
    public void FSUBP() => os.WriteLine("    fsubp");

    /// <summary>
    /// FMULP: pop operands from %st(0) and %st(1), push multiplication result back to %st(0).
    /// </summary>
    public void FMULP() {
        os.WriteLine("    fmulp");
    }

    /// <summary>
    /// FDIVP: pop operands from %st(0) and %st(1), push %st(0) / %st(1) back to %st(0).
    /// </summary>
    public void FDIVP() {
        os.WriteLine("    fdivp");
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

    public void MOVL(Reg from, Int32 offset, Reg to) => MOVL(StrReg(from), $"{offset}({StrReg(to)})");

    public void MOVL(Int32 offset, Reg from, Reg to) => MOVL($"{offset}({StrReg(to)})", StrReg(to));

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

    public void MOVZBL(Reg from, Reg to) => MOVZBL(StrReg(from), StrReg(to));

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
        MOVSBL($"{offset}({StrReg(from)})", StrReg(to));
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

    public void MOVB(Reg from, Reg to) => MOVB(StrReg(from), StrReg(to));

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
    public void LEA(String addr, String dst) {
        os.WriteLine("    lea " + addr);
    }

    public void LEA(String addr, Reg dst) => LEA(addr, StrReg(dst));

    public void LEA(Int32 offset, Reg from, Reg to) => LEA($"{offset}({from.ToString()})", StrReg(to));

    // CALL
    // ====
    // 
    public void CALL(String addr) {
        os.WriteLine("    call " + addr);
    }

    // CGenExpandStack
    // ===============
    // 
    public void CGenExpandStackTo(Int32 size, String comment = "") {
        if (size > stack_size) {
            SUBL(size - stack_size, StrReg(Reg.ESP), comment);
            stack_size = size;
        }
    }

    public void CGenExpandStackBy(Int32 nbytes) {
        stack_size += nbytes;
        SUBL(nbytes, Reg.ESP);
    }

    public void CGenExpandStackBy4Bytes(String comment = "") {
        stack_size += 4;
        SUBL(4, Reg.ESP);
    }

    public void CGenExpandStackBy8Bytes(String comment = "") {
        stack_size += 8;
        SUBL(8, Reg.ESP);
    }

    public void CGenShrinkStackBy4Bytes(String comment = "") {
        stack_size -= 4;
        ADDL(4, Reg.ESP);
    }

    public void CGenShrinkStackBy8Bytes(String comment = "") {
        stack_size -= 8;
        ADDL(8, Reg.ESP);
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
    /// NEG addr: addr = -addr
    /// </summary>
    public void NEG(String addr) => os.WriteLine($"    neg {addr}");

    public void NEG(Reg dst) => NEG(StrReg(dst));

    /// <summary>
    /// NOT: bitwise not
    /// </summary>
    public void NOT(String addr) => os.WriteLine($"    not {addr}");

    public void NOT(Reg dst) => NOT(StrReg(dst));

    /// <summary>
    /// ADDL: add long
    /// </summary>
    public void ADDL(String er, String ee, String comment = "") {
        os.Write($"    addl {er}, {ee}");
        if (comment == "") {
            os.WriteLine();
        } else {
            os.WriteLine($" # {comment}");
        }
    }

    public void ADDL(Int32 er, Reg ee, String comment = "") {
        ADDL($"${er}", StrReg(ee), comment);
    }

    public void ADDL(Reg er, Reg ee, String comment = "")
        => ADDL(StrReg(er), StrReg(ee), comment);

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

    public void ANDB(String er, String ee) =>
        os.WriteLine($"    andb {er}, {ee}");

    public void ANDB(Int32 er, Reg ee) => ANDB($"${er}", StrReg(ee));

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

    /// <summary>
    /// SALL er, ee
    /// ee = ee << er
    /// Note that there is only one kind of lshift.
    /// </summary>
    public void SALL(String er, String ee) {
        os.WriteLine("    sall " + er + ", " + ee);
    }

    public void SALL(Reg er, Reg ee) {
        SALL(StrReg(er), StrReg(ee));
    }

    /// <summary>
    /// SARL er, ee (arithmetic shift)
    /// ee = ee >> er (append sign bit)
    /// </summary>
    public void SARL(String er, String ee) {
        os.WriteLine($"    sarl {er}, {ee}");
    }

    public void SARL(Reg er, Reg ee) => SARL(StrReg(er), StrReg(ee));

    /// <summary>
    /// SHRL er, ee (logical shift)
    /// ee = ee >> er (append 0)
    /// </summary>
    public void SHRL(String er, String ee) {
        os.WriteLine($"    shrl {er}, {ee}");
    }

    public void SHRL(Reg er, Reg ee) => SHRL(StrReg(er), StrReg(ee));

    public void SHRL(Int32 er, Reg ee) => SHRL($"${er}", StrReg(ee));

    /// <summary>
    /// XORL er, ee
    /// ee = ee ^ er
    /// </summary>
    public void XORL(String er, String ee) {
        os.WriteLine("    xorl " + er + ", " + ee);
    }

    public void XORL(Reg er, Reg ee) {
        XORL(StrReg(er), StrReg(ee));
    }

    /// <summary>
    /// IMUL: signed multiplication. %edx:%eax = %eax * {addr}.
    /// </summary>
    public void IMUL(String addr) {
        os.WriteLine($"    imul {addr}");
    }

    public void IMUL(Reg er) {
        IMUL(StrReg(er));
    }

    /// <summary>
    /// MUL: unsigned multiplication. %edx:%eax = %eax * {addr}.
    /// </summary>
    public void MUL(String addr) {
        os.WriteLine($"    mul {addr}");
    }

    public void MUL(Reg er) {
        MUL(StrReg(er));
    }

    /// <summary>
    /// CLTD: used before division. clear %edx.
    /// </summary>
    public void CLTD() => os.WriteLine("    cltd");

    /// <summary>
    /// IDIVL: signed division. %eax = %edx:%eax / {addr}.
    /// </summary>
    public void IDIVL(String addr) {
        os.WriteLine($"    idivl {addr}");
    }

    public void IDIVL(Reg er) => IDIVL(StrReg(er));

    /// <summary>
    /// IDIVL: unsigned division. %eax = %edx:%eax / {addr}.
    /// </summary>
    public void DIVL(String addr) {
        os.WriteLine($"    divl {addr}");
    }

    public void DIVL(Reg er) => DIVL(StrReg(er));

    /// <summary>
    /// CMPL: compare based on subtraction.
    /// Note that the order is reversed, i.e. ee comp er.
    /// </summary>
    public void CMPL(String er, String ee) {
        os.WriteLine($"    cmpl {er}, {ee}");
    }

    public void CMPL(Reg er, Reg ee) => CMPL(StrReg(er), StrReg(ee));

    /// <summary>
    /// TESTL: used like testl %eax, %eax: compare %eax with zero.
    /// </summary>
    public void TESTL(String er, String ee) {
        os.WriteLine($"    testl {er}, {ee}");
    }

    public void TESTL(Reg er, Reg ee) => TESTL(StrReg(er), StrReg(ee));

    /// <summary>
    /// SETE: set if equal to.
    /// </summary>
    public void SETE(String dst) {
        os.WriteLine($"    sete {dst}");
    }

    public void SETE(Reg dst) => SETE(StrReg(dst));

    /// <summary>
    /// SETNE: set if not equal to.
    /// </summary>
    public void SETNE(String dst) => os.WriteLine($"    setne {dst}");
    public void SETNE(Reg dst) => SETNE(StrReg(dst));

    /// <summary>
    /// SETG: set if greater than (signed).
    /// </summary>
    public void SETG(String dst) {
        os.WriteLine($"    setg {dst}");
    }

    public void SETG(Reg dst) => SETG(StrReg(dst));

    /// <summary>
    /// SETGE: set if greater or equal to (signed).
    /// </summary>
    public void SETGE(String dst) {
        os.WriteLine($"    setge {dst}");
    }

    public void SETGE(Reg dst) => SETGE(StrReg(dst));

    /// <summary>
    /// SETL: set if less than (signed).
    /// </summary>
    public void SETL(String dst) {
        os.WriteLine($"    setl {dst}");
    }

    public void SETL(Reg dst) => SETL(StrReg(dst));

    /// <summary>
    /// SETLE: set if less than or equal to (signed).
    /// </summary>
    public void SETLE(String dst) {
        os.WriteLine($"    setle {dst}");
    }

    public void SETLE(Reg dst) => SETLE(StrReg(dst));

    /// <summary>
    /// SETB: set if below (unsigned).
    /// </summary>
    public void SETB(String dst) {
        os.WriteLine($"    setb {dst}");
    }

    public void SETB(Reg dst) => SETB(StrReg(dst));

    /// <summary>
    /// SETNB: set if not below (unsigned).
    /// </summary>
    public void SETNB(String dst) {
        os.WriteLine($"    setnb {dst}");
    }

    public void SETNB(Reg dst) => SETNB(StrReg(dst));

    /// <summary>
    /// SETA: set if above (unsigned).
    /// </summary>
    public void SETA(String dst) {
        os.WriteLine($"    seta {dst}");
    }

    public void SETA(Reg dst) => SETA(StrReg(dst));

    /// <summary>
    /// SETNA: set if not above (unsigned).
    /// </summary>
    public void SETNA(String dst) {
        os.WriteLine($"    setna {dst}");
    }

    public void SETNA(Reg dst) => SETNA(StrReg(dst));

    /// <summary>
    /// FUCOMIP: unordered comparison: %st(0) vs %st(1).
    /// </summary>
    public void FUCOMIP() => os.WriteLine("    fucomip %st(1), %st");

    public void JMP(Int32 label) => os.WriteLine($"    jmp .L{label}");

    public void JZ(Int32 label) => os.WriteLine($"    jz .L{label}");

    public void JNZ(Int32 label) => os.WriteLine($"    jz .L{label}");

    public void CLD() => os.WriteLine("    cld");

    /// <summary>
    /// Fast Memory Copy using assembly.
    /// Make sure that
    /// 1) %esi = source address
    /// 2) %edi = destination address
    /// 3) %ecx = number of bytes
    /// </summary>
    public void MemCpy() {
        MOVB(Reg.CL, Reg.AL);
        SHRL(2, Reg.ECX);
        CLD();
        os.WriteLine("    rep movsl");
        MOVB(Reg.AL, Reg.CL);
        ANDB(3, Reg.CL);
        os.WriteLine("    rep movsb");
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
        rodata.WriteLine("    .String \"" + str + "\"");
        rodata_idx++;
        return name;
    }

    public void CGenLabel(String label) => os.WriteLine($"{label}:");

    public void CGenLabel(Int32 label) => CGenLabel($".L{label}");

    private System.IO.StringWriter os;
    private System.IO.StringWriter rodata;
    private Int32 rodata_idx;
    public Int32 label_idx;

    private Status status;
    private Int32 stack_size;

}
