<<<<<<< HEAD
using System.Text;

namespace ACTests;

public class Tests
{
    private static readonly string currentDir = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string testDataDir = Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\testdata\"));
    private static readonly Resource? _resourceCL = Consts.DATABASES.Where(r => r.Name == "РљРѕРЅС‚СЂРѕР»СЊРЅС‹Р№ СЃРїРёСЃРѕРє").FirstOrDefault();
    
    [Theory]
    [MemberData(nameof(GetDateFormatTestData))]
    public void GetDateFormatTest(string date, string expected)
    {
        var actual = DataBasePG.GetDateFormat(date);
        Assert.Equal(expected, actual);
    }
    public static IEnumerable<object[]> GetDateFormatTestData()
    {
        yield return new object[] { "1999-01-01", "YYYY-MM-DD" };
        yield return new object[] { "01.01.1999", "DD.MM.YYYY" };
        yield return new object[] { "", "" };
        yield return new object[] { "01/01/1999", "" };
        yield return new object[] { null, "" };
    }

    [Fact]
    public void SelectDateTest()
    {
        using var db = new DataBasePG();
        var actual = db.SelectDate(_resourceCL!.TableName, Consts.CONTRACT_DR_FIELD);
        Assert.True(!string.IsNullOrEmpty(actual));
    }

    [Fact]
    public void GetExportResultStatementSimpleTest()
    {
        var resultFile = "result.csv";
        var expected = $"COPY ("
            + $"SELECT DISTINCT {Consts.TABLE_GOSUSLUGA}.* "
            + $"FROM {Consts.TABLE_GOSUSLUGA} "
            + $"INNER JOIN {_resourceCL!.TableName} ON "
            + $"(TRIM(LOWER({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD})) = TRIM(LOWER({_resourceCL.TableName}.{Consts.CONTRACT_FIO_FIELD})) "
            + $"AND to_date({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_DR_FIELD}, 'YYYY-MM-DD') = to_date({_resourceCL.TableName}.{Consts.CONTRACT_DR_FIELD}, 'DD.MM.YYYY')) "
            + $"ORDER BY {Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD}"
            + $") TO '{resultFile}' DELIMITER ';' CSV HEADER";

        using var db = new DataBasePG();
        var dateDB = db.SelectDate(_resourceCL.TableName, Consts.CONTRACT_DR_FIELD);
        var dateFormatDB = DataBasePG.GetDateFormat(dateDB!);

        var actual = DataBasePG.GetExportResultStatement(_resourceCL.TableName, dateFormatDB, Consts.TABLE_GOSUSLUGA, "YYYY-MM-DD", "result.csv", ";");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetExportResultStatementAddFieldsTest()
    {
        var resultFile = "result.csv";
        var expected = $"COPY ("
            + $"SELECT DISTINCT {Consts.TABLE_GOSUSLUGA}.*, {_resourceCL!.TableName}.status, {_resourceCL.TableName}.data_zagruzki "
            + $"FROM {Consts.TABLE_GOSUSLUGA} "
            + $"INNER JOIN {_resourceCL.TableName} ON "
            + $"(TRIM(LOWER({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD})) = TRIM(LOWER({_resourceCL.TableName}.{Consts.CONTRACT_FIO_FIELD})) "
            + $"AND to_date({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_DR_FIELD}, 'YYYY-MM-DD') = to_date({_resourceCL.TableName}.{Consts.CONTRACT_DR_FIELD}, 'DD.MM.YYYY')) "
            + $"ORDER BY {Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD}"
            + $") TO '{resultFile}' DELIMITER ';' CSV HEADER";

        using var db = new DataBasePG();
        var dateDB = db.SelectDate(_resourceCL.TableName, Consts.CONTRACT_DR_FIELD);
        var dateFormatDB = DataBasePG.GetDateFormat(dateDB!);

        var actual = DataBasePG.GetExportResultStatement(_resourceCL.TableName, dateFormatDB, Consts.TABLE_GOSUSLUGA, "YYYY-MM-DD", "result.csv", ";", ["status", "data_zagruzki"]);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetEncodingTestData))]
    public void GetEncodingTest(string file, Encoding expected)
    {
        var actual = CsvHelper.GetEncoding(file);
        Assert.Equal(expected, actual);
    }

    
    public static IEnumerable<object[]> GetEncodingTestData()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var windows1251 = Encoding.GetEncoding("windows-1251");
        var utf_8 = Encoding.GetEncoding("utf-8");

        yield return new object[] { Path.Combine(testDataDir, "windows-1251;.csv"), windows1251 };
        yield return new object[] { Path.Combine(testDataDir, "windows-1251;(2).csv"), windows1251 };
        yield return new object[] { Path.Combine(testDataDir, "utf-8,.csv"), utf_8 };
        yield return new object[] { Path.Combine(testDataDir, "utf-8,(2).csv"), utf_8 };
    }

    [Theory]
    [MemberData(nameof(DetectSeparatorData))]
    public void DetectSeparatorTest(string file, char expected)
    {
        var actual = CsvHelper.DetectSeparator(file);
        Assert.Equal(expected, expected);
    }
    
    public static IEnumerable<object[]> DetectSeparatorData()
    {
        yield return new object[] { Path.Combine(testDataDir, "windows-1251;.csv"), ';' };
        yield return new object[] { Path.Combine(testDataDir, "windows-1251;(2).csv"), ';' };
        yield return new object[] { Path.Combine(testDataDir, "utf-8,.csv"), ',' };
        yield return new object[] { Path.Combine(testDataDir, "utf-8,(2).csv"), ',' };
    }
