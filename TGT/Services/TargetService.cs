using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TGT.Messages;
using TGT.Models;

namespace TGT.Services
{
    public class    TargetService
    {
        private static TargetService _instance;
        public static TargetService Instance => _instance ??= new TargetService();

        private readonly DispatcherTimer _timer;
        public ObservableCollection<Target> Targets { get; } = new();

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
                        //탐지가 됬는지 확인
                if(t.IsDetected)
                { 
                            //통신보내기
                    
                }
                else
                {
                    //거리체크하기
                    double dLat = (t.CurLoc.Lat - MapService.Instance.Center.Lat) * 111.0;
                    double dLon = (t.CurLoc.Lon - MapService.Instance.Center.Lng) * 88.8;
                    double distanceKm = Math.Sqrt(dLat * dLat + dLon * dLon);

                    bool withinRange = distanceKm <= (MapService.Instance.Distance / 1000);
                    if (withinRange)
                    {
                        t.IsDetected = true;
                    }
                }
                // (선택) PathHistory 유지
                t.PathHistory.Add(t.CurLoc);

                // ✅ 메시지 전송(From → To +Altitude)
                WeakReferenceMessenger.Default.Send(
                    new TargetUpdateMessage(
                        new TargetUpdateData(
                            targetId: t.Id.ToString(),
                            from: new PointLatLng(t.CurLoc.Lat, t.CurLoc.Lon),
                            to: new PointLatLng(newLat, newLon),
                            altitude: t.Altitude,
                            pathPoints: null // or t.PathHistory.Select(p => new PointLatLng(p.Lat,p.Lon)).ToList()
                        )
                    )
                );
            }
        }


        // ✅ 표적 추가 메서드
        public void AddTarget(Target target)
        {
            Targets.Add(target);
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

        public void SelectTarget(Target selected)
        {
            if (selected == null) return;
            if (selected.IsMoving == false) return; // 시작 버튼 누를 때 이 함수가 실행되는것을 막기위해

            // 이미 선택된 항목을 클릭한 경우 → 모든 포커스 해제
            if (selected.IsFocused)
            {
                foreach (var t in Targets)
                    t.IsFocused = false;
                return;
            }

            // 새로 선택된 항목이라면 나머지 해제 후 현재만 true
            foreach (var t in Targets)
                t.IsFocused = false;

            selected.IsFocused = true;
        }

        private Target? FindTarget(char id)
        {
            return Targets.FirstOrDefault(t => t.Id == id);
        }

        public void RemoveTarget(Target target)
        {
            Targets.Remove(target);
        }

        public void StartAll()
        {
            foreach (var t in Targets)
                t.IsMoving = true;
        }
    }
}
