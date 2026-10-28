using Bikuida.Config;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bikuida.Menu
{
    public partial class EspMenuWindow : Window
    {
        private readonly EspMenuViewModel _vm;

        public EspMenuWindow(EspConfig config)
        {
            _vm = new EspMenuViewModel(config);
            InitializeComponent();
            DataContext = _vm;
            LoadIcon();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void LoadIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ico.ico");

                if (File.Exists(iconPath))
                {
                    this.Icon = BitmapFrame.Create(
                        new Uri(iconPath, UriKind.Absolute)
                    );
                }
            }
            catch { /* sem ícone */ }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Swatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button btn) return;
            string? tag = btn.Tag as string;
            if (string.IsNullOrEmpty(tag)) return;

            var currentBrush = btn.Background as SolidColorBrush;
            var currentColor = currentBrush?.Color ?? Colors.LimeGreen;

            var dlg = new System.Windows.Forms.ColorDialog
            {
                Color = System.Drawing.Color.FromArgb(
                    currentColor.A, currentColor.R, currentColor.G, currentColor.B),
                FullOpen = true,
                AnyColor = true
            };

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var picked = new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B));

            var prop = _vm.GetType().GetProperty(tag);
            prop?.SetValue(_vm, picked);

            btn.Background = picked;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
