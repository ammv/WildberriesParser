using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using System;
using System.Linq;
using System.Windows;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;
using WildberriesParser.Services;

namespace WildberriesParser
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        public static User CurrentUser { get; set; }

        public App()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            ConfigureWbServices(services);

            ConfigureOtherServices(services);
            ConfigureBaseServices(services);
            ConfigureAdminServices(services);
            ConfigureStaffServices(services);
        }

        private static void ConfigureWbServices(IServiceCollection services)
        {
            services.AddSingleton<Services.WbRequesterService>();
            services.AddSingleton<Services.WbParser>();
            services.AddSingleton<Services.Updater>();
        }

        private static void ConfigureStaffServices(IServiceCollection services)
        {
            // Configurere Staff services
            services.AddTransient<ViewModel.Staff.StaffMainViewModel>();

            services.AddSingleton<ViewModel.Staff.SettingsViewModel>();
            services.AddSingleton<ViewModel.Staff.SearchProductsViewModel>();
            services.AddSingleton<ViewModel.Staff.AutomatizationViewModel>();
            services.AddSingleton<ViewModel.Staff.TraceProductsViewModel>();

            services.AddTransient(provider => new View.Staff.StaffMainView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.StaffMainViewModel>()
            });

            services.AddSingleton(provider => new View.Staff.SettingsView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.SettingsViewModel>()
            });

            services.AddSingleton(provider => new View.Staff.SearchProductsView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.SearchProductsViewModel>()
            });

            services.AddSingleton(provider => new View.Staff.AutomatizationView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.AutomatizationViewModel>()
            });

            services.AddSingleton(provider => new View.Staff.TraceProductsView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.TraceProductsViewModel>()
            });

            // Configurere Staff.SearchProducts services.
            services.AddSingleton<ViewModel.Staff.SearchProducts.ByArticleViewModel>();
            services.AddSingleton<ViewModel.Staff.SearchProducts.BySearchViewModel>();

            services.AddTransient(provider => new View.Staff.SearchProducts.ByArticleView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.SearchProducts.ByArticleViewModel>()
            });

            services.AddTransient(provider => new View.Staff.SearchProducts.BySearchView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.SearchProducts.BySearchViewModel>()
            });
        }

        private static void ConfigureAdminServices(IServiceCollection services)
        {
            // Configurere Admin services
            services.AddTransient<ViewModel.Admin.AdminMainViewModel>();

            services.AddSingleton<ViewModel.Admin.UsersViewModel>();
            services.AddSingleton<ViewModel.Admin.UserAddViewModel>();
            services.AddSingleton<ViewModel.Admin.HistoryViewModel>();
            services.AddSingleton<ViewModel.Admin.SettingsViewModel>();

            services.AddTransient(provider => new View.Admin.AdminMainView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.AdminMainViewModel>()
            });

            services.AddSingleton(provider => new View.Admin.UsersView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.UsersViewModel>()
            });

            services.AddSingleton(provider => new View.Admin.UserAddView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.UserAddViewModel>()
            });

            services.AddSingleton(provider => new View.Admin.HistoryView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.HistoryViewModel>()
            });

            services.AddSingleton(provider => new View.Admin.SettingsView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.SettingsViewModel>()
            });
        }

        private static void ConfigureBaseServices(IServiceCollection services)
        {
            // Configurere base
            services.AddSingleton<ViewModel.LoadingViewModel>();
            services.AddSingleton<ViewModel.StartViewModel>();

            services.AddSingleton<ViewModel.AuthorizationViewModel>();
            services.AddSingleton<ViewModel.AdminRegistrationViewModel>();
            services.AddSingleton<ViewModel.SettingDatabaseServerViewModel>();
            services.AddSingleton<ViewModel.NeedSettingErrorViewModel>();

            services.AddSingleton(provider => new View.LoadingView
            {
                DataContext = provider.GetRequiredService<ViewModel.LoadingViewModel>()
            });

            services.AddTransient(provider => new View.StartView
            {
                DataContext = provider.GetRequiredService<ViewModel.StartViewModel>()
            });

            services.AddSingleton(provider => new View.AuthorizationView
            {
                DataContext = provider.GetRequiredService<ViewModel.AuthorizationViewModel>()
            });

            services.AddSingleton(provider => new View.AdminRegistrationView
            {
                DataContext = provider.GetRequiredService<ViewModel.AdminRegistrationViewModel>()
            });

            services.AddSingleton(provider => new View.SettingDatabaseServerView
            {
                DataContext = provider.GetRequiredService<ViewModel.SettingDatabaseServerViewModel>()
            });

            services.AddSingleton(provider => new View.NeedSettingErrorView
            {
                DataContext = provider.GetRequiredService<ViewModel.NeedSettingErrorViewModel>()
            });
        }

        private static void ConfigureOtherServices(IServiceCollection services)
        {
            // Configurere services
            services.AddSingleton<Services.INavigationService, Services.NavigationService>();
            services.AddSingleton<Services.ILoggerService, Services.DBLoggerService>();
            services.AddSingleton<ViewModel.NotImplementedViewModel>();
            services.AddSingleton<ExcelService>();
            services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider => viewModelType =>
                (ViewModelBase)serviceProvider.GetRequiredService(viewModelType));

            services.AddSingleton(provider => new View.NotImplementedView
            {
                DataContext = provider.GetRequiredService<ViewModel.NotImplementedViewModel>()
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var LoadingView = ServiceProvider.GetService<View.LoadingView>();
            LoadingView.Show();
            base.OnStartup(e);
        }

        private void Test()
        {
            int id = 149785597;
            Model.Data.WbProduct findProduct = DBEntities.GetContext().WbProduct.FirstOrDefault(x => x.ID == id);
            DateTime now = DateTime.Now.Date;

            if (findProduct == null)
            {
                var newProduct = new Model.Data.WbProduct
                {
                    ID = id,
                    Name = "блузка",
                    WbBrandID = 310498088,
                    LastUpdate = now
                };

                DBEntities.GetContext().WbProduct.Add(newProduct);

                var changes = new WbProductChanges
                {
                    WbProductID = id,
                    Date = now,
                    Discount = 24,
                    PriceWithDiscount = 34534,
                    PriceWithoutDiscount = 235345,
                    Quantity = 345
                };

                DBEntities.GetContext().WbProductChanges.Add(changes);

                DBEntities.GetContext().SaveChanges();
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
                        Discount = 2545,
                        PriceWithDiscount = 34536,
                        PriceWithoutDiscount = 23445,
                        Quantity = 3455
                    });

                    DBEntities.GetContext().SaveChanges();
                }
            }
        }
    }
}