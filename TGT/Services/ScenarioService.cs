using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

using TGT.Services;

namespace TGT.Models
{
    public class ScenarioService
    {
        private static ScenarioService _instance;
        public static ScenarioService Instance => _instance ??= new ScenarioService();

       
        private readonly Dictionary<string, List<(int timeMS, Key key)>> ScenarioDatabase = new();
        public IReadOnlyList<string> ScenarioIds => ScenarioDatabase.Keys.ToList();

        private ScenarioService() {
            AddDefaultScenarios();
        }

        // 시나리오 추가
        public void AddScenario(string scenarioId)
        {
            if (!ScenarioDatabase.ContainsKey(scenarioId))
                ScenarioDatabase.Add(scenarioId, new List<(int, Key)>());
        }

        private void AddDefaultScenarios()
        {
            AddScenario("Default");
            AddScenario("Scenario1");
            AddScenario("Scenario2");
            AddScenario("Scenario3");

            // 시나리오 (최대 3분 (180000ms)까지 되도록 했음 or 더 늘리고싶으면 아래 totalTime 수정)
            ScenarioDatabase["Default"] = new()
            {
                (0, Key.None),
            };

            ScenarioDatabase["Scenario1"] = new()
            {
                (0, Key.None),
                (1000, Key.Left),    // 10~25초 Left
                (25000, Key.Right),   // 25~40초 Right
                (40000, Key.None)     // 40~60초 None
            };

            ScenarioDatabase["Scenario2"] = new()
            {
                (0, Key.None),
                (15000, Key.Right),   // 15~30초 Right
                (30000, Key.Left),    // 30~45초 Left
                (45000, Key.None)     // 45~60초 None
            };

            ScenarioDatabase["Scenario3"] = new()
            {
                (0, Key.None),
                (12000, Key.Left),    // 12~20초 Left
                (20000, Key.None),    // 20~35초 None
                (35000, Key.Right),   // 35~60초 Right
                (60000, Key.None),   // 35~60초 Right
            };
        }

        public async Task StartScenario(Target target)
        {
            string scenarioId = target.ScenarioId;
            if (!ScenarioDatabase.ContainsKey(scenarioId))
                return;

            var scenario = ScenarioDatabase[scenarioId].ToList();

            int totalTime = 180000; // 3분(요구사항)
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
