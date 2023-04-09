using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Staff
{
    internal class SearchProductsViewModel : ViewModelBase
    {
        public string Title { get; } = "Поиск продуктов";

        private INavigationService _navigationService;

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public SearchProductsViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        private RelayCommand _searchByArticleViewCommand;

        public RelayCommand SearchByArticleViewCommand
        {
            get
            {
                return _searchByArticleViewCommand ??
                    (_searchByArticleViewCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<SearchProducts.ByArticleViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _searchByRequestViewCommand;

        public RelayCommand SearchByRequestViewCommand
        {
            get
            {
                return _searchByRequestViewCommand ??
                    (_searchByRequestViewCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<SearchProducts.BySearchViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _searchByCategoryViewCommand;

        public RelayCommand SearchByCategoryViewCommand
        {
            get
            {
                return _searchByCategoryViewCommand ??
                    (_searchByCategoryViewCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<NotImplementedViewModel>();
                    }
                    ));
            }
        }
    }
}