using Bikuida.Helpers;
using Bikuida.Models;
using System.Numerics;

namespace Bikuida.Services
{
    public class AimbotService
    {
        public void Aim(Vector2 screenSize, GameData gameData, float fov, Entity localPlayer, float smooth)
        {
            var center = new Vector2(screenSize.X / 2f, screenSize.Y / 2f);

            var closestEntity = gameData.Entities
                .Where(o => o != null && o.Team != localPlayer.Team)
                .Select(o =>
                {
                    var boneScreen = GetBoneScreen(o, 26);

                    float dx = boneScreen.X - center.X;
                    float dy = boneScreen.Y - center.Y;
                    float distToCenter = MathF.Sqrt(dx * dx + dy * dy);

                    return new
                    {
                        Entity = o,
                        BoneScreen = boneScreen,
                        DistToCenter = distToCenter
                    };
                })
                .Where(x => x.DistToCenter <= fov)
                .OrderBy(x => x.DistToCenter)
                .FirstOrDefault();

            if (closestEntity == null)
                return;

            // Calcula delta relativo ao centro
            int dxFinal = (int)(closestEntity.BoneScreen.X - center.X);
            int dyFinal = (int)(closestEntity.BoneScreen.Y - center.Y);

            // Suavização opcional
            dxFinal = (int)(dxFinal / smooth);
            dyFinal = (int)(dyFinal / smooth);

            Input.InjectMouse(dxFinal, dyFinal, Input.MouseButtons.Move);
        }



        private Vector2 GetBoneScreen(Entity entity, int boneIndex)
        {
            // Se já é screen space, apenas retorna
            return entity.Bones[boneIndex];
        }
    }
}
