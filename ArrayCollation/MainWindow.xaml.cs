namespace ArrayCollation;

public partial class MainWindow : Window
{
    private async void BtnImport_Click(object sender, RoutedEventArgs e)
    {
        var append = chbAdd.IsChecked ?? false;
        tbStatus.Clear();
        var progressStatus = new Progress<string>(s => SetStatus(s));
        var file = GetFile(tbImportFile.Text);
        var delimiter = GetDelimiter();
        var encoding = GetEncoding();
        

        try
        {
            var resource = Consts.DATABASES.Where(r => r.Name == cbDb.SelectedValue.ToString()).FirstOrDefault();
            if (resource != null)
            {
                await Task.Run(() =>
                {
                    Import(file, resource.TableName, delimiter!, encoding, append, progressStatus);
                });
            }
            else SetStatus($"Не выбрана база данных.");
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }


    private async void BtnCollate_Click(object sender, RoutedEventArgs e)
    {
        tbStatus.Clear();
        var msg = "Сверка завершена.";
        try
        {
            CheckCollateReady();
            SavePaths();
            var progressStatus = new Progress<string>(s => SetStatus(s));
            var resource = Consts.DATABASES.Where(r => r.Name == cbDb.SelectedValue.ToString()).FirstOrDefault();
            var dbTableName = resource?.TableName;
            var gosuslugaPath = GetPath(tbGosusluga.Text);
            var resultPath = tbResult.Text;
            var delimiter = GetDelimiter();
            var encoding = GetEncoding();
            var append = chbAdd.IsChecked ?? false;
            if (!string.IsNullOrEmpty(gosuslugaPath))
            {
                var files = Directory.GetFiles(gosuslugaPath).Where(f => f.Contains(".csv")).ToList();
                if(files.Count == 0)
                {
                    SetStatus($"В папке {gosuslugaPath} отсутствуют файлы .csv");
                }
                else
                {
                    SetStatus($"Идет сверка c БД '{cbDb.SelectedValue}':");
                    foreach ( var file in files)
                    {
                        try
                        {
                            await Task.Run(() => 
                            { 
                                Collate(dbTableName!, file, resultPath, resource!.TableColumns, delimiter!, encoding, append, progressStatus); 
                            });
                        }
                        catch (Exception)
                        {
                            SetStatus($"Не удалось произвести сверку: {file}");
                            continue;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
           msg = ex.Message;
        }
        finally
        {
            SetStatus(msg);
        }
    }

    private string? GetDelimiter()
    {
        if (rbComa.IsChecked == true)
            return rbComa.Content?.ToString();
        else if (rbSemicolon.IsChecked == true)
            return rbSemicolon.Content?.ToString();
        return rbComa.Content?.ToString();
    }

    private Encoding GetEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (rb1251.IsChecked == true)
            return Encoding.GetEncoding("windows-1251");
        else if (rbUtf8.IsChecked == true)
            return Encoding.UTF8;
        return Encoding.GetEncoding("windows-1251");
    }

    private void Collate(string dbTableName, string file, string resultPath, string[] dbTableColumns, string delimiter, Encoding encoding, bool append, IProgress<string>? progressStatus = null)
    {
        Import(file, Consts.TABLE_GOSUSLUGA, delimiter, encoding, append, progressStatus!);
        using var db = new DataBasePG();
        var dbDateFormat = DataBasePG.GetDateFormat(db.SelectDate(dbTableName, Consts.CONTRACT_DR_FIELD)!);
        var gosuslugaDateFormat = DataBasePG.GetDateFormat(db.SelectDate(Consts.TABLE_GOSUSLUGA, Consts.CONTRACT_DR_FIELD)!);
        var resultFileName = $"{Path.GetFileName(file).Replace(".csv", "")}_{dbTableName}_result.csv";
        var resultFile = Path.Combine(resultPath, resultFileName);
        string statement = string.Empty;
        statement = dbTableName == Consts.TABLE_DEAD 
            ? DataBasePG.GetDeadExportResultStatement(Consts.TABLE_GOSUSLUGA, resultFile, delimiter, dbTableColumns)
            : DataBasePG.GetExportResultStatement(dbTableName, dbDateFormat, Consts.TABLE_GOSUSLUGA, gosuslugaDateFormat, resultFile, delimiter, dbTableColumns);
        progressStatus?.Report($"Идет сверка.");
        var count = db.Execute(statement);
        progressStatus?.Report($"Совпадений: {count}");
        var lines = File.ReadAllLines(resultFile);
        if(lines.Length - 1 > 0) 
        {
            progressStatus?.Report($"Экспортирую в Excel.");
            var path = Path.Combine(resultPath, "Excel");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var excelFileName = Path.GetFileName(resultFile).Replace(".csv", $"_{lines.Length - 1}.xlsx");
            var excelFile = path + "\\" + excelFileName;
            ExportToExcel(excelFile, lines, delimiter);
        }
    }

    private void ExportToExcel(string excelFile, string[] lines, string delimiter)
    {
        using var excel = new ExcelApp();
        excel.OpenDoc(excelFile);
        for (var r = 0; r < lines.Length; r++)
        {
            var line = Statement.ParseCsvString(lines[r], delimiter[0]);
            for(var c = 0; c  < line.Length; c++)
            {
                excel.SetValue(line[c], r+1, c+1);
            }
        }
        excel.SaveBook();
    }

    private static void Import(string file, string tableName, string delimiter, Encoding sourceEncoding, bool append, IProgress<string>? progressStatus = null)
    {
        var fileInWork = file;
        if (string.IsNullOrEmpty(fileInWork))
        {
            progressStatus?.Report($"Файл не существует: {fileInWork}");
            return;
        }
        if (!append && tableName != Consts.TABLE_GOSUSLUGA)
        {
            if (!YesNoDialog())
            { return; }
        }

        progressStatus?.Report($"\r\nИмпортирую файл {Path.GetFileName(fileInWork)}");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var targetEncoding = Encoding.GetEncoding("windows-1251");

        if (sourceEncoding == Encoding.UTF8)
        {
            var convertedFileName = Path.GetFileName(file);
            var convertedFolder = Path.Combine(Path.GetDirectoryName(file)!, "converted");
            var convertedFile = convertedFolder + "\\" + convertedFileName;
            if (!File.Exists(convertedFile))
            {
                if (!Directory.Exists(convertedFolder))
                    Directory.CreateDirectory(convertedFolder);
                progressStatus?.Report($"Конвертирую файл {Path.GetFileName(fileInWork)}.");
                Statement.ConvertFileEncoding(file, convertedFile, sourceEncoding, targetEncoding);
            }
            fileInWork = convertedFile;
        }

        var headers = Statement.GetHeaders(fileInWork, delimiter, targetEncoding);

        if (!CheckContract(headers, tableName))
        {
            throw new Exception($"Импортируемый файл БД должен иметь поля '{Consts.CONTRACT_FIO_FIELD}' и '{Consts.CONTRACT_DR_FIELD}'."
                + $"Файл госулуг кроме того поле '{Consts.CONTRACT_CASE_DATE}'");
        }

        progressStatus?.Report($"Вычисляю размеры полей.");
        var sizes = Statement.GetColumnsSize(fileInWork, delimiter, targetEncoding);

        var statementCreateTable = Statement.GetStatement(tableName, headers, sizes);

        using var db = new DataBasePG();
        if (!append && db.DoesTableExist(tableName))
            db.DropTable(tableName);
        if (!db.DoesTableExist(tableName))
            db.Execute(statementCreateTable);

        progressStatus?.Report($"Импортирую данные в таблицу {tableName}.");
        var rowsCount = db.ImportData(fileInWork, tableName, delimiter);
        progressStatus?.Report($"Импортировано строк: {rowsCount}");
    }

    private static bool CheckContract(List<string> headers, string tableName)
    {
        headers = Statement.ConvertHeaders(headers);
        var contractForDb = headers.Contains(Consts.CONTRACT_FIO_FIELD)
            && headers.Contains(Consts.CONTRACT_DR_FIELD);
        var contractForGosusluga = contractForDb
            && headers.Contains(Consts.CONTRACT_CASE_DATE);
        var result = tableName == Consts.TABLE_GOSUSLUGA ? contractForGosusluga : contractForDb;
        return result;
    }

    private void CheckCollateReady()
    {
        var resource = Consts.DATABASES.Where(r => r.Name == cbDb.SelectedValue.ToString()).FirstOrDefault();
        if (string.IsNullOrEmpty(cbDb.SelectedValue?.ToString()))
            throw new Exception("Не выбрана БД");
        using var db = new DataBasePG();
        if (!db.DoesTableExist(resource!.TableName))
            throw new Exception($"БД {cbDb.SelectedValue} не существует.");
        if(db.GetRowCount(resource.TableName) == 0)
            throw new Exception($"В БД {cbDb.SelectedValue} нет записей.");
        if (string.IsNullOrEmpty(GetPath(tbGosusluga.Text)))
            throw new Exception($"Путь (госуслуги) не существует {tbGosusluga.Text}");
        else tbGosusluga.Text = GetPath(tbGosusluga.Text);
        if (string.IsNullOrEmpty(GetPath(tbResult.Text)))
            tbResult.Text = GetResultsPath(tbGosusluga.Text);
    }

    private string GetResultsPath(string sourcePath)
    {
        var result = Path.Combine(sourcePath, "results");
        if(!Directory.Exists(result))
            Directory.CreateDirectory(result);
        return result;
    }
}