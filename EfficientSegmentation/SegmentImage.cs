using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace EfficientSegmentation
{
    /// <summary>
    /// Представляет ребро графа и его вес.
    /// </summary>
    public struct Edge
    {
        /// <summary>
        /// Вершина, инцедентная ребру.
        /// </summary>
        public int A, B;

        /// <summary>
        /// Вес ребра.
        /// </summary>
        public double W;

    }

    /// <summary>
    /// Класс, осуществляющий эффективную сегментацию изображений.
    /// </summary>
    public class SegmentImage
    {
        /// <summary>
        /// Радиус размытия по Гауссу.
        /// </summary>
        public double Sigma { get; protected set; }

        /// <summary>
        /// Параметр пороговой функции.
        /// </summary>
        public double ThresholdFunctionParameter { get; protected set; }

        /// <summary>
        /// Минимальный размер сегмента в итоговом сегментированном изображении.
        /// </summary>
        public int MinSize { get; protected set; }

        /// <param name="sigma">Радиус размытия по Гауссу.</param>
        /// <param name="thresholdFunctionParameter">Параметр пороговой функции.</param>
        /// <param name="minSize">Минимальный размер сегмента в итоговом сегментированном изображении.</param>
        public SegmentImage(double sigma, double thresholdFunctionParameter, int minSize)
        {
            Sigma = sigma;
            ThresholdFunctionParameter = thresholdFunctionParameter;
            MinSize = minSize;
        }


        /// <summary>
        /// Выполняет эффективную сегментацию изображения.
        /// </summary>
        /// <param name="inputBitmap">Изображение, которое будет сегментировано.</param>
        /// <param name="outputFolder">Путь к папке, в которую нужно сохранить изображение.</param>
        /// <param name="segmentOut">Массив представителей для каждого пикселя.</param>
        /// <returns>Сегментированное изображение.</returns>
        public Bitmap ExecuteEfficientSegmentation(Bitmap inputBitmap, string outputFolder, out int[] segmentOut)
        {
            Bitmap outputBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height);
            using (Graphics graphics = Graphics.FromImage(outputBitmap)) //рисует на поверхности smoothedImage
            {
                graphics.DrawImage(inputBitmap, 0, 0, inputBitmap.Width, inputBitmap.Height); //рисует на поверхности изображение inputBitmap
            }

            //обернуть изображение в lockBitmap и далее пользоваться им
            LockBitmap lockBitmap = new LockBitmap(outputBitmap);

            lockBitmap = new GaussianBlur(Sigma).SmoothImage(lockBitmap);

            int edgesCount; //число ребер в графе
            Edge[] edges = BuildGraph(lockBitmap, out edgesCount);
            DisjointSet disjointSet = SegmentGraph(edges, edgesCount, lockBitmap.Width*lockBitmap.Height);

            JointSmallSegments(disjointSet, edges, edgesCount);

            segmentOut = Colorize(lockBitmap, disjointSet);

            return lockBitmap.Source;
        }

        /// <summary>
        /// Строит граф, в котором ребра соединяют все соседние пиксели (в том числе и по диагонали).
        /// </summary>
        /// <param name="lockBitmap">Изображение, для которого строится граф.</param>
        /// <param name="count">Число ребер в выходном массиве ребер.</param>
        /// <returns>Массив ребер графа.</returns>
        private Edge[] BuildGraph(LockBitmap lockBitmap, out int count)
        {
            lockBitmap.LockBits();

            int width = lockBitmap.Width - 1;
            int height = lockBitmap.Height - 1;

            //от каждого пикселя строится 4 ребра: вправо, вниз, вправо-вниз, вправо-вверх.
            Edge[] edges = new Edge[width *height*4];
            count = 0;

            for (int y = 0; y < height; y++)
            {
                int YtimesWidth = y*width; //вершины нумеруются построчно
                for (int x = 0; x < width; x++)
                {
                    if (x < width - 1)
                    {
                        edges[count].A = YtimesWidth + x;
                        edges[count].B = YtimesWidth + (x + 1);
                        edges[count].W = Difference(lockBitmap, x, y, x + 1, y);
                        count++;
                    }

                    if (y < width - 1)
                    {
                        edges[count].A = YtimesWidth + x;
                        edges[count].B = YtimesWidth + width + x;
                        edges[count].W = Difference(lockBitmap, x, y, x, y + 1);
                        count++;
                    }

                    if (x < width - 1 && y < height - 1)
                    {
                        edges[count].A = YtimesWidth + x;
                        edges[count].B = YtimesWidth + width + (x + 1);
                        edges[count].W = Difference(lockBitmap, x, y, x + 1, y + 1);
                        count++;
                    }

                    if (x < width - 1 && y > 0)
                    {
                        edges[count].A = YtimesWidth + x;
                        edges[count].B = YtimesWidth - width + x + 1;
                        edges[count].W = Difference(lockBitmap, x, y, x + 1, y - 1);
                        count++;
                    }
                }
            }

            lockBitmap.UnlockBits();

            return edges;
        }

        /// <summary>
        /// Рассчитывает разницу между пикселями.
        /// </summary>
        /// <returns>Разница между пикселями.</returns>
        private double Difference(LockBitmap lockBitmap, int x1, int y1, int x2, int y2)
        {
            double diffR = Math.Pow((int)lockBitmap.GetPixel(x1, y1).R - lockBitmap.GetPixel(x2, y2).R, 2);
            double diffG = Math.Pow((int)lockBitmap.GetPixel(x1, y1).G - lockBitmap.GetPixel(x2, y2).G, 2);
            double diffB = Math.Pow((int)lockBitmap.GetPixel(x1, y1).B - lockBitmap.GetPixel(x2, y2).B, 2);

            return Math.Sqrt(diffR + diffG + diffB);
        }

        /// <summary>
        /// Строит на основе графа лес минимальных остовных деревьев, каждое из которых представляет сегмент изображения.
        /// </summary>
        /// <param name="edges">Набор ребер, представляющих исходный граф.</param>
        /// <param name="edgesCount">Число ребер в графе.</param>
        /// <param name="numVerticies">Число вершин в графе.</param>
        /// <returns>Система непересекающихся множеств, каждое из которых представляет собой сегмент изображения.</returns>
        private DisjointSet SegmentGraph(Edge[] edges, int edgesCount, int numVerticies)
        {
            QuickSort(edges, 0, edgesCount - 1);

            DisjointSet disjointSet = new DisjointSet(numVerticies);

            //значение пороговой функции изначально дано для каждой вершины
            double[] thresholdValues = new double[numVerticies]; 
            for (int i = 0; i < numVerticies; i++)
                thresholdValues[i] = ThresholdFunction(1);

            for (int i = 0; i < edgesCount; i++)
            {
                //найти представителей сегментов, которые соединяются этим ребром
                int a = disjointSet.Find(edges[i].A);
                int b = disjointSet.Find(edges[i].B);

                if (a != b)
                {
                    if (edges[i].W <= thresholdValues[a] && edges[i].W <= thresholdValues[b])
                    {
                        disjointSet.Joint(a, b);
                        a = disjointSet.Find(a);
                        thresholdValues[a] = edges[i].W + ThresholdFunction(disjointSet.Size(a));
                    }
                }
            }

            return disjointSet;
        }

        /// <summary>
        /// Функция быстрой сортировки массива ребера (или его сегмента).
        /// </summary>
        /// <param name="edges">Массив ребер, который надо отсортировать.</param>
        /// <param name="first">Индекс первого элемента в сортируемом сегменте массива.</param>
        /// <param name="last">Индекс последнего элемента в сортируемом сегменте массива.</param>
        /// <returns>Отсортированный массив ребер.</returns>
        private void QuickSort(Edge[] edges, int first, int last)
        {
            int i = first;
            int j = last;
            double x = edges[(first + last)/2].W; //вес опорного элемента

            do
            {
                while (edges[i].W < x)
                    i++;
                while (edges[j].W > x)
                    j--;
                if (i <= j)
                {
                    if (edges[i].W > edges[j].W)
                    {
                        Edge t = edges[i];
                        edges[i] = edges[j];
                        edges[j] = t;
                    }
                    i++;
                    j--;
                }
            } while (i <= j);

            if (i < last)
                QuickSort(edges, i, last);
            if (first < j)
                QuickSort(edges, first, j);

        }

        /// <summary>
        /// Вычисляет значение пороговой функции для заданного сегмента.
        /// </summary>
        /// <param name="size">Количество элементов в сегменте.</param>
        /// <returns>Значение пороговой функции.</returns>
        private double ThresholdFunction(int size)
        {
            return ThresholdFunctionParameter/size;
        }

        /// <summary>
        /// Объединяет маленькие сегменты.
        /// </summary>
        /// <param name="disjointSet">Система непересекающихся множеств, представляющая сегменты.</param>
        /// <param name="edges">Массив всех ребер изображения.</param>
        /// <param name="count">Число всех ребер изображения.</param>
        private void JointSmallSegments(DisjointSet disjointSet, Edge[] edges, int edgesCount)
        {
            for (int i = 0; i < edgesCount; i++)
            {
                int a = disjointSet.Find(edges[i].A);
                int b = disjointSet.Find(edges[i].B);
                if (a != b && (disjointSet.Size(a) < MinSize || disjointSet.Size(b) < MinSize))
                    disjointSet.Joint(a, b);
            }
        }

        /// <summary>
        /// Раскрашивает изображение.
        /// </summary>
        /// <param name="lockBitmap">Изображение, которое будет раскрашено.</param>
        /// <param name="disjointSet">Система непересекающихся множеств, описывающая сегменты.</param>
        /// <returns>Массив, в котором каждому пикселю поставлен в соответствие представитель множества, которому он принадлежит.</returns>
        private int[] Colorize(LockBitmap lockBitmap, DisjointSet disjointSet)
        {
            lockBitmap.LockBits();
            int width = lockBitmap.Width;
            int height = lockBitmap.Height;
            int[] segmentOut = new int[width*height]; //массив представителей для каждого пикселя (обход - построчно)


            //создадим массив случайных цветов для каждого пикселя
            //но на практике понадобятся не все цвета, а только цвет представителя каждого сегмента
            Random random = new Random();
            Color[] colors = new Color[width*height];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));

            int temp;
            for (int y = 0; y < height - 1; y++)
            {
                int YtimesWidth = y*(width-1);
                temp = y;
                for (int x = 0; x < width - 1; x++)
                {
                    int parent = disjointSet.Find(YtimesWidth + x);
                    lockBitmap.SetPixel(x, y, colors[parent]);
                    segmentOut[temp] = parent;
                    temp += height;
                }
            }

            lockBitmap.UnlockBits();
            return segmentOut;
        }
    }
}
