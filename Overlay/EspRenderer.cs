using Bikuida.Config;
using Bikuida.Helpers;
using Bikuida.Models;
using ImGuiNET;
using System.Numerics;

namespace Bikuida.Overlay
{
    public class EspRenderer
    {
        private readonly Cs2WeaponsHelpers _cs2WeaponsHelpers = new Cs2WeaponsHelpers();
        private readonly EspConfig _config;

        private const float BOX_WIDTH_FACTOR = 3f;
        private const float LIFE_BAR_WIDTH_PERCENT = 0.05f;
        private const int MAX_NAME_LENGTH = 15;

        static readonly (int, int)[] PlayerBoneConnections = new (int, int)[]
        {
            // spine
            (1, 2),    // spine_lower -> spine_lower2
            (2, 23),   // spine_lower2 -> spine_middle2
            (23, 6),   // spine_middle2 -> spine_upper
            (6, 7),    // spine_upper -> neck

            // left arm
            (6, 9),    // spine_upper -> l_armpit
            (9, 10),   // l_armpit -> l_elbow
            (10, 11),  // l_elbow -> l_wrist

            // right arm
            (6, 13),   // spine_upper -> r_armpit
            (13, 14),  // r_armpit -> r_elbow
            (14, 15),  // r_elbow -> r_wrist

            // left leg
            (1, 17),   // spine_lower -> l_leg_joint2
            (17, 18),  // l_leg_joint2 -> l_knee
            (18, 19),  // l_knee -> l_foot

            // right leg
            (1, 20),   // spine_lower -> r_leg_joint2
            (20, 21),  // r_leg_joint2 -> r_knee
            (21, 22),  // r_knee -> r_foot
        };

        public EspRenderer(EspConfig config)
        {
            _config = config;
        }

        public void DrawEntity(Entity entity, ImDrawListPtr drawList, Vector2 windowOffset, int localPlayerTeam, Vector2 screenSize)
        {
            if (!IsValidEntity(entity)) return;

            if (entity == null || entity.Bones == null) return;

            bool isEnemy = entity.Team != localPlayerTeam;

            if (_config.ShowEnemy && isEnemy)
            {
                if (_config.ShowEnemySkeleton) DrawSkeleton(entity, drawList, windowOffset, _config.EnemySkeletonColor);
                if (_config.ShowEnemyBox) DrawBox(entity, drawList, windowOffset, _config.EnemyBoxColor);
                if (_config.ShowEnemyName) DrawName(entity, drawList, windowOffset, _config.EnemyNameColor);
                if (_config.ShowEnemyHealth) DrawLifeBar(entity, drawList, windowOffset, _config.EnemyHealthColor);
                if (_config.ShowEnemyLines) DrawLine(entity, drawList, windowOffset, _config.EnemyLinesColor, screenSize);
                if (_config.ShowEnemyWeapon) DrawWeaponText(entity, drawList, windowOffset, _config.EnemyWeaponColor, _config.ShowEnemyName);
            }
        }

        public void DrawFov(ImDrawListPtr drawList, Vector2 windowOffset, Vector2 screenSize, float fov)
        {
            var outlineColor = new Vector4(0f, 0f, 0f, 1);
            drawList.AddCircle(new Vector2(windowOffset.X + screenSize.X / 2, windowOffset.Y + screenSize.Y / 2), fov, GetUpdatedColorWithOpacity(outlineColor));
        }

        public void DrawAimbotFov(ImDrawListPtr drawList, Vector2 windowOffset, Vector2 screenSize, float fov)
        {
            var whiteColor = new Vector4(1f, 1f, 1f, 1);
            drawList.AddCircle(new Vector2(windowOffset.X + screenSize.X / 2, windowOffset.Y + screenSize.Y / 2), fov, GetUpdatedColorWithOpacity(whiteColor));
        }

        private void DrawSkeleton(Entity entity, ImDrawListPtr drawList, Vector2 offset, Vector4 color)
        {
            float height = CalculateEntityHeight(entity);
            float thicknessBase = height < 50 ? 0.8f : 1.6f;

            uint boneColor = GetUpdatedColorWithOpacity(color);

            for (int i = 0; i < PlayerBoneConnections.Length; i++)
            {
                Vector2 start = GetBoneScreen(entity, PlayerBoneConnections[i].Item1, offset);
                Vector2 end = GetBoneScreen(entity, PlayerBoneConnections[i].Item2, offset);

                if (start == Vector2.Zero || end == Vector2.Zero) continue;

                // espessura variável
                float thickness = GetBoneThickness(PlayerBoneConnections[i].Item1, thicknessBase);

                // linha principal
                drawList.AddLine(start, end, boneColor, thickness);
            }
        }

        private float GetBoneThickness(int boneIndex, float baseThickness)
        {
            if (IsTorsoBone(boneIndex)) return baseThickness * 1.5f;
            if (IsLegBone(boneIndex)) return baseThickness * 1.2f;
            if (IsArmBone(boneIndex)) return baseThickness * 1.0f;
            return baseThickness;
        }


