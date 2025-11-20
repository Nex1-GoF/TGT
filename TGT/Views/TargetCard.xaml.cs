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
using TGT.Models;
using TGT.ViewModels;

namespace TGT.Views
{
    /// <summary>
    /// TargetCard.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TargetCard : UserControl
    {
        public TargetCard()
        {
            InitializeComponent();
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 필요하다면 코드 추가
            // 지금은 XAML 에러만 해결하기 위해 비워둬도 됨
            var target = DataContext as Target;
            if (target == null)
                return;

            // 부모 UserControl(TargetPanel)에서 ViewModel을 가져오기
            var viewModel = FindParentViewModel();

            if (viewModel != null)
            {
                viewModel.SelectTargetCommand.Execute(target);
            }
        }
        private TargetViewModel? FindParentViewModel()
        {
            DependencyObject parent = this;

            while (parent != null)
            {
                if (parent is FrameworkElement fe && fe.DataContext is TargetViewModel vm)
                    return vm;

                parent = LogicalTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class TargetFocusedToBackgroundConverter : IMultiValueConverter
    {
        // values[0] = 현재 카드의 Target
        // values[1] = ViewModel.SelectedTarget
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Brushes.Transparent;

            var current = values[0] as Target;
            var selected = values[1] as Target;

            // ViewModel SelectedTarget이 null이거나 Current가 null이면 기본색
            if (current == null || selected == null)
                return (Brush)App.Current.Resources["TargetCardBrush"];

            // 같은 Target인가?
            bool isSelected = current.Id == selected.Id;

            if (isSelected)
            {
                // 선택된 카드 강조 배경
                return (Brush)App.Current.Resources["TargetCardFocusedHeaderBrush"];
            }
            else
            {
                // 일반 배경
                return (Brush)App.Current.Resources["TargetCardBrush"];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
