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
    internal class DataViewModel : ViewModelWithWindowButtonsBase
    {
        public string Title { get; set; } = "Данные";
        private INavigationService _navigationService;

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public DataViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            //NavigationService.NavigateTo<SearchProductsViewModel>();
        }

        private RelayCommand _toDataViewCommand;

        public RelayCommand ToDataViewCommand
        {
            get
            {
                return _toDataViewCommand ??
                    (_toDataViewCommand = new RelayCommand
                    ((obj) =>
                    {
                        int n = (int)obj;
                        switch (n)
                        {
                            case 0:
                                NavigationService.NavigateTo<Data.DataProductChangesViewModel>();
                                break;

                            case 1:
                                NavigationService.NavigateTo<Data.DataProductPosChangesViewModel>();
                                break;

                            case 2:
                                NavigationService.NavigateTo<Data.SellingProductViewModel>();
                                break;

                            case 3:
                                NavigationService.NavigateTo<Data.DataAllProductsViewModel>();
                                break;
                        }
                    }
                    ));
            }
        }
    }
}