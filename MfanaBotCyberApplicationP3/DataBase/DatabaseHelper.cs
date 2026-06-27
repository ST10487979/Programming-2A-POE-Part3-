using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace MfanaBotCyberApplicationP3.DataBase
{
    // Task model
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Activity Log model
    public class ActivityLogEntry
    {
        public int Id { get; set; }
        public string ActionDescription { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DatabaseHelper
    {

        private static string connectionString =
        "Server=localhost;Port=3306;Database=cybersecurity_bot;Uid=mfana;Pwd=harambe2468;SslMode=none;";


        public static string ConnectionString { get => connectionString; set => connectionString = value; }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("✅ Database connected successfully!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Database connection failed: {ex.Message}");
                return false;
            }
        }

        public static bool TestAllConnections()
        {
            string[] testStrings = {
                "Server=localhost;Database=cybersecurity_bot;Uid=root;Pwd=yourpassword;",
                "Server=127.0.0.1;Database=cybersecurity_bot;Uid=root;Pwd=yourpassword;",
                "Server=localhost;Port=3306;Database=cybersecurity_bot;Uid=root;Pwd=yourpassword;",
                "Server=localhost;Database=cybersecurity_bot;Uid=root;Pwd=yourpassword;SslMode=None;",
                "Server=127.0.0.1;Database=cybersecurity_bot;Uid=root;Pwd=;",
            };

            foreach (string connStr in testStrings)
            {
                try
                {
                    using (var conn = new MySqlConnection(connStr))
                    {
                        conn.Open();
                        System.Diagnostics.Debug.WriteLine($"✅ Connected with: {connStr}");
                        ConnectionString = connStr; // Save working connection
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Failed: {ex.Message}");
                }
            }
            return false;
        }

        public static string GetConnectionString()
        {
            return ConnectionString;
        }

        //Code was guided by artificial inteligance deepseek 

        public static void AddTask(Task task)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = @"INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted) 
                                 VALUES (@title, @desc, @reminder, @completed)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", task.Title);
                    cmd.Parameters.AddWithValue("@desc", task.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@reminder", task.ReminderDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@completed", task.IsCompleted);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Task> GetAllTasks()
        {
            var tasks = new List<Task>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Tasks ORDER BY Id DESC";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Task
                        {
                            Id = reader.GetInt32("Id"),
                            Title = reader.GetString("Title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? (DateTime?)null : reader.GetDateTime("ReminderDate"),
                            IsCompleted = reader.GetBoolean("IsCompleted"),
                            CreatedAt = reader.GetDateTime("CreatedAt")
                        });
                    }
                }
            }
            return tasks;
        }

        public static List<Task> GetActiveTasks()
        {
            var tasks = new List<Task>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Tasks WHERE IsCompleted = FALSE ORDER BY Id DESC";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Task
                        {
                            Id = reader.GetInt32("Id"),
                            Title = reader.GetString("Title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? (DateTime?)null : reader.GetDateTime("ReminderDate"),
                            IsCompleted = reader.GetBoolean("IsCompleted"),
                            CreatedAt = reader.GetDateTime("CreatedAt")
                        });
                    }
                }
            }
            return tasks;
        }

        public static void UpdateTask(Task task)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "UPDATE Tasks SET IsCompleted = @completed WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@completed", task.IsCompleted);
                    cmd.Parameters.AddWithValue("@id", task.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteTask(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "DELETE FROM Tasks WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static Task GetTaskById(int id)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Tasks WHERE Id = @id";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Task
                            {
                                Id = reader.GetInt32("Id"),
                                Title = reader.GetString("Title"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? (DateTime?)null : reader.GetDateTime("ReminderDate"),
                                IsCompleted = reader.GetBoolean("IsCompleted"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            };
                        }
                    }
                }
            }
            return null;
        }

        // --- ACTIVITY LOG METHODS ---

        public static void AddActivityLog(string description)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "INSERT INTO ActivityLogs (ActionDescription) VALUES (@desc)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<ActivityLogEntry> GetRecentActivityLogs(int count = 10)
        {
            var logs = new List<ActivityLogEntry>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM ActivityLogs ORDER BY Id DESC LIMIT @count";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@count", count);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new ActivityLogEntry
                            {
                                Id = reader.GetInt32("Id"),
                                ActionDescription = reader.GetString("ActionDescription"),
                                Timestamp = reader.GetDateTime("Timestamp")
                            });
                        }
                    }
                }
            }
            return logs;
        }

        public static List<ActivityLogEntry> GetAllActivityLogs()
        {
            var logs = new List<ActivityLogEntry>();
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM ActivityLogs ORDER BY Id DESC";
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new ActivityLogEntry
                        {
                            Id = reader.GetInt32("Id"),
                            ActionDescription = reader.GetString("ActionDescription"),
                            Timestamp = reader.GetDateTime("Timestamp")
                        });
                    }
                }
            }
            return logs;
        }
    }
}//Whole Code was guided by artificial inteligance deepseek 