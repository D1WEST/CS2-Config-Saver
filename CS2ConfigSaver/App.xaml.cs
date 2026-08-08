namespace CS2ConfigSaver
{
    using System;
    using System.Windows;
    using Application = System.Windows.Application;
    using Forms = System.Windows.Forms;

    public partial class App : Application
    {
        private Forms.NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;

        protected void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                // Отключаем аппаратное ускорение во избежание конфликта слоев AllowsTransparency
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

                _mainWindow = new MainWindow();

                _notifyIcon = new Forms.NotifyIcon();

                try
                {
                    // Безопасная попытка загрузки системной иконки
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
                catch
                {
                    // Если в ОС нет доступа к SystemIcons, используем пустую заглушку
                }

                _notifyIcon.Visible = true;
                _notifyIcon.MouseClick += NotifyIcon_MouseClick;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup Error:\n{ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NotifyIcon_MouseClick(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ToggleWindow();
            }
        }

        private void ToggleWindow()
        {
            if (_mainWindow == null) return;

            if (_mainWindow.IsVisible)
            {
                _mainWindow.Hide();
            }
            else
            {
                PositionWindow();
                _mainWindow.Show();
                _mainWindow.Activate();
            }
        }

        private void PositionWindow()
        {
            if (_mainWindow == null) return;

            var desktopWorkingArea = SystemParameters.WorkArea;

            _mainWindow.Left = desktopWorkingArea.Right - _mainWindow.Width - 10;
            _mainWindow.Top = desktopWorkingArea.Bottom - _mainWindow.Height - 10;
        }

        protected void OnExit(object sender, ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
            }
        }
    }
}