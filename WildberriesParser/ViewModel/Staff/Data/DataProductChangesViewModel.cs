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

namespace WildberriesParser.ViewModel.Staff.Data
{
    public class DataProductChangesViewModel : ViewModelBase
    {
        public string Title { get; } = "Отчет по изменению товара";

        private TraceType _selectedTraceType = TraceType.BrandName;
        private Dictionary<TraceType, SearchPatternType> _dbSearchTraceTypes = new Dictionary<TraceType, SearchPatternType>();

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

        public AsyncRelayCommand ShowCommand
        {
            get
            {
                return _ShowCommand ??
                    (_ShowCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            IsShowWorking = true;

                            DateTime? startTimeTemp = _startDate;
                            DateTime? endTimeTemp = _endDate;

                            if (string.IsNullOrEmpty(_article) && !startTimeTemp.HasValue && !endTimeTemp.HasValue)
                            {
                                _originalProducts = await DBEntities.GetContext().WbProductChanges
                                .OrderByDescending(x => x.Date)
                                .ToListAsync();
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
                                    _originalProducts = await DBEntities.GetContext().WbProductChanges
                                    .Where(x => x.WbProductID == article && x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                                    .OrderByDescending(x => x.Date)
                                    .ToListAsync();
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(_article))
                                    {
                                        _originalProducts = await DBEntities.GetContext().WbProductChanges
                                        .Where(x => x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                                        .OrderByDescending(x => x.Date)
                                        .ToListAsync();
                                    }
                                    else
                                    {
                                        _originalProducts = await DBEntities.GetContext().WbProductChanges
                                        .Where(x => x.WbProduct.Name.Contains(_article) && x.Date >= startTimeTemp && x.Date <= endTimeTemp)
                                        .OrderByDescending(x => x.Date)
                                        .ToListAsync();
                                    }
                                }
                            }

                            ProductPosChanges = new PagedList<WbProductChanges>(_originalProducts, _pageSizes[_selectedIndex]);
                            PagedCommands.Instance = ProductPosChanges;

                            IsShowWorking = false;

                            //await AddPosChangesToDB(result);
                        }).Task;
                    }));
            }
        }

        private DateTime? _startDate;
        private DateTime? _endDate;

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

                            foreach (var product in _originalProducts)
                            {
                                data["Артикул"].Add(product.WbProductID);
                                data["Ссылка"].Add($@"https://www.wildberries.ru/catalog/{product.WbProductID}/detail.aspx");
                                data["Дата"].Add(product.Date.ToString("G"));
                                data["Название"].Add(product.WbProduct.Name);
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