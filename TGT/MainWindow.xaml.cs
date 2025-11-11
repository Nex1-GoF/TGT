using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TGT.Network;

namespace TGT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ReceiveExit _receiveExit;
        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += OnPreviewKeyDown;
            _receiveExit = new ReceiveExit(50000);
            _receiveExit.Start();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 중앙 이벤트 전달
            InputEventBroker.RaiseKeyInput(e.Key);
        }
    }
}