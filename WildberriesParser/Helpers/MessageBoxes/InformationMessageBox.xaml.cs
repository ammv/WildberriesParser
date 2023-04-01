using System.Windows;

namespace WildberriesParser.Helpers.MessageBoxes
{
    /// <summary>
    /// Логика взаимодействия для QuestionMessageBox.xaml
    /// </summary>
    public partial class InformationMessageBox : Window
    {
        public string InformationMessage { get; set; }
        public MessageBoxHelperResult Result { get; set; } = MessageBoxHelperResult.NO;

        public InformationMessageBox(string informationMessage)
        {
            InformationMessage = informationMessage;
            InitializeComponent();
            DataContext = this;
        }

        public MessageBoxHelperResult ShowBox()
        {
            ShowDialog();
            return Result;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxHelperResult.OK;
            Close();
        }
    }
}