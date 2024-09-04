using System.Text.RegularExpressions;

namespace ArrayCollation;

public partial class CsvHelper()
{
    public static Encoding GetEncoding(string filename)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var windows1251 = Encoding.GetEncoding("windows-1251");
        var utf_8 = Encoding.GetEncoding("utf-8");
        var bom = new byte[4];
        using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            file.Read(bom, 0, 4);
        }

        if (bom[0] == 0x46 && bom[1] == 0x49 && bom[2] == 0x4F) return windows1251;
        if (bom[0] == 0xD0 && bom[1] == 0xA4 && bom[2] == 0xD0) return utf_8;

        return Encoding.Default;
    }

    public static char DetectSeparatorPrivate(string text)
    {
        var coma = Coma().Matches(text);
        var semicolon = Semicolon().Matches(text);
        if(coma.Count > semicolon.Count)
            return ',';
        else return ';';
    }

    public static char DetectSeparator(string csvFilePath)
    {
        string[] lines = File.ReadAllLines(csvFilePath);
        string text = string.Join("\r\n", lines);
        return DetectSeparatorPrivate(text);
    }

    [GeneratedRegex(@";")]
    private static partial Regex Semicolon();
    [GeneratedRegex(@",")]
    private static partial Regex Coma();
}
