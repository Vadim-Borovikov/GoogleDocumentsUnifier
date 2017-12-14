using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using GoogleDocumentsUnifier.Logic;

namespace GoogleDocumentsUnifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            string clientSecretPath = ConfigurationManager.AppSettings.Get("clientSecretPath");
            string[] docIds = ConfigurationManager.AppSettings.Get("docIds").Split(';');
            string[] pdfIds = ConfigurationManager.AppSettings.Get("pdfIds").Split(';');

            _thesises = new DocumentInfo(docIds[0], DocumentType.GoogleDocument);
            _feelings = new DocumentInfo(pdfIds[0], DocumentType.GooglePdf);
            _needs = new DocumentInfo(docIds[1], DocumentType.GoogleDocument);
            _dataManager = new DataManager(clientSecretPath);
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _dataManager?.Dispose();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            LockControls(true);

            var requests = new[]
            {
                new DocumentRequest(_thesises, DoubleToUint(ThesisesUpDown.Value)),
                new DocumentRequest(_feelings, DoubleToUint(FeelingsUpDown.Value)),
                new DocumentRequest(_needs, DoubleToUint(NeedsUpDown.Value))
            };
            string path = PathTextBox.Text.Replace('/', '\\');
            bool makeEvens = DoubleSideCheckBox.IsChecked.HasValue && DoubleSideCheckBox.IsChecked.Value;
            await Task.Run(() => _dataManager.Unify(requests, path, makeEvens));

            ShowFile(path);
            Close();
        }

        private static void ShowFile(string path)
        {
            string argument = $"/select, {path}";
            Process.Start("explorer.exe", argument);
        }

        private void LockControls(bool shouldLock)
        {
            ThesisesUpDown.IsEnabled = !shouldLock;
            FeelingsUpDown.IsEnabled = !shouldLock;
            NeedsUpDown.IsEnabled = !shouldLock;
            PathTextBox.IsEnabled = !shouldLock;
            Button.IsEnabled = !shouldLock;
        }

        private static uint DoubleToUint(double d)
        {
            if (d < 0.0)
            {
                return 0;
            }
            return (uint) Math.Floor(d);
        }

        private readonly DocumentInfo _thesises;
        private readonly DocumentInfo _feelings;
        private readonly DocumentInfo _needs;
        private readonly DataManager _dataManager;
    }
}
