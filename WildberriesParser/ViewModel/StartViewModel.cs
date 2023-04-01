using System.Windows;
using WildberriesParser.Infastructure.Commands;
using WildberriesParser.Services;

namespace WildberriesParser.ViewModel
{
    internal class StartViewModel : Infastructure.Core.ViewModelBase
    {
        private WindowState _windowState = WindowState.Normal;

        private INavigationService _navigationService;

        public WindowState WindowState
        {
            get => _windowState;
            set => Set(ref _windowState, value);
        }

        public StartViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private RelayCommand _closeWindowCommand;

        public RelayCommand CloseWindowCommand
        {
            get
            {
                return _closeWindowCommand ??
                    (_closeWindowCommand = new RelayCommand
                    ((obj) =>
                    {
                        if (Helpers.MessageBoxHelper.Question("Вы уверены, что хотите выйти?") == Helpers.MessageBoxHelperResult.YES)
                        {
                            (obj as Window).Close();
                        }
                    }
                    ));
            }
        }

        private RelayCommand _wrapWindowCommand;

        public RelayCommand WrapWindowCommand
        {
            get
            {
                return _wrapWindowCommand ??
                    (_wrapWindowCommand = new RelayCommand
                    ((obj) =>
                    {
                        WindowState = WindowState.Minimized;
                    }
                    ));
            }
        }

        public INavigationService NavigationService
        {
            get => _navigationService;
            set => Set(ref _navigationService, value);
        }
    }
}