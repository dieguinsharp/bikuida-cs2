namespace Bikuida.Models
{
    public class GameData
    {
        public Entity[] Entities { get; set; } = new Entity[64];
        public float[] ViewMatrix { get; set; } = new float[64];
    }
}
