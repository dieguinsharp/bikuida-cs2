using Bikuida.Config;
using Bikuida.Game;
using Bikuida.Menu;
using Bikuida.Models;
using Bikuida.Overlay;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Bikuida.Services
{
    public class BikuidaBootstrap
    {
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);
        private const string GameName = "cs2";

        [STAThread]
        public async Task StartBikuidaProject(EspConfig config)
        {
            var process = EnsureGameRunning();
            var offsets = await LoadOffsets();

            var screenSize = GetScreenSize();
            var gameData = new GameData();
            var localPlayer = new Entity();
            var keyDownService = new KeyDownService();
            var aimbotService = new AimbotService();

            var overlay = new MainOverlay(gameData, localPlayer, screenSize, config, keyDownService);

            var rendererThread = new Thread(async () => await overlay.Start())
            {
                IsBackground = true,
                Name = "Renderer"
            };
            rendererThread.Start();

            StartGameLoopThread(process, gameData, localPlayer, offsets, screenSize, keyDownService, aimbotService, config);
        }

        private Process EnsureGameRunning()
        {
            var process = Process.GetProcessesByName(GameName).FirstOrDefault();
            if (process == null)
            {
                MessageBox.Show("CS2 is not open.", "Bikuida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                Application.Exit();
            }
            return process;
        }

        private async Task<Offsets> LoadOffsets()
        {
            var offsetService = new OffsetsService();
            var offsets = await offsetService.GetOffsetsAsync();
            if (offsets.Record == null)
            {
                MessageBox.Show("Offsets not found :(", "Bikuida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                Application.Exit();
            }
            return offsets.Record;
        }

        private Vector2 GetScreenSize()
        {
            return new Vector2(GetSystemMetrics(0), GetSystemMetrics(1));
        }

        private void StartGameLoopThread(Process process, GameData gameData, Entity localPlayer,
                                            Offsets offsets, Vector2 screenSize,
                                            KeyDownService keyDownService, AimbotService aimbotService,
                                            EspConfig config)
        {
            var triggerService = new AutoTriggerService();
            var gameLoop = new GameLoop(gameData, localPlayer, offsets, screenSize, triggerService, aimbotService, keyDownService, config);

            var loopThread = new Thread(() => gameLoop.Run(process))
            {
                IsBackground = true,
                Name = "GameLoop",
                Priority = ThreadPriority.AboveNormal
            };
            loopThread.Start();
        }
    }
}
