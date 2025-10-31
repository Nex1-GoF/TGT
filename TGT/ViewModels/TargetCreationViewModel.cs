using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGT.Models;
using TGT.Services;


namespace TGT.ViewModels
{
    public partial class TargetCreationViewModel : ObservableObject
    {
        [ObservableProperty] private char detectedType;
        [ObservableProperty] private int speed = 3000;
        [ObservableProperty] private int altitude;
        [ObservableProperty] private double startLat = 36;
        [ObservableProperty] private double startLon = 125;
        [ObservableProperty] private double endLat = 37;
        [ObservableProperty] private double endLon = 127;

        private static char _nextId = 'A';

        [RelayCommand]
        private void AddTarget()
        {
            // 태현 - 출발 도착 위/경도로 초기 Yaw 도출하는 기능 추가 (마커가 방향 표시 제대로 하는지 확인하기 위해)
            double yawDeg = CalculateYaw(startLat, startLon, endLat, endLon); // 초기 Yaw 계산
            int yawInt = (int)(yawDeg * 100); // deg × 100 형식

            var target = new Target
            {
                Id = _nextId++,
                DetectedType = detectedType,
                Speed = speed,
                Altitude = altitude,
                CurLoc = (startLat, startLon),
                EndLoc = (endLat, endLon),
                Yaw = yawInt,
                IsFocused = false, 
                DetectTime = null,
                IsDetected = false
            };

            TargetService.Instance.AddTarget(target);
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