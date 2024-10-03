using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolTest.test_classes
{
    internal class TestHeaders
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderSmall
        {
            public UInt16 Type;
            public UInt32 Size;
            public double TimeStamp;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HeaderMedium
        {
            public UInt16 Type;
            public double TimeStamp;
            public UInt32 Size;
    //        public fixed byte Data[255];
        } 
    
    }
}
