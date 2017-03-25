using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EfficientSegmentation;

namespace Make3DApplication
{
    public partial class Manager
    {

        /// <summary>
        /// Функция генерирует суперпиксели, используя параметры по умолчанию.
        /// </summary>
        private void GenerateSuperpixelsEfficiently(Bitmap bitmap)
        {
            double[] scales = {0.8, 1.6, 5}; //масштабы суперпикселей (small, middle, large)

            int width = bitmap.Width;
            int height = bitmap.Height;

            //для каждого масштаба
            for (int j = 0; j < 3; j++)
            {
                SegmentImage segmentImageManager = new SegmentImage(Default.Sigma * scales[j],
                    Default.ThresholdFunctionParameter*scales[j], (int) (Default.MinSize*scales[j]));

                int[] segmentOut;
                Bitmap output = segmentImageManager.ExecuteEfficientSegmentation(bitmap, _outputFolder, out segmentOut);


            }

        }
    }
}
