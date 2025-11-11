using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TGT.Messages;
using TGT.Models;
using TGT.Services;
using TGT.Views;
using static TGT.Services.TargetService;


namespace TGT.ViewModels
{
    public partial class TargetCreationViewModel : ObservableObject
    {

        [ObservableProperty] private char detectedType;
        [ObservableProperty] private int speed = 3000;
        [ObservableProperty] private int altitude;
        [ObservableProperty] private double startLat = 36;
        [ObservableProperty] private double startLon = 127;
        [ObservableProperty] private double endLat = 37;
        [ObservableProperty] private double endLon = 127;

        private static char _nextId = '1';
        public TargetCreationViewModel()
        {
            DetectedType = 'A';
            WeakReferenceMessenger.Default.Register<MapClickMessage>(this, (r, msg) =>
            {
                var data = msg.Value;
                SetCommand(data.LatLng, data.IsFirst);
            }); 
            WeakReferenceMessenger.Default.Register<TargetListChangedMessage>(this, (r, m) =>
            {
                OnPropertyChanged(nameof(CanAdd));
            });
        }

        private void SetCommand(PointLatLng latLng , bool IsFirst)
        {
            if (IsFirst == true)
            {
                StartLat = latLng.Lat;
                StartLon = latLng.Lng;

            }
            else
            {
                EndLat = latLng.Lat;
                EndLon = latLng.Lng;
            }
        }
        public bool CanAdd => TargetService.Instance.Targets.Count < 4;

        [RelayCommand]
        private void AddTarget()
        {
            if (StartLat == 0 && StartLon == 0 && EndLat == 0 && EndLon == 0)
            {
                MessageBox.Show("시작/도착 위경도를 먼저 설정하세요.",
                                "표적 생성 불가",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }
            int count = TargetService.Instance.Targets.Count;

            // ✅ 이미 4개면 생성 불가
            if (count >= 4)
            {
                MessageBox.Show("표적은 최대 4개까지 생성할 수 있습니다.",
                                "표적 제한", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ✅ 등록 전 현재 남은 개수 알림
            int remain = 4 - count - 1; // 현재 생성 포함
            MessageBox.Show($"표적이 {count + 1}개 생성됩니다.\n" +
                            $"남은 생성 가능 개수: {remain}개",
                            "표적 생성", MessageBoxButton.OK, MessageBoxImage.Information);

            // ✅ 실제 타겟 생성
            CreateNewTarget();

            // ✅ 버튼 활성화/비활성화 갱신
            OnPropertyChanged(nameof(CanAdd));
        }
        private void CreateNewTarget()
        {
            // 너의 기존 생성 로직 그대로!
            double yawDeg = CalculateYaw(startLat, startLon, endLat, endLon);
            int yawInt = (int)(yawDeg * 100);

            var target = new Target
            {
                Id = _nextId++,
                DetectedType = detectedType,
                Speed = speed,
                Altitude = altitude,
                CurLoc = (startLat, startLon),
                EndLoc = (endLat, endLon),
                Yaw = yawInt
            };

            TargetService.Instance.AddTarget(target);

            // 리셋
            DetectedType = 'A';
            StartLat = 0;
            StartLon = 0;
            EndLat = 0;
            EndLon = 0;
        }

        private static double CalculateYaw(double lat1, double lon1, double lat2, double lon2)
        {
            double φ1 = ToRadians(lat1);
            double φ2 = ToRadians(lat2);
            double Δλ = ToRadians(lon2 - lon1);

            double y = Math.Sin(Δλ) * Math.Cos(φ2);
            double x = Math.Cos(φ1) * Math.Sin(φ2) -
                       Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(Δλ);

            double θ = Math.Atan2(y, x);          // 라디안
            double bearing = (ToDegrees(θ) + 360) % 360; // 0~360도
            return bearing;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180.0;
        private static double ToDegrees(double rad) => rad * 180.0 / Math.PI;
    }
}