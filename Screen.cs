using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Screen
{
    class Pixel
    {
        public Color PollPixel(Bitmap image, int x, int y)
        {
            var pixelColor = image.GetPixel(x, y);
            Console.WriteLine(pixelColor);
            return pixelColor;
        }

        public Bitmap CaptureApplication(string procName)
        {
            var proc = Process.GetProcessesByName(procName)[0];
            var rect = new User32.Rect();
            User32.GetWindowRect(proc.MainWindowHandle, ref rect);

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            return bmp;
        }

        public bool ShouldPlay(string procName, int x, int y)
        {
            var window = CaptureApplication(procName);
            if (x > 3 && y > 3)
            {
                var pixel1 = PollPixel(window, x, y);
                var pixel2 = PollPixel(window, x - 3, y - 3);
                var pixel3 = PollPixel(window, x + 3, y + 3);

                if (IsBlack(pixel1) && IsBlack(pixel2) && IsBlack(pixel3))
                {
                    return false;
                }
                else return true;
            }
            else return true;
        }

        public bool IsBlack(Color c)
        {
            if (c.R < 14 && c.G < 14 && c.B < 14)
            {
                return true;
            }
            else return false;
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
