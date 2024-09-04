namespace ArrayCollation;

public class Consts
{
    public const string CONNECTION_STRING_PG = "Host=localhost;Username=postgres;Password=578329;Database=postgres;CommandTimeout=1000";

    public const string TABLE_GOSUSLUGA = "gosusluga";
    public const string TABLE_DEAD = "dead";

    public static List<Resource> DATABASES = new List<Resource>()
    {
        new Resource
        {
            Name = "Контрольный список",
            TableName = "controllist",
            TableColumns = ["mesto_rozhdeniya", "status", "data_zagruzki"]
        },
        new Resource
        {
            Name = "Федеральный розыск",
            TableName = "fr",
            TableColumns = ["mesto_rozhdeniya", "kem_obiavlen_v_rozysk", "data_postanovki_v_rozysk"]
        },
        new Resource
        {
            Name = "Список умерших",
            TableName = "dead",
            TableColumns = ["mesto_rozhdeniya", "data_smerti", "seriya_svid_o_smerti", "nomer_svid_o_smerti"]
        },
    };

    public const char VALID_SEPARATOR = '_';
    public static char[] REPLACEMENT_CHARACTERS = [' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '`', '{', '|', '}', '~'];

    public const string CONTRACT_FIO_FIELD = "FIO";
    public const string CONTRACT_DR_FIELD = "Data_rozhdeniya";
    public const string CONTRACT_CASE_DATE = "Data_sozdaniya_dela";
}
