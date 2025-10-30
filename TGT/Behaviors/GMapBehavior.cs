using GMap.NET;
using GMap.NET.WindowsPresentation;
using Microsoft.Xaml.Behaviors;
using System.Collections;
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

        private static void OnMarkersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (GMapBehavior)d;
            if (behavior.AssociatedObject == null) return;

            if (e.OldValue is ObservableCollection<GMapMarker> oldMarkers)
                oldMarkers.CollectionChanged -= behavior.OnMarkersCollectionChanged;

            if (e.NewValue is ObservableCollection<GMapMarker> newMarkers)
            {
                newMarkers.CollectionChanged += behavior.OnMarkersCollectionChanged;
                behavior.UpdateMarkers();
            }
        }

        private void OnMarkersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateMarkers();
        }

        private void UpdateMarkers()
        {
            if (AssociatedObject == null || Markers == null)
                return;

            // 🔥 지도에 반영
            AssociatedObject.Markers.Clear();
            foreach (var marker in Markers)
                AssociatedObject.Markers.Add(marker);

            if(_circlePolygon!= null)
                AssociatedObject.Markers.Add(_circlePolygon);

            AssociatedObject.InvalidateVisual();
        }

        private GMapPolygon? _circlePolygon;

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += (s, e) =>
            {
                RefreshMarkers();
                DrawDetectionCircle();
            };

            if (Markers is INotifyCollectionChanged coll)
                coll.CollectionChanged += OnMarkersChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (Markers is INotifyCollectionChanged coll)
                coll.CollectionChanged -= OnMarkersChanged;
        }

        private void OnMarkersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshMarkers();
        }

        private void RefreshMarkers()
        {
            if (AssociatedObject == null || Markers == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // 기존 GMapMarker 목록
                var currentMarkers = AssociatedObject.Markers.OfType<GMapMarker>().ToList();

                // 새로 들어온 ViewModel 마커 목록
                var newMarkers = Markers.OfType<GMapMarker>().ToList();

                // 1️ 지도에 없는 마커만 추가
                foreach (var marker in newMarkers)
                {
                    if (!currentMarkers.Contains(marker))
                        AssociatedObject.Markers.Add(marker);
                }

                //2️ ViewModel에서 제거된 마커는 지도에서도 제거
                foreach (var marker in currentMarkers)
                {
                    if (!newMarkers.Contains(marker))
                        AssociatedObject.Markers.Remove(marker);
                }

                        //3 원 표시는 반영해줘야함
                if (_circlePolygon != null)
                    AssociatedObject.Markers.Add(_circlePolygon);
            });
        }


        private void DrawDetectionCircle()
        {
            var Center = MapService.Instance.Center;
            var DetectionRadius = MapService.Instance.Distance;

            if (AssociatedObject == null || DetectionRadius <= 0) return;

            var points = CreateCircle(Center, DetectionRadius, 72);
            var polygon = new GMapPolygon(points)
            {
                Shape = new Path
                {
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent
                }
            };

            _circlePolygon = polygon;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // ✅ 기존 Circle 삭제 후 새로 교체, 다른 마커는 그대로 유지
                var oldCircle = AssociatedObject.Markers.OfType<GMapPolygon>()
                    .FirstOrDefault(p => p == _circlePolygon);
                if (oldCircle != null)
                    AssociatedObject.Markers.Remove(oldCircle);

                AssociatedObject.Markers.Add(polygon);
            });
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
