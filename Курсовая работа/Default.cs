using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3DApplication
{
    /// <summary>
    /// Хранит все настройки по умолчанию.
    /// </summary>
    public struct Default
    {
        /// <summary>
        /// Радиус размытия по Гауссу.
        /// </summary>
        public const double Sigma = 0.5;
        /// <summary>
        /// Параметр пороговой функции при сегментации.
        /// </summary>
        public const double  ThresholdFunctionParameter = 100;
        /// <summary>
        /// Минимальный размер сегмента при сегментации.
        /// </summary>
        public const int MinSize = 100;
    }
}
