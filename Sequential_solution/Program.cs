using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Sequential_solution
{
    class Program
    {
        public static int szamlalo = 0;
        public static string fileName = "mediumvid.mp4";
        static List<Bitmap> read = new List<Bitmap>();
        static List<Bitmap> write = new List<Bitmap>();

        static void Main(string[] args)
        {Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            VideoFileReader reader = new VideoFileReader();
            reader.Open(fileName);
            for (int i = 0; i < reader.FrameCount; i++)
            {
                read.Add(reader.ReadVideoFrame());
                write.Add(NegativGeneralas(read[i]));
            }

            VideoFileWriter writer = new VideoFileWriter();
            writer.Open("eredmeny_" + fileName, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MPEG4, reader.BitRate);
            for (int i = 0; i < write.Count; i++)
            {
                writer.WriteVideoFrame(write[i]);
            }
            reader.Close();
            writer.Close();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.ReadLine();
        }

        public static Bitmap NegativGeneralas(Bitmap kepKocka)
        {
            
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
            szamlalo++;
            Debug.Write(szamlalo + "\n");
            return kepKocka;   
        }
    }
}
