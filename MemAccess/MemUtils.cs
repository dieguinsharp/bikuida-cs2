using System.Runtime.InteropServices;

namespace Bikuida
{
    public static class MemoryAccessHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        public static IntPtr? GetKernelHandleByProcessId(uint pid, uint accessMask)
        {
            IntPtr handle = OpenProcess(accessMask, false, pid);
            return handle == IntPtr.Zero ? null : handle;
        }

        public static bool Read(IntPtr address, IntPtr buffer, int size, IntPtr SourceProcess)
        {
            byte[] temp = new byte[size];
            return ReadProcessMemory(SourceProcess, address, temp, size, out _);
        }

        public static T Read<T>(IntPtr address, IntPtr SourceProcess) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            if (ReadProcessMemory(SourceProcess, address, buffer, size, out _))
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
            return default;
        }

        public static bool Write<T>(IntPtr address, T value, IntPtr SourceProcess) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(value, ptr, false);
                Marshal.Copy(ptr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return WriteProcessMemory(SourceProcess, address, buffer, size, out _);
        }

        public static byte[] ReadBytes(IntPtr address, int length, IntPtr SourceProcess)
        {
            byte[] buffer = new byte[length];
            if (ReadProcessMemory(SourceProcess, address, buffer, length, out _))
                return buffer;
            return Array.Empty<byte>();
        }

        public static bool WriteBytes(IntPtr address, byte[] data, IntPtr SourceProcess)
        {
            return WriteProcessMemory(SourceProcess, address, data, data.Length, out _);
        }
    }
}