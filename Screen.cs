using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Screen
{
	public class ScreenReader
	{
		private List<byte[]> buffer = new List<byte[]>();
		public static Process proc;
		User32.Rect rect = new User32.Rect();

		public void GetObsInstance()
		{
			if (Process.GetProcessesByName("obs64").Count() > 0)
			{
				Console.WriteLine("found obs64 instance!");
				proc = Process.GetProcessesByName("obs64")[0];
			}
			else if (Process.GetProcessesByName("obs32").Count() > 0)
			{
				Console.WriteLine("found obs32 instance!");
				proc = Process.GetProcessesByName("obs32")[0];
			}
			else
			{
				Console.WriteLine("Cannot find OBS instance");
				Thread.Sleep(1000);
			}
		}

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
			if (proc != null)
			{
				proc.Refresh();
				User32.GetWindowRect(proc.MainWindowHandle, ref rect);
				int width = rect.right - rect.left;
				int height = rect.bottom - rect.top;

				var bmp = new Bitmap(48, 24, PixelFormat.Format24bppRgb);
				Graphics graphics = Graphics.FromImage(bmp);
				graphics.CopyFromScreen(rect.left + width / 2, rect.top + height / 4, 0, 0, new Size(48, 24), CopyPixelOperation.SourceCopy);
				//bmp.Save("pic.png", ImageFormat.Png);
				var testRect = new Rectangle(0, 0, 48, 24);
				BitmapData bmpData = bmp.LockBits(testRect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
				IntPtr ptr = bmpData.Scan0;
				int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
				byte[] rgbValues = new byte[bytes];

				// Copy the RGB values into the array.
				System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
				return rgbValues;
			}
			return new byte[0];
		}

		public bool IsLoading()
		{
			var colorData = CaptureTestArea();
			UpdateBuffer(colorData);

			List<bool> blackBuffer = new List<bool>();
			foreach (var b in buffer)
			{
				var avg = b.Average(x => int.Parse(x.ToString()));
				if (avg > 10)
				{
					Console.Write("\r{0}%   ", avg);
					//buffer.Clear();
					blackBuffer.Add(false);
				}
				else
				{
					Console.Write("\r{0}%   ", avg);
					blackBuffer.Add(true);
				}
			}

			if (blackBuffer.All(t => t == true))
			{
				return true;
			}
			else
			{
				return false;
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
