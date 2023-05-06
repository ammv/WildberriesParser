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
    internal class TaskTraceChangesProductViewModel : ViewModelWithWindowButtonsBase
    {
        private string _taskName;
        private string _searchValue;
        private List<SearchPatternType> _searchPatternTypes;
        private SearchPatternType _selectedSearchPatternType;
        private DateTime? _endDate;

        public TaskTraceChangesProductViewModel(CollectorService collectorService)
        {
            SearchPatternTypes = DBEntities.GetContext().SearchPatternType.ToList();
            this.collectorService = collectorService;
        }

        private AsyncRelayCommand _AddTaskCommand;
        private readonly CollectorService collectorService;

        public AsyncRelayCommand AddTaskCommand
        {
            get
            {
                return _AddTaskCommand ??
                    (_AddTaskCommand = new AsyncRelayCommand
                    ((obj) =>
                    {
                        return App.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            CollectorTask task = null;
                            task = new CollectorTask
                            {
                                TypeID = 2,
                                StatusID = 2,
                                Duration = (DateTime.Now - EndDate.Value.Date).Days,
                                Name = TaskName,
                                CreatedBy = App.CurrentUser.ID,
                                TaskDetails = $"SearchValue={SearchValue};SelectedSearchPatternType={SelectedSearchPatternType.Name}"
                            };

                            try
                            {
                                if (await collectorService.StartTask(task))
                                {
                                    DBEntities.GetContext().CollectorTask.Add(task);
                                    await DBEntities.GetContext().SaveChangesAsync();
                                    Helpers.MessageBoxHelper.Information("Задача добавлена!");
                                }
                                else
                                {
                                    Helpers.MessageBoxHelper.Error("Не удалось добавить задачу");
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

        public string TaskName
        {
            get => _taskName;
            set => Set(ref _taskName, value);
        }

        public string SearchValue
        {
            get => _searchValue;
            set => Set(ref _searchValue, value);
        }

        public List<SearchPatternType> SearchPatternTypes
        {
            get => _searchPatternTypes;
            set => Set(ref _searchPatternTypes, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => Set(ref _endDate, value);
        }

        public SearchPatternType SelectedSearchPatternType
        {
            get => _selectedSearchPatternType;
            set => Set(ref _selectedSearchPatternType, value);
        }
    }
}