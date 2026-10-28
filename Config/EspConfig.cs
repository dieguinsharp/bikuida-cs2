using System.Numerics;

namespace Bikuida.Config
{
    public class EspConfig
    {
        private static readonly Vector4 _red = new(1.00f, 0.00f, 0.00f, 1.0f);

        // === Global toggles ===
        public bool ShowEnemy = true;
        public bool ShowTeam = false;

        // === Enemy ===
        public bool ShowEnemySkeleton = true;
        public bool ShowEnemyBox = false;
        public bool ShowEnemyName = true;
        public bool ShowEnemyHealth = true;
        public bool ShowEnemyLines = false;
        public bool ShowEnemyWeapon = false;
        public bool UseEnemyTrigger = false;
        public bool UseAimbot = false;

        public Vector4 EnemySkeletonColor = _red;
        public Vector4 EnemyBoxColor = _red;
        public Vector4 EnemyNameColor = _red;
        public Vector4 EnemyHealthColor = _red;
        public Vector4 EnemyLinesColor = _red;
        public Vector4 EnemyWeaponColor = _red;
        public Vector4 TriggerFovColor = _red;

        // === Team ===
        public bool ShowTeamSkeleton = false;
        public bool ShowTeamBox = false;
        public bool ShowTeamName = false;
        public bool ShowTeamHealth = false;
        public bool ShowTeamLines = false;
        public bool ShowTeamWeapon = false;

        // === Shared ===
        public float Fov { get; set; } = 12.0f;
        public float AimbotFov { get; set; } = 35.0f;
        public int Opacity { get; set; } = 100;
    }
}
