using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TGT.Views
{
    /// <summary>
    /// TargetMarker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TargetMarker : UserControl
    {
        private static readonly Brush UnfocusedFill = new SolidColorBrush(Color.FromRgb(255, 80, 80));
        private static readonly Brush FocusedFill = new SolidColorBrush(Colors.Yellow);
        public char TargetId { get; }
        public TargetMarker(char targetId)
        {
            InitializeComponent();
            TargetId = targetId;

            IdLabel.Text = $"TGT-00{targetId}";
            SetFocused(false);
        }

        public void SetYaw(double yaw)
        {
            Rt.Angle = yaw;
        }

        public void SetFocused(bool focused)
        {
            if (focused)
            {
                TargetRect.Fill = FocusedFill;
                IdLabel.Foreground = FocusedFill;
                return;
            }
            TargetRect.Fill = UnfocusedFill;
            IdLabel.Foreground = UnfocusedFill;
        }

        public void SetVisible(bool isVisible)
        {
            this.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
