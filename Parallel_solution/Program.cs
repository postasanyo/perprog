using Accord.Math;
using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Parallel_solution
{
    class Program
    {
        public static int negativeCounter = 0;
        private static string fileName = "mediumvid.mp4";
        private static int readerCount;
        private static int processCount;
        private static List<VideoFileReader> readers = new List<VideoFileReader>();
        private static List<Task> readingTasksList = new List<Task>();
        private static List<Task> processingTasksList = new List<Task>();
        private static ConcurrentQueue<OneFrame> queue = new ConcurrentQueue<OneFrame>();
        private static ConcurrentDictionary<int, Bitmap> writeDictionary = new ConcurrentDictionary<int, Bitmap>();
        
        private static int width;
        private static int height;
        private static long frameCount;
        private static int bitRate;
        private static Rational frameRate;

        static void Main(string[] args)
        {
            Console.WriteLine("Number Of Logical Processors: {0}", Environment.ProcessorCount);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            ReadProperties(fileName);

            readerCount = 1;
            processCount = (Environment.ProcessorCount / 2) + 1;
            Master();

            for (int i = 0; i < processCount; i++)
            {
                processingTasksList.Add(new Task(() => Worker(), TaskCreationOptions.LongRunning));
            }

            // Starting all the Tasks and the Thread
            StartTasks(readingTasksList);
            StartTasks(processingTasksList);

            Thread t = new Thread(WriteVideoFile);
            t.Start();

            // Wait till all the tasks are completed
            Task.WaitAll(readingTasksList.ToArray());
            Task.WaitAll(processingTasksList.ToArray());
            t.Join();

            // After completed, writing the information about the process to the console
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Console.WriteLine("Completed!");
            System.Diagnostics.Process.Start("result_" + fileName);
            Console.ReadLine();
        }
        // Read in the settings
        private static void ReadProperties(string filePath)
        {
            //setting attributes needed later for the writer
            VideoFileReader reader = new VideoFileReader();
            reader.Open(filePath);
            width = reader.Width;
            height = reader.Height;
            frameCount = reader.FrameCount;
            bitRate = reader.BitRate;
            frameRate = reader.FrameRate;
            reader.Close();
        }
        //Starts the tasks for a List of tasks
        private static void StartTasks(List<Task> taskList)
        {
            foreach (var task in taskList)
            {
                task.Start();
            }
        }

        //Read in the indexed frame, result written into the Output window
        static void ReadIn(int idx, VideoFileReader reader)
        {
            for (int i = 0; i < reader.FrameCount; i++)
            {
                int index = i;
                Bitmap bitmap = reader.ReadVideoFrame(i);
                queue.Enqueue(new OneFrame(index, bitmap));
                Debug.Write("Read frame " + index + "\n");
            }
            reader.Close();

            // Was made to create a new Task, but it was faster without it.

            //Task t = new Task(() => Worker(), TaskCreationOptions.LongRunning);
            //t.Start();
            //processingTasksList.Add(t);
            //Console.WriteLine("\nNew Processing Task Created " + t.Id + " " + t.Status + "\n");
        }
        // Write video into file
        private static void WriteVideoFile()
        {
            VideoFileWriter writer = new VideoFileWriter();
            writer.Open("result_" + fileName, width, height,frameRate, VideoCodec.Default, bitRate);
            int frameIndex = 0;
            while (writeDictionary.Count > 0 || frameIndex < frameCount)
            {
                Bitmap bmap;
                if (writeDictionary.TryRemove(frameIndex, out bmap))
                {
                    writer.WriteVideoFrame(bmap);
                    bmap.Dispose();
                    frameIndex++;
                    Debug.Write("Frame written " + frameIndex + "\n");
                }
            }
            writer.Close();
        }
        //The reading tasks are created here. In this case only one task is created.
        private static void Master()
        {
            for (int i = 0; i < readerCount; i++)
            {
                int j = i;
                readers.Add(new VideoFileReader());
                readers[j].Open(fileName);
                Task T = new Task(() => ReadIn(j, readers[j]), TaskCreationOptions.LongRunning);
                readingTasksList.Add(T);
            }
        }
        // The processing method, which generates the negatives and add the frames to the dictionary
        private static void Worker()
        {
            while (!(queue.Count == 0 && MasterIsCompleted()))
            {
                OneFrame frame;
                if (queue.TryDequeue(out frame))
                {
                    GenerateNegativePixel(frame);
                    writeDictionary.TryAdd(frame.IndexOfFrame, frame.BMP);
                }
            }
        }

        private static bool MasterIsCompleted()
        {
            bool completed = true;
            for (int i = 0; i < readingTasksList.Count; i++)
            {
                if (!readingTasksList[i].IsCompleted)
                {
                    completed = false;
                }
            }
            return completed;
        }
        //method to generate negative pixel
        public static OneFrame GenerateNegativePixel(OneFrame frame)
        {
            Bitmap kepKocka = frame.BMP;
            Color c;
            for (int i = 0; i < kepKocka.Width; i++)
            {
                for (int j = 0; j < kepKocka.Height; j++)
                {
                    c = kepKocka.GetPixel(i, j);
                    c = Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                    kepKocka.SetPixel(i, j, c);
                }
            }
            negativeCounter++;
            Debug.Write("Negative generated for frame : " + negativeCounter + "\n");
            frame.BMP = kepKocka;
            return frame;
        }
        
    }
}
