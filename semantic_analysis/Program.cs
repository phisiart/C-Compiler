using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace semantic_analysis {
    class Program {
        static void Main(string[] args) {
            TStruct struct1 = new TStruct();
            struct1.attribs.Add(new ScopeEntry("int16", new TInt16()));
            struct1.attribs.Add(new ScopeEntry("int32", new TInt32()));
            struct1.attribs.Add(new ScopeEntry("float64", new TFloat64()));
            struct1.attribs.Add(new ScopeEntry("char", new TInt8()));
            Console.WriteLine(struct1.GetOffset(0));
            Console.WriteLine(struct1.GetOffset(1));
            Console.WriteLine(struct1.GetOffset(2));
            Console.WriteLine(struct1.GetOffset(3));
            Console.WriteLine(struct1.SizeOf());


            TInt8 int8 = new TChar();
            //Console.WriteLine(int8);
            //TPointer ptr = new TPointer();
            //ptr.ref_type = int8;
            //Console.WriteLine(ptr);
            
        }
    }
}
