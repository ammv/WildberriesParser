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

    public class TraceProductsViewModel : ViewModelBase
    {
        public string Title { get; } = "Отследить товар";

        private TraceType _selectedTraceType = TraceType.BrandName;
        private Dictionary<TraceType, SearchPatternType> _dbSearchTraceTypes = new Dictionary<TraceType, SearchPatternType>();

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

        private WbAPI _wbApi;

        public TraceProductsViewModel(INavigationService navigationService,
            ExcelService excelService, WbAPI wbAPI)
        {
            NavigationService = navigationService;
            _wbApi = wbAPI;
            _excelService = excelService;

            _productPosChanges = new PagedList<WbProductPosChanges>(_originalProducts, _pageSizes[_selectedIndex]);
            _pagedCommands = new PagedListCommands<WbProductPosChanges>(_productPosChanges);

            int i = 1;
            foreach (var item in DBEntities.GetContext().SearchPatternType.ToList())
            {
                _dbSearchTraceTypes.Add((TraceType)i++, item);
            }
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;

        private async Task<List<WbCard>> _GetProducts()
        {
            var responses = await _wbApi.GetCardsFromSiteBySearch(_searchPattern, _tracePageCount[_selectedTracePageIndex]);

            List<WbCard> cards = new List<WbCard>();

            foreach (var response in responses)
            {
                if (response.Data?.Products?.Count > 0)
                {
                    cards.AddRange(response.Data.Products);
                }
            }
            return cards;
        }

        private List<WbProductPosChanges> _Trace(List<WbCard> cards)
        {
            List<WbProductPosChanges> wbProductPosChanges = new List<WbProductPosChanges>();

            DateTime now = DateTime.Now;

            for (int i = 0; i < cards.Count; i++)
            {
                switch (_selectedTraceType)
                {
                    case TraceType.BrandName:
                        if (!cards[i].brand.Equals(_tracedValue))
                        {
                            continue;
                        }
                        break;

                    case TraceType.BrandID:
                        if (cards[i].brandId != int.Parse(_tracedValue))
                        {
                            continue;
                        }
                        break;

                    case TraceType.ProductID:
                        if (cards[i].id != int.Parse(_tracedValue))
                        {
                            continue;
                        }
                        break;
                }

                wbProductPosChanges.Add(new WbProductPosChanges
                {
                    WbProductID = cards[i].id,
                    Date = now,
                    SearchPattern = _searchPattern,
                    Page = (int)Math.Ceiling((i + 1.0) / 100),
                    Position = (i + 1) % 100,
                    SearchPatternType = _dbSearchTraceTypes[_selectedTraceType]
                });
            }

            return wbProductPosChanges;
        }

        private async Task AddPosChangesToDB(IEnumerable<WbProductPosChanges> data)
        {
            DBEntities.GetContext().WbProductPosChanges.AddRange(data);
            await DBEntities.GetContext().SaveChangesAsync();
        }

        private async Task AddProductsToDB(IEnumerable<WbCard> cards)
        {
            DateTime now = DateTime.Now.Date;

            List<int> checkedProductID = new List<int>();
            List<int> checkedBrandID = new List<int>();

            foreach (var product in cards)
            {
                if (checkedProductID.Contains(product.id))
                {
                    continue;
                }

                WbBrand brand = await DBEntities.GetContext().WbBrand.FirstOrDefaultAsync(b => b.ID == product.brandId);
                if (!checkedBrandID.Contains(product.brandId) && brand == null)
                {
                    DBEntities.GetContext().WbBrand.Add(new WbBrand
                    {
                        Name = product.brand,
                        ID = product.brandId
                    });
                    checkedBrandID.Add(product.brandId);
                }

                Model.Data.WbProduct findProduct = await DBEntities.GetContext().WbProduct.FirstOrDefaultAsync((System.Linq.Expressions.Expression<Func<WbProduct, bool>>)(x => x.ID == product.id));
                if (findProduct == null)
                {
                    var newProduct = new Model.Data.WbProduct
                    {
                        ID = product.id,
                        Name = product.name,
                        WbBrandID = product.brandId,
                        LastUpdate = now
                    };

                    DBEntities.GetContext().WbProduct.Add(newProduct);
                    checkedProductID.Add(product.id);

                    DBEntities.GetContext().WbProductChanges.Add(new WbProductChanges
                    {
                        WbProduct = newProduct,
                        Date = now,
                        Discount = product.sale,
                        PriceWithDiscount = (int)product.salePriceU,
                        PriceWithoutDiscount = (int)product.priceU,
                        Quantity = product.Quantity ?? 0
                    });
                }
                else
                {
                    if (findProduct.LastUpdate.Date < now)
                    {
                        findProduct.LastUpdate = now;

                        DBEntities.GetContext().WbProductChanges.Add(new WbProductChanges
                        {
                            WbProduct = findProduct,
                            Date = now,
                            Discount = product.sale,
                            PriceWithDiscount = (int)product.salePriceU,
                            PriceWithoutDiscount = (int)product.priceU,
                            Quantity = product.Quantity ?? 0
                        });
                    }
                }
            }

            try
            {
                DBEntities.GetContext().SaveChanges();

                //DBEntities.GetContext().GetValidationErrors().ToList()[0].ValidationErrors.ToList()[0].
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ValidationErrors"))
                {
                    WriteEfErrosAndOpen(ex);
                }
                else
                {
                    Helpers.MessageBoxHelper.Error(ex.Message);
                }
            }
        }

        private static void WriteEfErrosAndOpen(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            var errorsEntities = DBEntities.GetContext().GetValidationErrors().ToList();
            foreach (var errorEntity in errorsEntities)
            {
                sb.Append($"Сущность: {errorEntity.Entry.Entity.GetType().Name}\n");
                sb.Append($"Ошибки: \n");
                foreach (var item in errorEntity.ValidationErrors)
                {
                    sb.Append($"\t - {item.ErrorMessage}\n");
                }
            }
            //List<string>
            File.WriteAllText("ef_erros.txt",
                sb.ToString());

            Helpers.MessageBoxHelper.Error(ex.Message);
            if (Helpers.MessageBoxHelper.Question("Ошибки записаны в ef_erros.txt. Открыть?") == Helpers.MessageBoxHelperResult.YES)
            {
                Process.Start("ef_erros.txt");
            }
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

                            var products = (await _GetProducts()).OrderBy(x => x.__sort);

                            var response = await _wbApi.GetCardsByArticleFromSite(products.Select(x => x.id).ToList());

                            await AddProductsToDB(response.Data.Products);

                            List<WbProductPosChanges> result = null;
                            if (products.Count() > 0)
                            {
                                result = _Trace(response.Data.Products);

                                foreach (var item in result)
                                {
                                    _originalProducts.Add(item);
                                }

                                if (result.Count > 0)
                                {
                                    ProductPosChanges = new PagedList<WbProductPosChanges>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                                    _pagedCommands.Instance = ProductPosChanges;
                                }
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
                            _selectedTraceType = (TraceType)(int)obj;
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
                                data["Дата"].Add(product.Date.ToString("G"));
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