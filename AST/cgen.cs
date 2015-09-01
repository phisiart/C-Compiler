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

    public static String RegToString(Reg reg) => reg_strs[reg];

    public CGenState() {
        os = new System.IO.StringWriter();
        rodata = new System.IO.StringWriter();
        rodata.WriteLine("    .section .rodata");

        rodata_idx = 0;
        label_idx = 2;
        status = Status.NONE;
        label_packs = new Stack<LabelPack>();
        return_label = -1;
    }

    public void TEXT() {
        if (status != Status.TEXT) {
            os.WriteLine("    .text");
            status = Status.TEXT;
        }
    }

    public void DATA() {
        if (status != Status.DATA) {
            os.WriteLine("    .data");
            status = Status.DATA;
        }
    }

    public void GLOBL(String name) => os.WriteLine($"    .globl {name}");

    public void LOCAL(String name) => os.WriteLine($"    .local {name}");

    public void ALIGN(Int32 align) => os.WriteLine($"    .align {align}");

    public void COMM(String name, Int32 size, Int32 align) => os.WriteLine($"    .comm {name},{size},{align}");

    public void BYTE(Int32 value) => os.WriteLine($"    .byte {value}");

    public void ZERO(Int32 size) => os.WriteLine($"    .zero {size}");

    public void VALUE(Int32 value) => os.WriteLine($"    .value {value}");

    public void LONG(Int32 value) => os.WriteLine($"    .long {value}");


    public void CGenFuncStart(String name) {
        os.WriteLine(name + ":");
        PUSHL(Reg.EBP);
        MOVL(Reg.ESP, Reg.EBP);
        stack_size = 0;
    }

    /// <summary>
    /// FCHS: %st(0) = -%st(0)
    /// </summary>
    public void FCHS() => os.WriteLine("    fchs");

    /// <summary>
    /// FLDS: load float to FPU stack.
    /// </summary>
    public void FLDS(String src) => os.WriteLine($"    flds {src}");

    public void FLDS(Int32 imm, Reg src) => FLDS($"{imm}({RegToString(src)})");

    /// <summary>
    /// FLDL: load double to FPU stack.
    /// </summary>
    /// <param name="addr">Address.</param>
    public void FLDL(String addr) => os.WriteLine($"    fldl {addr}");

    public void FLDL(Int32 imm, Reg from) => FLDL($"{imm}({RegToString(from)})");

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
    public void FSTS(String addr) => os.WriteLine($"    fsts {addr}");

    public void FSTS(Int32 imm, Reg to) => FSTS($"{imm}({RegToString(to)})");

    /// <summary>
    /// FSTPS: pop float from FPU stack, and store to {addr}.
    /// </summary>
    public void FSTPS(String addr) => os.WriteLine($"    fstps {addr}");

    public void FSTPS(Int32 imm, Reg to) => FSTPS($"{imm}({RegToString(to)})");

    /// <summary>
    /// FSTL: store double from FPU stack.
    /// </summary>
    public void FSTL(String addr) => os.WriteLine($"    fstl {addr}");

    public void FSTL(Int32 imm, Reg to) => FSTL($"{imm}({RegToString(to)})");

    /// <summary>
    /// FSTPL: pop from FPU and store *double*.
    /// </summary>
    public void FSTPL(String addr) => os.WriteLine($"    fstpl {addr}");

    public void FSTPL(Int32 imm, Reg to) => FSTPL($"{imm}({RegToString(to)})");

    /// <summary>
    /// FSTP: copy %st(0) to dst, then pop %st(0).
    /// </summary>
    public void FSTP(String dst) => os.WriteLine($"    fstp {dst}");

    public void FSTP(Reg dst) => FSTP(RegToString(dst));

    /// <summary>
    /// FADD: calculate %st(op1) + %st(op2) and rewrite %st(op2).
    /// </summary>
    public void FADD(Int32 op1, Int32 op2) => os.WriteLine($"    fadd %st({op1}), %st({op2})");

    /// <summary>
    /// FADDP: pop operands from %st(0) and %st(1),
    ///        push addition result back to %st(0).
    /// </summary>
    public void FADDP() => os.WriteLine("    faddp");

    /// <summary>
    /// FADD: calculate %st(op1) + %st(op2) and rewrite %st(op2).
    /// </summary>
    public void FSUB(Int32 op1, Int32 op2) => os.WriteLine($"    fsub %st({op1}), %st({op2})");

    /// <summary>
    /// FSUBP: pop operands from %st(0) and %st(1),
    ///        push %st(0) / %st(1) back to %st(0).
    /// </summary>
    public void FSUBP() => os.WriteLine("    fsubp");

    /// <summary>
    /// FMULP: pop operands from %st(0) and %st(1), push multiplication result back to %st(0).
    /// </summary>
    public void FMULP() => os.WriteLine("    fmulp");

    /// <summary>
    /// FDIVP: pop operands from %st(0) and %st(1), push %st(0) / %st(1) back to %st(0).
    /// </summary>
    public void FDIVP() => os.WriteLine("    fdivp");

    /// <summary>
    /// PUSHL: push long into stack.
    /// </summary>
    /// <remarks>
    /// PUSHL changes the size of the stack, which should be tracked carefully.
    /// So, PUSHL is set private. Consider using <see cref="CGenPushLong"/>
    /// </remarks>
    private void PUSHL(String src) => os.WriteLine($"    pushl {src}");

    private void PUSHL(Reg src) => PUSHL(RegToString(src));

    private void PUSHL(Int32 imm) => PUSHL($"${imm}");

    /// <summary>
    /// POPL: pop long from stack.
    /// </summary>
    /// <remarks>
    /// POPL changes the size of the stack, which should be tracked carefully.
    /// So, POPL is set private. Consider using <see cref="CGenPopLong"/>
    /// </remarks>
    private void POPL(String dst) => os.WriteLine($"    popl {dst}");

    private void POPL(Reg dst) => POPL(RegToString(dst));

    /// <summary>
    /// MOVL: move a 4-byte long
    /// </summary>
    public void MOVL(String src, String dst) => os.WriteLine($"    movl {src}, {dst}");

    public void MOVL(String src, Reg dst) => MOVL(src, RegToString(dst));

    public void MOVL(Int32 imm, String dst) => MOVL($"${imm}", dst);

    public void MOVL(Int32 imm, Reg dst) => MOVL($"${imm}", RegToString(dst));

    public void MOVL(Reg src, Reg dst) => MOVL(RegToString(src), RegToString(dst));

    public void MOVL(Reg src, Int32 offset, Reg dst) => MOVL(RegToString(src), $"{offset}({RegToString(dst)})");

    public void MOVL(Int32 offset, Reg src, Reg dst) => MOVL($"{offset}({RegToString(src)})", RegToString(dst));

    /// <summary>
    /// MOVZBL: move a byte and zero-extend to a 4-byte long
    /// </summary>
	public void MOVZBL(String src, String dst) => os.WriteLine($"    movzbl {src}, {dst}");

    public void MOVZBL(String src, Reg dst) => MOVZBL(src, RegToString(dst));

    public void MOVZBL(Int32 offset, Reg src, Reg dst) => MOVZBL($"{offset}({RegToString(src)})", RegToString(dst));

    public void MOVZBL(Reg src, Reg dst) => MOVZBL(RegToString(src), RegToString(dst));

    /// <summary>
    /// MOVSBL: move a byte and sign-extend to a 4-byte long
    /// </summary>
	public void MOVSBL(String src, String dst) => os.WriteLine($"    movsbl {src}, {dst}");

    public void MOVSBL(String src, Reg dst) => MOVSBL(src, RegToString(dst));

    public void MOVSBL(Int32 offset, Reg src, Reg dst) => MOVSBL($"{offset}({RegToString(src)})", RegToString(dst));

    public void MOVSBL(Reg src, Reg dst) => MOVSBL(RegToString(src), RegToString(dst));

    /// <summary>
    /// MOVB: move a byte
    /// </summary>
	public void MOVB(String src, String dst) => os.WriteLine($"    movb {src}, {dst}");

    public void MOVB(Reg from, Int32 imm, Reg to) {
        MOVB(RegToString(from), imm.ToString() + "(" + RegToString(to) + ")");
    }

    public void MOVB(Reg from, Reg to) => MOVB(RegToString(from), RegToString(to));

    /// <summary>
    /// MOVW: move a 2-byte word
    /// </summary>
    public void MOVW(String from, String to) {
        os.WriteLine("    movw " + from + ", " + to);
    }

    public void MOVW(Reg from, Int32 imm, Reg to) {
        MOVW(RegToString(from), imm.ToString() + "(" + RegToString(to) + ")");
    }

    /// <summary>
    /// MOVZWL: move a 2-byte word and zero-extend to a 4-byte long
    /// </summary>
	public void MOVZWL(String from, String to) {
        os.WriteLine("    movzwl " + from + ", " + to);
    }

    public void MOVZWL(String from, Reg to) {
        MOVZWL(from, RegToString(to));
    }

    public void MOVZWL(Int32 offset, Reg from, Reg to) {
        MOVZWL(offset.ToString() + RegToString(from), RegToString(to));
    }

    public void MOVZWL(Reg src, Reg dst) => MOVZWL(RegToString(src), RegToString(dst));

    /// <summary>
    /// MOVSWL: move a 2-byte word and sign-extend to a 4-byte long
    /// </summary>
	public void MOVSWL(String from, String to) {
        os.WriteLine("    movswl " + from + ", " + to);
    }

    public void MOVSWL(String from, Reg to) {
        MOVSWL(from, RegToString(to));
    }

    public void MOVSWL(Int32 offset, Reg from, Reg to) {
        MOVSWL(offset.ToString() + RegToString(from), RegToString(to));
    }

    public void MOVSWL(Reg src, Reg dst) => MOVSWL(RegToString(src), RegToString(dst));

    // LEA
    // ===
    // 
    public void LEA(String addr, String dst) => os.WriteLine($"    lea {addr}, {dst}");

    public void LEA(String addr, Reg dst) => LEA(addr, RegToString(dst));

    public void LEA(Int32 offset, Reg src, Reg dst) => LEA($"{offset}({RegToString(src)})", RegToString(dst));

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
            SUBL(size - stack_size, RegToString(Reg.ESP), comment);
            stack_size = size;
        }
    }

    public void CGenExpandStackBy(Int32 nbytes) {
        stack_size += nbytes;
        SUBL(nbytes, Reg.ESP);
    }

    public void CGenExpandStackWithAlignment(Int32 nbytes, Int32 align) {
        nbytes = AST.Utils.RoundUp(stack_size + nbytes, align) - stack_size;
        CGenExpandStackBy(nbytes);
    }

    public void CGenForceStackSizeTo(Int32 nbytes) {
        stack_size = nbytes;
        LEA(-nbytes, Reg.EBP, Reg.ESP);
    }

    public void CGenShrinkStackBy(Int32 nbytes) {
        stack_size -= nbytes;
        ADDL(nbytes, Reg.ESP);
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

    public void NEG(Reg dst) => NEG(RegToString(dst));

    /// <summary>
    /// NOT: bitwise not
    /// </summary>
    public void NOT(String addr) => os.WriteLine($"    not {addr}");

    public void NOT(Reg dst) => NOT(RegToString(dst));

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

    public void ADDL(Int32 er, Reg ee, String comment = "") => ADDL($"${er}", RegToString(ee), comment);

    public void ADDL(Reg er, Reg ee, String comment = "") => ADDL(RegToString(er), RegToString(ee), comment);

    /// <summary>
    /// SUBL: subtract long
    /// </summary>
    public void SUBL(String er, String ee, String comment = "") {
        os.Write($"    subl {er}, {ee}");
        if (comment == "") {
            os.WriteLine();
        } else {
            os.WriteLine(" # " + comment);
        }
    }

    private void SUBL(Int32 er, String ee, String comment = "") => SUBL($"${er}", ee, comment);

    public void SUBL(Int32 er, Reg ee, String comment = "") => SUBL($"${er}", RegToString(ee), comment);

    public void SUBL(Reg er, Reg ee, String comment = "") => SUBL(RegToString(er), RegToString(ee), comment);

    public override String ToString() {
        return os.ToString() + rodata.ToString();
    }

    /// <summary>
    /// ANDL er, ee
    /// ee = er & ee
    /// </summary>
    public void ANDL(String er, String ee) => os.WriteLine($"    andl {er}, {ee}");

    public void ANDL(Reg er, Reg ee) => ANDL(RegToString(er), RegToString(ee));

    public void ANDL(Int32 er, Reg ee) => ANDL($"${er}", RegToString(ee));

    public void ANDB(String er, String ee) => os.WriteLine($"    andb {er}, {ee}");

    public void ANDB(Int32 er, Reg ee) => ANDB($"${er}", RegToString(ee));

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
        ORL(RegToString(er), RegToString(ee), comment);
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
        SALL(RegToString(er), RegToString(ee));
    }

    /// <summary>
    /// SARL er, ee (arithmetic shift)
    /// ee = ee >> er (append sign bit)
    /// </summary>
    public void SARL(String er, String ee) {
        os.WriteLine($"    sarl {er}, {ee}");
    }

    public void SARL(Reg er, Reg ee) => SARL(RegToString(er), RegToString(ee));

    /// <summary>
    /// SHRL er, ee (logical shift)
    /// ee = ee >> er (append 0)
    /// </summary>
    public void SHRL(String er, String ee) {
        os.WriteLine($"    shrl {er}, {ee}");
    }

    public void SHRL(Reg er, Reg ee) => SHRL(RegToString(er), RegToString(ee));

    public void SHRL(Int32 er, Reg ee) => SHRL($"${er}", RegToString(ee));

    /// <summary>
    /// XORL er, ee
    /// ee = ee ^ er
    /// </summary>
    public void XORL(String er, String ee) {
        os.WriteLine("    xorl " + er + ", " + ee);
    }

    public void XORL(Reg er, Reg ee) {
        XORL(RegToString(er), RegToString(ee));
    }

    /// <summary>
    /// IMUL: signed multiplication. %edx:%eax = %eax * {addr}.
    /// </summary>
    public void IMUL(String addr) {
        os.WriteLine($"    imul {addr}");
    }

    public void IMUL(Reg er) {
        IMUL(RegToString(er));
    }

    /// <summary>
    /// MUL: unsigned multiplication. %edx:%eax = %eax * {addr}.
    /// </summary>
    public void MUL(String addr) {
        os.WriteLine($"    mul {addr}");
    }

    public void MUL(Reg er) {
        MUL(RegToString(er));
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

    public void IDIVL(Reg er) => IDIVL(RegToString(er));

    /// <summary>
    /// IDIVL: unsigned division. %eax = %edx:%eax / {addr}.
    /// </summary>
    public void DIVL(String addr) {
        os.WriteLine($"    divl {addr}");
    }

    public void DIVL(Reg er) => DIVL(RegToString(er));

    /// <summary>
    /// CMPL: compare based on subtraction.
    /// Note that the order is reversed, i.e. ee comp er.
    /// </summary>
    public void CMPL(String er, String ee) {
        os.WriteLine($"    cmpl {er}, {ee}");
    }

    public void CMPL(Reg er, Reg ee) => CMPL(RegToString(er), RegToString(ee));

    public void CMPL(Int32 imm, Reg ee) => CMPL($"${imm}", RegToString(ee));

    /// <summary>
    /// TESTL: used like testl %eax, %eax: compare %eax with zero.
    /// </summary>
    public void TESTL(String er, String ee) {
        os.WriteLine($"    testl {er}, {ee}");
    }

    public void TESTL(Reg er, Reg ee) => TESTL(RegToString(er), RegToString(ee));

    /// <summary>
    /// SETE: set if equal to.
    /// </summary>
    public void SETE(String dst) {
        os.WriteLine($"    sete {dst}");
    }

    public void SETE(Reg dst) => SETE(RegToString(dst));

    /// <summary>
    /// SETNE: set if not equal to.
    /// </summary>
    public void SETNE(String dst) => os.WriteLine($"    setne {dst}");
    public void SETNE(Reg dst) => SETNE(RegToString(dst));

    /// <summary>
    /// SETG: set if greater than (signed).
    /// </summary>
    public void SETG(String dst) {
        os.WriteLine($"    setg {dst}");
    }

    public void SETG(Reg dst) => SETG(RegToString(dst));

    /// <summary>
    /// SETGE: set if greater or equal to (signed).
    /// </summary>
    public void SETGE(String dst) {
        os.WriteLine($"    setge {dst}");
    }

    public void SETGE(Reg dst) => SETGE(RegToString(dst));

    /// <summary>
    /// SETL: set if less than (signed).
    /// </summary>
    public void SETL(String dst) {
        os.WriteLine($"    setl {dst}");
    }

    public void SETL(Reg dst) => SETL(RegToString(dst));

    /// <summary>
    /// SETLE: set if less than or equal to (signed).
    /// </summary>
    public void SETLE(String dst) {
        os.WriteLine($"    setle {dst}");
    }

    public void SETLE(Reg dst) => SETLE(RegToString(dst));

    /// <summary>
    /// SETB: set if below (unsigned).
    /// </summary>
    public void SETB(String dst) {
        os.WriteLine($"    setb {dst}");
    }

    public void SETB(Reg dst) => SETB(RegToString(dst));

    /// <summary>
    /// SETNB: set if not below (unsigned).
    /// </summary>
    public void SETNB(String dst) {
        os.WriteLine($"    setnb {dst}");
    }

    public void SETNB(Reg dst) => SETNB(RegToString(dst));

    /// <summary>
    /// SETA: set if above (unsigned).
    /// </summary>
    public void SETA(String dst) {
        os.WriteLine($"    seta {dst}");
    }

    public void SETA(Reg dst) => SETA(RegToString(dst));

    /// <summary>
    /// SETNA: set if not above (unsigned).
    /// </summary>
    public void SETNA(String dst) {
        os.WriteLine($"    setna {dst}");
    }

    public void SETNA(Reg dst) => SETNA(RegToString(dst));

    /// <summary>
    /// FUCOMIP: unordered comparison: %st(0) vs %st(1).
    /// </summary>
    public void FUCOMIP() => os.WriteLine("    fucomip %st(1), %st");

    public void JMP(Int32 label) => os.WriteLine($"    jmp .L{label}");

    public void JZ(Int32 label) => os.WriteLine($"    jz .L{label}");

    public void JNZ(Int32 label) => os.WriteLine($"    jz .L{label}");

    public void CLD() => os.WriteLine("    cld");

    public void STD() => os.WriteLine("    std");

    public Int32 CGenPushLong(Reg src) {
        PUSHL(src);
        stack_size += 4;
        return stack_size;
    }

    public Int32 CGenPushLong(Int32 imm) {
        PUSHL(imm);
        stack_size += 4;
        return stack_size;
    }

    public void CGenPopLong(Int32 saved_size, Reg dst) {
        if (stack_size == saved_size) {
            POPL(dst);
            stack_size -= 4;
        } else {
            MOVL(-saved_size, Reg.EBP, dst);
        }
    }

    public Int32 CGenPushFloat() {
        CGenExpandStackBy4Bytes();
        FSTS(0, Reg.ESP);
        return stack_size;
    }

    public Int32 CGenPushFloatP() {
        CGenExpandStackBy4Bytes();
        FSTPS(0, Reg.ESP);
        return stack_size;
    }

    public Int32 CGenPushDouble() {
        CGenExpandStackBy8Bytes();
        FSTL(0, Reg.ESP);
        return stack_size;
    }

    public Int32 CGenPushDoubleP() {
        CGenExpandStackBy8Bytes();
        FSTPL(0, Reg.ESP);
        return stack_size;
    }

    public void CGenPopDouble(Int32 saved_size) {
        FLDL(-saved_size, Reg.EBP);
        if (saved_size == stack_size) {
            CGenShrinkStackBy8Bytes();
        }
    }

    public void CGenPopFloat(Int32 saved_size) {
        FLDL(-saved_size, Reg.EBP);
        if (saved_size == stack_size) {
            CGenShrinkStackBy4Bytes();
        }
    }

    private void FISTL(String dst) => os.WriteLine($"    fistl {dst}");

    private void FISTL(Int32 offset, Reg dst) => FISTL($"{offset}({RegToString(dst)})");

    private void FILDL(String dst) => os.WriteLine($"    fildl {dst}");

    private void FILDL(Int32 offset, Reg dst) => FILDL($"{offset}({RegToString(dst)})");

    public void CGenConvertFloatToLong() {
        CGenExpandStackBy4Bytes();
        FISTL(0, Reg.ESP);
        MOVL(0, Reg.ESP, Reg.EAX);
        CGenShrinkStackBy4Bytes();
    }

    public void CGenConvertLongToFloat() {
        CGenExpandStackBy4Bytes();
        MOVL(Reg.EAX, 0, Reg.ESP);
        FILDL(0, Reg.ESP);
        CGenShrinkStackBy4Bytes();
    }

    /// <summary>
    /// Fast Memory Copy using assembly.
    /// Make sure that
    /// 1) %esi = source address
    /// 2) %edi = destination address
    /// 3) %ecx = number of bytes
    /// </summary>
    public void CGenMemCpy() {
        MOVB(Reg.CL, Reg.AL);
        SHRL(2, Reg.ECX);
        CLD();
        os.WriteLine("    rep movsl");
        MOVB(Reg.AL, Reg.CL);
        ANDB(3, Reg.CL);
        os.WriteLine("    rep movsb");
    }

    /// <summary>
    /// Fast Memory Copy using assembly.
    /// Make sure that
    /// 1) %esi = source address
    /// 2) %edi = destination address
    /// 3) %ecx = number of bytes
    /// </summary>
    public void CGenMemCpyReversed() {
        ADDL(Reg.ECX, Reg.ESI);
        ADDL(Reg.ECX, Reg.EDI);
        MOVL(Reg.ECX, Reg.EAX);

        ANDL(3, Reg.ECX); // now %ecx = 0, 1, 2, or 3
        STD();
        os.WriteLine("    rep movsb");

        MOVL(Reg.EAX, Reg.ECX);
        ANDL(~3, Reg.ECX);
        SHRL(2, Reg.ECX);
        os.WriteLine("    rep movsl");

        CLD();
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

    public Int32 StackSize {
        get {
            return stack_size;
        }
    }

    public Int32 RequestLabel() {
        return label_idx++;
    }


    //private Stack<Int32> _continue_labels;
    //private Stack<Int32> _break_labels;

    private struct LabelPack {
        public LabelPack(Int32 continue_label, Int32 break_label, Int32 default_label, Dictionary<Int32, Int32> value_to_label) {
            this.continue_label = continue_label;
            this.break_label = break_label;
            this.default_label = default_label;
            this.value_to_label = value_to_label;
        }
        public readonly Int32 continue_label;
        public readonly Int32 break_label;
        public readonly Int32 default_label;
        public readonly Dictionary<Int32, Int32> value_to_label;
    }

    private Stack<LabelPack> label_packs;

    public Int32 ContinueLabel =>
        label_packs.First(_ => _.continue_label != -1).continue_label;

    public Int32 BreakLabel =>
        label_packs.First(_ => _.break_label != -1).break_label;

    public Int32 DefaultLabel {
        get {
            Int32 ret = label_packs.First().default_label;
            if (ret == -1) {
                throw new InvalidOperationException("Not in a switch statement.");
            }
            return ret;
        }
    }

    public Int32 CaseLabel(Int32 value) =>
        label_packs.First(_ => _.value_to_label != null).value_to_label[value];
        // label_packs.First().value_to_label[value];

    public void InLoop(Int32 continue_label, Int32 break_label) {
        label_packs.Push(new LabelPack(continue_label, break_label, -1, null));
        //_continue_labels.Push(continue_label);
        //_break_labels.Push(break_label);
    }

    public void InSwitch(Int32 break_label, Int32 default_label, Dictionary<Int32, Int32> value_to_label) {
        label_packs.Push(new LabelPack(-1, break_label, default_label, value_to_label));
    }

    public void OutLabels() {
        label_packs.Pop();
        //_continue_labels.Pop();
        //_break_labels.Pop();
    }

    private Int32 return_label;
    public Int32 ReturnLabel {
        get {
            if (return_label == -1) {
                throw new InvalidOperationException("Not inside a function.");
            }
            return return_label;
        }
    }
    public void InFunction() {
        return_label = RequestLabel();
    }

    public void OutFunction() {
        return_label = -1;
    }
}
