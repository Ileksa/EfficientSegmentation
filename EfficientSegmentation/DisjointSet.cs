using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientSegmentation
{
    /// <summary>
    /// Описывает подмножество из системы непересекающихся множеств.
    /// </summary>
    public struct SubSetProperties
    {
        /// <summary>
        /// Ранг подмножества, означающий, что длина длиннейшей ветви не превышает это число.
        /// </summary>
        public int Rank { get; set; }
        /// <summary>
        /// Представитель подмножества.
        /// </summary>
        public int Parent { get; set; }
        /// <summary>
        /// Число элементов подмножества.
        /// </summary>
        public int Size { get; set; }
    }
    /// <summary>
    /// Система непересекающихся множеств, использующая ранги и сжатие путей.
    /// </summary>
    public class DisjointSet
    {
        private SubSetProperties[] _subsetsProperties;
        /// <summary>
        /// Число подмножеств.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Создает систему непересекающихся множеств, каждое из которых состоит из 1 элемента.
        /// </summary>
        /// <param name="elements">Число непересекающихся подмножеств.</param>
        public DisjointSet(int elements)
        {
            _subsetsProperties = new SubSetProperties[elements];
            Count = elements;
            for (int i = 0; i < elements; i++)
            {
                _subsetsProperties[i].Rank = 0;
                _subsetsProperties[i].Parent = i;
                _subsetsProperties[i].Size = 1;
            }

        }

        /// <summary>
        /// Найти представителя подмножества, которому принадлежит элемент x.
        /// </summary>
        /// <param name="x">Элемент, для которого осуществляется поиск.</param>
        /// <returns>Представитель подмножества.</returns>
        public int Find(int x)
        {
            int y = x;
            while (y != _subsetsProperties[y].Parent)
                y = _subsetsProperties[y].Parent;
            _subsetsProperties[x].Parent = y; //осуществляем сжатие пути для исходного элемента
            return y;
        }

        /// <summary>
        /// Объединить два подмножества в одно.
        /// </summary>
        /// <param name="x">Представитель первого подмножества.</param>
        /// <param name="y">Представитель второго подмножества.</param>
        public void Joint(int x, int y)
        {
            if (_subsetsProperties[x].Rank > _subsetsProperties[y].Rank)
            {
                _subsetsProperties[y].Parent = x;
                _subsetsProperties[x].Size += _subsetsProperties[y].Size;
            }
            else
            {
                _subsetsProperties[x].Parent = y;
                _subsetsProperties[y].Size += _subsetsProperties[x].Size;
                if (_subsetsProperties[x].Rank == _subsetsProperties[y].Rank)
                    _subsetsProperties[y].Rank++;
            }
            Count--;
        }

        /// <summary>
        /// Получить размер подмножества, к которому принадлежит элемент x.
        /// </summary>
        /// <param name="x">Представитель подмножества.</param>
        /// <returns>Число элементов в подмножестве.</returns>
        public int Size(int x)
        {
            return _subsetsProperties[x].Size;
        }
    }
}
