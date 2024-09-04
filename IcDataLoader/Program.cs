using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace IcDataLoader
{
    class Program
    {
        const string BdFr = "БД ФР";
        const string BdZags = "БД ЗАГС";
        static void Main(string[] args)
        {
            Console.WriteLine($"Программа получает из '{BdFr}','{BdZags}' информацию и записывает ее в csv-файл.");
            Console.WriteLine($"Заголовки таблицы получает из файлов headersFR.txt, headersZAGS.txt");
            Console.WriteLine($"Кодировка выходного файла: 1251, Разделитель ;");
            while (true)
            {
                try
                {
                    Console.WriteLine($"Введите 1, чтобы выгрузить '{BdFr}' или 2, чтобы выгрузить {BdZags}:");
                    var inputDB = Console.ReadLine();

                    var db = new OraConnector();
                    db.Connect();
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var exporter = new Exporter();

                    string file = null;
                    IEnumerable<string> headers = null;
                    List<IcData> data = new List<IcData>();
                    string delimiter = ";";
                    Encoding encoding = Encoding.GetEncoding("windows-1251");

                    if (inputDB == "1")
                    {
                        file = "exportFromFR.csv";
                        if (File.Exists(file)) { File.Delete(file); }

                        headers = File.ReadLines("headersFR.txt");
                        Console.WriteLine($"Получаю данные из {BdFr}.");
                        data = db.GetFrData();
                    }

                    else if (inputDB == "2")
                    {
                        Console.WriteLine($"Введите начальную дату (формат 00.00.0000):");
                        var startDate = Console.ReadLine();

                        Console.WriteLine($"Введите конечную дату:");
                        var endDate = Console.ReadLine();

                        file = $"exportFromZAGS_{startDate}_{endDate}.csv";
                        if (File.Exists(file)) { File.Delete(file); }

                        headers = File.ReadLines("headersZAGS.txt");
                        Console.WriteLine($"Получаю данные из {BdZags}.");
                        data = db.GetZagsData(startDate, endDate);
                    }
                    else { Console.WriteLine($"Введено некорректное значение."); }
                    db.Disconnect();

                    if (data.Count > 0)
                    {
                        Console.WriteLine($"Экспорт строк ({data.Count}) в файл {file}.");
                        exporter.ExportToCSV(file, data, headers, delimiter, encoding);
                    }
                    else { Console.WriteLine($"Количество полученных строк из бд - 0."); }

                    Console.WriteLine($"Работа завершена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
