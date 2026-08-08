namespace CS2ConfigSaver
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Linq;

    public partial class HelpWindow : Window
    {
        private readonly MainWindow _parent;
        private readonly DispatcherTimer _tooltipTimer3;
        private readonly DispatcherTimer _tooltipTimer10;

        public HelpWindow(MainWindow parent)
        {
            try
            {
                InitializeComponent();
                _parent = parent;

                _tooltipTimer3 = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                _tooltipTimer3.Tick += (s, e) => {
                    CopiedTooltip.Visibility = Visibility.Collapsed;
                    _tooltipTimer3.Stop();
                };

                _tooltipTimer10 = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                _tooltipTimer10.Tick += (s, e) => {
                    CopiedTooltip10.Visibility = Visibility.Collapsed;
                    _tooltipTimer10.Stop();
                };

                ApplyLocalization();

                this.Deactivated += Window_Deactivated;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Help Window Init Error:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ApplyLocalization()
        {
            var loc = _parent.CurrentLocale;

            HelpTitleText.Text = loc.HelpTitle;

            TxtStep1.Text = loc.Step1;
            TxtStep2.Text = loc.Step2;
            TxtStep5.Text = loc.Step3_1;
            TxtStep3Cmd.Text = loc.Step3_Cmd;
            TxtStep6.Text = loc.Step4;
            TxtStep7.Text = loc.Step5;
            TxtStep8.Text = loc.Step6;
            TxtStep9.Text = loc.Step8;

            TxtStep10Part1.Text = loc.Step10Part1;
            TxtStep10Cmd.Text = loc.Step10Cmd;

            string runText = loc.Run;
            BtnRunStep1.Content = runText;
            BtnRunStep2.Content = runText;
            BtnRunStep3.Content = runText;
            BtnRunStep5.Content = runText;
            BtnRunStep6.Content = runText;
            BtnRunStep7.Content = runText;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CmdBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtStep3Cmd.Text);
                CopiedTooltip.Text = _parent.CurrentLocale.Copied;
                CopiedTooltip.Visibility = Visibility.Visible;
                _tooltipTimer3.Stop();
                _tooltipTimer3.Start();
            }
            catch { }
        }

        private void CmdStep10_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtStep10Cmd.Text);
                CopiedTooltip10.Text = _parent.CurrentLocale.Copied;
                CopiedTooltip10.Visibility = Visibility.Visible;
                _tooltipTimer10.Stop();
                _tooltipTimer10.Start();
            }
            catch { }
        }

        private void BtnRunStep1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "steam://run/730",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running Steam: {ex.Message}");
            }
        }

        private async void BtnRunStep2_Click(object sender, RoutedEventArgs e)
        {
            string cmd = "host_writeconfig my_backup_config";
            await _parent.SendCommandToCS2Async(cmd, BtnRunStep2);
        }

        private void BtnRunStep3_Click(object sender, RoutedEventArgs e)
        {
            _parent.FindSteamFolders_Click(sender, e);
        }

        private void BtnRunStep5_Click(object sender, RoutedEventArgs e)
        {
            _parent.BackupPathInput_MouseDoubleClick(sender, null!);
        }

        private void BtnRunStep6_Click(object sender, RoutedEventArgs e)
        {
            _parent.SaveConfigStart_Click(sender, e);
        }

        private async void BtnRunStep7_Click(object sender, RoutedEventArgs e)
        {
            await _parent.AddConfigAndAutomateAsync(BtnRunStep7);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Если сейчас открыто файловое или системное диалоговое окно, игнорируем потерю фокуса
                if (_parent != null && _parent.IsDialogOpen)
                {
                    return;
                }

                if (_parent != null && _parent.IsActive)
                {
                    return; // Фокус на главном окне, не закрываемся
                }

                if (Application.Current.Windows.Cast<Window>().Any(w => w != this && w.IsActive))
                {
                    return;
                }

                this.Close();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}