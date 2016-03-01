using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGeneration {
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

        ST0
    
    }

    public class CGenState {
        private enum Status {
            NONE,
            TEXT,
            DATA
        }

        public static Dictionary<Reg, String> reg_strs = new Dictionary<Reg, String> {
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
            [Reg.ST0] = "%st(0)"
        };

        public static String RegToString(Reg reg) => reg_strs[reg];

        public CGenState() {
            this.os = new System.IO.StringWriter();
            this.rodata = new System.IO.StringWriter();
            this.rodata.WriteLine("    .section .rodata");

            this.rodata_idx = 0;
            this.label_idx = 2;
            this.status = Status.NONE;
            this.label_packs = new Stack<LabelPack>();
            this.return_label = -1;
        }

        public void TEXT() {
            if (this.status != Status.TEXT) {
                this.os.WriteLine("    .text");
                this.status = Status.TEXT;
            }
        }

        public void DATA() {
            if (this.status != Status.DATA) {
                this.os.WriteLine("    .data");
                this.status = Status.DATA;
            }
        }

        public void GLOBL(String name) => this.os.WriteLine($"    .globl {name}");

        public void LOCAL(String name) => this.os.WriteLine($"    .local {name}");

        public void ALIGN(Int32 align) => this.os.WriteLine($"    .align {align}");

        public void COMM(String name, Int32 size, Int32 align) => this.os.WriteLine($"    .comm {name},{size},{align}");

        public void BYTE(Int32 value) => this.os.WriteLine($"    .byte {value}");

        public void ZERO(Int32 size) => this.os.WriteLine($"    .zero {size}");

        public void VALUE(Int32 value) => this.os.WriteLine($"    .value {value}");

        public void LONG(Int32 value) => this.os.WriteLine($"    .long {value}");


        public void CGenFuncStart(String name) {
            this.os.WriteLine(name + ":");
            PUSHL(Reg.EBP);
            MOVL(Reg.ESP, Reg.EBP);
            this.StackSize = 0;
        }

        /// <summary>
        /// FCHS: %st(0) = -%st(0)
        /// </summary>
        public void FCHS() => this.os.WriteLine("    fchs");

        /// <summary>
        /// FLDS: load float to FPU stack.
        /// </summary>
        public void FLDS(String src) => this.os.WriteLine($"    flds {src}");

        public void FLDS(Int32 imm, Reg src) => FLDS($"{imm}({RegToString(src)})");

        /// <summary>
        /// FLDL: load double to FPU stack.
        /// </summary>
        /// <param name="addr">Address.</param>
        public void FLDL(String addr) => this.os.WriteLine($"    fldl {addr}");

        public void FLDL(Int32 imm, Reg from) => FLDL($"{imm}({RegToString(from)})");

        /// <summary>
        /// FLD1: push 1.0 to FPU stack.
        /// </summary>
        public void FLD1() => this.os.WriteLine("    fld1");

        /// <summary>
        /// FLD0: push 0.0 to FPU stack.
        /// </summary>
        public void FLDZ() => this.os.WriteLine("    fldz");

        /// <summary>
        /// FSTS: store float from FPU stack.
        /// </summary>
        /// <param name="addr"></param>
        public void FSTS(String addr) => this.os.WriteLine($"    fsts {addr}");

        public void FSTS(Int32 imm, Reg to) => FSTS($"{imm}({RegToString(to)})");

        /// <summary>
        /// FSTPS: pop float from FPU stack, and store to {addr}.
        /// </summary>
        public void FSTPS(String addr) => this.os.WriteLine($"    fstps {addr}");

        public void FSTPS(Int32 imm, Reg to) => FSTPS($"{imm}({RegToString(to)})");

        /// <summary>
        /// FSTL: store double from FPU stack.
        /// </summary>
        public void FSTL(String addr) => this.os.WriteLine($"    fstl {addr}");

        public void FSTL(Int32 imm, Reg to) => FSTL($"{imm}({RegToString(to)})");

        /// <summary>
        /// FSTPL: pop from FPU and store *double*.
        /// </summary>
        public void FSTPL(String addr) => this.os.WriteLine($"    fstpl {addr}");

        public void FSTPL(Int32 imm, Reg to) => FSTPL($"{imm}({RegToString(to)})");

        /// <summary>
        /// FSTP: copy %st(0) to dst, then pop %st(0).
        /// </summary>
        public void FSTP(String dst) => this.os.WriteLine($"    fstp {dst}");

        public void FSTP(Reg dst) => FSTP(RegToString(dst));

        /// <summary>
        /// FADD: calculate %st(op1) + %st(op2) and rewrite %st(op2).
        /// </summary>
        public void FADD(Int32 op1, Int32 op2) => this.os.WriteLine($"    fadd %st({op1}), %st({op2})");

        /// <summary>
        /// FADDP: pop operands from %st(0) and %st(1),
        ///        push addition result back to %st(0).
        /// </summary>
        public void FADDP() => this.os.WriteLine("    faddp");

        /// <summary>
        /// FADD: calculate %st(op1) + %st(op2) and rewrite %st(op2).
        /// </summary>
        public void FSUB(Int32 op1, Int32 op2) => this.os.WriteLine($"    fsub %st({op1}), %st({op2})");

        /// <summary>
        /// FSUBP: pop operands from %st(0) and %st(1),
        ///        push %st(0) / %st(1) back to %st(0).
        /// </summary>
        public void FSUBP() => this.os.WriteLine("    fsubp");

        /// <summary>
        /// FMULP: pop operands from %st(0) and %st(1), push multiplication result back to %st(0).
        /// </summary>
        public void FMULP() => this.os.WriteLine("    fmulp");

        /// <summary>
        /// FDIVP: pop operands from %st(0) and %st(1), push %st(0) / %st(1) back to %st(0).
        /// </summary>
        public void FDIVP() => this.os.WriteLine("    fdivp");

        /// <summary>
        /// PUSHL: push long into stack.
        /// </summary>
        /// <remarks>
        /// PUSHL changes the size of the stack, which should be tracked carefully.
        /// So, PUSHL is set private. Consider using <see cref="CGenPushLong"/>
        /// </remarks>
        private void PUSHL(String src) => this.os.WriteLine($"    pushl {src}");

        private void PUSHL(Reg src) => PUSHL(RegToString(src));

        private void PUSHL(Int32 imm) => PUSHL($"${imm}");

        /// <summary>
        /// POPL: pop long from stack.
        /// </summary>
        /// <remarks>
        /// POPL changes the size of the stack, which should be tracked carefully.
        /// So, POPL is set private. Consider using <see cref="CGenPopLong"/>
        /// </remarks>
        private void POPL(String dst) => this.os.WriteLine($"    popl {dst}");

        private void POPL(Reg dst) => POPL(RegToString(dst));

        /// <summary>
        /// MOVL: move a 4-byte long
        /// </summary>
        public void MOVL(String src, String dst) => this.os.WriteLine($"    movl {src}, {dst}");

        public void MOVL(String src, Reg dst) => MOVL(src, RegToString(dst));

        public void MOVL(Int32 imm, String dst) => MOVL($"${imm}", dst);

        public void MOVL(Int32 imm, Reg dst) => MOVL($"${imm}", RegToString(dst));

        public void MOVL(Reg src, Reg dst) => MOVL(RegToString(src), RegToString(dst));

        public void MOVL(Reg src, Int32 offset, Reg dst) => MOVL(RegToString(src), $"{offset}({RegToString(dst)})");

        public void MOVL(Int32 offset, Reg src, Reg dst) => MOVL($"{offset}({RegToString(src)})", RegToString(dst));

        /// <summary>
        /// MOVZBL: move a byte and zero-extend to a 4-byte long
        /// </summary>
        public void MOVZBL(String src, String dst) => this.os.WriteLine($"    movzbl {src}, {dst}");

        public void MOVZBL(String src, Reg dst) => MOVZBL(src, RegToString(dst));

        public void MOVZBL(Int32 offset, Reg src, Reg dst) => MOVZBL($"{offset}({RegToString(src)})", RegToString(dst));

        public void MOVZBL(Reg src, Reg dst) => MOVZBL(RegToString(src), RegToString(dst));

        /// <summary>
        /// MOVSBL: move a byte and sign-extend to a 4-byte long
        /// </summary>
        public void MOVSBL(String src, String dst) => this.os.WriteLine($"    movsbl {src}, {dst}");

        public void MOVSBL(String src, Reg dst) => MOVSBL(src, RegToString(dst));

        public void MOVSBL(Int32 offset, Reg src, Reg dst) => MOVSBL($"{offset}({RegToString(src)})", RegToString(dst));

        public void MOVSBL(Reg src, Reg dst) => MOVSBL(RegToString(src), RegToString(dst));

        /// <summary>
        /// MOVB: move a byte
        /// </summary>
        public void MOVB(String src, String dst) => this.os.WriteLine($"    movb {src}, {dst}");

        public void MOVB(Reg from, Int32 imm, Reg to) {
            MOVB(RegToString(from), imm + "(" + RegToString(to) + ")");
        }

        public void MOVB(Reg from, Reg to) => MOVB(RegToString(from), RegToString(to));

        /// <summary>
        /// MOVW: move a 2-byte word
        /// </summary>
        public void MOVW(String from, String to) {
            this.os.WriteLine("    movw " + from + ", " + to);
        }

        public void MOVW(Reg from, Int32 imm, Reg to) {
            MOVW(RegToString(from), imm + "(" + RegToString(to) + ")");
        }

        /// <summary>
        /// MOVZWL: move a 2-byte word and zero-extend to a 4-byte long
        /// </summary>
        public void MOVZWL(String from, String to) {
            this.os.WriteLine("    movzwl " + from + ", " + to);
        }

        public void MOVZWL(String from, Reg to) {
            MOVZWL(from, RegToString(to));
        }

        public void MOVZWL(Int32 offset, Reg from, Reg to) {
            MOVZWL(offset + RegToString(from), RegToString(to));
        }

        public void MOVZWL(Reg src, Reg dst) => MOVZWL(RegToString(src), RegToString(dst));

        /// <summary>
        /// MOVSWL: move a 2-byte word and sign-extend to a 4-byte long
        /// </summary>
        public void MOVSWL(String from, String to) {
            this.os.WriteLine("    movswl " + from + ", " + to);
        }

        public void MOVSWL(String from, Reg to) {
            MOVSWL(from, RegToString(to));
        }

        public void MOVSWL(Int32 offset, Reg from, Reg to) {
            MOVSWL(offset + RegToString(from), RegToString(to));
        }

        public void MOVSWL(Reg src, Reg dst) => MOVSWL(RegToString(src), RegToString(dst));

        // LEA
        // ===
        // 
        public void LEA(String addr, String dst) => this.os.WriteLine($"    lea {addr}, {dst}");

        public void LEA(String addr, Reg dst) => LEA(addr, RegToString(dst));

        public void LEA(Int32 offset, Reg src, Reg dst) => LEA($"{offset}({RegToString(src)})", RegToString(dst));

        // CALL
        // ====
        // 
        public void CALL(String addr) {
            this.os.WriteLine("    call " + addr);
        }

        // CGenExpandStack
        // ===============
        // 
        public void CGenExpandStackTo(Int32 size, String comment = "") {
            if (size > this.StackSize) {
                SUBL(size - this.StackSize, RegToString(Reg.ESP), comment);
                this.StackSize = size;
            }
        }

        public void CGenExpandStackBy(Int32 nbytes) {
            this.StackSize += nbytes;
            SUBL(nbytes, Reg.ESP);
        }

        public void CGenExpandStackWithAlignment(Int32 nbytes, Int32 align) {
            nbytes = AST.Utils.RoundUp(this.StackSize + nbytes, align) - this.StackSize;
            CGenExpandStackBy(nbytes);
        }

        public void CGenForceStackSizeTo(Int32 nbytes) {
            this.StackSize = nbytes;
            LEA(-nbytes, Reg.EBP, Reg.ESP);
        }

        public void CGenShrinkStackBy(Int32 nbytes) {
            this.StackSize -= nbytes;
            ADDL(nbytes, Reg.ESP);
        }

        public void CGenExpandStackBy4Bytes(String comment = "") {
            this.StackSize += 4;
            SUBL(4, Reg.ESP);
        }

        public void CGenExpandStackBy8Bytes(String comment = "") {
            this.StackSize += 8;
            SUBL(8, Reg.ESP);
        }

        public void CGenShrinkStackBy4Bytes(String comment = "") {
            this.StackSize -= 4;
            ADDL(4, Reg.ESP);
        }

        public void CGenShrinkStackBy8Bytes(String comment = "") {
            this.StackSize -= 8;
            ADDL(8, Reg.ESP);
        }

        public void LEAVE() {
            //os.WriteLine("    leave # pop frame, restore %ebp");
            this.os.WriteLine("    leave");
        }

        public void RET() {
            //os.WriteLine("    ret # pop old %eip, jump");
            this.os.WriteLine("    ret");
        }

        public void NEWLINE() {
            this.os.WriteLine();
        }

        public void COMMENT(String comment) {
            this.os.WriteLine("    # " + comment);
        }

        /// <summary>
        /// NEG addr: addr = -addr
        /// </summary>
        public void NEG(String addr) => this.os.WriteLine($"    neg {addr}");

        public void NEG(Reg dst) => NEG(RegToString(dst));

        /// <summary>
        /// NOT: bitwise not
        /// </summary>
        public void NOT(String addr) => this.os.WriteLine($"    not {addr}");

        public void NOT(Reg dst) => NOT(RegToString(dst));

        /// <summary>
        /// ADDL: add long
        /// </summary>
        public void ADDL(String er, String ee, String comment = "") {
            this.os.Write($"    addl {er}, {ee}");
            if (comment == "") {
                this.os.WriteLine();
            } else {
                this.os.WriteLine($" # {comment}");
            }
        }

        public void ADDL(Int32 er, Reg ee, String comment = "") => ADDL($"${er}", RegToString(ee), comment);

        public void ADDL(Reg er, Reg ee, String comment = "") => ADDL(RegToString(er), RegToString(ee), comment);

        /// <summary>
        /// SUBL: subtract long
        /// </summary>
        public void SUBL(String er, String ee, String comment = "") {
            this.os.Write($"    subl {er}, {ee}");
            if (comment == "") {
                this.os.WriteLine();
            } else {
                this.os.WriteLine(" # " + comment);
            }
        }

        private void SUBL(Int32 er, String ee, String comment = "") => SUBL($"${er}", ee, comment);

        public void SUBL(Int32 er, Reg ee, String comment = "") => SUBL($"${er}", RegToString(ee), comment);

        public void SUBL(Reg er, Reg ee, String comment = "") => SUBL(RegToString(er), RegToString(ee), comment);

        public override String ToString() {
            return this.os.ToString() + this.rodata;
        }

        /// <summary>
        /// ANDL er, ee
        /// ee = er & ee
        /// </summary>
        public void ANDL(String er, String ee) => this.os.WriteLine($"    andl {er}, {ee}");

        public void ANDL(Reg er, Reg ee) => ANDL(RegToString(er), RegToString(ee));

        public void ANDL(Int32 er, Reg ee) => ANDL($"${er}", RegToString(ee));

        public void ANDB(String er, String ee) => this.os.WriteLine($"    andb {er}, {ee}");

        public void ANDB(Int32 er, Reg ee) => ANDB($"${er}", RegToString(ee));

        /// <summary>
        /// ORL er, ee
        ///     ee = ee | er
        /// </summary>
        public void ORL(String er, String ee, String comment = "") {
            this.os.Write("    orl " + er + ", " + ee);
            if (comment == "") {
                this.os.WriteLine();
            } else {
                this.os.WriteLine(" # " + comment);
            }
        }

        public void ORL(Reg er, Reg ee, String comment = "") {
            ORL(RegToString(er), RegToString(ee), comment);
        }

        /// <summary>
        /// SALL er, ee
        /// ee = ee << er
        /// Note that there is only one Kind of lshift.
        /// </summary>
        public void SALL(String er, String ee) {
            this.os.WriteLine("    sall " + er + ", " + ee);
        }

        public void SALL(Reg er, Reg ee) {
            SALL(RegToString(er), RegToString(ee));
        }

        /// <summary>
        /// SARL er, ee (arithmetic shift)
        /// ee = ee >> er (append sign bit)
        /// </summary>
        public void SARL(String er, String ee) {
            this.os.WriteLine($"    sarl {er}, {ee}");
        }

        public void SARL(Reg er, Reg ee) => SARL(RegToString(er), RegToString(ee));

        /// <summary>
        /// SHRL er, ee (logical shift)
        /// ee = ee >> er (append 0)
        /// </summary>
        public void SHRL(String er, String ee) {
            this.os.WriteLine($"    shrl {er}, {ee}");
        }

        public void SHRL(Reg er, Reg ee) => SHRL(RegToString(er), RegToString(ee));

        public void SHRL(Int32 er, Reg ee) => SHRL($"${er}", RegToString(ee));

        /// <summary>
        /// XORL er, ee
        /// ee = ee ^ er
        /// </summary>
        public void XORL(String er, String ee) {
            this.os.WriteLine("    xorl " + er + ", " + ee);
        }

        public void XORL(Reg er, Reg ee) {
            XORL(RegToString(er), RegToString(ee));
        }

        /// <summary>
        /// IMUL: signed multiplication. %edx:%eax = %eax * {addr}.
        /// </summary>
        public void IMUL(String addr) {
            this.os.WriteLine($"    imul {addr}");
        }

        public void IMUL(Reg er) {
            IMUL(RegToString(er));
        }

        /// <summary>
        /// MUL: unsigned multiplication. %edx:%eax = %eax * {addr}.
        /// </summary>
        public void MUL(String addr) {
            this.os.WriteLine($"    mul {addr}");
        }

        public void MUL(Reg er) {
            MUL(RegToString(er));
        }

        /// <summary>
        /// CLTD: used before division. clear %edx.
        /// </summary>
        public void CLTD() => this.os.WriteLine("    cltd");

        /// <summary>
        /// IDIVL: signed division. %eax = %edx:%eax / {addr}.
        /// </summary>
        public void IDIVL(String addr) {
            this.os.WriteLine($"    idivl {addr}");
        }

        public void IDIVL(Reg er) => IDIVL(RegToString(er));

        /// <summary>
        /// IDIVL: unsigned division. %eax = %edx:%eax / {addr}.
        /// </summary>
        public void DIVL(String addr) {
            this.os.WriteLine($"    divl {addr}");
        }

        public void DIVL(Reg er) => DIVL(RegToString(er));

        /// <summary>
        /// CMPL: compare based on subtraction.
        /// Note that the order is reversed, i.e. ee comp er.
        /// </summary>
        public void CMPL(String er, String ee) {
            this.os.WriteLine($"    cmpl {er}, {ee}");
        }

        public void CMPL(Reg er, Reg ee) => CMPL(RegToString(er), RegToString(ee));

        public void CMPL(Int32 imm, Reg ee) => CMPL($"${imm}", RegToString(ee));

        /// <summary>
        /// TESTL: used like testl %eax, %eax: compare %eax with zero.
        /// </summary>
        public void TESTL(String er, String ee) {
            this.os.WriteLine($"    testl {er}, {ee}");
        }

        public void TESTL(Reg er, Reg ee) => TESTL(RegToString(er), RegToString(ee));

        /// <summary>
        /// SETE: set if equal to.
        /// </summary>
        public void SETE(String dst) {
            this.os.WriteLine($"    sete {dst}");
        }

        public void SETE(Reg dst) => SETE(RegToString(dst));

        /// <summary>
        /// SETNE: set if not equal to.
        /// </summary>
        public void SETNE(String dst) => this.os.WriteLine($"    setne {dst}");
        public void SETNE(Reg dst) => SETNE(RegToString(dst));

        /// <summary>
        /// SETG: set if greater than (signed).
        /// </summary>
        public void SETG(String dst) {
            this.os.WriteLine($"    setg {dst}");
        }

        public void SETG(Reg dst) => SETG(RegToString(dst));

        /// <summary>
        /// SETGE: set if greater or equal to (signed).
        /// </summary>
        public void SETGE(String dst) {
            this.os.WriteLine($"    setge {dst}");
        }

        public void SETGE(Reg dst) => SETGE(RegToString(dst));

        /// <summary>
        /// SETL: set if less than (signed).
        /// </summary>
        public void SETL(String dst) {
            this.os.WriteLine($"    setl {dst}");
        }

        public void SETL(Reg dst) => SETL(RegToString(dst));

        /// <summary>
        /// SETLE: set if less than or equal to (signed).
        /// </summary>
        public void SETLE(String dst) {
            this.os.WriteLine($"    setle {dst}");
        }

        public void SETLE(Reg dst) => SETLE(RegToString(dst));

        /// <summary>
        /// SETB: set if below (unsigned).
        /// </summary>
        public void SETB(String dst) {
            this.os.WriteLine($"    setb {dst}");
        }

        public void SETB(Reg dst) => SETB(RegToString(dst));

        /// <summary>
        /// SETNB: set if not below (unsigned).
        /// </summary>
        public void SETNB(String dst) {
            this.os.WriteLine($"    setnb {dst}");
        }

        public void SETNB(Reg dst) => SETNB(RegToString(dst));

        /// <summary>
        /// SETA: set if above (unsigned).
        /// </summary>
        public void SETA(String dst) {
            this.os.WriteLine($"    seta {dst}");
        }

        public void SETA(Reg dst) => SETA(RegToString(dst));

        /// <summary>
        /// SETNA: set if not above (unsigned).
        /// </summary>
        public void SETNA(String dst) {
            this.os.WriteLine($"    setna {dst}");
        }

        public void SETNA(Reg dst) => SETNA(RegToString(dst));

        /// <summary>
        /// FUCOMIP: unordered comparison: %st(0) vs %st(1).
        /// </summary>
        public void FUCOMIP() => this.os.WriteLine("    fucomip %st(1), %st");

        public void JMP(Int32 label) => this.os.WriteLine($"    jmp .L{label}");

        public void JZ(Int32 label) => this.os.WriteLine($"    jz .L{label}");

        public void JNZ(Int32 label) => this.os.WriteLine($"    jz .L{label}");

        public void CLD() => this.os.WriteLine("    cld");

        public void STD() => this.os.WriteLine("    std");

        public Int32 CGenPushLong(Reg src) {
            PUSHL(src);
            this.StackSize += 4;
            return this.StackSize;
        }

        public Int32 CGenPushLong(Int32 imm) {
            PUSHL(imm);
            this.StackSize += 4;
            return this.StackSize;
        }

        public void CGenPopLong(Int32 saved_size, Reg dst) {
            if (this.StackSize == saved_size) {
                POPL(dst);
                this.StackSize -= 4;
            } else {
                MOVL(-saved_size, Reg.EBP, dst);
            }
        }

        public Int32 CGenPushFloat() {
            CGenExpandStackBy4Bytes();
            FSTS(0, Reg.ESP);
            return this.StackSize;
        }

        public Int32 CGenPushFloatP() {
            CGenExpandStackBy4Bytes();
            FSTPS(0, Reg.ESP);
            return this.StackSize;
        }

        public Int32 CGenPushDouble() {
            CGenExpandStackBy8Bytes();
            FSTL(0, Reg.ESP);
            return this.StackSize;
        }

        public Int32 CGenPushDoubleP() {
            CGenExpandStackBy8Bytes();
            FSTPL(0, Reg.ESP);
            return this.StackSize;
        }

        public void CGenPopDouble(Int32 saved_size) {
            FLDL(-saved_size, Reg.EBP);
            if (saved_size == this.StackSize) {
                CGenShrinkStackBy8Bytes();
            }
        }

        public void CGenPopFloat(Int32 saved_size) {
            FLDL(-saved_size, Reg.EBP);
            if (saved_size == this.StackSize) {
                CGenShrinkStackBy4Bytes();
            }
        }

        private void FISTL(String dst) => this.os.WriteLine($"    fistl {dst}");

        private void FISTL(Int32 offset, Reg dst) => FISTL($"{offset}({RegToString(dst)})");

        private void FILDL(String dst) => this.os.WriteLine($"    fildl {dst}");

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
            this.os.WriteLine("    rep movsl");
            MOVB(Reg.AL, Reg.CL);
            ANDB(3, Reg.CL);
            this.os.WriteLine("    rep movsb");
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
            this.os.WriteLine("    rep movsb");

            MOVL(Reg.EAX, Reg.ECX);
            ANDL(~3, Reg.ECX);
            SHRL(2, Reg.ECX);
            this.os.WriteLine("    rep movsl");

            CLD();
        }

        public String CGenLongConst(Int32 val) {
            String name = ".LC" + this.rodata_idx;
            this.rodata.WriteLine("    .align 4");
            this.rodata.WriteLine(name + ":");
            this.rodata.WriteLine("    .long " + val);
            this.rodata_idx++;
            return name;
        }

        public String CGenLongLongConst(Int32 lo, Int32 hi) {
            String name = ".LC" + this.rodata_idx;
            this.rodata.WriteLine("    .align 8");
            this.rodata.WriteLine(name + ":");
            this.rodata.WriteLine("    .long " + lo);
            this.rodata.WriteLine("    .long " + hi);
            this.rodata_idx++;
            return name;
        }

        public String CGenString(String str) {
            String name = ".LC" + this.rodata_idx;
            this.rodata.WriteLine(name + ":");
            this.rodata.WriteLine("    .String \"" + str + "\"");
            this.rodata_idx++;
            return name;
        }

        public void CGenLabel(String label) => this.os.WriteLine($"{label}:");

        public void CGenLabel(Int32 label) => CGenLabel($".L{label}");

        private readonly System.IO.StringWriter os;
        private readonly System.IO.StringWriter rodata;
        private Int32 rodata_idx;
        public Int32 label_idx;

        private Status status;

        public Int32 StackSize { get; private set; }

        public Int32 RequestLabel() {
            return this.label_idx++;
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

        private readonly Stack<LabelPack> label_packs;

        public Int32 ContinueLabel => this.label_packs.First(_ => _.continue_label != -1).continue_label;

        public Int32 BreakLabel => this.label_packs.First(_ => _.break_label != -1).break_label;

        public Int32 DefaultLabel {
            get {
                Int32 ret = this.label_packs.First().default_label;
                if (ret == -1) {
                    throw new InvalidOperationException("Not in a switch statement.");
                }
                return ret;
            }
        }

        public Int32 CaseLabel(Int32 value) => this.label_packs.First(_ => _.value_to_label != null).value_to_label[value];
        // label_packs.First().value_to_label[Value];

        public void InLoop(Int32 continue_label, Int32 break_label) {
            this.label_packs.Push(new LabelPack(continue_label, break_label, -1, null));
            //_continue_labels.Push(continue_label);
            //_break_labels.Push(break_label);
        }

        public void InSwitch(Int32 break_label, Int32 default_label, Dictionary<Int32, Int32> value_to_label) {
            this.label_packs.Push(new LabelPack(-1, break_label, default_label, value_to_label));
        }

        public void OutLabels() {
            this.label_packs.Pop();
            //_continue_labels.Pop();
            //_break_labels.Pop();
        }

        private readonly Dictionary<String, Int32> _goto_labels = new Dictionary<String, Int32>();

        public Int32 GotoLabel(String label) {
            return this._goto_labels[label];
        }

        private Int32 return_label;
        public Int32 ReturnLabel {
            get {
                if (this.return_label == -1) {
                    throw new InvalidOperationException("Not inside a function.");
                }
                return this.return_label;
            }
        }

        public void InFunction(IReadOnlyList<String> goto_labels) {
            this.return_label = RequestLabel();
            this._goto_labels.Clear();
            foreach (String goto_label in goto_labels) {
                this._goto_labels.Add(goto_label, RequestLabel());
            }
        }

        public void OutFunction() {
            this.return_label = -1;
            this._goto_labels.Clear();
        }
    }
}