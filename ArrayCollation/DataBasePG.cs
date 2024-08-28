using Npgsql;
using System.Data;
using System.Text.RegularExpressions;

namespace ArrayCollation;

public class DataBasePG : IDisposable
{
    private NpgsqlConnection _connection;
    public DataBasePG()
    {
        _connection = new NpgsqlConnection(Consts.CONNECTION_STRING_PG);
        ConnectionOpen();
    }

    private void ConnectionOpen() 
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
            //Execute("set client_encoding = 'WIN1251'");
        }
    }

    public int Execute(string statement)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = statement;
        var resutl = cmd.ExecuteNonQuery();
        return resutl;
    }

    public int GetRowCount(string tableName)
    {
        int count = 0;
        try
        {
            var statement = $"SELECT count(*) FROM {tableName}";
            using var cmd = new NpgsqlCommand(statement, _connection);
            count = Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch {}
        return count;
    }

    public void GetRowCount(string tableName, IProgress<int> progress)
    {
        int count = 0;
        try
        {
            var statement = $"SELECT count(*) FROM {tableName}";
            using var cmd = new NpgsqlCommand(statement, _connection);
            count = Convert.ToInt32(cmd.ExecuteScalar());
        }
        catch { }
        progress.Report(count);
    }

    private void ConnectionClose() 
    {
        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close(); 
        } 
    }

    public void Dispose()
    {
        ConnectionClose();
        _connection.Dispose();
    }

    public int ImportData(string fileCsv, string tableName, string delimiter)
    {
        var statement = $@"COPY {tableName} FROM '{fileCsv}' WITH ENCODING 'WIN1251' DELIMITER '{delimiter}' CSV HEADER";
        return Execute(statement);
    }

    public void DropTable(string tableName)
    {
        var satement = $"DROP TABLE IF EXISTS {tableName}";
        Execute(satement);
    }

    public static string GetExportResultStatement(string dbTableName, string dbDateFormat, string gosuslugaTableName, 
        string gosuslugaDateFormat, string resultFile, string delimiter, params string[] dbTableColumns)
    {
        var selectPart = $"SELECT DISTINCT {gosuslugaTableName}.*";
        if (dbTableColumns != null)
        {
            for (int i = 0; i < dbTableColumns.Length; i++)
            {
                selectPart += $", {dbTableName}.{dbTableColumns[i]}";
            }
        }
        selectPart += $" ";
        var statement = $"COPY ("
            + selectPart
            + $"FROM {gosuslugaTableName} "
            + $"INNER JOIN {dbTableName} ON "
            + $"(TRIM(LOWER({gosuslugaTableName}.{Consts.CONTRACT_FIO_FIELD})) = TRIM(LOWER({dbTableName}.{Consts.CONTRACT_FIO_FIELD})) "
            + $"AND to_date({gosuslugaTableName}.{Consts.CONTRACT_DR_FIELD}, '{gosuslugaDateFormat}') = to_date({dbTableName}.{Consts.CONTRACT_DR_FIELD}, '{dbDateFormat}')) "
            + $"ORDER BY {gosuslugaTableName}.{Consts.CONTRACT_FIO_FIELD}"
            + $") TO '{resultFile}' DELIMITER '{delimiter}' CSV HEADER";
        return statement;
    }

    public static string GetDeadExportResultStatement(string gosuslugaTableName,
        string resultFile, string delimiter, params string[] dbTableColumns)
    {
        var selectPart = $"SELECT DISTINCT g.*";
        if (dbTableColumns != null)
        {
            for (int i = 0; i < dbTableColumns.Length; i++)
            {
                selectPart += $", d.{dbTableColumns[i]}";
            }
        }
        selectPart += $" ";
        var statement = $"COPY ("
            + selectPart
            + $"FROM {gosuslugaTableName} g "
            + $"INNER JOIN {Consts.TABLE_DEAD} d ON "
            + $"TRIM(LOWER(g.{Consts.CONTRACT_FIO_FIELD})) = TRIM(LOWER(d.{Consts.CONTRACT_FIO_FIELD})) "
            + $"AND split_part(g.{Consts.CONTRACT_DR_FIELD}, '-', 1)::int = split_part(d.{Consts.CONTRACT_DR_FIELD}, '.', 3)::int "
            + $"AND split_part(g.{Consts.CONTRACT_DR_FIELD}, '-', 2)::int = split_part(d.{Consts.CONTRACT_DR_FIELD}, '.', 2)::int "
            + $"AND split_part(g.{Consts.CONTRACT_DR_FIELD}, '-', 3)::int = split_part(d.{Consts.CONTRACT_DR_FIELD}, '.', 1)::int "
            + $"AND ("
            + $"split_part(g.{Consts.CONTRACT_CASE_DATE}, '-', 1)::int > split_part(d.Data_smerti, '.', 3)::int "
            + $"OR ("
            + $"split_part(g.{Consts.CONTRACT_CASE_DATE}, '-', 1)::int = split_part(d.Data_smerti, '.', 3)::int "
            + $"AND split_part(g.{Consts.CONTRACT_CASE_DATE}, '-', 2)::int > split_part(d.Data_smerti, '.', 2)::int"
            + $") "
            + $"OR ("
            + $"split_part(g.{Consts.CONTRACT_CASE_DATE}, '-', 1)::int = split_part(d.Data_smerti, '.', 3)::int "
            + $"AND split_part(g.{Consts.CONTRACT_CASE_DATE}, '-', 2)::int = split_part(d.Data_smerti, '.', 2)::int "
            + $"AND split_part(g.{Consts.CONTRACT_CASE_DATE}, '-', 3)::int > split_part(d.Data_smerti, '.', 1)::int "
            + $")) "
            + $"ORDER BY g.{Consts.CONTRACT_FIO_FIELD}"
            + $") TO '{resultFile}' DELIMITER '{delimiter}' CSV HEADER";
        return statement;
    }

    public static string GetDateFormat(string date)
    {
        if (!string.IsNullOrEmpty(date))
        {
            var dict = new Dictionary<string, string>
        {
            {"^\\d{4}-\\d{2}-\\d{2}$", "YYYY-MM-DD" },
            { "^\\d{2}\\.\\d{2}\\.\\d{4}$", "DD.MM.YYYY" }
        };
            foreach (var datePattern in dict)
            {
                var match = Regex.Match(date, datePattern.Key);
                if (match.Success)
                    return datePattern.Value;
            } 
        }
        return string.Empty;
    }

    public string? SelectDate(string table, string dateColumn)
    {
        var statement = $"SELECT {dateColumn} "
            + $"FROM {table} "
            + $"WHERE {dateColumn} IS NOT NULL "
            + $"LIMIT 1";
        using var cmd = new NpgsqlCommand(statement, _connection);

        var result = cmd.ExecuteScalar()?.ToString();
        return result;
    }

    public bool DoesTableExist(string tableName)
    {
        var statement = $"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{tableName}')";
        using var cmd = new NpgsqlCommand(statement, _connection);
        bool tableExists = Convert.ToBoolean(cmd.ExecuteScalar());
        return tableExists;
    }
}