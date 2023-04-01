namespace WildberriesParser.Helpers
{
    public enum MessageBoxHelperResult
    {
        YES,
        NO,
        CANCEL,
        OK
    }

    public static class MessageBoxHelper
    {
        public static MessageBoxHelperResult Error(string message)
        {
            return new MessageBoxes.ErrorMessageBox(message).ShowBox();
        }

        public static MessageBoxHelperResult Question(string message)
        {
            return new MessageBoxes.QuestionMessageBox(message).ShowBox();
        }

        public static MessageBoxHelperResult Information(string message)
        {
            return new MessageBoxes.InformationMessageBox(message).ShowBox();
        }
    }
}