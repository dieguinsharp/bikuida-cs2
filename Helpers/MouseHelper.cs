using System.Runtime.InteropServices;

namespace Bikuida.Helpers
{
    

    public class MouseHelper
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const uint MOUSEEVENTF_MOVE = 0x0001;

        // Move para posição absoluta na tela
        public static void MoveTo(int x, int y)
        {
            SetCursorPos(x, y);
        }

        // Move relativo à posição atual
        public static void MoveRelative(int dx, int dy)
        {
            mouse_event(MOUSEEVENTF_MOVE, dx, dy, 0, 0);
        }
    }

    public static class Input
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        // Constantes equivalentes às flags do mouse
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        [Flags]
        public enum MouseButtons : byte
        {
            None = 0,
            Move = 1 << 0,
            LeftDown = 1 << 1,
            LeftUp = 1 << 2,
            RightDown = 1 << 3,
            RightUp = 1 << 4
        }

        public static void InjectMouse(int x, int y, MouseButtons buttons)
        {
            uint flags = 0;

            if (buttons.HasFlag(MouseButtons.Move))
                flags |= MOUSEEVENTF_MOVE;

            if (buttons.HasFlag(MouseButtons.LeftDown))
                flags |= MOUSEEVENTF_LEFTDOWN;

            if (buttons.HasFlag(MouseButtons.LeftUp))
                flags |= MOUSEEVENTF_LEFTUP;

            if (buttons.HasFlag(MouseButtons.RightDown))
                flags |= MOUSEEVENTF_RIGHTDOWN;

            if (buttons.HasFlag(MouseButtons.RightUp))
                flags |= MOUSEEVENTF_RIGHTUP;

            // Executa o evento do mouse
            mouse_event(flags, x, y, 0, 0);
            Task.Delay(20);
        }
    }

}
