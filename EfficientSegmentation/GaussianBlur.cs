using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EfficientSegmentation
{
    /// <summary>
    /// Класс, предоставляющий возможность применить к изображению размытие по Гауссу.
    /// </summary>
    public class GaussianBlur
    {
        /// <summary>
        /// Параметр, используемый при вычислении длины вектора скручивания.
        /// </summary>
        public const double Width = 4;
        private double _sigma;
        /// <summary>
        /// Радиус размытия.
        /// </summary>
        public double Sigma
        {
            get { return _sigma; }
            set { _sigma = value > 0.01 ? value : 0.01; } //0.01 выбрано с практической точки зрения
        }

        /// <param name="sigma">Радиус размытия.</param>
        public GaussianBlur(double sigma)
        {
            Sigma = sigma;
        }

        /// <summary>
        /// Осуществляет размытие данного изображения.
        /// </summary>
        /// <param name="inputImage">Изображение, которое будет размыто.</param>
        /// <returns>Размытое изображение.</returns>
        public LockBitmap SmoothImage(LockBitmap inputImage)
        {
            double[] mask = CreateConvolutionVector();
            NormalizeVector(mask);
            //дважды применяем маску, чтобы осуществить размытие как по горизонтали, так и по вертикали
            LockBitmap smoothedImage = ApplyMask(inputImage, mask);
            smoothedImage = ApplyMask(smoothedImage, mask);

            return smoothedImage;

        }

        /// <summary>
        /// Создает вектор, на основе которого будет рассчитываться влияние соседних пикселей.
        /// </summary>
        /// <returns>Вектор скручивания (по аналогии с матрицей скручивания)</returns>
        private double[] CreateConvolutionVector()
        {
            int len = (int) Math.Round(Sigma + Width + 0.5) + 1;
            double[] mask = new double[len];

            for (int i = 0; i < len; i++)
                //mask[i] = Math.Exp(-0.5*Math.Sqrt(i/Sigma)); //если будет плохое размытие - использовать этот вариант
                mask[i] = Math.Exp(-0.5*Math.Pow(i/Sigma, 2));
            return mask;
        }

        /// <summary>
        /// Нормирует вектор.
        /// </summary>
        /// <param name="vector">Вектор, подлежащий нормированию.</param>
        private void NormalizeVector(double[] vector)
        {
            double sum = 0;
            for (int i = 1; i < vector.Length; i++)
                sum += Math.Abs(vector[i]);

            sum = 2*sum + Math.Abs(vector[0]);

            for (int i = 0; i < vector.Length; i++)
                vector[i] /= sum;
        }

        /// <summary>
        /// Применяет вектор скручивания к исходному изображению и на его основе формирует новое изображение.
        /// </summary>
        /// <param name="inputBitmap">Исходное изображение.</param>
        /// <param name="mask">Вектор скручивания.</param>
        /// <returns>Новое повернутое изображение, созданное на основе исходного с примененим вектора скручивания.</returns>
        private LockBitmap ApplyMask(LockBitmap inputBitmap, double[] mask)
        {
            inputBitmap.LockBits();

            Bitmap outputBitmap = new Bitmap(inputBitmap.Height, inputBitmap.Width);
            LockBitmap lockBitmap = new LockBitmap(outputBitmap);

            lockBitmap.LockBits();

            //проходим строчку за строчкой, для каждого элемента вычисляя определенную сумму
            for (int y = 0; y < inputBitmap.Height; y++)
            {
                for (int x = 0; x < inputBitmap.Width; x++)
                {
                    Color color = inputBitmap.GetPixel(x, y);
                    double sumR = mask[0]*color.R;
                    double sumG = mask[0]*color.G;
                    double sumB = mask[0]*color.B;

                    for (int i = 1; i < mask.Length; i++)
                    {
                        //просмотр соседних пикселей на расстоянии i от заданного координатам (x,y)
                        Color leftColor = inputBitmap.GetPixel(Math.Max(x - i, 0), y);
                        Color rightColor = inputBitmap.GetPixel(Math.Min(x + i, inputBitmap.Width - 1), y);
                        sumR += mask[i]*(leftColor.R + rightColor.R);
                        sumG += mask[i]*(leftColor.G + rightColor.G);
                        sumB += mask[i]*(leftColor.B + rightColor.B);
                    }
                    lockBitmap.SetPixel(y, x, Color.FromArgb((int)sumR, (int)sumG, (int)sumB));
                }
            }

            lockBitmap.UnlockBits();
            inputBitmap.UnlockBits();

            return lockBitmap;
        }
    }
}
