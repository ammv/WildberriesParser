using System.Windows;
using WildberriesParser.Infastructure.Commands;

namespace WildberriesParser.Infastructure.Core
{
    public abstract class ViewModelWithWindowButtonsBase : ViewModelBase
    {
        protected WindowState _windowState = WindowState.Maximized;

        protected RelayCommand _closeWindowCommand;

        public virtual RelayCommand CloseWindowCommand
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

        protected RelayCommand _recoverOrUnwrapWindowCommand;

        public virtual RelayCommand RecoverOrUnwrapWindowCommand
        {
            get
            {
                return _recoverOrUnwrapWindowCommand ??
                    (_recoverOrUnwrapWindowCommand = new RelayCommand
                    ((obj) =>
                    {
                        if (_windowState == WindowState.Maximized)
                        {
                            WindowState = WindowState.Normal;
                        }
                        else
                        {
                            WindowState = WindowState.Maximized;
                        }
                    }
                    ));
            }
        }

        protected RelayCommand _wrapWindowCommand;

        public virtual RelayCommand WrapWindowCommand
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

        public virtual WindowState WindowState
        {
            get => _windowState;
            set => Set(ref _windowState, value);
        }
    }
}