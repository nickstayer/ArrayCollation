using System;
using System.Reflection;

namespace IcDataLoader
{
    public class FrData : IcData
    {
        public string FrDepartment { get; set; }
        public string FrDay { get; set; }
        public string FrMonth { get; set; }
        public string FrYear { get; set; }

        //public static int GetFieldsCount()
        //{
        //    Type myClassType = typeof(FrData);
        //    FieldInfo[] fields = myClassType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //    return fields.Length;
        //}
    }
}