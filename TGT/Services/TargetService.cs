using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TGT.Models;

namespace TGT.Services
{
    public class TargetService
    {
        // ✅ 싱글톤 추가
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

        private void UpdateTargets(object sender, EventArgs e)
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

                double step = t.Speed * 0.1 / 111000.0; // 0.1초마다 이동량 (1도=111km)
                t.CurLoc = (t.CurLoc.Lat + dx / dist * step,
                            t.CurLoc.Lon + dy / dist * step);

                t.PathHistory.Add(t.CurLoc);
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

        private Target FindTarget(char id)
        {
            foreach (var t in Targets)
                if (t.Id == id) return t;
            return null;
        }
    }
}