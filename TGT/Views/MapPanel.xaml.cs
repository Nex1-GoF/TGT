using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TGT.ViewModels;

namespace TGT.Views
{
    public partial class MapPanel : UserControl
    {
        private readonly MapViewModel _viewModel;

        public MapPanel()
        {
            InitializeComponent();
            _viewModel = new MapViewModel();
            DataContext = _viewModel;

            Loaded += (s, e) => OnLoaded();
        }

        private void OnLoaded()
        {
            // 지도 기본 설정
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            PART_Map.MapProvider = OpenStreetMapProvider.Instance;
            PART_Map.MinZoom = 2;
            PART_Map.MaxZoom = 18;
            PART_Map.Zoom = 8;

            // 중심 좌표 (서울 근처)
            var center = new PointLatLng(37.5665, 126.9780);
            PART_Map.Position = center;

            // 탐지 반경 250km 원 추가
            var circlePoints = CreateCircle(center, 250_000, 72);
            var circle = new GMapPolygon(circlePoints)
            {
                Shape = new System.Windows.Shapes.Path
                {
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent
                }
            };
            PART_Map.Markers.Add(circle);

            // 🟡 초기 표적 마커 반영
            foreach (var marker in _viewModel.TargetMarkers)
                PART_Map.Markers.Add(marker);

            // 🟢 TargetMarkers가 변경되면 지도에 즉시 반영
            _viewModel.TargetMarkers.CollectionChanged += (s, e) =>
            {
                PART_Map.Markers.Clear();
                PART_Map.Markers.Add(circle); // 원 다시 추가
                foreach (var marker in _viewModel.TargetMarkers)
                    PART_Map.Markers.Add(marker);
            };
        }

        private static List<PointLatLng> CreateCircle(PointLatLng center, double radiusMeters, int segments)
        {
            var points = new List<PointLatLng>();
            const double EarthRadius = 6378137.0;

            double lat = ToRadians(center.Lat);
            double lon = ToRadians(center.Lng);
            double d = radiusMeters / EarthRadius;

            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double latPoint = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(angle));
                double lonPoint = lon + Math.Atan2(Math.Sin(angle) * Math.Sin(d) * Math.Cos(lat), Math.Cos(d) - Math.Sin(lat) * Math.Sin(latPoint));
                points.Add(new PointLatLng(ToDegrees(latPoint), ToDegrees(lonPoint)));
            }

            return points;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180.0;
        private static double ToDegrees(double rad) => rad * 180.0 / Math.PI;
    }
}
