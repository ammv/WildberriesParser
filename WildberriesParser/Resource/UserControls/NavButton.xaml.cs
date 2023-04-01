using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WildberriesParser.Resource.UserControls
{
    /// <summary>
    /// Логика взаимодействия для NavButton.xaml
    /// </summary>
    public partial class NavButton : UserControl
    {
        public NavButton()
        {
            InitializeComponent();
        }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(NavButton), new PropertyMetadata(default(bool)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(NavButton), new PropertyMetadata(default(ICommand)));

        public Style IfCheckedTrueStyle
        {
            get { return (Style)GetValue(IfCheckedTrueStyleProperty); }
            set { SetValue(IfCheckedTrueStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IfCheckedTrueStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IfCheckedTrueStyleProperty =
            DependencyProperty.Register("IfCheckedTrueStyle", typeof(Style), typeof(NavButton), new PropertyMetadata(default(Style)));

        public Style IfCheckedFalseStyle
        {
            get { return (Style)GetValue(IfCheckedFalseStyleProperty); }
            set { SetValue(IfCheckedFalseStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IfCheckedFalseStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IfCheckedFalseStyleProperty =
            DependencyProperty.Register("IfCheckedFalseStyle", typeof(Style), typeof(NavButton), new PropertyMetadata(default(Style)));

        public PackIconKind Kind
        {
            get { return (PackIconKind)GetValue(KindProperty); }
            set { SetValue(KindProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Kind.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register("Kind", typeof(PackIconKind), typeof(NavButton), new PropertyMetadata(default(PackIconKind)));

        public double KindHeight
        {
            get { return (double)GetValue(KindHeightProperty); }
            set { SetValue(KindHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KindHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KindHeightProperty =
            DependencyProperty.Register("KindHeight", typeof(double), typeof(NavButton), new PropertyMetadata(32.0));

        public double KindWidth
        {
            get { return (double)GetValue(KindWidthProperty); }
            set { SetValue(KindWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KindWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KindWidthProperty =
            DependencyProperty.Register("KindWidth", typeof(double), typeof(NavButton), new PropertyMetadata(32.0));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(NavButton), new PropertyMetadata(default(string)));

        public Thickness KindMargin
        {
            get { return (Thickness)GetValue(KindMarginProperty); }
            set { SetValue(KindMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KindMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KindMarginProperty =
            DependencyProperty.Register("KindMargin", typeof(Thickness), typeof(NavButton), new PropertyMetadata(default(Thickness)));

        public double TextSize
        {
            get { return (double)GetValue(TextSizeProperty); }
            set { SetValue(TextSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextSizeProperty =
            DependencyProperty.Register("TextSize", typeof(double), typeof(NavButton), new PropertyMetadata(16.0));
    }
}