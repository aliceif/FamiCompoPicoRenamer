using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FamiCompoPicoRenamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            PathTextBox.Text = dialog.SelectedPath;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var baseDir = new DirectoryInfo(PathTextBox.Text);

            var htmlFile = baseDir.EnumerateFiles("*.html", SearchOption.TopDirectoryOnly).FirstOrDefault();
            var nsfFiles = baseDir.EnumerateFiles("*.nsf", SearchOption.TopDirectoryOnly);

            if (!nsfFiles.Any())
            {
                MessageBox.Show("No NSF files found in the given folder!");
                return;
            }
            if (htmlFile == null)
            {
                MessageBox.Show("No HTML listing found in the given folder!");
                return;
            }

            var document = new HtmlDocument();
            document.Load(htmlFile.FullName, Encoding.UTF8);

            var tableroot = document.DocumentNode.SelectSingleNode("/html/body//table");
            var rows = tableroot.SelectNodes(".//tr");
            var rowData = rows.Skip(1).Select(r => r.SelectNodes("./td"));
            var trackData = rowData.Select(d => new { ID = d[0].InnerText, Name = d[1].InnerText });

            var outDir = baseDir.CreateSubdirectory(@".\output");

            foreach (var file in nsfFiles)
            {
                var trackID = file.Name.Substring(5, 3);
                var trackInfo = trackData.SingleOrDefault(td => td.ID == trackID);
                if (trackInfo != null)
                {
                    var safename = string.Join("_", trackInfo.Name.Split(System.IO.Path.GetInvalidFileNameChars()));
                    file.CopyTo(System.IO.Path.Combine(outDir.FullName, $"{trackID} -- {safename}.nsf"), true);
                }
                else
                {
                    var basename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    file.CopyTo(System.IO.Path.Combine(outDir.FullName, $"{trackID} -- {basename}.nsf"), true);
                }
            }

            System.Diagnostics.Process.Start(outDir.FullName);
        }
    }
}
