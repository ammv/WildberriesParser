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
using System.IO;
using SimpleWbApi;
using SimpleWbApi.Model;
using WildberriesParser.ViewModel.Staff.Data;

namespace WildberriesParser.ViewModel.Staff.SearchProducts
{
    public class BySearchViewModel : ViewModelBase
    {
        public string Title { get; } = "Поиск по запросу";

        private string _searchPattern;
        private bool _isExportWorking;
        private string _result = "Htess";
        private bool _isSearchWorking;
        private ExcelService _excelService;

        private int _selectedIndex = 0;
        private int[] _pageSizes = new int[] { 25, 50, 100, 250 };

        private ObservableCollection<WbCard> _originalProducts = new ObservableCollection<WbCard>();

        private PagedList<WbCard> _products;
        private PagedListCommands<WbCard> _pagedCommands;

        public PagedList<WbCard> Products
        {
            get => _products;
            set { Set(ref _products, value); }
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

        public BySearchViewModel(INavigationService navigationService, WbAPI wbAPI,
            ExcelService excelService)
        {
            NavigationService = navigationService;
            _wbApi = wbAPI;
            _excelService = excelService;

            _products = new PagedList<WbCard>(_originalProducts, _pageSizes[_selectedIndex]);
            _pagedCommands = new PagedListCommands<WbCard>(_products);
        }

        private INavigationService _navigationService;

        private AsyncRelayCommand _SearchCommand;
        private readonly WbAPI _wbApi;

        private async Task<List<WbCard>> _Search()
        {
            List<WbCard> products = new List<WbCard>();
            List<WbResponse> responses = await _wbApi.GetCardsFromSiteBySearch(_searchPattern, 1);
            foreach (var response in responses)
            {
                if (response.Data?.Products.Count > 0)
                {
                    products.AddRange(response.Data.Products);
                }
            }

            return products;
        }

        private async Task AddProductsToDB(IEnumerable<WbCard> wbProducts)
        {
            DateTime now = DateTime.Now.Date;

            List<int> checkedProductID = new List<int>();
            List<int> checkedBrandID = new List<int>();

            foreach (var product in wbProducts)
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

                WbProduct findProduct = await DBEntities.GetContext().WbProduct.FirstOrDefaultAsync(x => x.ID == product.id);
                if (findProduct == null)
                {
                    var newProduct = new Model.Data.WbProduct
                    {
                        ID = product.id,
                        Name = product.name,
                        WbBrandID = product.brandId,
                        LastDiscount = product.sale,
                        LastPriceWithDiscount = product.salePriceU,
                        LastPriceWithoutDiscount = product.priceU,
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
                        findProduct.LastDiscount = product.sale;
                        findProduct.LastPriceWithDiscount = product.salePriceU;
                        findProduct.LastPriceWithoutDiscount = product.priceU;

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
                               var products = await _Search();
                               if (products.Count > 0)
                               {
                                   var response = await _wbApi.GetCardsByArticleFromSite(products.Select(x => x.id).ToList());

                                   foreach (var product in response.Data.Products)
                                   {
                                       _originalProducts.Add(product);
                                   }

                                   Products = new PagedList<WbCard>(_originalProducts.Reverse(), _pageSizes[_selectedIndex]);
                                   _pagedCommands.Instance = Products;

                                   await AddProductsToDB(response.Data.Products);
                               }
                           }
                           catch (Exception ex)
                           {
                               Helpers.MessageBoxHelper.Error(ex.Message);
                           }

                           IsSearchWorking = false;
                       }).Task;
                    },
                    (obj) => !IsSearchWorking && !string.IsNullOrEmpty(_searchPattern)
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
                    }
                    ));
            }
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