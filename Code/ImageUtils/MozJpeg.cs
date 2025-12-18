using MozJpegSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Cupscale.ImageUtils
{
	class MozJpeg
	{
		public static void Encode(string inPath, string outPath, uint q, bool chromaSubSample = false)
		{
			try {
				using var bmp = new Bitmap(ImgUtils.GetImage(inPath));
				using var tjc = new TJCompressor();
				//byte[] compressed;
				TJSubsamplingOption subSample = TJSubsamplingOption.Chrominance420;
				if(!chromaSubSample){ subSample = TJSubsamplingOption.Chrominance444; }
				//var compressed = tjc.Compress(bmp, subSample, q, TJFlags.None);
				//File.WriteAllBytes(outPath, compressed);


				Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
				BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

				try {
					byte[] compressed = tjc.Compress(
						bmpData.Scan0,
						bmpData.Stride,
						bmpData.Width,
						bmpData.Height,
						TJPixelFormat.RGB,
						subSample,
						(int)q,
						TJFlags.None
					);

					File.WriteAllBytes(outPath, compressed);
				} finally {
					bmp.UnlockBits(bmpData);
				}

				Logger.Log("[MozJpeg] Written image to " + outPath);
			}
			catch (TypeInitializationException e)
			{
				Logger.ErrorMessage($"MozJpeg Initialization Error: {e.InnerException.Message}\n", e);
			}
			catch (Exception e)
			{
				Logger.ErrorMessage("MozJpeg Error: ", e);
			}
		}
	}
}
