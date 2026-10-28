using Bikuida.Config;
using Bikuida.Services;
using System.ComponentModel;
using System.Numerics;
using System.Windows.Media;

namespace Bikuida.Menu
{
    public class EspMenuViewModel : INotifyPropertyChanged
    {
        private readonly EspConfig _cfg;
        private readonly BikuidaBootstrap bikuidaBootstrap = new BikuidaBootstrap();
        public event PropertyChangedEventHandler? PropertyChanged;


        public EspMenuViewModel(EspConfig cfg)
        {
           _cfg = cfg;
           Fov = _cfg.Fov;
           AimbotFov = _cfg.AimbotFov;
           Opacity = _cfg.Opacity;
           _ = bikuidaBootstrap.StartBikuidaProject(_cfg);
        }

        // Helper: Vector4 ↔ SolidColorBrush
        private static SolidColorBrush V4ToBrush(Vector4 v) =>
            new(System.Windows.Media.Color.FromArgb(
                (byte)(v.W * 255),
                (byte)(v.X * 255),
                (byte)(v.Y * 255),
                (byte)(v.Z * 255)));

        private static Vector4 BrushToV4(SolidColorBrush b) =>
            new(b.Color.R / 255f, b.Color.G / 255f,
                b.Color.B / 255f, b.Color.A / 255f);

        // ── Global ──
        public bool ShowEnemy { get => _cfg.ShowEnemy; set { _cfg.ShowEnemy = value; PropertyChanged?.Invoke(this, new(nameof(ShowEnemy))); } }
        public bool ShowTeam { get => _cfg.ShowTeam; set { _cfg.ShowTeam = value; PropertyChanged?.Invoke(this, new(nameof(ShowTeam))); } }

        // ── Enemy bools ──
        public bool ShowEnemySkeleton { get => _cfg.ShowEnemySkeleton; set { _cfg.ShowEnemySkeleton = value; PropertyChanged?.Invoke(this, new(nameof(ShowEnemySkeleton))); } }
        public bool ShowEnemyBox { get => _cfg.ShowEnemyBox; set { _cfg.ShowEnemyBox = value; PropertyChanged?.Invoke(this, new(nameof(ShowEnemyBox))); } }
        public bool ShowEnemyName { get => _cfg.ShowEnemyName; set { _cfg.ShowEnemyName = value; PropertyChanged?.Invoke(this, new(nameof(ShowEnemyName))); } }
        public bool ShowEnemyHealth { get => _cfg.ShowEnemyHealth; set { _cfg.ShowEnemyHealth = value; PropertyChanged?.Invoke(this, new(nameof(ShowEnemyHealth))); } }
        public bool ShowEnemyLines { get => _cfg.ShowEnemyLines; set { _cfg.ShowEnemyLines = value; PropertyChanged?.Invoke(this, new(nameof(ShowEnemyLines))); } }
        public bool ShowEnemyWeapon { get => _cfg.ShowEnemyWeapon; set { _cfg.ShowEnemyWeapon = value; PropertyChanged?.Invoke(this, new(nameof(ShowEnemyWeapon))); } }
        public bool UseEnemyTrigger { get => _cfg.UseEnemyTrigger; set { _cfg.UseEnemyTrigger = value; PropertyChanged?.Invoke(this, new(nameof(UseEnemyTrigger))); } }
        public bool UseAimbot { get => _cfg.UseAimbot; set { _cfg.UseAimbot = value; PropertyChanged?.Invoke(this, new(nameof(UseAimbot))); } }


        // ── Enemy colors (as SolidColorBrush for the color picker popup) ──
        public SolidColorBrush EnemySkeletonColor { get => V4ToBrush(_cfg.EnemySkeletonColor); set { _cfg.EnemySkeletonColor = BrushToV4(value); PropertyChanged?.Invoke(this, new(nameof(EnemySkeletonColor))); } }
        public SolidColorBrush EnemyBoxColor { get => V4ToBrush(_cfg.EnemyBoxColor); set { _cfg.EnemyBoxColor = BrushToV4(value); PropertyChanged?.Invoke(this, new(nameof(EnemyBoxColor))); } }
        public SolidColorBrush EnemyNameColor { get => V4ToBrush(_cfg.EnemyNameColor); set { _cfg.EnemyNameColor = BrushToV4(value); PropertyChanged?.Invoke(this, new(nameof(EnemyNameColor))); } }
        public SolidColorBrush EnemyHealthColor { get => V4ToBrush(_cfg.EnemyHealthColor); set { _cfg.EnemyHealthColor = BrushToV4(value); PropertyChanged?.Invoke(this, new(nameof(EnemyHealthColor))); } }
        public SolidColorBrush EnemyLinesColor { get => V4ToBrush(_cfg.EnemyLinesColor); set { _cfg.EnemyLinesColor = BrushToV4(value); PropertyChanged?.Invoke(this, new(nameof(EnemyLinesColor))); } }
        public SolidColorBrush EnemyWeaponColor { get => V4ToBrush(_cfg.EnemyWeaponColor); set { _cfg.EnemyWeaponColor = BrushToV4(value); PropertyChanged?.Invoke(this, new(nameof(EnemyWeaponColor))); } }
        public SolidColorBrush UseEnemyTriggerColor { get => V4ToBrush(_cfg.TriggerFovColor); set { _cfg.TriggerFovColor = BrushToV4(value); PropertyChanged?.Invoke(this, new(nameof(UseEnemyTriggerColor))); } }


        // ── Team bools ──
        public bool ShowTeamSkeleton { get => _cfg.ShowTeamSkeleton; set { _cfg.ShowTeamSkeleton = value; PropertyChanged?.Invoke(this, new(nameof(ShowTeamSkeleton))); } }
        public bool ShowTeamBox { get => _cfg.ShowTeamBox; set { _cfg.ShowTeamBox = value; PropertyChanged?.Invoke(this, new(nameof(ShowTeamBox))); } }
        public bool ShowTeamName { get => _cfg.ShowTeamName; set { _cfg.ShowTeamName = value; PropertyChanged?.Invoke(this, new(nameof(ShowTeamName))); } }
        public bool ShowTeamHealth { get => _cfg.ShowTeamHealth; set { _cfg.ShowTeamHealth = value; PropertyChanged?.Invoke(this, new(nameof(ShowTeamHealth))); } }
        public bool ShowTeamLines { get => _cfg.ShowTeamLines; set { _cfg.ShowTeamLines = value; PropertyChanged?.Invoke(this, new(nameof(ShowTeamLines))); } }
        public bool ShowTeamWeapon { get => _cfg.ShowTeamWeapon; set { _cfg.ShowTeamWeapon = value; PropertyChanged?.Invoke(this, new(nameof(ShowTeamWeapon))); } }

        // ── Shared ──
        public float Fov
        {
            get => _cfg.Fov;
            set { _cfg.Fov = value; PropertyChanged?.Invoke(this, new(nameof(Fov))); }
        }

        public float AimbotFov
        {
            get => _cfg.AimbotFov;
            set { _cfg.AimbotFov = value; PropertyChanged?.Invoke(this, new(nameof(AimbotFov))); }
        }

        public int Opacity
        {
            get => _cfg.Opacity;
            set { _cfg.Opacity = value; PropertyChanged?.Invoke(this, new(nameof(Opacity))); }
        }

    }
}
