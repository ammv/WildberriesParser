using System.Windows;

namespace WildberriesParser.Helpers.MessageBoxes
{
    /// <summary>
    /// Логика взаимодействия для ErrorMessageBox.xaml
    /// </summary>
    public partial class ErrorMessageBox : Window
    {
        public string ErrorMessage { get; set; }

        public ErrorMessageBox(string errorMessage)
        {
            ErrorMessage = errorMessage;
            InitializeComponent();
            DataContext = this;
        }

        public MessageBoxHelperResult ShowBox()
        {
            ShowDialog();
            return MessageBoxHelperResult.OK;
        }
    }
}