using System;
using System.Net.Sockets;

/// 
/// <summary>
/// @author Brad Watson
/// </summary>
namespace DBMonitor
{

    public class WilyMetricReporter
    {

        public WilyMetricReporter(string metricName, string val)
        {
            Console.WriteLine("<metric type=\"LongCounter\" name=\"" + metricName + "\" value=\"" + val + "\"/>");
        }

        public WilyMetricReporter(string metricName, long val)
        {
            Console.WriteLine("<metric type=\"LongCounter\" name=\"" + metricName + "\" value=\"" + val + "\"/>");
        }

        public static void ReportLong(string metricName, int val)
        {
            Console.WriteLine("<metric type=\"LongCounter\" name=\"" + metricName + "\" value=\"" + val + "\"/>");
        }

        public static void ReportLong(string metricName, long val)
        {
            Console.WriteLine("<metric type=\"LongCounter\" name=\"" + metricName + "\" value=\"" + val + "\"/>");
        }

        public static void ReportString(string metricName, String val)
        {
            Console.WriteLine("<metric type=\"StringEvent\" name=\"" + metricName + "\" value=\"" + val + "\"/>");
        }

    }

}