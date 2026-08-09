namespace CS2ConfigSaver
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public partial class NotificationWindow : Window
    {
        public NotificationWindow(string message, NotificationType type, Window owner)
        {
            InitializeComponent();
            MsgText.Text = message;
            this.Owner = owner;

            this.Left = owner.Left + (owner.Width - this.Width) / 2;
            this.Top = owner.Top + (owner.Height - this.Height) / 2;

            ApplyTheme(type);

            this.Loaded += NotificationWindow_Loaded;
        }

        private void ApplyTheme(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success:
                    NotificationBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 102)); // Green
                    IconText.Text = "✅";
                    break;
                case NotificationType.Error:
                    NotificationBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 74, 74)); // Red
                    IconText.Text = "❌";
                    break;
                case NotificationType.Info:
                    NotificationBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(59, 130, 246)); // Blue
                    IconText.Text = "ℹ️";
                    break;
            }
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                BeginTime = TimeSpan.FromSeconds(1.8),
                Duration = TimeSpan.FromSeconds(0.4)
            };

            fadeAnimation.Completed += (s, ev) => this.Close();
            this.BeginAnimation(Window.OpacityProperty, fadeAnimation);
        }
    }
}