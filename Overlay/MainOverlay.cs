using Bikuida.Config;
using Bikuida.Menu;
using Bikuida.Models;
using Bikuida.Services;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Bikuida.Overlay
{
    public class MainOverlay : ClickableTransparentOverlay.Overlay
    {
        private readonly GameData _gameData;
        private readonly Entity _localPlayer;
        private readonly EspConfig _config;
        private readonly EspRenderer _espRenderer;
        private readonly KeyDownService _keyDownService;
        private readonly Vector2 _screenSize;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const uint WDA_NONE = 0x00000000;
        private IntPtr _hWnd;
        public Vector2 WindowPosition = Vector2.Zero;

        [DllImport("user32.dll")] static extern IntPtr FindWindow(string? cls, string? name);
        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hWnd, int idx);
        [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hWnd, int idx, int val);
        [DllImport("user32.dll")] static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint affinity);
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        private readonly Stopwatch _frameTimer = new();

        private readonly Entity[] _frameEntities = new Entity[64];
        private int _frameEntityCount = 0;

        public static ImDrawListPtr DrawList;

        public MainOverlay(GameData gameData, Entity localPlayer, Vector2 screenSize, EspConfig espConfig, KeyDownService keyDownService)
        {
            _gameData = gameData;
            _localPlayer = localPlayer;
            _screenSize = screenSize;

            _config = espConfig;
            _espRenderer = new EspRenderer(_config);
            _keyDownService = keyDownService;
        }

        protected override async Task PostInitialized()
        {
            await base.PostInitialized();

            unsafe { ImGui.GetIO().NativePtr->IniFilename = null; }

            _hWnd = FindWindow(null, "Overlay");
            if (_hWnd != IntPtr.Zero)
            {
                int style = GetWindowLong(_hWnd, GWL_EXSTYLE);
                style = (style | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW;
                SetWindowLong(_hWnd, GWL_EXSTYLE, style);
            }
        }

        protected override void Render()
        {
            SetWindowDisplayAffinity(_hWnd, WDA_NONE);
            SyncWindowPosition();

            DrawTransparentCanvas(_screenSize);
            DrawList = ImGui.GetWindowDrawList();

            CollectEntities();

            if (_config.UseEnemyTrigger && _keyDownService.HandleTriggerKey())
                _espRenderer.DrawFov(DrawList, WindowPosition, _screenSize, _config.Fov);

            if (_config.UseAimbot && _keyDownService.HandleAimbotKey())
                _espRenderer.DrawAimbotFov(DrawList, WindowPosition, _screenSize, _config.AimbotFov);

            for (int i = 0; i < _frameEntityCount; i++)
            {
                _espRenderer.DrawEntity(_frameEntities[i], DrawList, WindowPosition, _localPlayer.Team, _screenSize);
            };
        }

        private void DrawTransparentCanvas(Vector2 size)
        {
            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowPos(WindowPosition);
            ImGui.Begin("##Canvas",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoInputs |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse);
        }

        private void SyncWindowPosition()
        {
            IntPtr gameHwnd = FindWindow(null, "Counter Strike 2");
            if (gameHwnd != IntPtr.Zero && GetWindowRect(gameHwnd, out RECT rect))
            {
                WindowPosition.X = rect.Left;
                WindowPosition.Y = rect.Top;
            }
        }

        private void CollectEntities()
        {
            _frameEntityCount = 0;
            foreach (var entity in _gameData.Entities)
            {
                if (entity != null)
                    _frameEntities[_frameEntityCount++] = entity;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ImGui.End();
        }
    }
}
