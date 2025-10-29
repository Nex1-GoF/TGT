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
    }
}