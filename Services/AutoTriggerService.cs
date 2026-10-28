using Bikuida.Models;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Bikuida.Services
{
    public class AutoTriggerService
    {
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;

        }

        const uint INPUT_MOUSE = 0;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        private readonly Stopwatch _triggerStopwatch = Stopwatch.StartNew();
        private const int TriggerCooldownMs = 600;

        int[] CheckBones = { 6, 5, 4, 0, 13, 8, 25, 22 };

        public AutoTriggerService()
        {
            
        }

        private DateTime _lastClickTime = DateTime.MinValue;
        private readonly int _clickDelayMs = 280;

        public void Trigger(Entity localPlayer, Vector2 screenSize, GameData gameData, float fov)
        {
            if (_triggerStopwatch.ElapsedMilliseconds < TriggerCooldownMs) return;
            if (localPlayer == null || localPlayer.Position == Vector3.Zero) return;

            Vector2 screenCenter = new Vector2(screenSize.X / 2f, screenSize.Y / 2f);

            foreach (var entity in gameData.Entities)
            {
                if (entity == null || entity.Team == localPlayer.Team) continue;

                foreach (int boneIdx in CheckBones)
                {
                    if (entity.Bones == null || boneIdx >= entity.Bones.Length) continue;

                    Vector2 boneScreen = entity.Bones[boneIdx];

                    if (boneScreen == Vector2.Zero || boneScreen.X <= 0 || boneScreen.Y <= 0)
                        continue;

                    float dx = boneScreen.X - screenCenter.X;
                    float dy = boneScreen.Y - screenCenter.Y;
                    float distToCenter = MathF.Sqrt(dx * dx + dy * dy);

                    if (distToCenter <= fov)
                    {
                        if ((DateTime.Now - _lastClickTime).TotalMilliseconds >= _clickDelayMs)
                        {
                            Click();
                            _lastClickTime = DateTime.Now;
                        }
                        return;
                    }
                }
            }
        }


        private async void Click()
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            inputs[0].mi.dwExtraInfo = IntPtr.Zero;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));

            await Task.Delay(Random.Shared.Next(20, 60)); // delay entre DOWN e UP

            inputs[0].mi.dwFlags = MOUSEEVENTF_LEFTUP;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
