using System.Collections.Generic;
using System.Text;
using System.IO;

namespace IcDataLoader
{
    public class Exporter
    {
        public void ExportToCSV(string file, List<IcData> data, IEnumerable<string> headers, string delimiter, Encoding encoding)
        {
            WriteHeaders(file, headers, delimiter, encoding);
            WriteData(file, data, delimiter, encoding);
        }

        private void WriteHeaders(string file, IEnumerable<string> headers, string delimiter, Encoding encoding)
        {
            var headersLine = new StringBuilder();
            headersLine.AppendLine(GetHeadersLine(headers, delimiter));
            var result = headersLine.ToString().Replace("\"", "\"\"");
            File.AppendAllText(file, result, encoding);
        }

        private void WriteData(string file, List<IcData> data, string delimiter, Encoding encoding)
        {
            var lines = new StringBuilder();
            foreach (var row in data)
            {
                if (row is FrData)
                    lines.AppendLine(GetFrDataLine(row, delimiter));
                if (row is ZagsData)
                    lines.AppendLine(GetZagsDataLine(row, delimiter));
            }
            var result = lines.ToString().Replace("\"", "\"\"");
            File.AppendAllText(file, result, encoding);
        }

        private string GetHeadersLine(IEnumerable<string> headers, string delimiter)
        {
            var line = new StringBuilder();
            foreach (var field in headers)
            {
                line.Append($"{field.Replace(delimiter, ",")}{delimiter}");
            }
            var result = line.ToString().Substring(0, line.ToString().Length - 1);
            return result;
        }

        private string GetFrDataLine(IcData data, string delimiter)
        {
            var frData = (FrData)data;
            var line = new StringBuilder();
            line.Append($"{GetFIO(frData).Replace(delimiter, ",")}{delimiter}");
            line.Append($"{GetDate(data.Bday, data.Bmonth, data.Byear).Replace(delimiter, ",")}{delimiter}");
            line.Append($"{data.Bplace.Replace(delimiter, ",")}{delimiter}");
            line.Append($"{frData.FrDepartment.Replace(delimiter, ",")}{delimiter}");
            line.Append($"{GetDate(frData.FrDay, frData.FrMonth, frData.FrYear).Replace(delimiter, ",")}");
            return line.ToString();
        }

        private string GetZagsDataLine(IcData data, string delimiter)
        {
            var zagsData = (ZagsData)data;
            var line = new StringBuilder();
            line.Append($"{GetFIO(zagsData).Replace(delimiter, ",")}{delimiter}");
            line.Append($"{GetDate(data.Bday, data.Bmonth, data.Byear).Replace(delimiter, ",")}{delimiter}");
            line.Append($"{data.Bplace.Replace(delimiter, ",")}{delimiter}");
            line.Append($"{GetDate(zagsData.DeadDay, zagsData.DeadMonth, zagsData.DeadYear).Replace(delimiter, ",")}{delimiter}");
            line.Append($"{zagsData.CertSeries.Replace(delimiter, ",")}{delimiter}");
            line.Append($"{zagsData.CertNumber.Replace(delimiter, ",")}");
            return line.ToString();
        }

        private string GetFIO(IcData data)
        {
            var lastname = string.IsNullOrEmpty(data.LastName) || data.LastName.ToLower() == "нет" ? "" : data.LastName;
            var firstname = string.IsNullOrEmpty(data.FirstName) || data.FirstName.ToLower() == "нет" ? "" : data.FirstName;
            var othername = string.IsNullOrEmpty(data.OtherName) || data.OtherName.ToLower() == "нет" ? "" : data.OtherName;
            var result = $"{lastname} {firstname} {othername}".Trim().Replace("  "," ");
            return result;
        }

        public string GetDate(string day, string month, string year)
        {
            day = ParseDateComponent(string.IsNullOrEmpty(day) || day == "0" ? "00" : day, "00");
            month = ParseDateComponent(string.IsNullOrEmpty(month) || month == "0" ? "00" : month, "00");
            year = ParseDateComponent(string.IsNullOrEmpty(year) || year == "0" ? "0000" : year, "0000");
            var result = $"{day}.{month}.{year}".Trim();
            return result;
        }

        private string ParseDateComponent(string dateComponent, string format)
        {
            var resultInt = int.Parse(dateComponent);
            var result = resultInt.ToString(format);
            return result;
        }
    }
}