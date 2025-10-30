using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Shapes;
using TGT.Messages;
using TGT.Models;
using TGT.Services;

namespace TGT.ViewModels
{
    public class MapViewModel
    {
        private readonly MapService _mapService = MapService.Instance;
        private readonly TargetService _targetService = TargetService.Instance;

        public ObservableCollection<GMapMarker> TargetMarkers { get; } = new();

        // ✅ Behavior에서 바인딩할 속성 추가
        public PointLatLng Center => _mapService.Center;
        public double DetectionRadius => _mapService.Distance;

        public MapViewModel()
        {
            // 기존 표적 데이터 등록
            foreach (var target in _targetService.Targets)
                AddOrUpdateMarker(target);

            // 새 표적 생성 감지
            _targetService.Targets.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<Target>())
                        AddOrUpdateMarker(item);
                }
            };

            // 표적 위치 업데이트 시 지도에 반영
            WeakReferenceMessenger.Default.Register<TargetUpdateMessage>(this, (r, msg) =>
            {
                var data = msg.Value;
                var marker = TargetMarkers.FirstOrDefault(m => (string)m.Tag == data.TargetId);
                if (marker != null)
                {
                    marker.Position = new PointLatLng(data.Latitude, data.Longitude);
                }
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
                Tag = target.Id.ToString()
            };
            TargetMarkers.Add(marker);
        }
    }
}
