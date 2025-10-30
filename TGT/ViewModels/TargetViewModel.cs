using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGT.Models;
using TGT.Services;

namespace TGT.ViewModels
{
    public partial class TargetViewModel : ObservableObject
    {
        private readonly TargetService _service;
        public ObservableCollection<Target> Targets => _service.Targets;

        [ObservableProperty]
        private Target selectedTarget;

        public TargetViewModel(TargetService service)
        {
            _service = service;
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

    }
}