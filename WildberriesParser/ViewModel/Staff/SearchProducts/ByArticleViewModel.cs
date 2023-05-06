using System.Collections.Generic;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.ObjectModel;
using WildberriesParser.Infastructure.Commands;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;
using System.Windows.Documents;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using SimpleWbApi;
using SimpleWbApi.Model;
using WildberriesParser.ViewModel.Staff.Data;

namespace WildberriesParser.ViewModel.Staff.SearchProducts
{
    public class ByArticleViewModel : ViewModelBase
    {
        public string Title { get; } = "Поиск по артикулу";

        private string _article;

        private string _result = "Htess";
        private bool _isSearchWorking;
        private bool _isExportWorking;
        private int _selectedIndex = 0;
        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };
        private ExcelService _excelService;
        private ILoggerService _loggerService;

        private ObservableCollection<WbCard> _originalProducts = new ObservableCollection<WbCard>();

        private PagedList<WbCard> _products;
        private PagedListCommands<WbCard> _pagedCommands;

        public PagedList<WbCard> Products
        {
            get => _products;
            set { Set(ref _products, value); }
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

        public ByArticleViewModel(INavigationService navigationService,
            WbAPI wbAPI,
            ExcelService excelService, ILoggerService loggerService)
        {
            NavigationService = navigationService;
            _wbApi = wbAPI;
            _excelService = excelService;
            _loggerService = loggerService;

            _products = new PagedList<WbCard>(_originalProducts, _pageSizes[_selectedIndex]);
            _pagedCommands = new PagedListCommands<WbCard>(_products);
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;
        private readonly WbAPI _wbApi;

        private async Task<WbCard> _Search()
        {
            WbResponse response = await _wbApi.GetCardFromSiteByArticle(Int32.Parse(_article));
            WbCard product = null;
            if (response?.Data?.Products.Count > 0)
            {
                product = response.Data.Products[0];
            }
            return product;
        }

        private WbCard _selectedEntity;

        public WbCard SelectedEntity
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
                                    vm.Article = SelectedEntity.id.ToString();
                                    vm.updateData();
                                    _navigationService.NavigateTo<Data.DataProductChangesViewModel>();
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
                                    vm.Article = SelectedEntity.id.ToString();
                                    vm.updateData();
                                    _navigationService.NavigateTo<Data.SellingProductViewModel>();
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
                                    vm.Article = SelectedEntity.id.ToString();
                                    vm.updateData();
                                    _navigationService.NavigateTo<Data.DataProductPosChangesViewModel>();
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

        private void AddProductToDB(WbCard wbProduct)
        {
            if (DBEntities.GetContext().WbBrand.FirstOrDefault(b => b.ID == wbProduct.brandId) == null)
            {
                DBEntities.GetContext().WbBrand.Add(new WbBrand
                {
                    Name = wbProduct.brand,
                    ID = wbProduct.brandId
                });

                DBEntities.GetContext().SaveChanges();
            }

            WbProduct findProduct = DBEntities.GetContext().WbProduct.FirstOrDefault(x => x.ID == wbProduct.id);
            DateTime now = DateTime.Now.Date;

            if (findProduct == null)
            {
                var newProduct = new Model.Data.WbProduct
                {
                    ID = wbProduct.id,
                    Name = wbProduct.name,
                    WbBrandID = wbProduct.brandId,
                    LastDiscount = wbProduct.sale,
                    LastPriceWithDiscount = wbProduct.salePriceU,
                    LastPriceWithoutDiscount = wbProduct.priceU,
                    LastUpdate = now
                };

                DBEntities.GetContext().WbProduct.Add(newProduct);

                DBEntities.GetContext().WbProductChanges.Add(new WbProductChanges
                {
                    WbProduct = newProduct,
                    Date = now,
                    Discount = wbProduct.sale,
                    PriceWithDiscount = wbProduct.salePriceU,
                    PriceWithoutDiscount = wbProduct.priceU,
                    Quantity = wbProduct.Quantity ?? 0
                });

                DBEntities.GetContext().SaveChanges();
            }
            else
            {
                if (findProduct.LastUpdate.Date < now)
                {
                    findProduct.LastUpdate = now;
                    findProduct.LastDiscount = wbProduct.sale;
                    findProduct.LastPriceWithDiscount = wbProduct.salePriceU;
                    findProduct.LastPriceWithoutDiscount = wbProduct.priceU;

                    DBEntities.GetContext().WbProductChanges.Add(new WbProductChanges
                    {
                        WbProduct = findProduct,
                        Date = now,
                        Discount = wbProduct.sale,
                        PriceWithDiscount = (int)wbProduct.salePriceU,
                        PriceWithoutDiscount = (int)wbProduct.priceU,
                        Quantity = wbProduct.Quantity ?? 0
                    });

                    DBEntities.GetContext().SaveChanges();
                }
            }
        }

        public AsyncRelayCommand SearchCommand
        {
            get
            {
                return _SearchCommand ??
                    (_SearchCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            IsSearchWorking = true;
                            try
                            {
                                WbCard product = await _Search();

                                if (product != null)
                                {
                                    _originalProducts.Add(product);

                                    Products = new PagedList<WbCard>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                                    _pagedCommands.Instance = Products;

                                    AddProductToDB(product);
                                }
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error(ex.Message);
                            }

                            IsSearchWorking = false;
                        }).Task;
                    },
                    (obj) => !IsSearchWorking && !string.IsNullOrEmpty(_article) && Int32.TryParse(_article, out int _)
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
                        Products = new PagedList<WbCard>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                        PagedCommands.Instance = Products;
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
                            IsExportWorking = true;

                            try
                            {
                                _exportWork();
                            }
                            catch (Exception ex)
                            {
                                Helpers.MessageBoxHelper.Error($"Во время экспорта произошла ошибка:\n{ex.Message}");
                            }
                            IsExportWorking = false;
                        }).Task;
                    },
                    (obj) => !IsExportWorking
                    ));
            }
        }

        private void _exportWork()
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
            data.Add("Бренд", new List<object>());
            data.Add("Остатки", new List<object>());
            data.Add("Скидка", new List<object>());
            data.Add("Цена без скидки", new List<object>());
            data.Add("Цена со скидкой", new List<object>());
            data.Add("Рейтинг", new List<object>());
            data.Add("Отзывы", new List<object>());
            data.Add("Промо текст", new List<object>());

            foreach (var product in _originalProducts)
            {
                data["Артикул"].Add(product.id);
                data["Ссылка"].Add($@"https://www.wildberries.ru/catalog/{product.id}/detail.aspx");
                data["Название"].Add(product.name);
                data["Бренд"].Add(product.brand);
                data["Остатки"].Add(product.Quantity);
                data["Скидка"].Add(product.sale);
                data["Цена без скидки"].Add(product.priceU);
                data["Цена со скидкой"].Add(product.salePriceU);
                data["Рейтинг"].Add(product.rating);
                data["Отзывы"].Add(product.feedbacks);
                data["Промо текст"].Add(product.promoTextCat);
            }

            var columns = ExcelColumn.FromDictionary(data);
            columns[1].CellFormatType = ExcelCellFormatType.Hyperlink;
            _excelService.Export(columns, path, "Карточки");
            if (Helpers.MessageBoxHelper.Question("Экcпортировано успешно! Открыть файл?") == Helpers.MessageBoxHelperResult.YES)
            {
                Process.Start(path);
            }
        }

        public bool IsSearchWorking
        {
            get => _isSearchWorking;
            set => Set(ref _isSearchWorking, value);
        }

        public string Result
        {
            get => _result;
            set => Set(ref _result, value);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                Set(ref _selectedIndex, value);
                Products.PageSize = _pageSizes[value];
            }
        }

        public bool IsExportWorking
        {
            get => _isExportWorking;
            set => Set(ref _isExportWorking, value);
        }

        public PagedListCommands<WbCard> PagedCommands
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