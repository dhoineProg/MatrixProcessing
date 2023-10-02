using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace lab1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ThreadTests();
        }

        private void ProcessMatrixSequential(float[,] matrixA, float[,] matrixB, int size)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrixA[i, j] += matrixB[i, j];
                }
            }
        }

        private void ProcessMatrixParallel(float[,] matrixA, float[,] matrixB, int size, int numThreads)
        {
            CountdownEvent countdownEvent = new CountdownEvent(numThreads);

            for (int i = 0; i < numThreads; i++)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    int threadIndex = (int)state;
                    int start = threadIndex * size / numThreads;
                    int end = (threadIndex + 1) * size / numThreads;
                    for (int row = start; row < end; row++)
                    {
                        for (int col = 0; col < size; col++)
                        {
                            matrixA[row, col] += matrixB[row, col];
                        }
                    }
                    countdownEvent.Signal(); // Сигнализируем о завершении работы потока
                }, i);
            }

            // Ожидание завершения всех потоков
            countdownEvent.Wait();
            countdownEvent.Dispose(); // Не забываем освободить ресурсы CountdownEvent
        }

        private void ThreadTests()
        {
            int size = 5000; // Размер матрицы
            float[,] matrixA = new float[size, size];
            float[,] matrixB = new float[size, size];
            Random rand = new Random();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    matrixA[i, j] = (float)rand.NextDouble();
                    matrixB[i, j] = (float)rand.NextDouble();
                }
            }

            int[] mValues = { 1, 2, 3, 4, 5, 6, 7, 8 };
            double[] executionTimesSequential = new double[mValues.Length];
            double[] executionTimesParallel = new double[mValues.Length];

            for (int i = 0; i < mValues.Length; i++)
            {
                int numThreads = mValues[i];
                float[,] matrixCopy = new float[size, size];
                Array.Copy(matrixA, matrixCopy, matrixA.Length);

                var watch = Stopwatch.StartNew();
                ProcessMatrixSequential(matrixCopy, matrixB, size);
                watch.Stop();
                executionTimesSequential[i] = watch.Elapsed.TotalMilliseconds;

                watch.Restart();
                ProcessMatrixParallel(matrixCopy, matrixB, size, numThreads);
                watch.Stop();
                executionTimesParallel[i] = watch.Elapsed.TotalMilliseconds;
            }

            Chart chart1 = new Chart();
            chart1.Size = new System.Drawing.Size(800, 400);
            chart1.ChartAreas.Add(new ChartArea());

            Series seriesSequential = new Series("Sequential");
            seriesSequential.ChartType = SeriesChartType.Line;

            Series seriesParallel = new Series("Parallel");
            seriesParallel.ChartType = SeriesChartType.Line;

            for (int i = 0; i < mValues.Length; i++)
            {
                seriesSequential.Points.AddXY(mValues[i], executionTimesSequential[i]);
                seriesParallel.Points.AddXY(mValues[i], executionTimesParallel[i]);
            }

            chart1.Series.Add(seriesSequential);
            chart1.Series.Add(seriesParallel);
            chart1.ChartAreas[0].AxisX.Title = "Thread Count (m)";
            chart1.ChartAreas[0].AxisY.Title = "Execution Time (ms)";

            chart1.Dock = DockStyle.Fill;
            this.Controls.Add(chart1);
        }
    }
}