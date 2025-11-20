using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
  

        private readonly TargetService _targetService = TargetService.Instance;

        private MapService() { }

        
        public void Initialize(GMapControl map)
        {
            // 맵 초기화
            _map = map;
            _map.MapProvider = CartoDarkMapProvider.Instance;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            _map.MinZoom = 2;
            _map.MaxZoom = 18;
            _map.Zoom = 8;
            _map.Position = Center;
            _map.CanDragMap = true;
            
            // 원 그리기
        }

        public GMapPolygon DrawDetectionCircle()
        {

            var points = CreateCircle(Center, Distance, 72);
            GMapPolygon CirclePolygon = new GMapPolygon(points);
            const double R = 6378137.0;
            var pts = new List<PointLatLng>();
            double lat = Deg2Rad(Center.Lat);
            double lon = Deg2Rad(Center.Lng);
            double d = Distance / R;

            for (int i = 0; i <= 72; i++)
            {
                double a = 2 * System.Math.PI * i / 72;
                double latP = System.Math.Asin(System.Math.Sin(lat) * System.Math.Cos(d) +
                                               System.Math.Cos(lat) * System.Math.Sin(d) * System.Math.Cos(a));
                double lonP = lon + System.Math.Atan2(System.Math.Sin(a) * System.Math.Sin(d) * System.Math.Cos(lat),
                                                      System.Math.Cos(d) - System.Math.Sin(lat) * System.Math.Sin(latP));
                pts.Add(new PointLatLng(Rad2Deg(latP), Rad2Deg(lonP)));
            }

            return new GMapPolygon(pts)
            {
                Shape = new System.Windows.Shapes.Path
                {
                    Stroke = new SolidColorBrush(Color.FromRgb(232, 247, 255)),
                    StrokeThickness = 2.5,
                    StrokeDashArray = new DoubleCollection { 4, 6 }, // 레이더 점선 느낌
                    Opacity = 0.8,
                    Fill = Brushes.Transparent,
                    Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(0, 255, 200),
                        BlurRadius = 25,
                        ShadowDepth = 0,
                        Opacity = 0.7
                    }
                }
            };
        }

        private static double Deg2Rad(double d) => d * System.Math.PI / 180.0;
        private static double Rad2Deg(double r) => r * 180.0 / System.Math.PI;


        private static List<PointLatLng> CreateCircle(PointLatLng center, double radiusMeters, int segments)
        {
            const double EarthRadius = 6378137.0;
            var points = new List<PointLatLng>();
            double lat = ToRadians(center.Lat);
            double lon = ToRadians(center.Lng);
            double d = radiusMeters / EarthRadius;

            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double latPoint = Math.Asin(Math.Sin(lat) * Math.Cos(d) +
                                            Math.Cos(lat) * Math.Sin(d) * Math.Cos(angle));
                double lonPoint = lon + Math.Atan2(Math.Sin(angle) * Math.Sin(d) * Math.Cos(lat),
                                                   Math.Cos(d) - Math.Sin(lat) * Math.Sin(latPoint));
                points.Add(new PointLatLng(ToDegrees(latPoint), ToDegrees(lonPoint)));
            }
            return points;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180.0;
        private static double ToDegrees(double rad) => rad * 180.0 / Math.PI;
    }
}
