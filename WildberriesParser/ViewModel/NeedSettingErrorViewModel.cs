using System.Diagnostics;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Infastructure.Core;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel
{
    internal class NeedSettingErrorViewModel : ViewModelBase
    {
        public NeedSettingErrorViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        private RelayCommand _runAsAdminCommand;
        private readonly INavigationService navigationService;

        public RelayCommand RunAsAdminCommand
        {
            get
            {
                return _runAsAdminCommand ??
                    (_runAsAdminCommand = new RelayCommand
                    ((obj) =>
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
                        proc.StartInfo.UseShellExecute = true;
                        proc.StartInfo.Verb = "runas";
                        if (proc.Start())
                        {
                            System.Windows.Application.Current.Shutdown();
                        }
                        else
                        {
                            Helpers.MessageBoxHelper.Error("Не удалось перезапустить программу от имени администратора");
                        }
                    }
                    ));
            }
        }
    }
}