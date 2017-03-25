using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EfficientSegmentation
{
    public class LockBitmap
    {
        public Bitmap Source { get; private set; }
        IntPtr Iptr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public byte[] Pixels { get; set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public LockBitmap(Bitmap source)
        {
            this.Source = source;
        }

        /// <summary>
        /// Блокирует данные изображения.
        /// </summary>
        public void LockBits()
        {
            try
            {
                Width = Source.Width;
                Height = Source.Height;
                int PixelCount = Width * Height;

                Rectangle rect = new Rectangle(0, 0, Width, Height);

                Depth = System.Drawing.Bitmap.GetPixelFormatSize(Source.PixelFormat);
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                //блокирует изображение и возвращается BitmapData
                bitmapData = Source.LockBits(rect, ImageLockMode.ReadWrite, Source.PixelFormat);

                //создает массив байтов, чтобы скопировать их значения пикселей
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];
                Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Разблокировать данные изображения.
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                //копировать данные из массива байтов в указатель
                Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                Source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Получить цвет определенного пикселя.
        /// </summary>
        /// <param name="x">Координата по оси X.</param>
        /// <param name="y">Координата по оси Y.</param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            Color clr = Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = Color.FromArgb(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = Color.FromArgb(c, c, c);
            }
            return clr;
        }

        /// <summary>
        /// Установить цвет определенного пикселя.
        /// </summary>
        public void SetPixel(int x, int y, Color color)
        {
            // Получить число цветовых компонентов
            int cCount = Depth / 8;

            // Получить стартовый индекс заданного пикселя
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }
    }
}
