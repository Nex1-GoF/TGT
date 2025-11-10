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
using TGT.Models;
using TGT.Services;
using TGT.ViewModels;

namespace TGT.Views
{
    public partial class MapPanel : UserControl
    {
        private readonly MapViewModel _viewModel;
        private readonly FpsCounter _fpsCounter;

        public MapPanel()
        {
            InitializeComponent();
            _viewModel = new MapViewModel(PART_Map);
            DataContext = _viewModel;
            PART_Map.MouseLeftButtonUp += OnMapClick; // View 계층에서 한 이유: ViewModel을 호출해야하기 때문에
            _fpsCounter = new FpsCounter();
        }

        private void OnMapClick(object sender, MouseButtonEventArgs e)
        {
            // 클릭 위치 (픽셀 기준)
            var point = e.GetPosition(PART_Map);

            // 1️ 표적 마커 중 가장 가까운 마커 찾기
            GMapMarker? nearestMarker = null;
            double minDistance = double.MaxValue;

            foreach (var marker in PART_Map.Markers)
            {
                // Todo: 표적 마커 이외는 무시 (예: Tag를 통해 구분)
                if (marker.Tag is string markerTag && !markerTag.StartsWith("TGT-"))
                    continue;

                var pos = PART_Map.FromLatLngToLocal(marker.Position);
                double dx = point.X - pos.X;
                double dy = point.Y - pos.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < 20 && distance < minDistance)
                {
                    minDistance = distance;
                    nearestMarker = marker;
                }
            }

            // 2️ 마커가 클릭되지 않은 경우 → 일반 지도 클릭 처리
            if (nearestMarker == null)
            {
                var latlng = PART_Map.FromLocalToLatLng((int)point.X, (int)point.Y);
                _viewModel.SetPosition(latlng);
                return;
            }

            // 3️ 마커가 클릭된 경우 → 해당 Target 선택
            if (nearestMarker.Tag is string nearestMarkerTag && nearestMarkerTag.StartsWith("TGT-"))
            {
                // "TGT-" 이후의 부분만 추출
                string targetId = nearestMarkerTag.Substring(4);

                _viewModel.PressTargetMarker(targetId);

            }
        }

    }

    // 메시지 정의

}
