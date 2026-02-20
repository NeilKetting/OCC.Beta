using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Features.MobileHub.Models;
using OCC.Client.ViewModels.Core;
using System;

namespace OCC.Client.Features.MobileHub.ViewModels
{
    public partial class MobileLayoutSelectorViewModel : ViewModelBase
    {
        private readonly Action<MobileLayoutType> _onLayoutSelected;

        public MobileLayoutSelectorViewModel(Action<MobileLayoutType> onLayoutSelected)
        {
            _onLayoutSelected = onLayoutSelected;
        }

        [RelayCommand]
        private void SelectLayout(MobileLayoutType layoutType)
        {
            _onLayoutSelected?.Invoke(layoutType);
        }
    }
}
