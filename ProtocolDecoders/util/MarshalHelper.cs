using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolDecoders.util
{
    internal class MarshalHelper
    {
        static public byte[] GetStructBytes<T>(T memoryAlignedStruct) 
            where T : struct
        {
            int size = Marshal.SizeOf(memoryAlignedStruct);
            byte[] arr = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(memoryAlignedStruct, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

        static public T? FromBytes<T>(byte[] arr)
            where T : struct
        {
            T? memoryAlignedStruct = null;

            int size = Marshal.SizeOf(memoryAlignedStruct);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(arr, 0, ptr, size);

                memoryAlignedStruct = Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return memoryAlignedStruct;
        }
    }
}
