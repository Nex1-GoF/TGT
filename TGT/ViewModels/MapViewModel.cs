using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using TGT.Models;
using TGT.Services;

namespace TGT.ViewModels
{
    public class MapViewModel
    {
        private readonly TargetService _targetService;
        public ObservableCollection<GMapMarker> TargetMarkers { get; } = new();

        public MapViewModel()
        {
            _targetService = TargetService.Instance;

            // 새 표적이 추가될 때마다 마커 생성
            _targetService.Targets.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Target t in e.NewItems)
                        AddOrUpdateMarker(t);
                }
            };

            // 위치 갱신용 타이머 (10Hz)
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, e) => UpdateMarkerPositions();
            timer.Start();
        }

        private void AddOrUpdateMarker(Target target)
        {
            var marker = new GMapMarker(new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon))
            {
                Shape = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                },
                Tag = target.Id
            };

            TargetMarkers.Add(marker);
        }

        private void UpdateMarkerPositions()
        {
            foreach (var marker in TargetMarkers)
            {
                var id = marker.Tag?.ToString();
                var target = _targetService.Targets.FirstOrDefault(t => t.Id.ToString() == id);
                if (target != null)
                {
                    marker.Position = new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon);
                }
            }
        }
    }
}
