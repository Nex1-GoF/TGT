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
        [ObservableProperty] private int speed;
        [ObservableProperty] private int altitude;
        [ObservableProperty] private double startLat;
        [ObservableProperty] private double startLon;
        [ObservableProperty] private double endLat;
        [ObservableProperty] private double endLon;

        private static char _nextId = 'A';

        [RelayCommand]
        private void AddTarget()
        {
            var target = new Target
            {
                Id = _nextId++,
                DetectedType = detectedType,
                Speed = speed,
                Altitude = altitude,
                CurLoc = (startLat, startLon),
                EndLoc = (endLat, endLon),
                DetectTime = null
            };

            TargetService.Instance.AddTarget(target);
        }
    }
}