using Bikuida.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bikuida.Services
{
    public class Dumper
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        const int PROCESS_WM_READ = 0x0010;

        static int FindOffsetInRange(byte[] moduleBytes, byte[] pattern, string mask,
            int start, int end, string debugName = "")
        {
            int scanEnd = Math.Min(end, moduleBytes.Length) - pattern.Length;
            int firstMatch = -1;
            int matchCount = 0;

            for (int i = start; i <= scanEnd; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (mask[j] == 'x' && moduleBytes[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    matchCount++;
                    if (firstMatch == -1)
                        firstMatch = i;
                }
            }
            return firstMatch;
        }

        static (int start, int size) GetTextSectionBounds(byte[] moduleBytes)
        {
            int e_lfanew = BitConverter.ToInt32(moduleBytes, 0x3C);
            int fileHeaderOffset = e_lfanew + 4;
            ushort numberOfSections = BitConverter.ToUInt16(moduleBytes, fileHeaderOffset + 2);
            ushort sizeOfOptionalHeader = BitConverter.ToUInt16(moduleBytes, fileHeaderOffset + 16);
            int optionalHeaderOffset = fileHeaderOffset + 20;
            int sectionTableOffset = optionalHeaderOffset + sizeOfOptionalHeader;
            const int SECTION_HEADER_SIZE = 40;

            for (int i = 0; i < numberOfSections; i++)
            {
                int sectionOffset = sectionTableOffset + (i * SECTION_HEADER_SIZE);
                string name = System.Text.Encoding.ASCII.GetString(moduleBytes, sectionOffset, 8).TrimEnd('\0');

                if (name == ".text")
                {
                    int virtualSize = BitConverter.ToInt32(moduleBytes, sectionOffset + 8);
                    int virtualAddress = BitConverter.ToInt32(moduleBytes, sectionOffset + 12);
                    return (virtualAddress, virtualSize);
                }
            }

            throw new Exception(".text section não encontrada");
        }

        static int ResolveRipRelative(byte[] moduleBytes, int matchIndex, int rel32Offset, int instrLength)
        {
            int rel32 = BitConverter.ToInt32(moduleBytes, matchIndex + rel32Offset);
            return matchIndex + instrLength + rel32;
        }

        public Offsets GetOffsetsByDump()
        {
            Offsets offsets = new Offsets();
            Process proc = Process.GetProcessesByName("cs2")[0];
            IntPtr handle = OpenProcess(PROCESS_WM_READ, false, proc.Id);

            ProcessModule clientDll = null;
            foreach (ProcessModule m in proc.Modules)
            {
                if (m.ModuleName == "client.dll")
                {
                    clientDll = m;
                    break;
                }
            }

            byte[] moduleBytes = new byte[clientDll.ModuleMemorySize];
            ReadProcessMemory(handle, clientDll.BaseAddress, moduleBytes, moduleBytes.Length, out int bytesRead);

            var (textStart, textSize) = GetTextSectionBounds(moduleBytes);
            int textEnd = textStart + textSize;

            // ---------- dwEntityList ----------
            // 48 89 0D ???????? ; mov [rip+X], rcx
            // E9 ????????        ; jmp rel32
            // CC                  ; int3
            {
                byte[] pattern = {
                0x48, 0x89, 0x0D, 0x00, 0x00, 0x00, 0x00,
                0xE9, 0x00, 0x00, 0x00, 0x00,
                0xCC
            };
                string mask = "xxx????x????x";

                int matchIndex = FindOffsetInRange(moduleBytes, pattern, mask, textStart, textEnd, "dwEntityList");
                if (matchIndex >= 0)
                {
                    int dwEntityList = ResolveRipRelative(moduleBytes, matchIndex, rel32Offset: 3, instrLength: 7);
                    offsets.dwEntityList = dwEntityList;
                }
                else
                {
                    MessageBox.Show("dwEntityList not found.", "Bikudia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            // ---------- dwViewMatrix ----------
            // 48 8D 0D ???????? ; lea rcx, [rip+X]
            // 48 C1 E0 06         ; shl rax, 6
            {
                byte[] pattern = {
                0x48, 0x8D, 0x0D, 0x00, 0x00, 0x00, 0x00,
                0x48, 0xC1, 0xE0, 0x06
            };
                string mask = "xxx????xxxx";

                int matchIndex = FindOffsetInRange(moduleBytes, pattern, mask, textStart, textEnd, "dwViewMatrix");
                if (matchIndex >= 0)
                {
                    int dwViewMatrix = ResolveRipRelative(moduleBytes, matchIndex, rel32Offset: 3, instrLength: 7);
                    offsets.dwViewMatrix = dwViewMatrix;
                }
                else
                {
                    MessageBox.Show("dwViewMatrix not found.", "Bikudia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            // ---------- dwLocalPlayerPawn (derivado de dwPrediction) ----------
            // Passo 1: achar dwPrediction
            // 48 8D 05 ???????? ; lea rax, [rip+X]
            // C3                  ; ret
            // CC x8               ; padding (int3 * 8)
            // 40 53 56            ; rex; push rbx; push rsi
            // 41 54               ; push r12
            {
                byte[] predictionPattern = {
                0x48, 0x8D, 0x05, 0x00, 0x00, 0x00, 0x00,
                0xC3,
                0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC,
                0x40, 0x53, 0x56,
                0x41, 0x54
            };
                string predictionMask = "xxx????x" + new string('x', 8) + "xxx" + "xx";

                int predictionMatch = FindOffsetInRange(moduleBytes, predictionPattern, predictionMask, textStart, textEnd, "dwPrediction");

                if (predictionMatch >= 0)
                {
                    int dwPrediction = ResolveRipRelative(moduleBytes, predictionMatch, rel32Offset: 3, instrLength: 7);

                    // Passo 2: achar o pattern secundário em QUALQUER lugar de .text
                    // 4C 39 B6 ???????? ; cmp [rsi+disp32], r14  (disp32 é o valor que queremos, NÃO é rip-relative)
                    // 74 ??               ; je curto
                    // 44 88 BE            ; mov [rsi+X], r15b
                    byte[] secondaryPattern = {
                    0x4C, 0x39, 0xB6, 0x00, 0x00, 0x00, 0x00,
                    0x74, 0x00,
                    0x44, 0x88, 0xBE
                };
                    string secondaryMask = "xxx????x?xxx";

                    int secondaryMatch = FindOffsetInRange(moduleBytes, secondaryPattern, secondaryMask, textStart, textEnd, "dwLocalPlayerPawn (secondary)");

                    if (secondaryMatch >= 0)
                    {
                        // Aqui NÃO é rip-relative: o valor é lido direto como imediato de 4 bytes
                        // e somado cru ao rva do dwPrediction (igual ao callback no repo).
                        int rawDisp = BitConverter.ToInt32(moduleBytes, secondaryMatch + 3);
                        int dwLocalPlayerPawn = dwPrediction + rawDisp;
                        offsets.dwLocalPlayerPawn = dwLocalPlayerPawn;
                    }
                    else
                    {
                        MessageBox.Show("Secondary pattern of dwLocalPlayerPawn not found.", "Bikudia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                else
                {
                    MessageBox.Show("dwPrediction not found.", "Bikudia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            return offsets;
        }
    }
}