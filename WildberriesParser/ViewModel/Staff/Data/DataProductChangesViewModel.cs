using SimpleWbApi;
using SimpleWbApi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using DataLayer;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel.Staff.Data
{
    public class DataProductChangesViewModel : ViewModelBase
    {
        public string Title { get; } = "Отчет по изменению товара";

        private string _article;
        private string _tracedValue;
        private bool _isExportWorking;
        private bool _isShowWorking;

        private ExcelService _excelService;

        // Для пагинации
        private int _selectedIndex = 0;

        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        private List<WbProductChanges> _originalProducts;

        private PagedList<WbProductChanges> _productPosChanges;
        private PagedListCommands<WbProductChanges> _pagedCommands;

        public PagedList<WbProductChanges> ProductPosChanges
        {
            get => _productPosChanges;
            set { Set(ref _productPosChanges, value); }
        }

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }

        public string Article
        {
            get => _article;
            set
            {
                Set(ref _article, value);
            }
        }

        private WbAPI _wbApi;

        public DataProductChangesViewModel(INavigationService navigationService,
            ExcelService excelService, WbAPI wbAPI)
        {
            NavigationService = navigationService;
            _wbApi = wbAPI;
            _excelService = excelService;

            _originalProducts = new List<WbProductChanges>(DBEntities.GetContext().WbProductChanges
                                .OrderByDescending(x => x.Date)
                                .ToList());

            ProductPosChanges = new PagedList<WbProductChanges>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands = new PagedListCommands<WbProductChanges>(ProductPosChanges);
            //PagedCommands.Instance = ProductPosChanges;
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _ShowCommand;

        public AsyncRelayCommand ShowCommand
        {
            get
            {
                return _ShowCommand ??
                    (_ShowCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                       {
                           IsShowWorking = true;
                           try
                           {
                               updateData();
                           }
                           catch (Exception ex)
                           {
                               Helpers.MessageBoxHelper.Information(ex.Message);
                           }
                           IsShowWorking = false;
                       }).Task;
                    },
                    (obj) => (int.TryParse(_article, out _) || string.IsNullOrEmpty(_article)) && _isShowWorking == false));
            }
        }

        public void updateData()
        {
            DateTime? startTimeTemp = _startDate;
            DateTime? endTimeTemp = _endDate;

            if (string.IsNullOrEmpty(_article) && !startTimeTemp.HasValue && !endTimeTemp.HasValue)
            {
                _originalProducts = DBEntities.GetContext().WbProductChanges
                .OrderByDescending(x => x.Date)
                .ToList();
            }
            else
            {
                if (_startDate == null)
                {
                    startTimeTemp = DateTime.MinValue;
                }

                if (_endDate == null)
                {
                    endTimeTemp = DateTime.MaxValue;
                }

                if (int.TryParse(_article, out int article))
                {
                    // Поиск по артикулу без поискового запроса
                    _originalProducts = DBEntities.GetContext().WbProductChanges
                    .Where(x => x.WbProductID == article && x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                    .OrderByDescending(x => x.Date)
                    .ToList();
                }
                else
                {
                    // Поиск по артикулу без поискового запроса
                    _originalProducts = DBEntities.GetContext().WbProductChanges
                    .Where(x => x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                    .OrderByDescending(x => x.Date)
                    .ToList();
                }
            }

            ProductPosChanges = new PagedList<WbProductChanges>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands.Instance = ProductPosChanges;
        }

        private DateTime? _startDate;
        private DateTime? _endDate;

        private AsyncRelayCommand _exportCommand;

        public AsyncRelayCommand ExportCommand
        {
            get
            {
                return _exportCommand ??
                    (_exportCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            string path = _excelService.ShowSaveAsFileDialog();
                            if (path == null)
                            {
                                Helpers.MessageBoxHelper.Error("Вы не выбрали файл!");
                                return;
                            }
                            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>();
                            data.Add("Артикул", new List<object>());
                            data.Add("Ссылка", new List<object>());
                            data.Add("Название", new List<object>());
                            data.Add("Дата", new List<object>());
                            data.Add("Бренд", new List<object>());
                            data.Add("Скидка", new List<object>());
                            data.Add("Цена без скидки", new List<object>());
                            data.Add("Цена со скидкой", new List<object>());

                            foreach (var product in _originalProducts)
                            {
                                data["Артикул"].Add(product.WbProductID);
                                data["Ссылка"].Add($@"https://www.wildberries.ru/catalog/{product.WbProductID}/detail.aspx");
                                data["Название"].Add(product.WbProduct.Name);
                                data["Дата"].Add(product.Date.ToString(@"dd/MM/yy"));
                                data["Бренд"].Add(product.WbProduct.WbBrand.Name);
                                data["Скидка"].Add(product.Discount);
                                data["Цена без скидки"].Add(product.PriceWithoutDiscount);
                                data["Цена со скидкой"].Add(product.PriceWithDiscount);
                            }
                            try
                            {
                                var columns = ExcelColumn.FromDictionary(data);
                                columns[1].CellFormatType = ExcelCellFormatType.Hyperlink;
                                _excelService.Export(columns, path, "Карточки");

                                if (Helpers.MessageBoxHelper.Question("Экcпортировано успешно! Открыть файл?") == Helpers.MessageBoxHelperResult.YES)
                                {
                                    Process.Start(path);
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error($"Во время экспорта произошла ошибка:\n{ex.Message}");
                            }
                            finally
                            {
                                IsExportWorking = false;
                            }
                        }).Task;
                    }
                    ));
            }
        }

        private WbProductChanges _selectedEntity;

        public WbProductChanges SelectedEntity
        {
            get => _selectedEntity;
            set => Set(ref _selectedEntity, value);
        }

        private AsyncRelayCommand _PriceDynamicCommand;

        public AsyncRelayCommand PriceDynamicCommand
        {
            get
            {
                return _PriceDynamicCommand ??
                    (_PriceDynamicCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (SelectedEntity != null)
                                {
                                    var vm = App.ServiceProvider.GetService(typeof(DataProductChangesViewModel)) as DataProductChangesViewModel;
                                    vm.Article = SelectedEntity.WbProductID.ToString();
                                    vm.updateData();
                                    NavigationService.NavigateTo<Data.DataProductChangesViewModel>();
                                }
                                else
                                {
                                    Helpers.MessageBoxHelper.Error("Вы не выбрали данные!");
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error(ex.Message);
                            }
                        }).Task;
                    }
                    ));
            }
        }

        private AsyncRelayCommand _SalesCommand;

        public AsyncRelayCommand SalesCommand
        {
            get
            {
                return _SalesCommand ??
                    (_SalesCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (SelectedEntity != null)
                                {
                                    var vm = App.ServiceProvider.GetService(typeof(SellingProductViewModel)) as SellingProductViewModel;
                                    vm.Article = SelectedEntity.WbProductID.ToString();
                                    vm.updateData();
                                    NavigationService.NavigateTo<Data.SellingProductViewModel>();
                                }
                                else
                                {
                                    Helpers.MessageBoxHelper.Error("Вы не выбрали данные!");
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error(ex.Message);
                            }
                        }).Task;
                    }
                    ));
            }
        }

        private AsyncRelayCommand _TracecxCommand;

        public AsyncRelayCommand TraceCxCommand
        {
            get
            {
                return _TracecxCommand ??
                    (_TracecxCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                if (SelectedEntity != null)
                                {
                                    var vm = App.ServiceProvider.GetService(typeof(DataProductPosChangesViewModel)) as DataProductPosChangesViewModel;
                                    vm.Article = SelectedEntity.WbProductID.ToString();
                                    vm.updateData();
                                    NavigationService.NavigateTo<Data.DataProductPosChangesViewModel>();
                                }
                                else
                                {
                                    Helpers.MessageBoxHelper.Error("Вы не выбрали данные!");
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error(ex.Message);
                            }
                        }).Task;
                    }
                    ));
            }
        }

        public bool IsShowWorking
        {
            get => _isShowWorking;
            set => Set(ref _isShowWorking, value);
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

        public PagedListCommands<WbProductChanges> PagedCommands
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

        public DateTime? StartDate
        {
            get => _startDate;
            set => Set(ref _startDate, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => Set(ref _endDate, value);
        }
    }
}