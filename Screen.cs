using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Screen
{
    public class ScreenReader
    {
        private List<byte[]> buffer = new List<byte[]>();
        Process proc = Process.GetProcessesByName("obs64")[0];
        User32.Rect rect = new User32.Rect();

        public void UpdateBuffer(byte[] data)
        {
            if (buffer.Count >= 2)
            {
                buffer.Remove(buffer[0]);
            }
            buffer.Add(data);
        }

        public byte[] CaptureTestArea()
        {
            User32.GetWindowRect(proc.MainWindowHandle, ref rect);
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

            var testRect = new Rectangle(bmp.Width / 2, bmp.Height / 3, 24, 24);
            BitmapData bmpData = bmp.LockBits(testRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            return rgbValues;
        }

        public bool IsLoading()
        {
            var colorData = CaptureTestArea();
            UpdateBuffer(colorData);

            List<bool> blackOrNot = new List<bool>();
            foreach (var b in buffer)
            {
                var avg = b.Average(x => int.Parse(x.ToString()) * 2);
                if (avg > 75)
                {
                    Console.WriteLine("not black enough: " + avg);
                    //buffer.Clear();
                    blackOrNot.Add(false);
                }
                else
                {
                    Console.WriteLine("black enough: " + avg);
                    blackOrNot.Add(true);
                }
            }

            if (blackOrNot.All(t => t != true))
            {
                Console.WriteLine("Not Loading");
                return false;
            }
            else
            {
                Console.WriteLine("Loading");
                return true;
            }

        }

        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Rect
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
        }
    }
}
