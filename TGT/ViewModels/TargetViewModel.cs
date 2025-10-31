using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        }
        [RelayCommand]
        private void RemoveTarget(Target target)
        {
            _service.RemoveTarget(target);
        }

        [RelayCommand]
        private void StartAll()
        {
            _service.StartAll();
        }
        [RelayCommand]
        private void StartTarget(Target t)
        {
            _service.StartTarget(t.Id);
        }

        // 태현 -- 표적 포커스 변경
        [RelayCommand]
        private void SelectTarget(Target selected)
        {
            _service.SelectTarget(selected);
        }
        public void HandleKeyInput(Key key)
        {
            if (SelectedTarget == null)
                return;
            var t = _service.Targets.FirstOrDefault(x => x.Id == SelectedTarget.Id);
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
            }
        }
    }
}