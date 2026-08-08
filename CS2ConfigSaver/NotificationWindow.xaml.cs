namespace CS2ConfigSaver
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    public partial class NotificationWindow : Window
    {
        public NotificationWindow(string message, Window owner)
        {
            InitializeComponent();
            MsgText.Text = message;
            this.Owner = owner;

            this.Left = owner.Left + (owner.Width - this.Width) / 2;
            this.Top = owner.Top + (owner.Height - this.Height) / 2;

            this.Loaded += NotificationWindow_Loaded;
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                BeginTime = TimeSpan.FromSeconds(1.2),
                Duration = TimeSpan.FromSeconds(0.4)
            };

            fadeAnimation.Completed += (s, ev) => this.Close();
            this.BeginAnimation(Window.OpacityProperty, fadeAnimation);
        }
    }
}