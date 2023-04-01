using System;
using WildberriesParser.Infastructure.Commands.Base;

namespace WildberriesParser.Infastructure.Commands
{
    public class RelayCommand : Command
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            bool result = canExecute?.Invoke(parameter) ?? true;
            return result;
        }

        public override void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}