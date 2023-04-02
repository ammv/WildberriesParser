﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Model.Data;

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
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configurere base services
            services.AddSingleton<ViewModel.LoadingViewModel>();
            services.AddSingleton<ViewModel.AuthorizationViewModel>();
            services.AddSingleton<ViewModel.AdminRegistrationViewModel>();
            services.AddSingleton<ViewModel.StartViewModel>();
            services.AddSingleton<ViewModel.SettingDatabaseServerViewModel>();
            services.AddSingleton<ViewModel.NeedSettingErrorViewModel>();

            services.AddSingleton<Services.INavigationService, Services.NavigationService>();

            services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider => viewModelType =>
                (ViewModelBase)serviceProvider.GetRequiredService(viewModelType));

            services.AddSingleton(provider => new View.LoadingView
            {
                DataContext = provider.GetRequiredService<ViewModel.LoadingViewModel>()
            });

            services.AddSingleton(provider => new View.AuthorizationView
            {
                DataContext = provider.GetRequiredService<ViewModel.AuthorizationViewModel>()
            });

            services.AddSingleton(provider => new View.AdminRegistrationView
            {
                DataContext = provider.GetRequiredService<ViewModel.AdminRegistrationViewModel>()
            });

            services.AddSingleton(provider => new View.StartView
            {
                DataContext = provider.GetRequiredService<ViewModel.StartViewModel>()
            });

            services.AddSingleton(provider => new View.SettingDatabaseServerView
            {
                DataContext = provider.GetRequiredService<ViewModel.SettingDatabaseServerViewModel>()
            });

            services.AddSingleton(provider => new View.NeedSettingErrorView
            {
                DataContext = provider.GetRequiredService<ViewModel.NeedSettingErrorViewModel>()
            });

            // Configurere Admin services
            services.AddSingleton<ViewModel.Admin.AdminMainViewModel>();
            services.AddSingleton<ViewModel.Admin.UsersViewModel>();
            services.AddSingleton<ViewModel.Admin.HistoryViewModel>();
            services.AddSingleton<ViewModel.Admin.SettingsViewModel>();

            services.AddSingleton(provider => new View.Admin.AdminMainView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.AdminMainViewModel>()
            });

            services.AddSingleton(provider => new View.Admin.UsersView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.UsersViewModel>()
            });

            services.AddSingleton(provider => new View.Admin.HistoryView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.HistoryViewModel>()
            });

            services.AddSingleton(provider => new View.Admin.SettingsView
            {
                DataContext = provider.GetRequiredService<ViewModel.Admin.SettingsViewModel>()
            });

            // Configurere Staff services
            services.AddSingleton<ViewModel.Staff.StaffMainViewModel>();

            services.AddSingleton(provider => new View.Staff.StaffMainView
            {
                DataContext = provider.GetRequiredService<ViewModel.Staff.StaffMainViewModel>()
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var LoadingView = ServiceProvider.GetService<View.LoadingView>();
            LoadingView.Show();
            base.OnStartup(e);
        }
    }
}