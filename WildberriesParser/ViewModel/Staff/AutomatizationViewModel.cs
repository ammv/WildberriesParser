using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using DataLayer;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Staff
{
    public class AutomatizationViewModel : ViewModelBase
    {
        public string Title { get; } = "Автоматизация";

        private string _searchText;

        // Для пагинации
        private int _selectedIndex = 0;

        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        private List<WbProduct> _originalProducts;

        private PagedList<WbProduct> _productPosChanges;
        private PagedListCommands<WbProduct> _pagedCommands;

        public PagedList<WbProduct> ProductPosChanges
        {
            get => _productPosChanges;
            set { Set(ref _productPosChanges, value); }
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                Set(ref _searchText, value);
            }
        }

        public AutomatizationViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            _originalProducts = DBEntities.GetContext().WbProduct
                                .OrderByDescending(x => x.ID)
                                .ToList();

            ProductPosChanges = new PagedList<WbProduct>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands = new PagedListCommands<WbProduct>(ProductPosChanges);
            //PagedCommands.Instance = ProductPosChanges;
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _AddTaskCommand;

        public AsyncRelayCommand AddTaskCommand
        {
            get
            {
                return _AddTaskCommand ??
                    (_AddTaskCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                _navigationService.NavigateTo<Automatization.AddTaskViewModel>();
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error(ex.Message);
                            }
                        }).Task;
                    }));
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                ProductPosChanges.PageSize = _pageSizes[value];
            }
        }

        public PagedListCommands<WbProduct> PagedCommands
        {
            get => _pagedCommands;
            set
            {
                Set(ref _pagedCommands, value);
            }
        }

        public int[] PageSizes
        {
            get => _pageSizes;
            set => _pageSizes = value;
        }
    }
}