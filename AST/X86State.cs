//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//public class X86State {

//    public RegContent eax {
//        get {
//            return _eax;
//        }
//        set {
//            switch (value.kind) {
//                case RegContent.Kind.GPREG:
//                    MOVL((GPReg)value, _eax);
//                    break;
//                default:
//                    throw new NotImplementedException();
//            }
//        }
//    }

//    public RegContent ebx {
//        get {
//            return _ebx;
//        }
//        set {
            
//        }
//    }

//    private EAX _eax = new EAX();
//    private EBX _ebx = new EBX();

//    public void MOVL(GPReg src, GPReg dst) => Console.WriteLine($"    movl {src}, {dst}");
//    public void test() {

//    }

//    public abstract class GPReg : RegContent {
//        public GPReg()
//            : base(Kind.GPREG) {
//        }
//    }

//    public class EAX : GPReg {
//        public override String ToString() => "%eax";
//    }

//    public class ECX : GPReg {
//        public override String ToString() => "%ecx";
//    }

//    public class EDX : GPReg {
//        public override String ToString() => "%edx";
//    }

//    public class EBX : GPReg {
//        public override String ToString() => "%ebx";
//    }

//    //public static 

//    //public enum GPReg {
//    //    EAX,
//    //    ECX,
//    //    EDX,
//    //    EBX,
//    //}

//    //public enum AddrReg {
//    //    ESI,
//    //    EDI,
//    //    EBP,
//    //    ESP,
//    //}

//    public abstract class RegContent {
//        public RegContent(Kind kind) {
//            this.kind = kind;
//        }

//        public enum Kind {
//            ADDRESS,
//            GPREG
//        }

//        public readonly Kind kind;
//    }

//    //public class Address {
//    //    public Address(AddrReg base_addr, Int32 offset, Int32 scale) {
//    //        this.base_addr = base_addr;
//    //        this.offset = offset;
//    //        this.scale = scale;
//    //    }
//    //    public readonly AddrReg base_addr;
//    //    public readonly Int32 offset;
//    //    public readonly Int32 scale;
//    //}
//}
