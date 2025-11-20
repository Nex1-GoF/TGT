using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TGT.Messages;
using TGT.Models;
using TGT.Services;

namespace TGT.ViewModels
{
    public partial class TargetViewModel : ObservableObject
    {
        private readonly TargetService _service;
        public ObservableCollection<Target> Targets => _service.Targets;


        [ObservableProperty]
        static public Target? selectedTarget;

        public TargetViewModel(TargetService service)
        {
            _service = service;
            InputEventBroker.OnKeyInput += HandleKeyInput;
            WeakReferenceMessenger.Default.Register<TargetSelectMessage>(this, (r, msg) =>
            {
                selectedTarget = _service.SelectedTarget;
            });

        }
        [RelayCommand]
        private void RemoveTarget(Target target)
        {
            _service.RemoveTarget(target);
        }

        [RelayCommand]
        private async Task StartAll()
        {
            var tasks = Targets
                .Where(t => !t.IsMoving)        // 이미 움직이는 표적 제외
                .Select(t => RunTargetAsync(t)) // 비동기 실행 묶기
                .ToList();

            await Task.WhenAll(tasks); // 전체 완료까지 기다림
        }



        private async Task RunTargetAsync(Target t)
        {
            if (t.IsMoving) return;

            t.IsMoving = true;
            _service.StartTarget(t.Id);

            await ScenarioService.Instance.StartScenario(t);

            t.IsMoving = false;
            _service.RemoveTarget(t);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task StartTarget(Target t)
        {
            await RunTargetAsync(t);
        }


        // 태현 -- 표적 포커스 변경
        [RelayCommand]
        private void SelectTarget(Target selected)
        {
            _service.SelectTarget(selected);
        }
        public void HandleKeyInput(Key key)
        {
            if (_service.SelectedTarget == null)
                return;
            var t = _service.Targets.FirstOrDefault(x => x.Id == _service.SelectedTarget.Id);


            switch (key)
            {
                case Key.Left:
                    t.Yaw -= 200;
                    if (t.Yaw < 0) t.Yaw += 36000;
                    //_scenarioService.AddKeyInputData()
                    break;
                case Key.Right:
                    t.Yaw += 200;
                    if (t.Yaw >= 36000) t.Yaw -= 36000;
                    break;
            }
        }


    }



}