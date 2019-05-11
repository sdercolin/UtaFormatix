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
        private string version = "ver1.51";
        private Data maindata;
        private Data exportingdata;
        private bool Imported = false;

        public MainWindow()
        {
            InitializeComponent();
            label.Content = version;
        }

        private void ExportVsq4(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Imported)
            {
                exportingdata = new Data(maindata);
                if (!transformlyrics(Data.UtaFormat.Vsq4))
                {
                    return;
                }
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Export";
                saveFileDialog.Filter = "VOCALOID Project|*.vsqx";
                saveFileDialog.FileName = exportingdata.ProjectName;
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    exportingdata.ExportVsq4(filename);
                }
                exportingdata = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void ExportCcs(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Imported)
            {
                exportingdata = new Data(maindata);
                if (!transformlyrics(Data.UtaFormat.Ccs))
                {
                    return;
                }
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Export";
                saveFileDialog.Filter = "CeVIO Project|*.ccs";
                saveFileDialog.FileName = exportingdata.ProjectName;
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    exportingdata.ExportCcs(filename);
                }
                exportingdata = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void ExportUst(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Imported)
            {
                exportingdata = new Data(maindata);
                if (!transformlyrics(Data.UtaFormat.Ust))
                {
                    return;
                }
                var selectFolder = new FolderBrowserDialog();
                selectFolder.Description = "Please choose a folder to export ust(s):";
                selectFolder.SelectedPath = exportingdata.Files[0].Replace(System.IO.Path.GetFileName(exportingdata.Files[0]), "");
                if (selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string foldpath = selectFolder.SelectedPath;
                    exportingdata.ExportUst(foldpath);
                }
                exportingdata = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private void Import(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Import";
            openFileDialog.Filter = "VOCALOID Project|*.vsqx|UTAU Project|*.ust|CeVIO Project|*.ccs";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            var result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            var fileNames = new List<string>();
            fileNames.AddRange(openFileDialog.FileNames);
            maindata = new Data();
            Imported = maindata.Import(fileNames);
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
            maindata = new Data();
            Imported = maindata.Import(fileNames);
            Droping.Visibility = Visibility.Hidden;
        }

        private bool transformlyrics(Data.UtaFormat toFormat)
        {
            var changelyrics = new ChangeLyrics();
            switch (maindata.Lyric.TypeAnalysed)
            {
                case Lyric.LyricType.None:
                    System.Windows.MessageBox.Show("The type of the lyrics is not detected, please select the correct type by yourself.", "Lyrics Transformation");
                    changelyrics.radioButton_from1.IsChecked = true;
                    break;

                case Lyric.LyricType.RomajiTandoku:
                    changelyrics.radioButton_from1.IsChecked = true;
                    break;

                case Lyric.LyricType.RomajiRenzoku:
                    changelyrics.radioButton_from2.IsChecked = true;
                    break;

                case Lyric.LyricType.KanaTandoku:
                    changelyrics.radioButton_from3.IsChecked = true;
                    break;

                case Lyric.LyricType.KanaRenzoku:
                    changelyrics.radioButton_from4.IsChecked = true;
                    break;

                default:
                    break;
            }
            changelyrics.radioButton_to3.IsChecked = true;
            switch (toFormat)
            {
                case Data.UtaFormat.Vsq4:
                    changelyrics.radioButton_to2.Visibility = Visibility.Hidden;
                    changelyrics.radioButton_to4.Visibility = Visibility.Hidden;
                    changelyrics.radioButton_to3.Margin = changelyrics.radioButton_to2.Margin;
                    break;

                case Data.UtaFormat.Ccs:
                    changelyrics.radioButton_to1.Visibility = Visibility.Hidden;
                    changelyrics.radioButton_to2.Visibility = Visibility.Hidden;
                    changelyrics.radioButton_to4.Visibility = Visibility.Hidden;
                    changelyrics.radioButton_to3.Margin = changelyrics.radioButton_to1.Margin;
                    break;

                default:
                    break;
            }
            bool? dialogResult = changelyrics.ShowDialog();
            if (dialogResult == true)
            {
                exportingdata.Lyric.Transform(changelyrics.fromType, changelyrics.ToType);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}