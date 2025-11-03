using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TGT.Messages;
using TGT.Models;
using TGT.ViewModels;

namespace TGT.Services
{
    public class    TargetService
    {
        private static TargetService _instance;
        public static TargetService Instance => _instance ??= new TargetService();

        private readonly DispatcherTimer _timer;
        public ObservableCollection<Target> Targets { get; } = new();
        public Target? SelectedTarget;

        private TargetService()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) }; // 10Hz
            _timer.Tick += UpdateTargets;
            _timer.Start();
        }

        private void UpdateTargets(object? sender, EventArgs e)
        {
            const double EarthMetersPerDegree = 111_000.0; // 위도/경도 변환용 근사값
            const double deltaTime = 0.1; // 100ms (Timer 주기)

            var updatedTargets = new List<TargetUpdateData>(); // ✅ 한 번에 보낼 데이터 모음

            foreach (var t in Targets)
            {
                if (!t.IsMoving)
                    continue;

                // yaw는 degree*100 단위니까, 라디안으로 변환
                double yawRad = (t.Yaw / 100.0) * Math.PI / 180.0;

                // 진행 방향 벡터 계산 (yaw 기준)
                double dx = Math.Cos(yawRad);
                double dy = Math.Sin(yawRad);

                // 이동 거리 (m/s * Δt) → degree 단위로 환산
                double step = (t.Speed * deltaTime) / EarthMetersPerDegree;

                // 새로운 좌표 계산
                var newLat = t.CurLoc.Lat + dx * step;
                var newLon = t.CurLoc.Lon + dy * step;

                // 업데이트
                t.CurLoc = (newLat, newLon);

                // 탐지 여부 확인
                if (t.IsDetected)
                {
                    //감지된 타겟 배열에 넣기(통신용!)
                }
                else
                {
                    // 거리체크
                    double dLat = (t.CurLoc.Lat - MapService.Instance.Center.Lat) * 111.0;
                    double dLon = (t.CurLoc.Lon - MapService.Instance.Center.Lng) * 88.8;
                    double distanceKm = Math.Sqrt(dLat * dLat + dLon * dLon);

                    bool withinRange = distanceKm <= (MapService.Instance.Distance / 1000);
                    if (withinRange)
                        t.IsDetected = true;
                }

                // (선택) 이동 경로 저장
                t.PathHistory.Add(t.CurLoc);

                // ✅ 리스트에 데이터 추가
                updatedTargets.Add(new TargetUpdateData(
                    targetId: t.Id.ToString(),
                    from: new PointLatLng(t.CurLoc.Lat, t.CurLoc.Lon),
                    to: new PointLatLng(newLat, newLon),
                    altitude: t.Altitude,
                    pathPoints: null // 필요 시 t.PathHistory 변환 가능
                ));
            }
            // TODO: 통신 보내기 (여기에 로직)

            // ✅ 루프 바깥에서 단 한 번만 메시지 전송
            if (updatedTargets.Count > 0)
            {
                WeakReferenceMessenger.Default.Send(new TargetBatchUpdateMessage(updatedTargets));
            }
        }



        // 표적 추가 메서드
        public void AddTarget(Target target)
        {
            Targets.Add(target); // 모델 업데이트
            WeakReferenceMessenger.Default.Send(new TargetAddMessage(new TargetAddData(target))); // Map Viewmodel로 전달 
        }

        public void StartTarget(char id)
        {
            var target = FindTarget(id);
            if (target != null && !target.IsMoving)
            {
                target.IsMoving = true;
                target.PathHistory.Clear();
                target.PathHistory.Add(target.CurLoc);
            }
        }

        private Target? FindTarget(char id)
        {
            return Targets.FirstOrDefault(t => t.Id == id);
        }

        public void RemoveTarget(Target target)
        {
            Targets.Remove(target);
            WeakReferenceMessenger.Default.Send(new TargetRemoveMessage(new TargetRemoveData(target.Id.ToString())));
        }

        public void StartAll()
        {
            foreach (var t in Targets)
                t.IsMoving = true;
        }
        public void SelectTarget(Target selected)
        {
            if (selected.IsMoving == false) return;
            if (SelectedTarget != null && selected.Id == SelectedTarget.Id) 
            {
                SelectedTarget = null;
                WeakReferenceMessenger.Default.Send(new TargetSelectMessage(new TargetSelectData("NULL")));
                return;
            }
                
            SelectedTarget = selected;
            WeakReferenceMessenger.Default.Send(new TargetSelectMessage(new TargetSelectData(selected.Id.ToString())));
        }

    }
}
