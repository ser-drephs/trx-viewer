using System.ComponentModel;
using System.Windows;
using Trx.Viewer.Ui.Properties;

namespace Trx.Viewer.Ui.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Settings MainWindowSettings = Settings.Default;

        public MainWindow()
        {
            InitializeComponent();
            RestoreWindowLocations();
        }

        void RestoreWindowLocations()
        {
            if (MainWindowSettings.WindowWidth > 0) this.Width = MainWindowSettings.WindowWidth;
            if (MainWindowSettings.WindowHeight > 0) this.Height = MainWindowSettings.WindowHeight;
            if (MainWindowSettings.WindowLocationX > 0) this.Top = MainWindowSettings.WindowLocationX;
            if (MainWindowSettings.WindowLocationY > 0) this.Left = MainWindowSettings.WindowLocationY;
            if (MainWindowSettings.WindowMaximizedState) this.WindowState = WindowState.Maximized;
        }

        void App_Closing(object sender, CancelEventArgs e)
        {
            MainWindowSettings.WindowMaximizedState = this.WindowState == WindowState.Maximized;
            // Only save new size and location setting if the window was not in maximized state.
            // This keeps the location throughout the maximized state.
            if (this.WindowState != WindowState.Maximized)
            {
                MainWindowSettings.WindowHeight = this.ActualHeight;
                MainWindowSettings.WindowWidth = this.ActualWidth;
                MainWindowSettings.WindowLocationX = this.Top;
                MainWindowSettings.WindowLocationY = this.Left;
            }
            MainWindowSettings.Save();
        }
    }
}
