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
using TGT.Views;

namespace TGT.ViewModels
{
    public class MapViewModel
    {
        private readonly MapService _mapService = MapService.Instance;
        private readonly TargetService _targetService = TargetService.Instance;

        public ObservableCollection<GMapMarker> TargetMarkers { get; } = new();
        public ObservableCollection<GMapRoute> TargetRoutes { get; } = new();

        public PointLatLng Center => _mapService.Center;
        public double DetectionRadius => _mapService.Distance;

        public MapViewModel()
        {
            // ✅ 기존 표적 데이터 등록
            foreach (var target in _targetService.Targets)
                AddOrUpdateMarker(target);

            // ✅ 새 표적 생성 감지
            _targetService.Targets.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<Target>())
                        AddOrUpdateMarker(item);
                }
            };

            // ✅ 표적 위치 업데이트 메시지 수신
            WeakReferenceMessenger.Default.Register<TargetUpdateMessage>(this, (r, msg) =>
            {
                var data = msg.Value;

                // ① 마커 위치 갱신
                UpdateTargetPosition(data.TargetId, data.To.Lat, data.To.Lng);

                // ② 새 선분(From–To) 추가
                AddSegmentToRoute(data.TargetId, data.From, data.To);
            });
        }

        // 🔸 표적 마커 추가 또는 위치 갱신
        private void AddOrUpdateMarker(Target target)
        {
            var existing = TargetMarkers.FirstOrDefault(m => (string)m.Tag == target.Id.ToString());
            if (existing != null)
            {
                existing.Position = new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon);
                return;
            }

            var marker = new GMapMarker(new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon))
            {
                Shape = new TargetMarker(target),
                Offset = new Point(-20, -20),
                Tag = target.Id.ToString()
            };
            TargetMarkers.Add(marker);
        }

        // 🔸 마커 위치 갱신
        private void UpdateTargetPosition(string targetId, double lat, double lon)
        {
            var marker = TargetMarkers.FirstOrDefault(m => (string)m.Tag == targetId);
            if (marker != null)
                marker.Position = new PointLatLng(lat, lon);
        }

        // 🔸 From–To 기반으로 선분(GMapRoute) 추가
        private void AddSegmentToRoute(string id, PointLatLng from, PointLatLng to)
        {
            var segment = new GMapRoute(new List<PointLatLng> { from, to })
            {
                Shape = new Path
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 3,
                    Opacity = 0.8
                },
                Tag = $"SEG-{id}-{Guid.NewGuid()}"
            };

            TargetRoutes.Add(segment);
        }
    }
}
