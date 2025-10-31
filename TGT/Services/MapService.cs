using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TGT.Models;
using TGT.Services;
using TGT.ViewModels;

namespace TGT.Services
{
    public class MapService
    {
        private static MapService _instance;
        public static MapService Instance => _instance ??= new MapService();

        private GMapControl? _map;
        public GMapControl? Map => _map;

        public PointLatLng Center { get; set; } = new PointLatLng(37.5665, 126.9780);
        public double Distance { get; set; } = 250_000;

        // 🔹 실제 지도에 표시될 마커/경로 컬렉션
        public ObservableCollection<GMapMarker> TargetMarkers { get; } = new();
        public ObservableCollection<GMapRoute> TargetRoutes { get; } = new();

        private readonly TargetService _targetService = TargetService.Instance;

        private MapService() { }

        // ============================================================
        // ✅ GMapControl 초기화
        // ============================================================
        public void Initialize(GMapControl map)
        {
            _map = map;
        }

        // ============================================================
        // ✅ 마커 추가 or 갱신
        // ============================================================
        public void AddOrUpdateMarker(Target target)
        {
            var existing = TargetMarkers.FirstOrDefault(m => (string)m.Tag == target.Id.ToString());
            if (existing != null)
            {
                existing.Position = new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon);
                return;
            }

            var marker = CreateMarker(target);
            TargetMarkers.Add(marker);
        }

        // ============================================================
        // ✅ 마커 생성
        // ============================================================
        private GMapMarker CreateMarker(Target target)
        {
            var triangle = new Path
            {
                Data = Geometry.Parse("M 0,-15 L 10,15 L -10,15 Z"),
                Stroke = Brushes.Black,
                StrokeThickness = 1.2,
                Fill = new SolidColorBrush(Colors.Red),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(target.Yaw / 100.0),
                Cursor = Cursors.Hand,
                IsHitTestVisible = true
            };

            triangle.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true;
                _targetService.SelectTarget(target);
                Console.WriteLine($"[MapService] Target {target.Id} clicked");
            };

            target.PropertyChanged += (s, e) =>
            {
                //if (e.PropertyName == nameof(Target.IsFocused))
                //{
                //    var brush = (SolidColorBrush)triangle.Fill;
                //    brush.Color = target.IsFocused ? Colors.Yellow : Colors.Red;
                //}
                //else if (e.PropertyName == nameof(Target.Yaw))
                //{
                //    if (triangle.RenderTransform is RotateTransform rot)
                //        rot.Angle = target.Yaw / 100.0;
                //}
            };

            return new GMapMarker(new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon))
            {
                Shape = triangle,
                Offset = new Point(-20, -20),
                Tag = target.Id.ToString()
            };
        }

        // ============================================================
        // ✅ 마커 위치 업데이트
        // ============================================================
        public void UpdateTargetPosition(string targetId, double lat, double lon)
        {
            var marker = TargetMarkers.FirstOrDefault(m => (string)m.Tag == targetId);
            if (marker != null)
                marker.Position = new PointLatLng(lat, lon);
        }

        // ============================================================
        // ✅ 마커 제거
        // ============================================================
        public void RemoveMarker(string targetId)
        {
            var marker = TargetMarkers.FirstOrDefault(m => (string)m.Tag == targetId);
            if (marker != null)
                TargetMarkers.Remove(marker);
        }

        // ============================================================
        // ✅ 경로(선분) 추가
        // ============================================================
        public void AddSegmentToRoute(string id, PointLatLng from, PointLatLng to)
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

        // ============================================================
        // ✅ 전체 초기화
        // ============================================================
        public void ClearAll()
        {
            TargetMarkers.Clear();
            TargetRoutes.Clear();
        }
    }
}
