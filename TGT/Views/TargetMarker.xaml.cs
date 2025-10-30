using System.Windows.Controls;
using System.Windows.Media;
using TGT.Models;
using TGT.ViewModels;

namespace TGT.Views
{
    public partial class TargetMarker : UserControl
    {
        private readonly Target _target;

        public TargetMarker(Target target)
        {
            InitializeComponent();
            _target = target;
            DataContext = new TargetMarkerViewModel(target);
        }
    }


}
