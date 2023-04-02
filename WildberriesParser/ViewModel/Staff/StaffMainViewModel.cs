﻿using System.Windows;
using System.Windows.Navigation;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Staff
{
    internal class StaffMainViewModel : ViewModelWithWindowButtonsBase
    {
        public User CurrentUser
        {
            get => App.CurrentUser;
        }

        private INavigationService _navigationService;

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public StaffMainViewModel(INavigationService navigationService)
        {
            WindowState = System.Windows.WindowState.Normal;
            NavigationService = navigationService;
            NavigationService.NavigateTo<SearchProducts.SearchProductsMainView>();
        }

        private RelayCommand _searchProductsMainCommand;

        public RelayCommand SearchProductsMainCommand
        {
            get
            {
                return _searchProductsMainCommand ??
                    (_searchProductsMainCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<SearchProducts.SearchProductsMainView>();
                    }
                    ));
            }
        }

        private RelayCommand _SettingsCommand;

        public RelayCommand SettingsCommand
        {
            get
            {
                return _SettingsCommand ??
                    (_SettingsCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<SettingsViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _traceProductCommand;

        public RelayCommand TraceProductCommand
        {
            get
            {
                return _traceProductCommand ??
                    (_traceProductCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<TraceProductsViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _automatizationCommand;

        public RelayCommand AutomatizationCommand
        {
            get
            {
                return _automatizationCommand ??
                    (_automatizationCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<AutomatizationViewModel>();
                    }
                    ));
            }
        }

        private RelayCommand _exitAccountCommand;

        public RelayCommand ExitAccountCommand
        {
            get
            {
                return _exitAccountCommand ??
                    (_exitAccountCommand = new RelayCommand
                    ((obj) =>
                    {
                        Window window = obj as Window;
                        window.Hide();
                        NavigationService.NavigateTo<AuthorizationViewModel>();
                        (App.ServiceProvider.GetService(typeof(View.StartView)) as Window).Show();
                        window.Close();
                    }
                    ));
            }
        }
    }
}