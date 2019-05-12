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

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonFrom1.IsChecked == true || RadioButtonFrom2.IsChecked == true || RadioButtonFrom3.IsChecked == true || RadioButtonFrom4.IsChecked == true)
            {
                if (RadioButtonTo1.IsChecked == true || RadioButtonTo2.IsChecked == true || RadioButtonTo3.IsChecked == true || RadioButtonTo4.IsChecked == true)
                {
                    if (RadioButtonFrom1.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.RomajiTandoku;
                    }
                    else if (RadioButtonFrom2.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.RomajiRenzoku;
                    }
                    else if (RadioButtonFrom3.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.KanaTandoku;
                    }
                    else if (RadioButtonFrom4.IsChecked == true)
                    {
                        fromType = Lyric.LyricType.KanaRenzoku;
                    }
                    if (RadioButtonTo1.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.RomajiTandoku;
                    }
                    else if (RadioButtonTo2.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.RomajiRenzoku;
                    }
                    else if (RadioButtonTo3.IsChecked == true)
                    {
                        ToType = Lyric.LyricType.KanaTandoku;
                    }
                    else if (RadioButtonTo4.IsChecked == true)
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
    }
}