        private bool IsTorsoBone(int boneIndex)
        {
            // tronco: coluna, peito, quadril
            return boneIndex == 6   // cabeça-base/neck
                || boneIndex == 5   // peito
                || boneIndex == 4   // coluna
                || boneIndex == 3;  // quadril
        }

        private bool IsArmBone(int boneIndex)
        {
            // braços: ombros, cotovelos, mãos
            return boneIndex == 7   // ombro esquerdo
                || boneIndex == 8   // cotovelo esquerdo
                || boneIndex == 9   // mão esquerda
                || boneIndex == 10  // ombro direito
                || boneIndex == 11  // cotovelo direito
                || boneIndex == 12; // mão direita
        }

        private bool IsLegBone(int boneIndex)
        {
            // pernas: quadril, joelhos, pés
            return boneIndex == 13  // quadril esquerdo
                || boneIndex == 14  // joelho esquerdo
                || boneIndex == 15  // pé esquerdo
                || boneIndex == 16  // quadril direito
                || boneIndex == 17  // joelho direito
                || boneIndex == 18; // pé direito
        }


        private void DrawBox(Entity entity, ImDrawListPtr drawList, Vector2 offset, Vector4 color)
        {
            if (entity.HeadPosition2D == Vector2.Zero || entity.Bones == null || entity.Bones.Length == 0)
                return;

            var foot = entity.Bones[0];

            color.W = _config.Opacity / 100.0f;

            float entityHeight = foot.Y - entity.HeadPosition2D.Y;

            // Margens extras
            float marginTop = 5.0f;   // espaço acima da cabeça
            float marginBottom = 5.0f; // espaço abaixo dos pés
            float marginSides = 3.0f;  // espaço lateral

            float boxLeft = entity.HeadPosition2D.X - entityHeight / BOX_WIDTH_FACTOR - marginSides;
            float boxRight = entity.HeadPosition2D.X + entityHeight / BOX_WIDTH_FACTOR + marginSides;

            Vector2 rectTop = new Vector2(boxLeft, entity.HeadPosition2D.Y - marginTop) + offset;
            Vector2 rectBottom = new Vector2(boxRight, foot.Y + marginBottom) + offset;

            float width = rectBottom.X - rectTop.X;
            float cornerSize = width / 4;

            float thickness = 2.0f;
            if (entityHeight < 50)
            {
                thickness = 1.0f;
                cornerSize = width / 3;
            }
            if (width < 10) cornerSize = width / 2 - 1;
            if (cornerSize < 2) cornerSize = 2;

            uint outlineColor = GetUpdatedColorWithOpacity(new Vector4(0, 0, 0, 1));

            void DrawCornerLine(Vector2 start, Vector2 end)
            {
                drawList.AddLine(start, end, outlineColor, thickness + 1.5f);
                drawList.AddLine(start, end, ImGui.ColorConvertFloat4ToU32(color), thickness);
            }

            // Top Left
            DrawCornerLine(rectTop, new Vector2(rectTop.X + cornerSize, rectTop.Y));
            DrawCornerLine(rectTop, new Vector2(rectTop.X, rectTop.Y + cornerSize));

            // Top Right
            Vector2 topRight = new Vector2(rectBottom.X, rectTop.Y);
            DrawCornerLine(topRight, new Vector2(topRight.X - cornerSize, topRight.Y));
            DrawCornerLine(topRight, new Vector2(topRight.X, topRight.Y + cornerSize));

            // Bottom Left
            Vector2 bottomLeft = new Vector2(rectTop.X, rectBottom.Y);
            DrawCornerLine(bottomLeft, new Vector2(bottomLeft.X + cornerSize, bottomLeft.Y));
            DrawCornerLine(bottomLeft, new Vector2(bottomLeft.X, bottomLeft.Y - cornerSize));

            // Bottom Right
            DrawCornerLine(rectBottom, new Vector2(rectBottom.X - cornerSize, rectBottom.Y));
            DrawCornerLine(rectBottom, new Vector2(rectBottom.X, rectBottom.Y - cornerSize));
        }



