using System;
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

namespace AsyncExampleWPF1
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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            cts = new CancellationTokenSource();

            resultTextBox.Clear();

            try
            {
                cts.CancelAfter(2000);

                await AccessWebAsync(cts.Token);
                resultTextBox.Text += "\r\nDownloads succeeded.\r\n";
            }
            catch(OperationCanceledException)
            {
                resultTextBox.Text += "\r\nDownloads canceled.\r\n";
            }
            catch(Exception)
            {
                resultTextBox.Text += "\r\nDownloads failed.\r\n";
            }
            cts = null;
            startButton.IsEnabled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        async Task AccessWebAsync(CancellationToken ct)
        {
            HttpClient httpClient = new HttpClient();

            var urlList = SetUpURLList();

            foreach(var url in urlList)
            {
                HttpResponseMessage message =
                    await httpClient.GetAsync(url, ct);

                byte[] urlContents = await message.Content.ReadAsByteArrayAsync();

                resultTextBox.Text +=
                    $"\r\nLength of the downloaded string: {urlContents.Length}.\r\n";
            }
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
