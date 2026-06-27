using MfanaBotCyberApplicationP3.DataBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace MfanaBotCyberApplicationP3.Services
{
    public class ActivityLogServices
    {
        private List<string> inMemoryLogs = new List<string>();

        public void LogAction(string description)
        {
            string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {description}";
            inMemoryLogs.Add(entry);

            try
            {
                DatabaseHelper.AddActivityLog(description);
            }
            catch
            {
                
            }
        }//Code was guided by artificial inteligance deepseek 

        public string GetRecentLogs(int count = 10)
        {
            try
            {
                var dbLogs = DatabaseHelper.GetRecentActivityLogs(count);
                if (dbLogs.Count == 0)
                    return "No activity logged yet.";

                var sb = new StringBuilder();
                sb.AppendLine("Recent Activity Log:");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                foreach (var log in dbLogs)
                {
                    sb.AppendLine($"   {log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.ActionDescription}");
                }
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return sb.ToString();
            }
            catch
            {
                // Fallback to in-memory
                if (inMemoryLogs.Count == 0)
                    return "No activity logged yet.";

                var sb = new StringBuilder();
                sb.AppendLine("Recent Activity Log:");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                int start = Math.Max(0, inMemoryLogs.Count - count);
                for (int i = inMemoryLogs.Count - 1; i >= start; i--)
                {
                    sb.AppendLine($"   {inMemoryLogs[i]}");
                }
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return sb.ToString();
            }
        }

        public string GetFullLog()
        {
            try
            {
                var dbLogs = DatabaseHelper.GetAllActivityLogs();
                if (dbLogs.Count == 0)
                    return "No activity logged yet.";

                var sb = new StringBuilder();
                sb.AppendLine(" Full Activity Log:");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                foreach (var log in dbLogs)
                {
                    sb.AppendLine($"   {log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.ActionDescription}");
                }
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return sb.ToString();
            }
            catch
            {
                if (inMemoryLogs.Count == 0)
                    return "No activity logged yet.";

                var sb = new StringBuilder();
                sb.AppendLine("Full Activity Log:");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                foreach (var log in inMemoryLogs)
                {
                    sb.AppendLine($"   {log}");
                }
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                return sb.ToString();
            }
        }//Code was guided by artificial inteligance deepseek 
    }
}