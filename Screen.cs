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
		private List<Color> buffer = new List<Color>();
		public Color PollPixel(Bitmap image, int x, int y)
		{
			var pixelColor = image.GetPixel(x, y);
			if (buffer.Count >= 24)
			{
				buffer.Remove(buffer[0]);
				buffer.Remove(buffer[1]);
				buffer.Remove(buffer[2]);
				buffer.Remove(buffer[3]);
				buffer.Remove(buffer[4]);
				buffer.Remove(buffer[5]);
			}
			buffer.Add(pixelColor);
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

		public bool IsLoading(string procName, int x, int y)
		{
			var window = CaptureApplication(procName);
			if (x > 3 && y > 3)
			{
				PollPixel(window, window.Width/2, window.Height/3);
				PollPixel(window, window.Width/2 - 5, window.Height/3 - 5);
				PollPixel(window, window.Width/2 + 15, window.Height/3 - 20);
				PollPixel(window, window.Width/2 - 5, window.Height/3 - 25);
				PollPixel(window, window.Width/2 + 15, window.Height/3 - 15);
				PollPixel(window, window.Width/2 + 30, window.Height/3 - 30);
				

				if (buffer.Any(o => o != buffer[0]) == false && buffer.All(o => o.R < 32 && o.G < 32 && o.B < 32))
				{
					Console.WriteLine("resetting");
					buffer.Clear();
					return false;
				}
				else if (buffer.All(o => o.R < 12 && o.G < 12 && o.B < 12)) {
					buffer.Clear();
					return false;
				}
				else
				{
					Console.WriteLine("playing");
					return true;
				}
			}
			else
			{
				Console.WriteLine("playing");
				return true;
			}
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
