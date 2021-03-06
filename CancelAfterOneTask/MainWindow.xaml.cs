﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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

namespace CancelAfterOneTask
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private CancellationTokenSource cts;

        private void InitializeCancellationTokenSource()
        {
            cts = new CancellationTokenSource();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeCancellationTokenSource();

            resultTextBox.Clear();
            try
            {
                await AccessWebAsync(cts.Token);
                resultTextBox.Text += "\r\nDownload complete.";
            } 
            catch(OperationCanceledException)
            {
                resultTextBox.Text += "\r\nDownload canceled.";
            }
            catch(Exception)
            {
                resultTextBox.Text += "\r\nDownload failed.";
            }
            // Set the CancellationTokenSource to null when the download is complete.
            cts = null;
        }

        async Task AccessWebAsync(CancellationToken ct)
        {
            HttpClient httpClient = new HttpClient();

            var urlList = SetUpURLList();

            IEnumerable<Task<int>> downloadTasksQuery =
                from url in urlList select ProcessURLAsync(url, httpClient, ct);

            Task<int>[] downloadTasks = downloadTasksQuery.ToArray();

            //等待第一个任务完成
            Task<int> firstFinishedTask = await Task.WhenAny(downloadTasks);

            //取消剩余任务
            cts.Cancel();

            var length = await firstFinishedTask;
            resultTextBox.Text += $"\r\nLength of the downloaded website: {length}\r\n";
        }

        async Task<int> ProcessURLAsync(string url, HttpClient client,CancellationToken ct)
        {
            HttpResponseMessage message = await client.GetAsync(url,ct);

            byte[] urlContents = await message.Content.ReadAsByteArrayAsync();
            return urlContents.Length;
        }

        private List<string> SetUpURLList()
        {
            List<string> urls = new List<string>
            {
            "https://msdn.microsoft.com",
            "https://msdn.microsoft.com/library/windows/apps/br211380.aspx",
            "https://msdn.microsoft.com/library/hh290136.aspx",
            "https://msdn.microsoft.com/library/ee256749.aspx",
            "https://msdn.microsoft.com/library/ms404677.aspx",
            "https://msdn.microsoft.com/library/ff730837.aspx"
            };
            return urls;
        }
    }
}
