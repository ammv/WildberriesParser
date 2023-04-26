using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Staff
{
    internal class ReportViewModel : ViewModelWithWindowButtonsBase
    {
        private INavigationService _navigationService;

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public ReportViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            //NavigationService.NavigateTo<SearchProductsViewModel>();
        }

        private RelayCommand _reportProductChanges;

        public RelayCommand ReportProductChanges
        {
            get
            {
                return _reportProductChanges ??
                    (_reportProductChanges = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<NotImplementedViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _reportProductPosChanges;

        public RelayCommand ReportProductPosChanges
        {
            get
            {
                return _reportProductPosChanges ??
                    (_reportProductPosChanges = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<NotImplementedViewModel>();
                    }
                    ));
            }
        }
    }
}