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
        public char Id { get; set; }
        public char DetectedType { get; set; }
        public int Speed { get; set; }     // m/s
        public int Altitude { get; set; }  // m
        public int Yaw { get; set; }       // deg*100
        public (double Lat, double Lon) EndLoc { get; set; }
        public DateTime? DetectTime { get; set; }

        private (double Lat, double Lon) _curLoc;
        public (double Lat, double Lon) CurLoc
        {
            get => _curLoc;
            set
            {
                _curLoc = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurLocDisplay));
            }
        }

        public string CurLocDisplay => $"{CurLoc.Lat:F5}, {CurLoc.Lon:F5}";
        public string EndLocDisplay => $"{EndLoc.Lat:F5}, {EndLoc.Lon:F5}";

        private bool _isMoving;
        public bool IsMoving
        {
            get => _isMoving;
            set { _isMoving = value; OnPropertyChanged(); }
        }

        private bool _isDetected;

        public bool IsDetected {
            get => _isDetected;
            set { _isDetected = value; OnPropertyChanged(); }
        }

        // 태현 - 우선 Focused로 선택한 표적을 확인할 수 있도록 추가
        // 포커스 = 자동모드 일경우, 자동 모드로 수정
        private bool _isFocused; // 포커스 테스트용으로 추가
        public bool IsFocused
        {
            get => _isFocused;
            set { _isFocused = value; OnPropertyChanged(); }
        }

        public List<(double Lat, double Lon)> PathHistory { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}