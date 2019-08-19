using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Odbc;
using System.Diagnostics;
using System.Threading;

namespace DBMonitor
{
	class Program
	{
		static void Main(string[] args)
		{
            String fileName = ConfigurationManager.AppSettings["LogFile"];
            bool exists = true;
            int x = 0;
            while(exists)
            {
                String section = "Queries/Query." + x;
                var applicationSettings = ConfigurationManager.GetSection(section) as NameValueCollection;
                bool empty = true;

                if (applicationSettings == null)
                {
                    exists = false;
                }
                else
                {
                    int count = 0;
                    String path = applicationSettings["MetricTree"] + "|" + applicationSettings["MetricName"] + ":";
                    try
                    {
                        String query = applicationSettings["Query"];
                        LogMessage("Query: " + query, fileName);
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        OdbcConnection con = new OdbcConnection(applicationSettings["ConnectionString"]);
                        con.Open();
                        OdbcCommand com = new OdbcCommand(query, con);
                        OdbcDataReader reader = com.ExecuteReader();
                        ArrayList rows = new ArrayList();
                        while (reader.Read())
                        {
                            stopwatch.Stop();
                            if (reader.FieldCount > 1)
                            {
                                String result = "";
                                for (int j = 0; j < reader.FieldCount; j++)
                                {
                                    if ((j + 1) == reader.FieldCount)
                                    {
                                        result += reader.GetString(j).Trim();
                                    }
                                    else
                                    {
                                        result += reader.GetString(j) + "|";
                                    }
                                }
                                rows.Add(result);
                            }
                            else
                            {
                                rows.Add(reader.GetString(0).Trim());
                            }
                        }
                        count = rows.Count;

                        String fmt = "";
                        long previousRowCount;
                        long.TryParse(applicationSettings["PreviousRowCount"], out previousRowCount);
                        if (count > 999 || previousRowCount > 999)
                        {
                            fmt = "0000";
                        }
                        else if (count > 99 || previousRowCount > 99)
                        {
                            fmt = "000";
                        }
                        else if (count > 9 || previousRowCount > 9)
                        {
                            fmt = "00";
                        }

                        if (count > 1)
                        {
                            int y = 0;
                            foreach (String row in rows)
                            {
                                y++;
                                long val;
                                if (long.TryParse(row, out val))
                                {
                                    WilyMetricReporter.ReportLong(path + "Result-Row-" + y.ToString(fmt), val);
                                }
                                else
                                {
                                    WilyMetricReporter.ReportString(path + "Result-Row-" + y.ToString(fmt), row);
                                }
                            }
                        }
                        else
                        {
                            long val;
                            if (long.TryParse(rows[0].ToString(), out val))
                            {
                                WilyMetricReporter.ReportLong(path + "Result", val);
                            }
                            else
                            {
                                WilyMetricReporter.ReportString(path + "Result", rows[0].ToString());

                            }
                        }
                        PrintBlanks(section, path, count);
                        WilyMetricReporter.ReportLong(path + "Time", stopwatch.ElapsedMilliseconds);
                        WilyMetricReporter.ReportLong(path + "Success", 1);
                        WriteRowCount(section, count);
                    }
                    catch (Exception ex)
                    {
                        WilyMetricReporter.ReportLong(path + "Success", 0);
                        LogMessage(ex.GetType().FullName, fileName);
                        LogMessage("Message : " + ex.Message, fileName);
                        LogMessage("StackTrace : " + ex.StackTrace, fileName);
                        WriteRowCount(section, 0);
                        PrintBlanks(section, path, count);
                    }
                }
            x++;
            }
        }

        private static void LogMessage(String message, String fileName)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@fileName, true))
            {
                file.WriteLine(message);
            }
        }

        private static void WriteRowCount(String section, int value)
        {
            String key = "PreviousRowCount";
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = ((AppSettingsSection)configFile.GetSection(section)).Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value + "");
                }
                else
                {
                    settings[key].Value = value + "";
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        private static void PrintBlanks(String section, String path, int count)
        {
            //We need to account for old string values that might be reporting but not up to date. We will report them as blank values
            String fmt = "";
            long previousRowCount;
            var applicationSettings = ConfigurationManager.GetSection(section) as NameValueCollection;
            int diff = 0;
            if (long.TryParse(applicationSettings["PreviousRowCount"], out previousRowCount))
            {
                diff = (int)long.Parse(applicationSettings["PreviousRowCount"]) - count;
            }
            if (count > 999 || previousRowCount > 999)
            {
                fmt = "0000";
            }
            else if (count > 99 || previousRowCount > 99)
            {
                fmt = "000";
            }
            else if (count > 9 || previousRowCount > 9)
            {
                fmt = "00";
            }
            if (diff > 0)
            {
                for (int j = (int)(previousRowCount - diff + 1); j <= previousRowCount; j++)
                {

                    WilyMetricReporter.ReportString(path + "Result-Row-" + j.ToString(fmt), "");
                }
            }
        }
    }
}