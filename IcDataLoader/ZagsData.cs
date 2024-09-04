using System;
using System.Reflection;

namespace IcDataLoader
{
    public class ZagsData : IcData
    {
        public string DeadDay { get; set; }
        public string DeadMonth { get; set; }
        public string DeadYear { get; set; }
        public string CertSeries { get; set; }
        public string CertNumber { get; set; }

        //public static int GetFieldsCount()
        //{
        //    Type myClassType = typeof(ZagsData);
        //    FieldInfo[] fields = myClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //    return fields.Length;
        //}
    }
}