using System.Numerics;

namespace Bikuida.Helpers
{
    public static class WHHelpers
    {
        public static Vector2 WorldToScreen(float[] matrix, Vector3 pos, Vector2 windowSize)
        {
            float screenW = (matrix[12] * pos.X) + (matrix[13] * pos.Y) + (matrix[14] * pos.Z) + matrix[15];

            if (screenW > 0.001f)
            {
                float screenX = (matrix[0] * pos.X) + (matrix[1] * pos.Y) + (matrix[2] * pos.Z) + matrix[3];
                float screenY = (matrix[4] * pos.X) + (matrix[5] * pos.Y) + (matrix[6] * pos.Z) + matrix[7];

                float X = (windowSize.X / 2) + (windowSize.X / 2) * screenX / screenW;
                float Y = (windowSize.Y / 2) - (windowSize.Y / 2) * screenY / screenW;


                return new Vector2(X, Y);
            }
            else
            {
                return new Vector2(-99, -99);
            }

        }

        public static Vector2 WorldToScreenVector3(Vector3 pos, Matrix4x4 viewProj, Vector2 screenSize)
        {
            // Multiplica posição pelo view-projection matrix
            Vector4 clipCoords = Vector4.Transform(new Vector4(pos, 1.0f), viewProj);

            if (clipCoords.W < 0.1f)
                return Vector2.Zero; // atrás da câmera

            // Normaliza
            Vector3 ndc = new Vector3(
                clipCoords.X / clipCoords.W,
                clipCoords.Y / clipCoords.W,
                clipCoords.Z / clipCoords.W
            );

            // Converte para coordenadas de tela
            return new Vector2(
                (screenSize.X / 2f * ndc.X) + (screenSize.X / 2f),
                -(screenSize.Y / 2f * ndc.Y) + (screenSize.Y / 2f)
            );
        }

    }
}
