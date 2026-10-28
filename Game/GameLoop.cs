using Bikuida.Config;
using Bikuida.Helpers;
using Bikuida.Models;
using Bikuida.Services;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Bikuida.Game
{
    public class GameLoop
    {
        public readonly int _aimbotSmooth = 1;
        private readonly TimeSpan _frameTime;
        private readonly GameData _gameData;
        private readonly Entity _localPlayer;
        private readonly Offsets _offsets;
        private readonly AutoTriggerService _autoTriggerService;
        private readonly AimbotService _aimbotService;
        private readonly KeyDownService _keyDownService;
        private readonly EspConfig _config;

        Vector2 _screenSize;

        private const int MaxEntities = 64;
        private const int BonesCount = 80;

        public GameLoop(GameData gameData, Entity localPlayer, Offsets offsets, Vector2 screenSize, AutoTriggerService autoTriggerService, AimbotService aimbotService, KeyDownService keyDownService, EspConfig config)
        {
            _gameData = gameData;
            _localPlayer = localPlayer;
            _offsets = offsets;
            _screenSize = screenSize;
            _autoTriggerService = autoTriggerService;
            _keyDownService = keyDownService;
            _config = config;
            _aimbotService = aimbotService;
            _frameTime = TimeSpan.FromSeconds(1.0 / 244);
        }

        public void Run(Process process)
        {
            ProcessModule? client = process.Modules
                .Cast<ProcessModule>()
                .FirstOrDefault(m => m.ModuleName == "client.dll");

            if (client == null)
            {
                MessageBox.Show("Client.dll not found!", "Bikuida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var sw = Stopwatch.StartNew();

            nint entityList = MemoryAccessHelper.Read<nint>(client.BaseAddress + _offsets.dwEntityList, process.Handle);
            nint list = MemoryAccessHelper.Read<nint>(entityList + 0x10, process.Handle);

            while (true)
            {
                sw.Restart();
                try
                {
                    Tick(process, client, entityList, list);

                    if (_config.UseAimbot)
                    {
                        bool canAimbot = _keyDownService.HandleAimbotKey();
                        if (canAimbot)
                            _aimbotService.Aim(_screenSize, _gameData, _config.AimbotFov, _localPlayer, _aimbotSmooth);
                    }

                    if (_config.UseEnemyTrigger)
                    {
                        bool canTrigger = _keyDownService.HandleTriggerKey();
                        if (canTrigger)
                            _autoTriggerService.Trigger(_localPlayer, _screenSize, _gameData, _config.Fov);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}", "Bikuida", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    Application.Exit();
                }

                TimeSpan elapsed = sw.Elapsed;
                if (elapsed < _frameTime)
                    Thread.Sleep(_frameTime - elapsed);
            }
        }


        private void Tick(Process process, ProcessModule client, nint entityList, nint list)
        {
            float[] viewMatrix = ReadViewMatrix(process, client, _offsets);

            _localPlayer.Address = MemoryAccessHelper.Read<nint>(
                client.BaseAddress + _offsets.dwLocalPlayerPawn, process.Handle);

            Parallel.For(1, MaxEntities, i =>
            {
                IntPtr controller = MemoryAccessHelper.Read<IntPtr>(list + i * 0x70, process.Handle);
                if (controller == IntPtr.Zero) { _gameData.Entities[i] = null; return; }

                int pawnHandle = MemoryAccessHelper.Read<int>(controller + _offsets.m_hPlayerPawn, process.Handle);
                if (pawnHandle == 0) { _gameData.Entities[i] = null; return; }

                IntPtr entry2 = MemoryAccessHelper.Read<IntPtr>(
                    entityList + 0x8 * ((pawnHandle & 0x7fff) >> 9) + 0x10, process.Handle);
                if (entry2 == IntPtr.Zero) { _gameData.Entities[i] = null; return; }

                IntPtr pawn = MemoryAccessHelper.Read<IntPtr>(entry2 + 0x70 * (pawnHandle & 0x1FF), process.Handle);
                if (pawn == IntPtr.Zero) { _gameData.Entities[i] = null; return; }

                int lifeState = MemoryAccessHelper.Read<int>(pawn + _offsets.m_lifeState, process.Handle);
                if (lifeState != 256) { _gameData.Entities[i] = null; return; }

                if (_localPlayer.Address == pawn)
                {
                    UpdateLocalPlayer(process, controller, pawn, _localPlayer, _offsets);
                    return;
                }

                _gameData.Entities[i] = ReadEntity(process, controller, pawn, viewMatrix, i, _offsets, _screenSize, _gameData);
            });
        }

        static string ReadName(Process process, IntPtr controller, Offsets offsets)
        => Encoding.UTF8.GetString(
            MemoryAccessHelper.ReadBytes(controller + offsets.m_iszPlayerName, 16, process.Handle));

        static float[] ReadViewMatrix(Process process, ProcessModule client, Offsets offsets)
        {
            byte[] bytes = MemoryAccessHelper.ReadBytes(client.BaseAddress + offsets.dwViewMatrix, 16 * 4, process.Handle);
            return Enumerable.Range(0, 16)
                             .Select(i => BitConverter.ToSingle(bytes, i * 4))
                             .ToArray();
        }

        static Vector2[] ReadBones(Process process, nint boneMatrix, float[] viewMatrix, Vector2 screenSize)
        {
            var bones = new Vector2[BonesCount];
            for (int x = 0; x < BonesCount; x++)
            {
                var pos3D = MemoryAccessHelper.Read<Vector3>(boneMatrix + x * 32, process.Handle);
                bones[x] = WHHelpers.WorldToScreen(viewMatrix, pos3D, screenSize);
            }
            return bones;
        }

        static void UpdateLocalPlayer(Process process, IntPtr controller, IntPtr pawn, Entity localPlayer, Offsets offsets)
        {
            localPlayer.Position = MemoryAccessHelper.Read<Vector3>(pawn + offsets.m_vOldOrigin, process.Handle);
            localPlayer.Name = ReadName(process, controller, offsets);
            localPlayer.Health = MemoryAccessHelper.Read<int>(pawn + offsets.m_iHealth, process.Handle);
            localPlayer.Team = MemoryAccessHelper.Read<int>(pawn + offsets.m_iTeamNum, process.Handle);
            localPlayer.IsLocalPlayer = true;
        }

        static Vector3 ReadPosition(Process process, IntPtr pawn, Offsets offsets)
        {
            var sceneNode = MemoryAccessHelper.Read<nint>(pawn + offsets.m_pGameSceneNode, process.Handle);
            var boneMatrix = MemoryAccessHelper.Read<nint>(sceneNode + offsets.m_modelState + offsets.m_skeletonInstance, process.Handle);
            var headBone = MemoryAccessHelper.Read<Vector3>(boneMatrix + 6 * 32, process.Handle);
            return headBone;
        }


        public static Entity? ReadEntity(Process process, IntPtr controller, IntPtr pawn, float[] viewMatrix, int slot, Offsets offsets, Vector2 screenSize, GameData gameData)
        {
            var position = ReadPosition(process, pawn, offsets);
            var health = MemoryAccessHelper.Read<int>(pawn + offsets.m_iHealth, process.Handle);

            if (position == Vector3.Zero || health <= 0 || health > 100) return null;

            var name = ReadName(process, controller, offsets);
            var team = MemoryAccessHelper.Read<int>(pawn + offsets.m_iTeamNum, process.Handle);
            var sceneNode = MemoryAccessHelper.Read<nint>(pawn + offsets.m_pGameSceneNode, process.Handle);
            var boneMatrix = MemoryAccessHelper.Read<nint>(sceneNode + offsets.m_modelState + offsets.m_skeletonInstance, process.Handle);

            var headBone = position;
            var position2D = WHHelpers.WorldToScreen(viewMatrix, position, screenSize);
            var headPosition2D = WHHelpers.WorldToScreen(viewMatrix, headBone, screenSize);

            var currentEntity = gameData.Entities[slot] ?? new Entity();
            var bones = ReadBones(process, boneMatrix, viewMatrix, screenSize);

            currentEntity.Address = pawn;
            currentEntity.Name = name;
            currentEntity.Health = health;
            currentEntity.Team = team;
            currentEntity.Matrix = viewMatrix;
            currentEntity.Position = position;
            currentEntity.Position2D = position2D;
            currentEntity.HeadBone = headBone;
            currentEntity.HeadPosition2D = headPosition2D;
            currentEntity.Bones = bones;

            return currentEntity;
        }

    }
}
