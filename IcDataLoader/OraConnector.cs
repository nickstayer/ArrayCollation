using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace IcDataLoader
{
    internal class OraConnector
    {
        private OracleConnection Conn { get; set; }

        public  void Connect()
        {
            var connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.196.192.26)(PORT=1521))(CONNECT_DATA=(SERVER = DEDICATED)(SERVICE_NAME = Upmo)));User Id = uvm_coder; Password=le,jdcrjd;";
            Conn = new OracleConnection(connectionString);
            try 
            {
                Conn.Open();
                OracleCommand cmd = Conn.CreateCommand();
                cmd.CommandText = "ALTER SESSION set NLS_DATE_FORMAT = \'DD.MM.YYYY\'";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public  void Disconnect()
        {
            Conn.Close();
            Conn.Dispose();
        }

        public List<IcData> GetFrData()
        {
            var statement =
                $"SELECT "
                + $"FAM,IMJ,OTCH,D_ROJD,M_ROJD,Y_ROJD,MES_ROJD,KODREG1,D_UTV,M_UTV,Y_UTV "
                + $"FROM case1.T019 "
                + $"WHERE IBD_ARX = 1 AND DOSTUP = '1'";

            var result = new List<IcData>();
            try
            {
                OracleCommand cmd = new OracleCommand(statement, Conn);
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                var columnsCount = dr.VisibleFieldCount;
                    var fr = new FrData();
                    for (int i = 0; i < columnsCount; i++)
                    {
                        fr.LastName = dr.GetValue(0).ToString();
                        fr.FirstName = dr.GetValue(1).ToString();
                        fr.OtherName = dr.GetValue(2).ToString();
                        fr.Bday = dr.GetValue(3).ToString();
                        fr.Bmonth = dr.GetValue(4).ToString();
                        fr.Byear = dr.GetValue(5).ToString();
                        fr.Bplace = dr.GetValue(6).ToString();
                        fr.FrDepartment = dr.GetValue(7).ToString();
                        fr.FrDay = dr.GetValue(8).ToString();
                        fr.FrMonth = dr.GetValue(9).ToString();
                        fr.FrYear = dr.GetValue(10).ToString();
                    }
                    result.Add(fr);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return result;
        }

        public List<IcData> GetZagsData(string startDate, string endDate)
        {
            var startDateArr = startDate.Split('.');
            var endDateArr = endDate.Split('.');
            var statement =
                $"SELECT "
                + $"FAM,IMJ,OTCH,D_ROJD,M_ROJD,Y_ROJD,MEST_ROJ,D_SMER,M_SMER,Y_SMER,SERIY_SVID_SM,NUM_SVID_SM "
                + $"FROM IGNATUK.ZAGS_UMER_FBD z "
                + $"WHERE lpad(to_char(z.d_smer),2,'0') >= '{startDateArr[0]}' "
                + $"AND lpad(to_char(z.m_smer),2,'0') >= '{startDateArr[1]}' "
                + $"AND lpad(to_char(z.y_smer),4,'0') >= '{startDateArr[2]}' "
                + $"AND lpad(to_char(z.d_smer),2,'0') <= '{endDateArr[0]}' "
                + $"AND lpad(to_char(z.m_smer),2,'0') <= '{endDateArr[1]}' "
                + $"AND lpad(to_char(z.y_smer),4,'0') <= '{endDateArr[2]}'";

            var result = new List<IcData>();
            try
            {
                OracleCommand cmd = new OracleCommand(statement, Conn);
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var columnsCount = dr.VisibleFieldCount;
                    var zags = new ZagsData();
                    for (int i = 0; i < columnsCount; i++)
                    {
                        zags.LastName = dr.GetValue(0).ToString();
                        zags.FirstName = dr.GetValue(1).ToString();
                        zags.OtherName = dr.GetValue(2).ToString();
                        zags.Bday = dr.GetValue(3).ToString();
                        zags.Bmonth = dr.GetValue(4).ToString();
                        zags.Byear = dr.GetValue(5).ToString();
                        zags.Bplace = dr.GetValue(6).ToString();
                        zags.DeadDay = dr.GetValue(7).ToString();
                        zags.DeadMonth = dr.GetValue(8).ToString();
                        zags.DeadYear = dr.GetValue(9).ToString();
                        zags.CertSeries = dr.GetValue(10).ToString();
                        zags.CertNumber = dr.GetValue(11).ToString();
                    }
                    result.Add(zags);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return result;
        }

        public List<object[]> GetRows(string statement)
        {
            var result = new List<object[]>();
            try
            {

                OracleCommand cmd = new OracleCommand(statement, Conn);
                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    try
                    {
                        var columnsCount = dr.VisibleFieldCount;
                        var row = new object[columnsCount];

                        for (int i = 0; i < columnsCount; i++)
                        {
                            row[i] = dr.GetValue(i);
                        }
                        result.Add(row);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return result;
        }
    }
}