using System.Reflection;
using System;

namespace IcDataLoader
{
    public abstract class IcData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string OtherName { get; set; }
        public string Bday { get; set; }
        public string Bmonth { get; set; }
        public string Byear { get; set; }
        public string Bplace { get; set; }
    }
}
