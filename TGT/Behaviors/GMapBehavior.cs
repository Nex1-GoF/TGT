using GMap.NET;
using GMap.NET.WindowsPresentation;
using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using TGT.Services;

namespace TGT.Behaviors
{
    public class GMapBehavior : Behavior<GMapControl>
    {
        //#region Markers (표적)
        public static readonly DependencyProperty MarkersProperty =
            DependencyProperty.Register(
                nameof(Markers),
                typeof(ObservableCollection<GMapMarker>),
                typeof(GMapBehavior),
                new PropertyMetadata(null, OnMarkersChanged));

        public ObservableCollection<GMapMarker> Markers
        {
            get => (ObservableCollection<GMapMarker>)GetValue(MarkersProperty);
            set => SetValue(MarkersProperty, value);
        }
        //#endregion

        //#region 📈 Routes (경로)
        public static readonly DependencyProperty RoutesProperty =
            DependencyProperty.Register(
                nameof(Routes),
                typeof(ObservableCollection<GMapRoute>),
                typeof(GMapBehavior),
                new PropertyMetadata(null, OnRoutesChanged));

        public ObservableCollection<GMapRoute> Routes
        {
            get => (ObservableCollection<GMapRoute>)GetValue(RoutesProperty);
            set => SetValue(RoutesProperty, value);
        }
        //#endregion

        private GMapPolygon? _circlePolygon;

        //#region 🧭 PropertyChanged Handlers
        private static void OnMarkersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (GMapBehavior)d;

            if (e.OldValue is ObservableCollection<GMapMarker> oldMarkers)
                oldMarkers.CollectionChanged -= behavior.OnMarkersCollectionChanged;
            if (e.NewValue is ObservableCollection<GMapMarker> newMarkers)
                newMarkers.CollectionChanged += behavior.OnMarkersCollectionChanged;

            behavior.RefreshAll();
        }

        private static void OnRoutesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (GMapBehavior)d;

            if (e.OldValue is ObservableCollection<GMapRoute> oldRoutes)
                oldRoutes.CollectionChanged -= behavior.OnRoutesCollectionChanged;
            if (e.NewValue is ObservableCollection<GMapRoute> newRoutes)
                newRoutes.CollectionChanged += behavior.OnRoutesCollectionChanged;

            behavior.RefreshAll();
        }
        //#endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += (s, e) =>
            {
                DrawDetectionCircle();
                RefreshAll();
            };
        }

        private void OnMarkersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshAll();
        private void OnRoutesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshAll();

        //#region 🗺️ 갱신 로직
        private void RefreshAll()
        {
            if (AssociatedObject == null)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                AssociatedObject.Markers.Clear();

                // 표적
                if (Markers != null)
                {
                    foreach (var m in Markers)
                        AssociatedObject.Markers.Add(m);
                }

                // 경로
                if (Routes != null)
                {
                    foreach (var r in Routes)
                        AssociatedObject.Markers.Add(r);
                }

                // 탐지 원
                if (_circlePolygon != null)
                    AssociatedObject.Markers.Add(_circlePolygon);

                AssociatedObject.InvalidateVisual();
            });
        }
        //#endregion

        //#region 🟢 Circle
        private void DrawDetectionCircle()
        {
            var center = MapService.Instance.Center;
            var detectionRadius = MapService.Instance.Distance;

            if (AssociatedObject == null || detectionRadius <= 0) return;

            var points = CreateCircle(center, detectionRadius, 72);
            _circlePolygon = new GMapPolygon(points)
            {
                Shape = new Path
                {
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent
                },
                Tag = "CIRCLE"
            };
        }

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
        //#endregion
    }
}
