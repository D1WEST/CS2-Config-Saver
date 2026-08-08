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
                System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

                _mainWindow = new MainWindow();
                _notifyIcon = new Forms.NotifyIcon();

                try
                {
                    // Загружаем PNG из встроенных ресурсов утилиты
                    var iconUri = new Uri("pack://application:,,,/Resources/app.png");
                    var streamInfo = Application.GetResourceStream(iconUri);
                    if (streamInfo != null)
                    {
                        // Читаем поток как Bitmap (стандартное представление PNG в .NET)
                        using (var bitmap = new System.Drawing.Bitmap(streamInfo.Stream))
                        {
                            // Получаем Win32 дескриптор иконки из Bitmap
                            IntPtr hIcon = bitmap.GetHicon();
                            _notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
                        }
                    }
                    else
                    {
                        // Если файл не найден, используем стандартный значок Windows
                        _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                    }
                }
                catch
                {
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
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