using CommunityToolkit.Mvvm.Messaging;
using GMap.NET;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using TGT.Messages;
using TGT.Models;

namespace TGT.Services
{
    public class TargetService
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
            foreach (var t in Targets)
            {
                if (!t.IsMoving) continue;

                double dx = t.EndLoc.Lat - t.CurLoc.Lat;
                double dy = t.EndLoc.Lon - t.CurLoc.Lon;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist < 0.00005)
                {
                    t.IsMoving = false;
                    continue;
                }

                double step = t.Speed * 0.1 / 111000.0;
                var prev = t.CurLoc;
                var nextLat = t.CurLoc.Lat + dx / dist * step;
                var nextLon = t.CurLoc.Lon + dy / dist * step;

                t.CurLoc = (nextLat, nextLon);

                // (선택) PathHistory 유지
                t.PathHistory.Add(t.CurLoc);

                // ✅ 메시지 전송 (From → To + Altitude)
                WeakReferenceMessenger.Default.Send(
                    new TargetUpdateMessage(
                        new TargetUpdateData(
                            targetId: t.Id.ToString(),
                            from: new PointLatLng(prev.Lat, prev.Lon),
                            to: new PointLatLng(nextLat, nextLon),
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



        private Target? FindTarget(char id)
        {
            return Targets.FirstOrDefault(t => t.Id == id);
        }
    }
}
