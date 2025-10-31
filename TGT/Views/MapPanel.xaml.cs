using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TGT.Services;
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
            MapService.Instance.Initialize(PART_Map);
            PART_Map.MouseLeftButtonDown += OnMapClick;
        }

        private void OnLoaded()
        {
            // 지도 기본 설정
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            PART_Map.MapProvider = OpenStreetMapProvider.Instance;
            PART_Map.MinZoom = 2;
            PART_Map.MaxZoom = 18;
            PART_Map.Zoom = 8;
            PART_Map.Position = _viewModel.Center;
            PART_Map.CanDragMap = false;

        }

        private void OnMapClick(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(PART_Map);
            var latlng = PART_Map.FromLocalToLatLng((int)point.X, (int)point.Y);
            //PART_Map.Markers.
            // 좌표를 ViewModel에게 알림
            WeakReferenceMessenger.Default.Send(
                new MapClickMessage(new PointLatLng(latlng.Lat, latlng.Lng))
            );
        }
    }

    // 메시지 정의
    public record MapClickMessage(PointLatLng Location);
}
