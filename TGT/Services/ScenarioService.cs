using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

using TGT.Services;
using GMap.NET;

namespace TGT.Models
{
    public class ScenarioService
    {
        private static ScenarioService _instance;
        public static ScenarioService Instance => _instance ??= new ScenarioService();

       
        private readonly Dictionary<string, List<(int timeMS, Key key)>> ScenarioDatabase = new();
        private readonly Dictionary<string, (PointLatLng start, PointLatLng end)> ScenarioRoutes = new();
        public IReadOnlyList<string> ScenarioIds => ScenarioDatabase.Keys.ToList();

        private ScenarioService() {
            AddDefaultScenarios();
        }
        public void AddScenario(string scenarioId)
        {
            if (!ScenarioDatabase.ContainsKey(scenarioId))
                ScenarioDatabase.Add(scenarioId, new List<(int, Key)>());
        }
        // 시나리오 추가
        public void AddScenarioWithStart(string scenarioId, PointLatLng start, double yaw)
        {
            if (!ScenarioDatabase.ContainsKey(scenarioId))
                ScenarioDatabase.Add(scenarioId, new List<(int, Key)>());

            double distance = 5000; // 5km
            double R = 6371000; // 지구 반지름(m)
            double lat1 = ToRad(start.Lat);
            double lon1 = ToRad(start.Lng);
            double bearing = ToRad(yaw); // 북=0°, 동=90°

            double lat2 = Math.Asin(
                Math.Sin(lat1) * Math.Cos(distance / R) +
                Math.Cos(lat1) * Math.Sin(distance / R) * Math.Cos(bearing)
            );

            double lon2 = lon1 + Math.Atan2(
                Math.Sin(bearing) * Math.Sin(distance / R) * Math.Cos(lat1),
                Math.Cos(distance / R) - Math.Sin(lat1) * Math.Sin(lat2)
            );

            // 라디안 → 도 다시 변환
            var end = new PointLatLng(ToDeg(lat2), ToDeg(lon2));
            if (!ScenarioRoutes.ContainsKey(scenarioId))
                ScenarioRoutes.Add(scenarioId, (start, end));
        }
        private static double ToRad(double deg) => deg * Math.PI / 180.0;
        private static double ToDeg(double rad) => rad * 180.0 / Math.PI;

        private void AddDefaultScenarios()
        {
            PointLatLng startPoint = new PointLatLng();
            // 시나리오 (최대 3분 (180000ms)까지 되도록 했음 or 더 늘리고싶으면 아래 totalTime 수정)
            
            // ----- 디폴트 시나리오

            AddScenario("Default");
            ScenarioDatabase["Default"] = new()
            {
                (0, Key.None),
            };

            // ----- 시나리오 1
            startPoint.Lat = 37.5665;
            startPoint.Lng = 126.9780;
            AddScenarioWithStart("Scenario1", startPoint, 0.0); // 0.0 -> 북쪽
            ScenarioDatabase["Scenario1"] = new()
            {
                (0, Key.Left),          // 0~5초 직진
                (20000, Key.Right),       // 5~35초: 왼쪽 반원(30초)
            };

            startPoint.Lat = 37.5665;
            startPoint.Lng = 126.9780;
            AddScenarioWithStart("Scenario2", startPoint, 0.0); // 0.0 -> 북쪽
            ScenarioDatabase["Scenario2"] = new()
            {
                (0, Key.Right),          // 0~5초 직진
                (20000, Key.Left),       // 5~35초: 왼쪽 반원(30초)
            };


            startPoint.Lat = 37.5665;
            startPoint.Lng = 126.9780;
            AddScenarioWithStart("Scenario3", startPoint, 90.0); // 90.0 -> 동쪽
            ScenarioDatabase["Scenario3"] = new()
            {
                (0, Key.None),
                (12000, Key.Left),    // 12~20초 Left
                (20000, Key.None),    // 20~35초 None
                (35000, Key.Right),   // 35~60초 Right
                (60000, Key.None),   // 60~180초 Right
            };

        }

        public bool TryGetRoute(string scenarioId, out PointLatLng start, out PointLatLng end)
        {
            if (ScenarioRoutes.ContainsKey(scenarioId))
            {
                start = ScenarioRoutes[scenarioId].start;
                end = ScenarioRoutes[scenarioId].end;
                return true;
            }
            start = end = new PointLatLng();
            return false;
        }

        public async Task StartScenario(Target target)
        {
            if (target.ScenarioRunning)
                return;
            target.ScenarioRunning = true;

            string scenarioId = target.ScenarioId;
            if (!ScenarioDatabase.ContainsKey(scenarioId))
                return;

            

            var scenario = ScenarioDatabase[scenarioId].ToList();

            int totalTime = 1800000; // 3분(요구사항)
            int tickMS = 500;      // 키 입력 주기(요구사항)

            int currentIndex = 0;
            Key currentKey = Key.None;

            int elapsed = 0;

            while (elapsed <= totalTime)
            {
                // 현재 시나리오 지점 적용
                if (currentIndex < scenario.Count && elapsed >= scenario[currentIndex].timeMS)
                {
                    currentKey = scenario[currentIndex].key;
                    currentIndex++;
                }

                // 키 입력 로직 적용
                ApplyKeyToTarget(currentKey, target);

                await Task.Delay(tickMS);
                elapsed += tickMS;
                
            }
            target.ScenarioRunning = false;
        }

        // ⭐ 실제 Key 입력에 따른 타깃의 변화
        private void ApplyKeyToTarget(Key key, Target t)
        {
            switch (key)
            {
                case Key.Left:
                    t.Yaw -= 200;
                    if (t.Yaw < 0) t.Yaw += 36000;
                    break;

                case Key.Right:
                    t.Yaw += 200;
                    if (t.Yaw >= 36000) t.Yaw -= 36000;
                    break;

                case Key.None:
                    // 방향 변화 없음
                    break;
            }
        }
    }
}
