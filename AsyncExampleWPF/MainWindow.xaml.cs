using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace AsyncExampleWPF
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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            resultsTextBox.Clear();
            //await SumPageSizesAsync();
            await CreateMulitTasksAsync();
            resultsTextBox.Text += "\r\nControl returned to startButton_Click.";
            startButton.IsEnabled = true;
        }

        private async Task CreateMulitTasksAsync()
        {
            HttpClient httpClient = new HttpClient() {
                MaxResponseContentBufferSize = 100000
            };

            Task<int> download1 = ProcessURLAsync(
                "https://www.baidu.com/s?ie=utf-8&f=3&rsv_bp=1&tn=baidu&wd=%E6%96%B0%E5%86%A0%E8%82%BA%E7%82%8E%E7%96%AB%E6%83%85%E5%AE%9E%E6%97%B6%E5%8A%A8%E6%80%81&oq=%25E6%2596%25B0%25E5%2586%25A0&rsv_pq=93a58e2d002d888c&rsv_t=690enU3oRpZ8dWI%2BSjrvwxGmczcj5O869ZOEwYUxbPV2v0OMBPs%2Bfc0lDmE&rqlang=cn&rsv_dl=ts_2&rsv_enter=1&rsv_sug3=17&rsv_sug1=15&rsv_sug7=100&rsv_sug2=1&rsv_btype=t&prefixsug=%25E6%2596%25B0%25E5%2586%25A0%25E8%2582%25BA%25E7%2582%258E%25E7%2596%25AB%25E6%2583%2585&rsp=2&inputT=10199&rsv_sug4=13910",
                httpClient);

            Task<int> download2 = ProcessURLAsync(
                "http://39.107.142.187/zentao/bug-view-551.html",
                httpClient);

            Task<int> download3 = ProcessURLAsync(
                "https://www.cnblogs.com/zeroone/p/4311117.html", 
                httpClient);

            int length1 = await download1;
            int length2 = await download2;
            int length3 = await download3;

            int total = length1 + length2 + length3;

            resultsTextBox.Text += $"\r\n\r\nTotal bytes returned: {total}\r\n";

        }

        private async Task<int> ProcessURLAsync(string url, HttpClient client)
        {
            var byteArray = await client.GetByteArrayAsync(url);
            DisplayResults(url, byteArray);
            return byteArray.Length;
        }

        private async Task SumPageSizesAsync()
        {
            List<string> urls = SetUpURLList();

            //var total = 0;
            //foreach(var url in urls)
            //{
            //    byte[] urlContents = await GetURLContents(url);
            //    DisplayResults(url, urlContents);
            //    total += urlContents.Length;
            //}

            IEnumerable<Task<int>> downloadTasksQuery =
                from url in urls select ProcessURLAsync(url);

            Task<int>[] downloadTasks = downloadTasksQuery.ToArray();

            int[] lengths = await Task.WhenAll(downloadTasks);

            int total = lengths.Sum();

            resultsTextBox.Text += $"\r\n\r\nTotal bytes returned: {total}\r\n";
        }

        private void DisplayResults(string url,byte[] content)
        {
            var bytes = content.Length;
            var displayURL = url.Replace("https://", "");
            resultsTextBox.Text += $"\n{displayURL, -58},{bytes, 8}";
        }

        private async Task<int> ProcessURLAsync(string url)
        {
            var byteArray = await GetURLContents(url);
            DisplayResults(url, byteArray);
            return byteArray.Length;
        }

        private async Task<byte[]> GetURLContents(string url)
        {
            var content = new MemoryStream();

            var webReq = (HttpWebRequest)WebRequest.Create(url);
            
            using(WebResponse response = await webReq.GetResponseAsync())
            {
                using(Stream responseStream = response.GetResponseStream())
                {
                    await responseStream.CopyToAsync(content);
                }
            }
            return content.ToArray();
        }

        private List<string> SetUpURLList()
        {
            var urls = new List<string>
            {
            "https://www.baidu.com/s?ie=utf-8&f=3&rsv_bp=1&tn=baidu&wd=%E6%96%B0%E5%86%A0%E8%82%BA%E7%82%8E%E7%96%AB%E6%83%85%E5%AE%9E%E6%97%B6%E5%8A%A8%E6%80%81&oq=%25E6%2596%25B0%25E5%2586%25A0&rsv_pq=93a58e2d002d888c&rsv_t=690enU3oRpZ8dWI%2BSjrvwxGmczcj5O869ZOEwYUxbPV2v0OMBPs%2Bfc0lDmE&rqlang=cn&rsv_dl=ts_2&rsv_enter=1&rsv_sug3=17&rsv_sug1=15&rsv_sug7=100&rsv_sug2=1&rsv_btype=t&prefixsug=%25E6%2596%25B0%25E5%2586%25A0%25E8%2582%25BA%25E7%2582%258E%25E7%2596%25AB%25E6%2583%2585&rsp=2&inputT=10199&rsv_sug4=13910",
            "http://39.107.142.187/zentao/bug-view-551.html",
            "https://www.cnblogs.com/zeroone/p/4311117.html",
            };
            return urls;
        }

        /// <summary>
        /// 无返回值的异步方法
        /// </summary>
        /// <returns></returns>
        static async Task WaitAndApologise()
        {
            await Task.Delay(2000);
            Console.WriteLine("\nSorry for the delay!");
        }

        public static async Task DisplayCurrentInfo()
        {
            await WaitAndApologise();
            Console.WriteLine($"Today is {DateTime.Now:D}");
            Console.WriteLine($"The current time is {DateTime.Now.TimeOfDay:t}");
            Console.WriteLine("The current temperature is 76 degrees.");
        }

        static async Task<int> GetLeisureHours()
        {
            var today = await Task.FromResult<string>(DateTime.Now.DayOfWeek.ToString());
            int leisureHours;
            if (today.First() == 'S')
                leisureHours = 16;
            else
                leisureHours = 5;
            return leisureHours;
        }

        /// <summary>
        /// 带返回值的异步方法
        /// </summary>
        /// <returns></returns>
        public static async Task<string> ShowTodaysInfo()
        {
            string ret = $"Today is {DateTime.Today:D}\n" +    
                "Today's hours of leisure: " +    
                $"{await GetLeisureHours()}";
            return ret;
        }


        private CancellationTokenSource cts;

        /// <summary>
        /// 初始化取消标识
        /// </summary>
        private void InitalizeCancellationTokenSource()
        {
            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// 设置取消标识为true
        /// </summary>
        private void CancelTask()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }
        /// <summary>
        /// 带取消标识的异步方法
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task<int> AccessTheWebAsync(CancellationToken ct)
        {
            HttpClient client = new HttpClient();
            await Task.Delay(250);

            HttpResponseMessage responseMessage =
                await client.GetAsync("https://msdn.microsoft.com/library/dd470362.aspx", ct);

            byte[] urlContents = await responseMessage.Content.ReadAsByteArrayAsync();
            return urlContents.Length;

        }

        public async void AccessTheWebAsync()
        {
            try
            {
                // ***Send a token to carry the message if cancellation is requested.
                int contentLength = await AccessTheWebAsync(cts.Token);
            } 
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task Canceled.");
            }
            catch(Exception)
            {
                Console.WriteLine("Task Failed.");
            }
        }


    }
}
