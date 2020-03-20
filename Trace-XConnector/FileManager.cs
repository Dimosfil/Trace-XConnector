using System;
using System.Globalization;
using System.IO;

namespace Trace_XConnector
{
    public class FileManager
    {
        public static FileManager Instance => instance;
        public static FileManager instance;
        public static void Init()
        {
            instance = new FileManager();
        }

        public void WriteXml(string text)
        {
            // создаем каталог для файла
            string path = @"C:\work\XConnectorXml";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            string name = "orderData_" + DateTime.Now.ToString("s");

            name = name.Replace(':', '_');
            // запись в файл
            using (FileStream fstream = new FileStream($"{path}\\{name}.txt", FileMode.OpenOrCreate))
            {
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes(text);
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);
                Console.WriteLine("Текст записан в файл");
            }
        }

    }
}