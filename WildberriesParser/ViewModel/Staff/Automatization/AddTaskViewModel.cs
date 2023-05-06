using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using WildberriesParser.View;

namespace WildberriesParser.ViewModel.Staff.Automatization
{
    internal class AddTaskViewModel : ViewModelWithWindowButtonsBase
    {
        private INavigationService _navigationService;

        private CollectorTaskType _selectedCollectorTaskType;

        private List<CollectorTaskType> _collectorTaskTypes;

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        private INavigationService _TaskNavigationService;

        public INavigationService TaskNavigationService
        {
            get => _TaskNavigationService;
            set => Set(ref _TaskNavigationService, value);
        }

        public AddTaskViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            NavigationService.NavigateTo<SearchProductsViewModel>();

            TaskNavigationService = new Services.NavigationService(App.ServiceProvider.GetService(typeof(Func<Type, ViewModelBase>)) as Func<Type, ViewModelBase>);
            TaskNavigationService.NavigateTo<TaskNotSelectedViewModel>();

            CollectorTaskTypes = DBEntities.GetContext().CollectorTaskType.ToList();
        }

        private RelayCommand _backCommand;

        public RelayCommand BackCommand
        {
            get
            {
                return _backCommand ??
                    (_backCommand = new RelayCommand
                    ((obj) =>
                    {
                        NavigationService.NavigateTo<AutomatizationViewModel>();
                    }
                    ));
            }
        }

        private PackIconKind _currentIcon = PackIconKind.Help;

        public PackIconKind CurrentIcon
        {
            get => _currentIcon;
            set => Set(ref _currentIcon, value);
        }

        public CollectorTaskType SelectedCollectorTaskType
        {
            get => _selectedCollectorTaskType;
            set
            {
                if (Set(ref _selectedCollectorTaskType, value))
                {
                    switch (_selectedCollectorTaskType.ID)
                    {
                        case 1:
                            CurrentIcon = PackIconKind.ShoppingSearch;
                            TaskNavigationService.NavigateTo<TaskTraceSearchProductViewModel>();
                            break;

                        case 2:
                            CurrentIcon = PackIconKind.ChartLineVariant;
                            TaskNavigationService.NavigateTo<TaskTraceChangesProductViewModel>();
                            break;

                        default:
                            CurrentIcon = PackIconKind.Help;
                            break;
                    }
                }
            }
        }

        public List<CollectorTaskType> CollectorTaskTypes
        {
            get => _collectorTaskTypes;
            set => Set(ref _collectorTaskTypes, value);
        }
    }
}