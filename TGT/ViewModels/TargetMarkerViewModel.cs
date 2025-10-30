using System.ComponentModel;
using System.Windows.Media;
using TGT.Models;

namespace TGT.ViewModels
{
    public class TargetMarkerViewModel : INotifyPropertyChanged
    {
        private readonly Target _target;
        private readonly SolidColorBrush _fillBrush = new(Colors.Red);

        public TargetMarkerViewModel(Target target)
        {
            _target = target;
            _target.PropertyChanged += OnTargetPropertyChanged;
            UpdateFill();
        }

        public double YawAngle => _target.Yaw / 100.0;
        public SolidColorBrush FillBrush => _fillBrush;

        private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Target.IsFocused))
                UpdateFill();
            else if (e.PropertyName == nameof(Target.Yaw))
                OnPropertyChanged(nameof(YawAngle));
        }

        private void UpdateFill()
        {
            _fillBrush.Color = _target.IsFocused ? Colors.Yellow : Colors.Red;
            OnPropertyChanged(nameof(FillBrush)); // 색상 갱신 통보
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