=======
using Microsoft.VisualBasic;

namespace ACTests;

public class Tests
{
    private static Resource? _resourceCL = Consts.DATABASES.Where(r => r.Name == "Контрольный список").FirstOrDefault();
    
    [Theory]
    [MemberData(nameof(GetDateFormatTestData))]
    public void GetDateFormatTest(string date, string expected)
    {
        var actual = DataBasePG.GetDateFormat(date);
        Assert.Equal(expected, actual);
    }
    public static IEnumerable<object[]> GetDateFormatTestData()
    {
        yield return new object[] { "1999-01-01", "YYYY-MM-DD" };
        yield return new object[] { "01.01.1999", "DD.MM.YYYY" };
        yield return new object[] { "", "" };
        yield return new object[] { "01/01/1999", "" };
        yield return new object[] { null, "" };
    }

    [Fact]
    public void SelectDateTest()
    {
        
        using var db = new DataBasePG();
        var actual = db.SelectDate(_resourceCL!.TableName, Consts.CONTRACT_DR_FIELD);
        Assert.True(!string.IsNullOrEmpty(actual));
    }

    [Fact]
    public void GetExportResultStatementSimpleTest()
    {
        var resultFile = "result.csv";
        var expected = $"COPY ("
            + $"SELECT DISTINCT {Consts.TABLE_GOSUSLUGA}.* "
            + $"FROM {Consts.TABLE_GOSUSLUGA} "
            + $"INNER JOIN {_resourceCL!.TableName} ON "
            + $"(TRIM(LOWER({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD})) = TRIM(LOWER({_resourceCL.TableName}.{Consts.CONTRACT_FIO_FIELD})) "
            + $"AND to_date({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_DR_FIELD}, 'YYYY-MM-DD') = to_date({_resourceCL.TableName}.{Consts.CONTRACT_DR_FIELD}, 'DD.MM.YYYY')) "
            + $"ORDER BY {Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD}"
            + $") TO '{resultFile}' DELIMITER ';' CSV HEADER";

        using var db = new DataBasePG();
        var dateDB = db.SelectDate(_resourceCL.TableName, Consts.CONTRACT_DR_FIELD);
        var dateFormatDB = DataBasePG.GetDateFormat(dateDB!);

        var actual = DataBasePG.GetExportResultStatement(_resourceCL.TableName, dateFormatDB, Consts.TABLE_GOSUSLUGA, "YYYY-MM-DD", "result.csv", ";");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetExportResultStatementAddFieldsTest()
    {
        var resultFile = "result.csv";
        var expected = $"COPY ("
            + $"SELECT DISTINCT {Consts.TABLE_GOSUSLUGA}.*, {_resourceCL!.TableName}.status, {_resourceCL.TableName}.data_zagruzki "
            + $"FROM {Consts.TABLE_GOSUSLUGA} "
            + $"INNER JOIN {_resourceCL.TableName} ON "
            + $"(TRIM(LOWER({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD})) = TRIM(LOWER({_resourceCL.TableName}.{Consts.CONTRACT_FIO_FIELD})) "
            + $"AND to_date({Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_DR_FIELD}, 'YYYY-MM-DD') = to_date({_resourceCL.TableName}.{Consts.CONTRACT_DR_FIELD}, 'DD.MM.YYYY')) "
            + $"ORDER BY {Consts.TABLE_GOSUSLUGA}.{Consts.CONTRACT_FIO_FIELD}"
            + $") TO '{resultFile}' DELIMITER ';' CSV HEADER";

        using var db = new DataBasePG();
        var dateDB = db.SelectDate(_resourceCL.TableName, Consts.CONTRACT_DR_FIELD);
        var dateFormatDB = DataBasePG.GetDateFormat(dateDB!);

        var actual = DataBasePG.GetExportResultStatement(_resourceCL.TableName, dateFormatDB, Consts.TABLE_GOSUSLUGA, "YYYY-MM-DD", "result.csv", ";", ["status", "data_zagruzki"]);

        Assert.Equal(expected, actual);
    }
>>>>>>> 023ee99bd8795049b54ff6405ab4888b81935d31
}