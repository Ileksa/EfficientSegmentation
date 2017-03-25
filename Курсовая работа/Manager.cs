using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Make3DApplication
{
    /// <summary>
    /// Класс, являющийся посредником между пользовательским интерфейсом и непосредственно классами вычислений.
    /// </summary>
    public partial class Manager
    {
        private string _imgPath;
        private string _parametersFolder;
        private string _outputFolder;

        /// <summary>
        /// Описание последней ошибки, которая возникла при выполнении программы.
        /// </summary>
        public string ErrorMessage { get; private set; }

        public Manager(string outputFolder)
        {
            InitializeParametersFolder();
            _imgPath = String.Empty;
            _outputFolder = outputFolder ?? Directory.GetCurrentDirectory();
            ErrorMessage = String.Empty;
        }

        /// <summary>
        /// Задает путь к папке с фиксированными параметрами для MRF.
        /// </summary>
        private void InitializeParametersFolder()
        {
            string path;
            try
            {
                //подняться на две директории выше
                path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = "Не удалось найти все необходимые компоненты программы: папка params не найдена.";
                path = String.Empty;
                return;
            }

            path = Path.Combine(path, "params");
            _parametersFolder = path;
        }

        /// <summary>
        /// Осуществляет создание 3d модели из двухмерного изображения.
        /// </summary>
        /// <param name="imagePath">Путь к двухмерному изображению.</param>
        /// <returns>Истина, если операция завершена успешно.</returns>
        public bool Create3DFrom2D(string imagePath)
        {
            //Проверка входных параметров, всех путей к файлам и папкам
            if (_parametersFolder == String.Empty)
                return false;

            Bitmap bitmap;
            try
            {
                _imgPath = imagePath;
                bitmap = new Bitmap(imagePath);
            }
            catch (ArgumentException)
            {
                ErrorMessage = "Изображение не найдено";
                return false;
            }

            //генерация суперпикселей
            GenerateSuperpixelsEfficiently(bitmap);

            return true;
        }

    }
}
