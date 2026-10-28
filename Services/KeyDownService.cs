using System.Runtime.InteropServices;

namespace Bikuida.Services
{
    public class KeyDownService
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_SHIFT = 0x10;
        private const int VK_ALT = 0x12;

        private static bool IsKeyDown(int vKey)
            => (GetAsyncKeyState(vKey) & 0x8000) != 0;

        private bool _triggerEnabled = false;
        private bool _keyWasDown = false;

        public bool HandleTriggerKey()
        {
            bool isDown = IsKeyDown(VK_SHIFT);

            if (isDown && !_keyWasDown)
                _triggerEnabled = !_triggerEnabled;

            _keyWasDown = isDown;

            return _triggerEnabled;
        }

        private bool _aimbotEnabled = false;
        private bool _aimbotKeyWasDown = false;

        public bool HandleAimbotKey()
        {
            bool isDown = IsKeyDown(VK_ALT);

            if (isDown && !_aimbotKeyWasDown)
                _aimbotEnabled = !_aimbotEnabled;

            _aimbotKeyWasDown = isDown;

            return _aimbotEnabled;
        }
    }

}
