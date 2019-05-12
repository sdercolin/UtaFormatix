using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            LabelVersion.Content = version;
        }

        private async void ExportVsq4(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (imported)
            {
                exportingData = new Data(mainData);
                if (!await TransformLyrics(Data.UtaFormat.Vsq4))
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
                    string fileName = saveFileDialog.FileName;
                    await ProcessExport(Data.UtaFormat.Vsq4, fileName);
                }
                exportingData = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private async void ExportVpr(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (imported)
            {
                exportingData = new Data(mainData);
                if (!await TransformLyrics(Data.UtaFormat.Vpr))
                {
                    return;
                }
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Export",
                    Filter = "VOCALOID5 Project|*.vpr",
                    FileName = exportingData.ProjectName,
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    await ProcessExport(Data.UtaFormat.Vpr, filename);
                }
                exportingData = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private async void ExportCcs(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (imported)
            {
                exportingData = new Data(mainData);
                if (!await TransformLyrics(Data.UtaFormat.Ccs))
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
                    await ProcessExport(Data.UtaFormat.Ccs, fileName);
                }
                exportingData = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private async void ExportUst(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (imported)
            {
                exportingData = new Data(mainData);
                if (!await TransformLyrics(Data.UtaFormat.Ust))
                {
                    return;
                }
                var selectFolder = new FolderBrowserDialog();
                selectFolder.Description = "Please choose a folder to export ust(s):";
                selectFolder.SelectedPath = exportingData.Files[0].Replace(System.IO.Path.GetFileName(exportingData.Files[0]), "");
                if (selectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    await ProcessExport(Data.UtaFormat.Vsq4, selectFolder.SelectedPath);
                }
                exportingData = null;
            }
            else
            {
                System.Windows.MessageBox.Show("You have not imported a project.", "Export");
            }
        }

        private async void Import(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Import",
                Filter = "VOCALOID5 Project|*.vpr|VOCALOID Project|*.vsqx|UTAU Project|*.ust|CeVIO Project|*.ccs",
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
            var fileNames = openFileDialog.FileNames.ToList();
            mainData = new Data();
            ShowProcessingScreen();
            imported = await mainData.Import(fileNames);
            HideProcessingScreen();
        }

        private void BtnImport_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnImport.Opacity = 0.1;
            BtnImportCover.Visibility = Visibility.Visible;
        }

        private void BtnImport_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnImport.Opacity = 1;
            BtnImportCover.Visibility = Visibility.Hidden;
        }

        private void BtnExportCcs_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportCcs.Opacity = 0.1;
            BtnExportCcsCover.Visibility = Visibility.Visible;
        }

        private void BtnExportUst_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportUst.Opacity = 0.1;
            BtnExportUstCover.Visibility = Visibility.Visible;
        }

        private void BtnExportVsqx_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVsqx.Opacity = 0.1;
            BtnExportVsqxCover.Visibility = Visibility.Visible;
        }

        private void BtnExportVpr_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVpr.Opacity = 0.1;
            BtnExportVprCover.Visibility = Visibility.Visible;
        }

        private void BtnExportVsqx_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVsqx.Opacity = 1;
            BtnExportVsqxCover.Visibility = Visibility.Hidden;
        }

        private void BtnExportVpr_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportVpr.Opacity = 1;
            BtnExportVprCover.Visibility = Visibility.Hidden;
        }

        private void BtnExportUst_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportUst.Opacity = 1;
            BtnExportUstCover.Visibility = Visibility.Hidden;
        }

        private void BtnExportCcs_Cover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnExportCcs.Opacity = 1;
            BtnExportCcsCover.Visibility = Visibility.Hidden;
        }

        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            LayerDropping.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            LayerDropping.Visibility = Visibility.Hidden;
        }

        private async void Grid_Drop(object sender, System.Windows.DragEventArgs e)
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
            LayerDropping.Visibility = Visibility.Hidden;
            ShowProcessingScreen();
            imported = await mainData.Import(fileNames);
            HideProcessingScreen();
        }

        private async Task<bool> TransformLyrics(Data.UtaFormat toFormat)
        {
            var changeLyrics = new ChangeLyrics();
            switch (mainData.Lyric.TypeAnalysed)
            {
                case Lyric.LyricType.None:
                    System.Windows.MessageBox.Show("The type of the lyrics is not detected, please select the correct type by yourself.", "Lyrics Transformation");
                    changeLyrics.RadioButtonFrom1.IsChecked = true;
                    break;

                case Lyric.LyricType.RomajiTandoku:
                    changeLyrics.RadioButtonFrom1.IsChecked = true;
                    break;

                case Lyric.LyricType.RomajiRenzoku:
                    changeLyrics.RadioButtonFrom2.IsChecked = true;
                    break;

                case Lyric.LyricType.KanaTandoku:
                    changeLyrics.RadioButtonFrom3.IsChecked = true;
                    break;

                case Lyric.LyricType.KanaRenzoku:
                    changeLyrics.RadioButtonFrom4.IsChecked = true;
                    break;

                default:
                    break;
            }
            changeLyrics.RadioButtonTo3.IsChecked = true;
            switch (toFormat)
            {
                case Data.UtaFormat.Vsq4:
                    changeLyrics.RadioButtonTo2.Visibility = Visibility.Hidden;
                    changeLyrics.RadioButtonTo4.Visibility = Visibility.Hidden;
                    changeLyrics.RadioButtonTo3.Margin = changeLyrics.RadioButtonTo2.Margin;
                    break;

                case Data.UtaFormat.Ccs:
                    changeLyrics.RadioButtonTo1.Visibility = Visibility.Hidden;
                    changeLyrics.RadioButtonTo2.Visibility = Visibility.Hidden;
                    changeLyrics.RadioButtonTo4.Visibility = Visibility.Hidden;
                    changeLyrics.RadioButtonTo3.Margin = changeLyrics.RadioButtonTo1.Margin;
                    break;

                default:
                    break;
            }
            bool? dialogResult = changeLyrics.ShowDialog();
            if (dialogResult == true)
            {
                ShowProcessingScreen();
                await Task.Run(() =>
                {
                    exportingData.Lyric.Transform(changeLyrics.fromType, changeLyrics.ToType);
                });
                HideProcessingScreen();
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task ProcessExport(Data.UtaFormat format, string fileName)
        {
            ShowProcessingScreen();
            switch (format)
            {
                case Data.UtaFormat.Vsq4:
                    await exportingData.ExportVsq4(fileName);
                    break;
                case Data.UtaFormat.Ust:
                    await exportingData.ExportUst(fileName);
                    break;
                case Data.UtaFormat.Ccs:
                    await exportingData.ExportCcs(fileName);
                    break;
                case Data.UtaFormat.Vpr:
                    await exportingData.ExportVpr(fileName);
                    break;
                default:
                    break;
            }
            HideProcessingScreen();
        }

        private void ShowProcessingScreen()
        {
            LayerProcessing.Visibility = Visibility.Visible;
            LabelProcessing.Visibility = Visibility.Visible;
        }

        private void HideProcessingScreen()
        {
            LayerProcessing.Visibility = Visibility.Hidden;
            LabelProcessing.Visibility = Visibility.Hidden;
        }
    }
}