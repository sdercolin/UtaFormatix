using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace UtaFormatix
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string version = "ver 2.0";
        private Data mainData;
        private Data exportingData;
        private bool imported = false;

        public MainWindow()
        {
            InitializeComponent();
            label.Content = version;
        }

        private void ExportVsq4(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (imported)
            {
                exportingData = new Data(mainData);
                if (!transformLyrics(Data.UtaFormat.Vsq4))
                {
                    return;
                }
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export",
                    Filter = "VOCALOID Project|*.vsqx",
                    FileName = exportingData.ProjectName,
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    exportingData.ExportVsq4(filename);
                }
                exportingData = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void ExportCcs(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (imported)
            {
                exportingData = new Data(mainData);
                if (!transformLyrics(Data.UtaFormat.Ccs))
                {
                    return;
                }
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export",
                    Filter = "CeVIO Project|*.ccs",
                    FileName = exportingData.ProjectName,
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    exportingData.ExportCcs(fileName);
                }
                exportingData = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void ExportUst(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (imported)
            {
                exportingData = new Data(mainData);
                if (!transformLyrics(Data.UtaFormat.Ust))
                {
                    return;
                }
                var selectFolder = new FolderBrowserDialog();
                selectFolder.Description = "Please choose a folder to export ust(s):";
                selectFolder.SelectedPath = exportingData.Files[0].Replace(System.IO.Path.GetFileName(exportingData.Files[0]), "");
                if (selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string foldpath = selectFolder.SelectedPath;
                    exportingData.ExportUst(foldpath);
                }
                exportingData = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void Import(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Import",
                Filter = "VOCALOID Project|*.vsqx|UTAU Project|*.ust|CeVIO Project|*.ccs",
                FileName = string.Empty,
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };
            var result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            var fileNames = new List<string>();
            fileNames.AddRange(openFileDialog.FileNames);
            mainData = new Data();
            imported = mainData.Import(fileNames);
        }

        private void BtnImport_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnImport.Opacity = 0.1;
            BtnImport_Cover.Visibility = Visibility.Visible;
        }

        private void BtnImport_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnImport.Opacity = 1;
            BtnImport_Cover.Visibility = Visibility.Hidden;
        }

        private void BtnExportCcs_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportCcs.Opacity = 0.1;
            BtnExportCcs_Cover.Visibility = Visibility.Visible;
        }

        private void BtnExportUst_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportUst.Opacity = 0.1;
            BtnExportUst_Cover.Visibility = Visibility.Visible;
        }

        private void BtnExportVsqx_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVsqx.Opacity = 0.1;
            BtnExportVsqx_Cover.Visibility = Visibility.Visible;
        }

        private void BtnExportVsqx_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVsqx.Opacity = 1;
            BtnExportVsqx_Cover.Visibility = Visibility.Hidden;
        }

        private void BtnExportUst_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportUst.Opacity = 1;
            BtnExportUst_Cover.Visibility = Visibility.Hidden;
        }

        private void BtnExportCcs_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportCcs.Opacity = 1;
            BtnExportCcs_Cover.Visibility = Visibility.Hidden;
        }

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            Droping.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            Droping.Visibility = Visibility.Hidden;
        }

        private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Link;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            var fileNames = new List<string>();
            var dropfiles = (System.Array)e.Data.GetData(System.Windows.DataFormats.FileDrop);
            foreach (var dropfile in dropfiles)
            {
                fileNames.Add(dropfile.ToString());
            }
            mainData = new Data();
            imported = mainData.Import(fileNames);
            Droping.Visibility = Visibility.Hidden;
        }

        private bool transformLyrics(Data.UtaFormat toFormat)
        {
            var changeLyrics = new ChangeLyrics();
            switch (mainData.Lyric.TypeAnalysed)
            {
                case Lyric.LyricType.None:
                    System.Windows.MessageBox.Show("The type of the lyrics is not detected, please select the correct type by yourself.", "Lyrics Transformation");
                    changeLyrics.radioButton_from1.IsChecked = true;
                    break;

                case Lyric.LyricType.RomajiTandoku:
                    changeLyrics.radioButton_from1.IsChecked = true;
                    break;

                case Lyric.LyricType.RomajiRenzoku:
                    changeLyrics.radioButton_from2.IsChecked = true;
                    break;

                case Lyric.LyricType.KanaTandoku:
                    changeLyrics.radioButton_from3.IsChecked = true;
                    break;

                case Lyric.LyricType.KanaRenzoku:
                    changeLyrics.radioButton_from4.IsChecked = true;
                    break;

                default:
                    break;
            }
            changeLyrics.radioButton_to3.IsChecked = true;
            switch (toFormat)
            {
                case Data.UtaFormat.Vsq4:
                    changeLyrics.radioButton_to2.Visibility = Visibility.Hidden;
                    changeLyrics.radioButton_to4.Visibility = Visibility.Hidden;
                    changeLyrics.radioButton_to3.Margin = changeLyrics.radioButton_to2.Margin;
                    break;

                case Data.UtaFormat.Ccs:
                    changeLyrics.radioButton_to1.Visibility = Visibility.Hidden;
                    changeLyrics.radioButton_to2.Visibility = Visibility.Hidden;
                    changeLyrics.radioButton_to4.Visibility = Visibility.Hidden;
                    changeLyrics.radioButton_to3.Margin = changeLyrics.radioButton_to1.Margin;
                    break;

                default:
                    break;
            }
            bool? dialogResult = changeLyrics.ShowDialog();
            if (dialogResult == true)
            {
                exportingData.Lyric.Transform(changeLyrics.fromType, changeLyrics.ToType);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}