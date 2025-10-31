using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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
        private readonly MapService _mapService = MapService.Instance;

        public ObservableCollection<GMapMarker> TargetMarkers { get; } = new();
        public ObservableCollection<GMapRoute> TargetRoutes  { get; } = new();
        public GMapPolygon? CirclePolygon { get; private set; }

        private GMapControl _map;
        private GMapMarker? _startMarker;
        private GMapMarker? _endMarker;
        private bool _isStart = true;

        // Circle & Radius => Service 계층과 Behavior에서 관리하고, 뷰모델에서는 관리하지 않음
        // 이유: Service 계층에는 범위 판단을 위해 필요하지만, 앱 실행 중 바뀔 일이 없기 때문에

        public MapViewModel(GMapControl map)
        {
            _map = map;
            InitializeMap();
            WeakReferenceMessenger.Default.Register<TargetBatchUpdateMessage>(this, (r, msg) =>
            {
                var targetList = msg.Value; // ✅ 이제 List<TargetUpdateData>

                foreach (var data in targetList)
                {
                    UpdateTargetMarker(data.TargetId, data.To.Lat, data.To.Lng);
                    AddSegmentToRoute(data.TargetId, data.From, data.To);
                }
            });

            WeakReferenceMessenger.Default.Register<TargetRemoveMessage>(this, (r, msg) =>
            {
                var data = msg.Value;
                RemoveTargetMarker(data.TargetId);
            });
            WeakReferenceMessenger.Default.Register<TargetAddMessage>(this, (r, msg) =>
            {
                var data = msg.Value;
                CreateTargetMarker(data.Target);
            });
            WeakReferenceMessenger.Default.Register<TargetSelectMessage>(this, (r, msg) =>
            {
                var data = msg.Value;
                SelectTargetMarker(data.TargetId);
            });
        }

        private void InitializeMap()
        {
            _mapService.Initialize(_map);

            CirclePolygon = _mapService.DrawDetectionCircle();

            RefreshAll();
        }


        public void CreateTargetMarker(Target target)
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
                //e.Handled = true;
                _targetService.SelectTarget(target);
            };

            var newMarker = new GMapMarker(new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon))
            {
                Shape = triangle,
                Offset = new Point(-20, -20),
                Tag = $"TGT-{target.Id}",
            };

            TargetMarkers.Add(newMarker);

            // 경로마커 제거용
            _startMarker = null;
            _endMarker = null;
            _isStart = true;

            RefreshAll();
        }
        public void SelectTargetMarker(string targetId)
        { 
            foreach(GMapMarker marker in TargetMarkers){

                
                if (marker == null) continue;

                if (marker.Shape is Path path)
                {
                    if((string)marker.Tag == $"TGT-{targetId}")
                        path.Fill = new SolidColorBrush(Colors.Yellow);
                    else
                        path.Fill = new SolidColorBrush(Colors.Red);

                }

            }
            RefreshAll();
        }

        public void PressTargetMarker(string targetId)
        {
            var target = _targetService.Targets.FirstOrDefault(m => m.Id.ToString() == targetId);
            if (target == null) return;
            _targetService.SelectTarget(target);
        }

        private void UpdateTargetMarker(string targetId, double lat, double lon)
        {
            var marker = TargetMarkers.FirstOrDefault(m => (string)m.Tag == $"TGT-{targetId}");
            var target = _targetService.Targets.FirstOrDefault(m => m.Id.ToString() == targetId);

            if (marker != null && target != null)
            {
                marker.Position = new PointLatLng(lat, lon);
                marker.Shape.RenderTransform = new RotateTransform(target.Yaw / 100.0);
                RefreshAll();
            }
        }

        private void RemoveTargetMarker(string targetId)
        {
            var marker = TargetMarkers.FirstOrDefault(m => (string)m.Tag == $"TGT-{targetId}");
            if (marker != null)
            {
                TargetMarkers.Remove(marker);
                RefreshAll();
            }
        }

        public void SetPosition(PointLatLng latLng)
        {
            if (_isStart)
            {
                _startMarker = new GMapMarker(latLng)
                {
                    Shape = new Ellipse
                    {
                        Width = 12,
                        Height = 12,
                        Fill = Brushes.LimeGreen,
                        Stroke = Brushes.DarkGreen,
                        StrokeThickness = 1.2,
                        Opacity = 0.9
                    },
                    Offset = new Point(-6, -6), // 중심 정렬
                    Tag = "START"
                };
            }
            else
            {
                _endMarker = new GMapMarker(latLng)
                {
                    Shape = new Ellipse
                    {
                        Width = 12,
                        Height = 12,
                        Fill = Brushes.Red,
                        Stroke = Brushes.DarkRed,
                        StrokeThickness = 1.2,
                        Opacity = 0.9
                    },
                    Offset = new Point(-6, -6),
                    Tag = "END"
                };
            }
            // Viewmodel <-> Viewmodel 끼리 어쩔 수 없이 상호작용 해야함 (지도 업데이트와 표적 업데이트를 같이해야하기 때문에)
            WeakReferenceMessenger.Default.Send(new MapClickMessage(new MapClickData(latLng, _isStart)));
            _isStart = !_isStart;
            RefreshAll();
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

        public void RefreshAll()
        {
            if (_map == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _map.Markers.Clear();

                // 표적 마커 추가
                foreach (var m in TargetMarkers)
                    _map.Markers.Add(m);

                // 경로 추가
                foreach (var r in TargetRoutes)
                    _map.Markers.Add(r);

                // 탐지 원
                if (CirclePolygon != null)
                    _map.Markers.Add(CirclePolygon);

                // 경로 폴리곤
                if(_startMarker != null)
                    _map.Markers.Add(_startMarker);

                if(_endMarker != null)
                    _map.Markers.Add(_endMarker);

                _map.InvalidateVisual();
            });
        }

    }
}
