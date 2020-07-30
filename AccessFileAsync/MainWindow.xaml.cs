using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace AccessFileAsync
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
            //await ProcessWriteAsync();
            //await ProcessReadAsync();
            await ProcessWriteMultAsync();
            startButton.IsEnabled = true;
        }

        public async Task ProcessWriteAsync()
        {
            string filePath = @"temp.txt";
            string text = "Hello world!";

            await WriteTextAsync(filePath, text);
        }

        /// <summary>
        /// 异步写入文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task WriteTextAsync(string filePath,string text)
        {
            byte[] encodingText = Encoding.Unicode.GetBytes(text);

            using(FileStream sourceStream = new FileStream(filePath,FileMode.Append,
                FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodingText, 0, encodingText.Length);
            }
        }

        public async Task ProcessReadAsync()
        {
            string filePath = @"temp.txt";

            if (File.Exists(filePath) == false)
            {
                Debug.WriteLine("file not found: " + filePath);
            }
            else
            {
                try
                {
                    string text = await ReadTextAsync(filePath);
                    resultTextBox.Text = text;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 异步读取文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<string> ReadTextAsync(string filePath)
        {
            using(FileStream stream = new FileStream(filePath,FileMode.Open,
                FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();
                byte[] buffer = new byte[0x1000];
                int numRead;
                while((numRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }
                return sb.ToString();
            }
        }

        public async Task ProcessWriteMultAsync()
        {
            string folder = @"tempfolder\";
            List<Task> tasks = new List<Task>();
            List<FileStream> sourceStreams = new List<FileStream>();
            try
            {
                for (int index = 1; index <= 10; index++)
                {
                    string text = "In file " + index.ToString() + "\r\n";
                    string fileName = "thefile" + index.ToString("00") + ".txt";
                    string filePath = folder + fileName;
                    byte[] encodedText = Encoding.Unicode.GetBytes(text);
                    FileStream sourceStream = new FileStream(filePath,
                    FileMode.Append, FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true);
                    Task theTask = sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                    sourceStreams.Add(sourceStream);
                    tasks.Add(theTask);
                }
                await Task.WhenAll(tasks);
            } 
            finally
            {
                foreach (FileStream sourceStream in sourceStreams)
                {
                    sourceStream.Close();
                }
            }
        }
    }
}
