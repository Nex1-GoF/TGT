using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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
        private readonly TargetService _targetService = TargetService.Instance;
        private readonly MapService _mapService = MapService.Instance;
        private readonly UpdateDispatcher _updateDispatcher = UpdateDispatcher.Instance;

        public ObservableCollection<GMapRoute> TargetRoutes { get; } = new();

        private GMapControl _map;
        //public GMapPolygon? CirclePolygon { get; private set; }
        //private GMapMarker? _startMarker;
        //private GMapMarker? _endMarker;
        Dictionary<string, GMapMarker> _customMarkers = new();
        Dictionary<string, List<GMapRoute>> _routes = new();
        private bool _isStart = true;

        // Circle & Radius => Service 계층과 Behavior에서 관리하고, 뷰모델에서는 관리하지 않음
        // 이유: Service 계층에는 범위 판단을 위해 필요하지만, 앱 실행 중 바뀔 일이 없기 때문에

        public MapViewModel(GMapControl map)
        {
            _map = map;
            InitializeMap();

            WeakReferenceMessenger.Default.Register<TargetRemoveMessage>(this, (r, msg) =>
            {
                var data = msg.Value;
                RemoveCustomMarker($"TGT-{data.TargetId}");
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

            _updateDispatcher.Register(Update);
        }
        ~MapViewModel()
        {
            _updateDispatcher.Unregister(Update);
        }

        private void InitializeMap()
        {
            _mapService.Initialize(_map);

            _map.Markers.Add(_mapService.DrawDetectionCircle());
        }

        private void Update()
        {
            var targetList = _targetService.Targets.ToList(); // ✅ 이제 List<TargetUpdateData>

            foreach (var data in targetList)
            {
                var curPosition = new PointLatLng(data.CurLoc.Lat, data.CurLoc.Lon);
                PointLatLng updatedPosition = (PointLatLng)UpdateTargetMarker(data.Id, data.CurLoc.Lat, data.CurLoc.Lon);
                AddSegmentToRoute(data.Id, curPosition, updatedPosition);
            }
        }


        public void RemoveCustomMarker(string key)
        {
            if (_customMarkers.TryGetValue(key, out var marker))
            {
                if (_map.Markers.Contains(marker))
                    _map.Markers.Remove(marker);
                _customMarkers.Remove(key);
            }
        }

        public void CreateTargetMarker(Target target)
        {
            // ✅ START / END 둘 다 제거 (경로 설정 중이면 초기화)
            RemoveCustomMarker("START");
            RemoveCustomMarker("END");

            string key = $"TGT-{target.Id}";

            //// ✅ 이미 같은 ID의 표적 마커가 존재하면 위치 업데이트만 수행
            //if (_customMarkers.TryGetValue(key, out var existingMarker))
            //{
            //    existingMarker.Position = new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon);
            //    if (existingMarker.Shape is Path p)
            //        p.RenderTransform = new RotateTransform(target.Yaw / 100.0);
            //    return;
            //}

            // ✅ 새 마커 생성
            var triangle = new Path
            {
                Data = Geometry.Parse("M 0,-15 L 10,15 L -10,15 Z"),
                Stroke = Brushes.Black,
                StrokeThickness = 1.2,
                Fill = Brushes.Red,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(target.Yaw / 100.0),
                Cursor = Cursors.Hand,
                IsHitTestVisible = true
            };

            triangle.MouseLeftButtonUp += (s, e) =>
            {
                _targetService.SelectTarget(target);
            };

            var shape = new TargetMarker(target.Id);   // 너가 만든 픽토그램 Shape
            var marker = new GMapMarker(new PointLatLng(target.CurLoc.Lat, target.CurLoc.Lon))
            {
                Shape = shape,
                Offset = new System.Windows.Point(-20, -20), // 중심 정렬
                Tag = key
            };

            // ✅ 지도에 등록
            if (!_map.Markers.Contains(marker))
                _map.Markers.Add(marker);

            // ✅ 딕셔너리에 저장
            _customMarkers[key] = marker;

            // ✅ 다음 클릭은 START부터
            _isStart = true;
        }

        public void SelectTargetMarker(string targetId)
        {
            string selectedKey = $"TGT-{targetId}";

            foreach (var kvp in _customMarkers)
            {
                var key = kvp.Key;
                var marker = kvp.Value;

                // 표적 마커만 대상으로 (START/END는 무시)
                if (!key.StartsWith("TGT-"))
                    continue;

                if (marker.Shape is Path path)
                {
                    // 선택된 마커
                    if (key == selectedKey)
                        path.Fill = new SolidColorBrush(Colors.Yellow);
                    else
                        path.Fill = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public void PressTargetMarker(string targetId)
        {
            var target = _targetService.Targets.FirstOrDefault(m => m.Id.ToString() == targetId);
            if (target == null) return;
            _targetService.SelectTarget(target);
        }

        private PointLatLng UpdateTargetMarker(char targetId, double lat, double lon)
        {
            string key = $"TGT-{targetId}";
            var previousPosition = new PointLatLng(0, 0);

            if (_customMarkers.TryGetValue(key, out var marker))
            {
                previousPosition = marker.Position;
                marker.Position = new PointLatLng(lat, lon);

                // (선택) Shape 회전 등 추가 업데이트 가능
                if (marker.Shape is TargetMarker shape && _targetService.Targets.FirstOrDefault(t => t.Id == targetId) is Target tgt)
                {
                    shape.SetYaw(tgt.Yaw / 100.0);
                }
            }
            else
            {
                Debug.WriteLine($"[MapViewModel] Target marker not found: {key}");
            }

            return previousPosition;
        }

        public void SetPosition(PointLatLng latLng)
        {
            string key = _isStart ? "START" : "END";

            // 1️⃣ START/END 마커 존재 확인
            if (!_customMarkers.TryGetValue(key, out var marker))
            {
                // 2️⃣ 없으면 새로 생성
                var newMarker = new GMapMarker(latLng)
                {
                    Shape = new Ellipse
                    {
                        Width = 12,
                        Height = 12,
                        Fill = _isStart ? Brushes.LimeGreen : Brushes.Red,
                        Stroke = _isStart ? Brushes.DarkGreen : Brushes.DarkRed,
                        StrokeThickness = 1.2,
                        Opacity = 0.9
                    },
                    Offset = new Point(-10, -15),
                    Tag = key
                };

                // 지도에 바로 추가
                _map.Markers.Add(newMarker);

                // 딕셔너리에 참조 저장
                _customMarkers[key] = newMarker;
            }
            else
            {
                // 3️⃣ 이미 있으면 위치만 업데이트
                marker.Position = latLng;
            }

            // 4️⃣ 메시지 전송 (ViewModel ↔ ViewModel 연동용)
            WeakReferenceMessenger.Default.Send(new MapClickMessage(new MapClickData(latLng, _isStart)));

            // 5️⃣ 다음 클릭 시 역할 반전
            _isStart = !_isStart;
        }


        private void AddSegmentToRoute(char id, PointLatLng from, PointLatLng to)
        {
            if (_map == null) return;

            string key = $"TGT-{id}";

            // ✅ 새 경로 세그먼트 생성
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

            // ✅ 딕셔너리에 경로 리스트 없으면 생성
            if (!_routes.TryGetValue(key, out var routeList))
            {
                routeList = new List<GMapRoute>();
                _routes[key] = routeList;
            }

            // ✅ 리스트 및 맵에 누적 추가
            routeList.Add(route);
            _map.Markers.Add(route);
        }
    }
}
