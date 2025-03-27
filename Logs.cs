using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Data.SqlClient;
namespace Restaurant
{
    public class Logs
    {
        public void Log(int app,string s)
        {
            string IP = LocalIPAdress();
            using(SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationSettings.AppSettings["Log"]))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $"insert into Logs ({app}, '{s}', '{IP}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')";


                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) { ex.ToString(); }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }

                }

            }
        }

        private string LocalIPAdress()
        {
            List<string> lstIPAddress = new List<string>();
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipa in IpEntry.AddressList)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    lstIPAddress.Add(ipa.ToString());
            }
            return lstIPAddress[0]; // result: 192.168.1.17 ......

        }
    }
}
