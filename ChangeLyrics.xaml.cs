using System.Windows;

namespace UtaFormatix
{
    /// <summary>
    /// ChangeLyrics.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeLyrics : Window
    {
        public ChangeLyrics()
        {
            InitializeComponent();
        }

        public Lyric.LyricType fromType = Lyric.LyricType.None;
        public Lyric.LyricType ToType = Lyric.LyricType.None;

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void button_confirm_Click(object sender, RoutedEventArgs e)
        {
            if (radioButton_from1.IsChecked == true || radioButton_from2.IsChecked == true || radioButton_from3.IsChecked == true || radioButton_from4.IsChecked == true)
            {
                if (radioButton_to1.IsChecked == true || radioButton_to2.IsChecked == true || radioButton_to3.IsChecked == true || radioButton_to4.IsChecked == true)
                {
                    if (radioButton_from1.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.RomajiTandoku;
                    }
                    else if (radioButton_from2.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.RomajiRenzoku;
                    }
                    else if (radioButton_from3.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.KanaTandoku;
                    }
                    else if (radioButton_from4.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.KanaRenzoku;
                    }
                    if (radioButton_to1.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.RomajiTandoku;
                    }
                    else if (radioButton_to2.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.RomajiRenzoku;
                    }
                    else if (radioButton_to3.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.KanaTandoku;
                    }
                    else if (radioButton_to4.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.KanaRenzoku;
                    }
                    DialogResult = true;
                    Close();
                    return;
                }
            }
            DialogResult = false;
            MessageBox.Show("Please select the types correctly.", "Lyrics Transformation");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}