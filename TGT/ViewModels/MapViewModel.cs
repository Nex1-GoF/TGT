using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using TGT.Messages;
using TGT.Models;
using TGT.Services;

namespace TGT.ViewModels
{
    public class MapViewModel
    {
        private readonly TargetService _targetService = TargetService.Instance;

        public ObservableCollection<GMapMarker> TargetMarkers { get; } = new();
        public ObservableCollection<GMapRoute> TargetRoutes { get; } = new();
        
        // Circle & Radius => Service 계층과 Behavior에서 관리하고, 뷰모델에서는 관리하지 않음
        // 이유: Service 계층에는 범위 판단을 위해 필요하지만, 앱 실행 중 바뀔 일이 없기 때문에

        public MapViewModel()
        {
            foreach (var target in _targetService.Targets)
                AddOrUpdateMarker(target);

            _targetService.Targets.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (var item in e.NewItems.OfType<Target>())
                        AddOrUpdateMarker(item);
            };

            WeakReferenceMessenger.Default.Register<TargetUpdateMessage>(this, (r, msg) =>
            {
                var data = msg.Value;
                UpdateTargetPosition(data.TargetId, data.To.Lat, data.To.Lng);
                AddSegmentToRoute(data.TargetId, data.From, data.To);
            });
        }

        private void AddOrUpdateMarker(Target target)
        {
            var existing = TargetMarkers.FirstOrDefault(m => (string)m.Tag == target.Id.ToString());
            if (existing != null)
            {
                existing.Position = new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon);
                return;
            }

            // 🔺 마커 생성 (TargetMarker.xaml 대체)
            var triangle = new Path
            {
                Data = Geometry.Parse("M 0,-15 L 10,15 L -10,15 Z"),
                Stroke = Brushes.Black,
                StrokeThickness = 1.2,
                Fill = new SolidColorBrush(Colors.Red),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(target.Yaw / 100.0)
            };

            // 실시간 색/회전 반영
            target.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Target.IsFocused))
                {
                    var brush = (SolidColorBrush)triangle.Fill;
                    brush.Color = target.IsFocused ? Colors.Yellow : Colors.Red;
                }
                else if (e.PropertyName == nameof(Target.Yaw))
                {
                    if (triangle.RenderTransform is RotateTransform rot)
                        rot.Angle = target.Yaw / 100.0;
                }
            };

            var marker = new GMapMarker(new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon))
            {
                Shape = triangle,
                Offset = new Point(-20, -20),
                Tag = target.Id.ToString()
            };

            TargetMarkers.Add(marker);
        }

        private void UpdateTargetPosition(string targetId, double lat, double lon)
        {
            var marker = TargetMarkers.FirstOrDefault(m => (string)m.Tag == targetId);
            if (marker != null)
                marker.Position = new PointLatLng(lat, lon);
        }

        private void AddSegmentToRoute(string id, PointLatLng from, PointLatLng to)
        {
            var route = new GMapRoute(new List<PointLatLng> { from, to })
            {
                Shape = new Path
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Opacity = 0.8
                },
                Tag = $"SEG-{id}-{Guid.NewGuid()}"
            };

            TargetRoutes.Add(route);
        }
    }
}
