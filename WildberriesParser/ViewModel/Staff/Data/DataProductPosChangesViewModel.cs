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
    public class DataProductPosChangesViewModel : ViewModelBase
    {
        public string Title { get; } = "Отчет по изменению товара";

        private string _article;
        private bool _isExportWorking;
        private bool _isShowWorking;

        private ExcelService _excelService;

        // Для пагинации
        private int _selectedIndex = 0;

        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        private List<WbProductPosChanges> _originalProducts;

        private PagedList<WbProductPosChanges> _productPosChanges;
        private PagedListCommands<WbProductPosChanges> _pagedCommands;

        private string _searchPattern;

        public string SearchPattern
        {
            get => _searchPattern;
            set => Set(ref _searchPattern, value);
        }

        public PagedList<WbProductPosChanges> ProductPosChanges
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

        public DataProductPosChangesViewModel(INavigationService navigationService,
            ExcelService excelService)
        {
            NavigationService = navigationService;
            _excelService = excelService;

            _originalProducts = new List<WbProductPosChanges>(DBEntities.GetContext().WbProductPosChanges
                                .OrderByDescending(x => x.Date)
                                .ToList());

            ProductPosChanges = new PagedList<WbProductPosChanges>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands = new PagedListCommands<WbProductPosChanges>(ProductPosChanges);
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
                        return System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
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
                    }, (obj) => (int.TryParse(_article, out _) || string.IsNullOrEmpty(_article)) && _isShowWorking == false));
            }
        }

        public void updateData()
        {
            DateTime? startTimeTemp = _startDate;
            DateTime? endTimeTemp = _endDate;

            if (string.IsNullOrEmpty(_article) && !startTimeTemp.HasValue && !endTimeTemp.HasValue && string.IsNullOrEmpty(_searchPattern))
            {
                _originalProducts = DBEntities.GetContext().WbProductPosChanges
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
                    if (!string.IsNullOrEmpty(_searchPattern))
                    {
                        // Поиск по артикулу и поисковому запросу
                        _originalProducts = DBEntities.GetContext().WbProductPosChanges
                       .Where(x => x.WbProductID == article && x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                       .OrderByDescending(x => x.Date)
                       .ToList();

                        _originalProducts = _originalProducts
                        .Where(x => x.SearchPattern.ToLower().Contains(_searchPattern.ToLower()))
                        .ToList();
                    }
                    else
                    {
                        // Поиск по артикулу без поискового запроса
                        _originalProducts = DBEntities.GetContext().WbProductPosChanges
                        .Where(x => x.WbProductID == article && x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                        .OrderByDescending(x => x.Date)
                        .ToList();
                    }
                }
                else
                {
                    // Поиск по поисковому запросу
                    if (!string.IsNullOrEmpty(_searchPattern))
                    {
                        // Поиск по артикулу и поисковому запросу
                        _originalProducts = DBEntities.GetContext().WbProductPosChanges
                       .Where(x => x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                       .OrderByDescending(x => x.Date)
                       .ToList();

                        _originalProducts = _originalProducts
                        .Where(x => x.SearchPattern.IndexOf(_searchPattern, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                    }
                    // Поиск по дате
                    else
                    {
                        _originalProducts = DBEntities.GetContext().WbProductPosChanges
                        .Where(x => x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                        .OrderByDescending(x => x.Date)
                        .ToList();
                    }
                }
            }

            ProductPosChanges = new PagedList<WbProductPosChanges>(_originalProducts, _pageSizes[_selectedIndex]);
            PagedCommands.Instance = ProductPosChanges;
        }

        private WbProductPosChanges _selectedEntity;

        public WbProductPosChanges SelectedEntity
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
                            DateTime now = DateTime.Now.Date;
                            IsExportWorking = true;
                            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>();
                            data.Add("Артикул", new List<object>());
                            data.Add("Ссылка", new List<object>());
                            data.Add("Дата", new List<object>());
                            data.Add("Название", new List<object>());
                            data.Add("Страница", new List<object>());
                            data.Add("Позиция", new List<object>());
                            data.Add("Поисковой запрос", new List<object>());
                            data.Add("Тип поискового запроса", new List<object>());

                            foreach (var product in _originalProducts)
                            {
                                data["Артикул"].Add(product.WbProductID);
                                data["Ссылка"].Add($@"https://www.wildberries.ru/catalog/{product.WbProductID}/detail.aspx");
                                data["Дата"].Add(product.Date.ToString("dd/MM/yy"));
                                data["Название"].Add(product.WbProduct.Name);
                                data["Страница"].Add(product.Page);
                                data["Позиция"].Add(product.Position);
                                data["Поисковой запрос"].Add(product.SearchPattern);
                                data["Тип поискового запроса"].Add(product.SearchPatternType.Name);
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

        public bool IsShowWorking
        {
            get => _isShowWorking;
            set => Set(ref _isShowWorking, value);
        }

        public PagedListCommands<WbProductPosChanges> PagedCommands
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