using System.Windows;

namespace WildberriesParser.Helpers.MessageBoxes
{
    /// <summary>
    /// Логика взаимодействия для QuestionMessageBox.xaml
    /// </summary>
    public partial class QuestionMessageBox : Window
    {
        public string QuestionMessage { get; set; }
        public MessageBoxHelperResult Result { get; set; } = MessageBoxHelperResult.NO;

        public QuestionMessageBox(string questionMessage)
        {
            QuestionMessage = questionMessage;
            InitializeComponent();
            DataContext = this;
        }

        public MessageBoxHelperResult ShowBox()
        {
            ShowDialog();
            return Result;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxHelperResult.YES;
            Close();
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}