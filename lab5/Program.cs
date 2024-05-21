using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiTaskingExample
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            // Zadanie 1: Obliczanie Symbolu Newtona
            int N = 5;
            int K = 3;
            var taskResult = CalculateBinomialCoefficientUsingTasks(N, K).Result;
            Console.WriteLine($"(Task) Symbol Newtona dla N={N} i K={K} wynosi {taskResult}");

            var delegateResult = CalculateBinomialCoefficientUsingTasksWithDelegates(N, K).Result;
            Console.WriteLine($"(Delegate) Symbol Newtona dla N={N} i K={K} wynosi {delegateResult}");

            var asyncAwaitResult = CalculateBinomialCoefficientAsync(N, K).Result;
            Console.WriteLine($"(Async/Await) Symbol Newtona dla N={N} i K={K} wynosi {asyncAwaitResult}");

            // Zadanie 2: Obliczanie i-ego wyrazu ciągu Fibonacciego
            Application.Run(new FibonacciCalculator());

            // Zadanie 3: Kompresja plików
            Application.Run(new FileCompressor());
        }

        // Implementacje z zadania 1

        // Implementacja przy użyciu klas Task i Task<T>
        static async Task<int> CalculateBinomialCoefficientUsingTasks(int N, int K)
        {
            Task<int> numeratorTask = Task.Run(() => Factorial(N));
            Task<int> denominatorTask = Task.Run(() => Factorial(K) * Factorial(N - K));

            int numerator = await numeratorTask;
            int denominator = await denominatorTask;

            return numerator / denominator;
        }

        // Implementacja przy użyciu Task zamiast BeginInvoke/EndInvoke do wywołania metod asynchronicznych
        static async Task<int> CalculateBinomialCoefficientUsingTasksWithDelegates(int N, int K)
        {
            var numeratorTask = Task.Run(() => Factorial(N));
            var denominatorTask1 = Task.Run(() => Factorial(K));
            var denominatorTask2 = Task.Run(() => Factorial(N - K));

            int numerator = await numeratorTask;
            int denominator = await denominatorTask1 * await denominatorTask2;

            return numerator / denominator;
        }

        // Implementacja przy użyciu metody asynchronicznej async-await
        static async Task<int> CalculateBinomialCoefficientAsync(int N, int K)
        {
            Task<int> numeratorTask = Task.Run(() => Factorial(N));
            Task<int> denominatorTask = Task.Run(() => Factorial(K) * Factorial(N - K));

            int numerator = await numeratorTask;
            int denominator = await denominatorTask;

            return numerator / denominator;
        }

        static int Factorial(int n)
        {
            int result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }
    }

    // Implementacja zadania 2 przy użyciu klasy BackgroundWorker
    public class FibonacciCalculator : Form
    {
        private BackgroundWorker worker = new BackgroundWorker();
        private ProgressBar progressBar = new ProgressBar();
        private Label resultLabel = new Label();

        public FibonacciCalculator()
        {
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            progressBar.Dock = DockStyle.Top;
            resultLabel.Dock = DockStyle.Top;

            Controls.Add(resultLabel);
            Controls.Add(progressBar);

            worker.RunWorkerAsync(10); // Liczba wyrazów ciągu Fibonacciego do obliczenia
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int n = (int)e.Argument;
            int result = Fibonacci(n, (BackgroundWorker)sender, e);
            e.Result = result;
        }

        private int Fibonacci(int n, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int a = 0;
            int b = 1;
            for (int i = 0; i <= n; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return 0;
                }

                int temp = a;
                a = b;
                b = temp + b;
                Thread.Sleep(5); // Spowolnienie pętli
                worker.ReportProgress((i * 100) / n);
            }
            return a;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                resultLabel.Text = "Obliczenia zostały przerwane.";
            }
            else if (e.Error != null)
            {
                resultLabel.Text = "Wystąpił błąd: " + e.Error.Message;
            }
            else
            {
                resultLabel.Text = "Wynik: " + e.Result.ToString();
            }
        }
    }

    // Implementacja zadania 3 przy użyciu GZipStream
    public class FileCompressor : Form
    {
        private Button compressButton = new Button { Text = "Kompresuj" };
        private FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        public FileCompressor()
        {
            compressButton.Dock = DockStyle.Top;
            compressButton.Click += CompressButton_Click;

            Controls.Add(compressButton);
        }

        private async void CompressButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                string[] files = Directory.GetFiles(selectedPath);

                await Task.WhenAll(files.Select(file => Task.Run(() => CompressFile(file))));
            }
        }

        private void CompressFile(string file)
        {
            string compressedFile = file + ".gz";

            using (FileStream originalFileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            using (FileStream compressedFileStream = new FileStream(compressedFile, FileMode.Create))
            using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
            {
                originalFileStream.CopyTo(compressionStream);
            }
        }
    }
}
