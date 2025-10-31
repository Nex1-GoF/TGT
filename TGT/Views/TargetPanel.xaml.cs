using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TGT.Services;
using TGT.ViewModels;

namespace TGT.Views
{
    public partial class TargetPanel : UserControl
    {
        public TargetPanel()
        {
            InitializeComponent();
            DataContext = new TargetViewModel(TargetService.Instance);
        }
    }
}