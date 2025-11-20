using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    /// TargetCreationPanel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TargetCreationPanel : UserControl
    {
        public TargetCreationPanel()
        {
            InitializeComponent();
        }
    }

    public class CharToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is char c && parameter is string p && p.Length == 1)
            {
                return c == p[0];
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 라디오 버튼에서 체크(true)된 경우 VM 값 변경
            if (value is bool b && b && parameter is string p && p.Length == 1)
            {
                return p[0];
            }

            // 체크 해제된 라디오는 무시
            return Binding.DoNothing;
        }
    }
}
