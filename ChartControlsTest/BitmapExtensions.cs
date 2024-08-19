using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ChartControlsTest {
    internal static class BitmapExtensions {
        public static bool PixelsEquals(this BitmapSource image, BitmapSource otherimage) {
            byte[] pixels1 = image.GetPixels();
            byte[] pixels2 = otherimage.GetPixels();
            bool result = pixels1.SequenceEqual(pixels2);
            if (result) return true;
            if (pixels1.Length != pixels2.Length) {
                Debug.WriteLine($"Deviation in size!");
                return false;
            }
            for (int i = 0; i < pixels1.Length; i++) {
                if (pixels1[i] != pixels2[i]) {
                    Debug.WriteLine($"Pixel deviation: Pixel {i}, {pixels1[i]} vs {pixels2[i]}");
                    result = false;
                }
            }
            return result;
        }

        public static byte[] GetPixels(this BitmapSource image) {
            int stride = image.GetStride();
            byte[] pixels = new byte[image.PixelHeight * stride];
            image.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public static int GetStride(this BitmapSource image) {
            return image.PixelWidth * (image.Format.BitsPerPixel / 8);
        }

        public static BitmapSource Subtract(this BitmapSource image, BitmapSource otherimage) {
            byte[] pixels1 = image.GetPixels();
            byte[] pixels2 = otherimage.GetPixels();
            if (pixels1.Length != pixels2.Length) return null;
            for (int i = 0; i < pixels1.Length; i++) {
                if (pixels1[i] != pixels2[i]) {
                    if (pixels1[1] == 255) pixels1[i] = pixels2[i];
                    else if (pixels2[1] != 255) pixels1[i] = Math.Min(pixels1[i], pixels2[i]);
                } else {
                    pixels1[i] = (byte)255;
                }
            }
            return BitmapSource.Create(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY, image.Format, image.Palette, pixels1, image.GetStride());
        } 

        public static void SaveToFile(this BitmapSource image, string path) {
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using FileStream stream = File.Create(path); encoder.Save(stream);
        }
    }
}
