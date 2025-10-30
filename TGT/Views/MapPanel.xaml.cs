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
            PART_Map.Position = TGT.Services.MapService.Instance.Center;

        }
    }
}