        private void DrawName(Entity entity, ImDrawListPtr drawList, Vector2 offset, Vector4 color)
        {
            if (entity.HeadPosition2D.X < 0 || entity.HeadPosition2D.Y <= 0) return;

            try
            {
                string displayName = ProcessEnemyName(entity.Name);

                Vector2 textSize = ImGui.CalcTextSize(displayName);

                float centerX = entity.HeadPosition2D.X - (textSize.X / 2f);

                float entityHeight = CalculateEntityHeight(entity);
                float topY = entity.HeadPosition2D.Y - entityHeight * 0.1f - textSize.Y - 2f;

                Vector2 textPosition = new Vector2(centerX, topY) + offset;
                color.W = _config.Opacity / 100.0f;
                DrawTextWithOutline(textPosition, GetUpdatedColorWithOpacity(color),
                                  displayName, GetUpdatedColorWithOpacity(new Vector4(0, 0, 0, 1)), drawList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error drawing name: {ex.Message}", "Bikuida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawWeaponText(Entity entity, ImDrawListPtr drawList, Vector2 offset, Vector4 color, bool showingName)
        {
            try
            {
                string weaponName = _cs2WeaponsHelpers.GetWeaponByID(entity.Weapon);

                Vector2 textSize = ImGui.CalcTextSize(weaponName);

                float centerX = entity.HeadPosition2D.X - (textSize.X / 2f);

                float entityHeight = CalculateEntityHeight(entity);
                float topY = 0;
                if (showingName)
                    topY = entity.HeadPosition2D.Y - entityHeight * 0.1f - textSize.Y - 13f;
                else
                    topY = entity.HeadPosition2D.Y - entityHeight * 0.1f - textSize.Y - 2f;

                Vector2 textPosition = new Vector2(centerX, topY) + offset;
                color.W = _config.Opacity / 100.0f;
                DrawTextWithOutline(textPosition, GetUpdatedColorWithOpacity(color),
                                  weaponName, GetUpdatedColorWithOpacity(new Vector4(0, 0, 0, 1)), drawList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao desenhar texto da arma: {ex.Message}", "Bikuida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawLifeBar(Entity entity, ImDrawListPtr drawList, Vector2 windowOffset, Vector4 color)
        {
            if (entity.HeadPosition2D == Vector2.Zero || entity.Bones == null || entity.Bones.Length == 0)
                return;

            var foot = entity.Bones[0];
            float entityHeight = foot.Y - entity.HeadPosition2D.Y;

            float barWidth = entityHeight * LIFE_BAR_WIDTH_PERCENT;
            float barLeft = (entity.HeadPosition2D.X + windowOffset.X) - (entityHeight / BOX_WIDTH_FACTOR) - 9.0f;

            float lifePercentage = Math.Clamp(entity.Health / 100f, 0f, 1f);

            // Topo da barra ajustado pelo percentual de vida
            float barTopY = entity.HeadPosition2D.Y + windowOffset.Y + (entityHeight * (1f - lifePercentage));
            float barBottomY = foot.Y + windowOffset.Y;

            Vector2 barTop = new Vector2(barLeft, barTopY);
            Vector2 barBottom = new Vector2(barLeft + barWidth, barBottomY);

            // Background
            Vector2 bgTop = new Vector2(barLeft - 1f, entity.HeadPosition2D.Y + windowOffset.Y - 1f);
            Vector2 bgBottom = new Vector2(barBottom.X + 1f, barBottomY + 1f);

            drawList.AddRectFilled(bgTop, bgBottom, GetUpdatedColorWithOpacity(new Vector4(0, 0, 0, 1)));

            // Barra de vida
            color.W = _config.Opacity / 100.0f;
            drawList.AddRectFilled(barTop, barBottom, GetUpdatedColorWithOpacity(color));

            // Texto de vida (ao lado do pé)
            Vector2 textPos = new Vector2(barBottom.X + 2, barBottomY);
            string hpText = $"{entity.Health:F0}";

            DrawTextWithOutline(textPos, GetUpdatedColorWithOpacity(color),
                                hpText, GetUpdatedColorWithOpacity(new Vector4(0, 0, 0, 1)), drawList);
        }



        private void DrawLine(Entity entity, ImDrawListPtr drawList, Vector2 offset, Vector4 color, Vector2 screenSize)
        {
            Vector2 lineStart = new Vector2(screenSize.X / 2, screenSize.Y / 2) + offset;
            Vector2 lineEnd = entity.Position2D + offset;
            color.W = _config.Opacity / 100.0f;
            drawList.AddLine(lineStart, lineEnd, ImGui.ColorConvertFloat4ToU32(color));
        }

        private bool IsValidEntity(Entity entity)
            => entity != null && entity.Position2D.X >= 0 && entity.Position2D.Y >= 0;

        private float CalculateEntityHeight(Entity entity)
        {
            if (entity.HeadPosition2D == Vector2.Zero || entity.Bones == null || entity.Bones.Length == 0)
                return 0f;

            var foot = entity.Bones[0];
            return foot.Y - entity.HeadPosition2D.Y;
        }


        private Vector2 GetBoneScreen(Entity entity, int boneIndex, Vector2 offset)
            => new Vector2(
                entity.Bones[boneIndex].X + offset.X,
                entity.Bones[boneIndex].Y + offset.Y);

        private void DrawTextWithOutline(Vector2 pos, uint color, string text, uint outlineColor, ImDrawListPtr drawListPtr)
        {
            drawListPtr.AddText(pos + new Vector2(1, 1), outlineColor, text);
            drawListPtr.AddText(pos, color, text);
        }

        private uint GetUpdatedColorWithOpacity(Vector4 color)
        {
            color.W = _config.Opacity / 100.0f;
            return ImGui.ColorConvertFloat4ToU32(color);
        }

        private string ProcessEnemyName(string originalName)
        {
            if (string.IsNullOrWhiteSpace(originalName))
                return "Unknown";

            string safeName = string.Concat(originalName
                .Where(c => c >= 32 && c <= 126 && !char.IsControl(c)));

            if (string.IsNullOrWhiteSpace(safeName))
                return "Unknown";

            return safeName.Length <= MAX_NAME_LENGTH
                ? safeName
                : safeName.Substring(0, MAX_NAME_LENGTH);
        }
    }
}
