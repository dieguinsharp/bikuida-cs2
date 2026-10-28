using System.Numerics;

namespace Bikuida.Models
{
    public class Entity
    {
        public bool IsLocalPlayer { get; set; } = false;
        public nint Address { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public float Health { get; set; }
        public int Team { get; set; }
        public Vector3 Position { get; set; }
        public Vector2 Position2D { get; set; }
        public Vector2 HeadPosition2D { get; set; }
        public Vector2[] Bones { get; set; }
        public Vector3 HeadBone { get; set; }
        public int Weapon { get; set; }
        public float[] Matrix { get; set; }
        public bool IsSpotted { get; set; }
    }
}
