using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TGT.Models
{
    public class Target : INotifyPropertyChanged
    {
        // Target.cs 내부
        public double SimPosX { get; set; }  // 기준점 기준 동쪽 방향 (m)
        public double SimPosY { get; set; }  // 기준점 기준 북쪽 방향 (m)
        public char Id { get; set; }
        public char DetectedType { get; set; }
        public int Speed { get; set; }     // m/s
        public int Altitude { get; set; }  // mprivate int _yaw;
        private int _yaw;
        public int Yaw
        {
            get => _yaw;
            set
            {
                int normalized = NormalizeYawRaw(value);

                _yaw = normalized;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurYawDisplay));
            }
        }
        public (double Lat, double Lon) EndLoc { get; set; }
        public DateTime? DetectTime { get; set; }
        public string ScenarioId { get; set; } = string.Empty;
        public bool ScenarioRunning { get; set; }

        private (double Lat, double Lon) _curLoc;
        public (double Lat, double Lon) CurLoc
        {
            get => _curLoc;
            set
            {
                _curLoc = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurLatDisplay));
                OnPropertyChanged(nameof(CurLonDisplay));
                OnPropertyChanged(nameof(CurLocDisplay));
            }
        }
        public string CurYawDisplay => $"{(double)Yaw / 100.0:F3}";
        public string CurLocDisplay => $"{CurLoc.Lat:F3}, {CurLoc.Lon:F3}";
        public string EndLocDisplay => $"{EndLoc.Lat:F3}, {EndLoc.Lon:F3}";

        public string CurLatDisplay => $"{CurLoc.Lat:F3}";
        public string CurLonDisplay => $"{CurLoc.Lon:F3}";

        private bool _isMoving;
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                if (_isMoving != value) // 값이 변경될 때만 갱신
                {
                    _isMoving = value;
                    OnPropertyChanged(nameof(IsMoving)); // 1. IsMoving 갱신 알림
                    OnPropertyChanged(nameof(CanStart)); // 2. CanStart 갱신 알림 (필수!)
                }
            }
        }
        public bool CanStart => !IsMoving;

        private bool _isDetected;

        public bool IsDetected {
            get => _isDetected;
            set { _isDetected = value; OnPropertyChanged(); }
        }

        // Todo: 시나리오 모드인지 아닌지 추가


        private int NormalizeYawRaw(int raw)
        {
            // raw = deg * 100
            double deg = raw / 100.0;

            // 0~360 범위로 정규화
            double norm = (deg % 360.0 + 360.0) % 360.0;

            return (int)Math.Round(norm * 100);
        }

        public List<(double Lat, double Lon)> PathHistory { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}