namespace ACTests;

public class Tests
{
    private static readonly Resource? _resourceCL = Consts.DATABASES.Where(r => r.Name == "Контрольный список").FirstOrDefault();
    
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
}