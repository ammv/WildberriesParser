using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using WildberriesParser.ViewModel.Admin;

namespace WildberriesParser.ViewModel.Staff
{
    public enum TraceType
    {
        BrandName = 1,
        BrandID,
        ProductID
    }

    public class TraceProductViewModel : ViewModelBase
    {
        public string Title { get; } = "Отследить товар";

        private TraceType _selectedTraceType = TraceType.BrandID;

        private string _searchPattern;
        private string _tracedValue;
        private bool _isExportWorking;
        private bool _isTraceWorking;

        private ExcelService _excelService;

        // Для пагинации
        private int _selectedIndex = 0;

        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        // до какой страницы отслеживать
        private int _selectedTracePageIndex = 0;

        private int[] _tracePageCount = new int[] { 1, 3, 5, 10, 15 };

        private ObservableCollection<WbProductPosChanges> _originalProducts = new ObservableCollection<WbProductPosChanges>();

        private PagedList<WbProductPosChanges> _productPosChanges;
        private PagedListCommands<WbProductPosChanges> _pagedCommands;

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

        public string SearchPattern
        {
            get => _searchPattern;
            set
            {
                Set(ref _searchPattern, value);
            }
        }

        public TraceProductViewModel(INavigationService navigationService, WbParser wbParser,
            ExcelService excelService, WbRequesterService wbRequesterService)
        {
            NavigationService = navigationService;
            _wbRequesterService = wbRequesterService;
            _wbParser = wbParser;
            _excelService = excelService;

            _productPosChanges = new PagedList<WbProductPosChanges>(_originalProducts, _pageSizes[_selectedIndex]);
            _pagedCommands = new PagedListCommands<WbProductPosChanges>(_productPosChanges);
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;
        private readonly WbRequesterService _wbRequesterService;
        private readonly WbParser _wbParser;

        private async Task<List<WbProduct>> _GetProducts()
        {
            var responsesJson = await _wbRequesterService.GetProductCardsBySearch(_searchPattern, _tracePageCount[_selectedTracePageIndex]);
            List<WbProduct> products = new List<WbProduct>();

            foreach (var responseJson in responsesJson)
            {
                var response = _wbParser.ParseResponse(responseJson);
                if (response.Data?.Products?.Count > 0)
                {
                    products.AddRange(response.Data.Products);
                }
            }

            return products;
        }

        private List<WbProductPosChanges> _Trace(List<WbProduct> products)
        {
            List<WbProductPosChanges> wbProductPosChanges = new List<WbProductPosChanges>();

            DateTime now = DateTime.Now.Date;

            for (int i = 0; i < products.Count; i++)
            {
                switch (_selectedTraceType)
                {
                    case TraceType.BrandName:
                        if (!products[i].brand.Equals(_tracedValue))
                        {
                            continue;
                        }
                        break;

                    case TraceType.BrandID:
                        if (products[i].WbBrandID != int.Parse(_tracedValue))
                        {
                            continue;
                        }
                        break;

                    case TraceType.ProductID:
                        if (products[i].id != int.Parse(_tracedValue))
                        {
                            continue;
                        }
                        break;
                }

                wbProductPosChanges.Add(new WbProductPosChanges
                {
                    WbProductID = products[i].id,
                    Date = now,
                    SearchPattern = _searchPattern,
                    Page = (int)Math.Ceiling((i + 1.0) / 100),
                    Position = (i + 1) % 100
                });
            }

            return wbProductPosChanges;
        }

        private async Task AddPosChangesToDB(IEnumerable<WbProductPosChanges> data)
        {
            DBEntities.GetContext().WbProductPosChanges.AddRange(data);
            await DBEntities.GetContext().SaveChangesAsync();
        }

        public AsyncRelayCommand TraceCommand
        {
            get
            {
                return _SearchCommand ??
                    (_SearchCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            IsTraceWorking = true;

                            var products = await _GetProducts();
                            List<WbProductPosChanges> result = null;
                            if (products.Count > 0)
                            {
                                result = _Trace(products);

                                foreach (var item in result)
                                {
                                    _originalProducts.Add(item);
                                }

                                ProductPosChanges = new PagedList<WbProductPosChanges>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                                _pagedCommands.Instance = ProductPosChanges;
                            }

                            IsTraceWorking = false;

                            await AddPosChangesToDB(result);
                        }).Task;
                    },
                    (obj) => !IsTraceWorking &&
                        !string.IsNullOrEmpty(_searchPattern) &&
                        !string.IsNullOrEmpty(_tracedValue) &&
                        (_selectedTraceType == TraceType.BrandID ||
                        _selectedTraceType == TraceType.ProductID ?
                        int.TryParse(_tracedValue, out _) : true)
                    ));
            }
        }

        private RelayCommand _clearCommand;

        public RelayCommand ClearCommand
        {
            get
            {
                return _clearCommand ??
                    (_clearCommand = new RelayCommand
                    ((obj) =>
                    {
                        _originalProducts.Clear();
                        ProductPosChanges = new PagedList<WbProductPosChanges>(_originalProducts, _pageSizes[_selectedIndex]);
                        PagedCommands.Instance = ProductPosChanges;
                    }
                    ));
            }
        }

        private AsyncRelayCommand _selectTraceTypeCommand;

        public AsyncRelayCommand SelectTraceTypeCommand
        {
            get
            {
                return _selectTraceTypeCommand ??
                    (_selectTraceTypeCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(() =>
                        {
                        }).Task;
                    }
                    ));
            }
        }

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
                            IsExportWorking = true;
                            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>();
                            data.Add("Артикул", new List<object>());

                            foreach (var product in _originalProducts)
                            {
                                data["Артикул"].Add(product.WbProductID);
                            }
                            try
                            {
                                _excelService.Export(data, path);
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

        public bool IsTraceWorking
        {
            get => _isTraceWorking;
            set => Set(ref _isTraceWorking, value);
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

        public int SelectedTracePageIndex
        {
            get => _selectedTracePageIndex;
            set => Set(ref _selectedTracePageIndex, value);
        }

        public int[] TracePageCount
        {
            get => _tracePageCount;
            set => Set(ref _tracePageCount, value);
        }

        public string TracedValue
        {
            get => _tracedValue;
            set => Set(ref _tracedValue, value);
        }

        public TraceType SelectedTraceType
        {
            get => _selectedTraceType;
            set => Set(ref _selectedTraceType, value);
        }
    }
}