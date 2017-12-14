﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ControlLib;
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
            _dataManager = new DataManager(clientSecretPath);
            _amounts = new Dictionary<DocumentInfo, NumericUpDown>();

            string[] pdfPaths = ConfigurationManager.AppSettings.Get("pdfPaths")?.Split(';') ?? new string[0];
            foreach (string id in pdfPaths)
            {
                AddSource(id, DocumentType.LocalPdf);
            }

            string[] pdfIds = ConfigurationManager.AppSettings.Get("pdfIds")?.Split(';') ?? new string[0];
            foreach (string name in pdfIds)
            {
                AddSource(name, DocumentType.GooglePdf);
            }

            string[] docIds = ConfigurationManager.AppSettings.Get("docIds")?.Split(';') ?? new string[0];
            foreach (string name in docIds)
            {
                AddSource(name, DocumentType.GoogleDocument);
            }
        }

        private void AddSource(string id, DocumentType documentType)
        {
            var info = new DocumentInfo(id, documentType);

            string name = GetName(info);
            var textBlock = new TextBlock
            {
                Text = name,
                Height = 28
            };

            var numericUpDown = new NumericUpDown
            {
                Value = 5,
                Width = 40,
                Height = 28
            };
            _amounts.Add(info, numericUpDown);

            Grid.RowDefinitions.Add(new RowDefinition());

            Grid.Children.Add(textBlock);
            Grid.SetRow(textBlock, Grid.RowDefinitions.Count - 1);

            Grid.Children.Add(numericUpDown);
            Grid.SetRow(numericUpDown, Grid.RowDefinitions.Count - 1);
            Grid.SetColumn(numericUpDown, 1);
        }

        private string GetName(DocumentInfo info)
        {
            switch (info.DocumentType)
            {
                case DocumentType.LocalPdf:
                    return Path.GetFileName(info.Id);
                case DocumentType.GooglePdf:
                case DocumentType.GoogleDocument:
                    return _dataManager.GetName(info.Id);
                default:
                    throw new ArgumentOutOfRangeException(nameof(DocumentType));
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) => _dataManager?.Dispose();

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            LockControls(true);

            List<DocumentRequest> requests =
                _amounts.Select(p => new DocumentRequest(p.Key, DoubleToUint(p.Value.Value))).ToList();
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
            foreach (NumericUpDown numericUpDown in _amounts.Values)
            {
                numericUpDown.IsEnabled = !shouldLock;
            }
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

        private readonly Dictionary<DocumentInfo, NumericUpDown> _amounts;
        private readonly DataManager _dataManager;
    }
}
