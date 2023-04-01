using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel
{
    public class LoadingViewModel : Infastructure.Core.ViewModelBase
    {
        #region Properties

        private string _fact;
        private readonly double _incrementTimeMs = 50;
        private string _state;
        private double _value = 0;
        private double _maximum = 120;
        private readonly bool _isRunAsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        private readonly Version _version = Services.Updater.GetCurrentVersion();
        private readonly Random _rnd = new Random();

        public double Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        public double Maximum
        {
            get => _maximum;
            set => Set(ref _maximum, value);
        }

        public string Fact
        {
            get => _fact;
            set => Set(ref _fact, value);
        }

        public string State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        public string Version
        {
            get => "Версия: " + _version;
        }

        #endregion Properties

        #region fields

        private readonly List<string> _facts = File.ReadAllLines(@"Resource\Other\LoadingFacts.txt").ToList();
        private Window _currentView;
        private Window _startView;

        #endregion fields

        public LoadingViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private async void Load()
        {
            await Task.Delay(0);
            State = "Получение представлений...";
            GetViews();
            await PrettyIncrementValue(12);

            State = "Проверка настроек...";
            bool hasDbConnectionString = !string.IsNullOrEmpty(Properties.Settings.Default.ConnectionString);
            await PrettyIncrementValue(12);

            State = "Проверка подключения к базе данных...";
            bool hasDbConnection = hasDbConnectionString ? DBEntities.CheckConnectionString(Properties.Settings.Default.ConnectionString) : false;
            await PrettyIncrementValue(36);

            State = "Соеденение с базой данных...";
            if (hasDbConnection)
            {
                DBEntities.SetContext(Properties.Settings.Default.ConnectionString);
            }
            await PrettyIncrementValue(36);

            State = "Проверка наличия пользователей...";
            bool hasUsersInDb = HasUsersInDb();
            await PrettyIncrementValue(24);

            State = "Настройка навигации...";
            SetNavigation(hasDbConnectionString, hasDbConnection, hasUsersInDb);
            await PrettyIncrementValue(24);

            State = "Готово";

            EndLoading();
        }

        private void GetViews()
        {
            _currentView = App.ServiceProvider.GetService(typeof(View.LoadingView)) as Window;

            _startView = App.ServiceProvider.GetService(typeof(View.StartView)) as Window;
        }

        private async Task PrettyIncrementValue(double value)
        {
            double step = value / _incrementTimeMs;
            for (int i = 0; i < _incrementTimeMs; i++)
            {
                Value += step;
                await Task.Delay(1);
            }
        }

        private RelayCommand _windowLoadedCommand;
        private readonly INavigationService _navigationService;

        public RelayCommand WindowLoadedCommand
        {
            get
            {
                return _windowLoadedCommand ??
                    (_windowLoadedCommand = new RelayCommand(obj =>
                    {
                        ShowFacts();
                        Load();
                    }));
            }
        }

        private void ShowFacts()
        {
            LoadFacts();
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            timer.Tick += ChangeFact_Tick;
            ChangeFact_Tick(null, null);
            timer.Start();
        }

        private void LoadFacts()
        {
        }

        private void ChangeFact_Tick(object sender, EventArgs e)
        {
            Fact = _facts[_rnd.Next(0, _facts.Count)];
        }

        private bool HasUsersInDb()
        {
            return DBEntities.GetContext().User.Count() > 0;
        }

        private void SetNavigation(bool hasDbConnectionString, bool hasDbConnection, bool hasUsersInDb)
        {
            if (hasDbConnectionString && hasDbConnection)
            {
                if (hasUsersInDb)
                {
                    _navigationService.NavigateTo<AuthorizationViewModel>();
                }
                else
                {
                    if (_isRunAsAdmin)
                    {
                        _navigationService.NavigateTo<AdminRegistrationViewModel>();
                    }
                    else
                    {
                        _navigationService.NavigateTo<NeedSettingErrorViewModel>();
                    }
                }
            }
            else
            {
                if (_isRunAsAdmin)
                {
                    _navigationService.NavigateTo<SettingDatabaseServerViewModel>();
                }
                else
                {
                    _navigationService.NavigateTo<NeedSettingErrorViewModel>();
                }
            }
        }

        private void EndLoading()
        {
            _currentView.Hide();
            _startView.Show();
            _currentView.Close();
        }
    }
}