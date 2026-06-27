using MfanaBotCyberApplicationP3.DataBase;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MfanaBotCyberApplicationP3.Services
{
    public class TaskService
    {
        public List<Task> GetAllTasks() => DatabaseHelper.GetAllTasks();
        public List<Task> GetActiveTasks() => DatabaseHelper.GetActiveTasks();

        public string AddTask(string title, string description = null, DateTime? reminderDate = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "Please provide a task title mfana!";

            var task = new Task
            {
                Title = title.Trim(),
                Description = description?.Trim(),
                ReminderDate = reminderDate,
                IsCompleted = false
            };

            DatabaseHelper.AddTask(task);
            DatabaseHelper.AddActivityLog($"Task added: '{title}'" +
                (reminderDate.HasValue ? $" (Reminder set for {reminderDate.Value.ToShortDateString()})" : ""));

            string response = $"✅ Task '{title}' added successfully.";
            if (reminderDate.HasValue)
                response += $" I'll remind you on {reminderDate.Value.ToShortDateString()}.";
            return response;
        }

        public string CompleteTask(int taskId)
        {
            var task = DatabaseHelper.GetTaskById(taskId);
            if (task == null)
                return "❌ Task not found.";

            if (task.IsCompleted)
                return $"Task '{task.Title}' is already completed.";

            task.IsCompleted = true;
            DatabaseHelper.UpdateTask(task);
            DatabaseHelper.AddActivityLog($"Task completed: '{task.Title}'");
            return $"Task '{task.Title}' marked as completed! Lekker mfana! 🎉";
        }

        public string DeleteTask(int taskId)
        {
            var task = DatabaseHelper.GetTaskById(taskId);
            if (task == null)
                return "❌ Task not found.";

            DatabaseHelper.DeleteTask(taskId);
            DatabaseHelper.AddActivityLog($"Task deleted: '{task.Title}'");
            return $"Task '{task.Title}' has been deleted.";
        }

        public string GetTaskSummary()
        {
            var tasks = DatabaseHelper.GetAllTasks();
            var active = tasks.FindAll(t => !t.IsCompleted);
            var completed = tasks.FindAll(t => t.IsCompleted);

            if (tasks.Count == 0)
                return "No tasks yet. Add one by saying 'Add task - Enable 2FA'";

            string summary = $"Task Summary: {active.Count} active, {completed.Count} completed.\n\n";

            if (active.Count > 0)
            {
                summary += "📌 Active tasks:\n";
                foreach (var t in active)
                {
                    summary += $"   • {t.Title}";
                    if (t.ReminderDate.HasValue)
                        summary += $" {t.ReminderDate.Value.ToShortDateString()}";
                    summary += "\n";
                }
            }

            if (completed.Count > 0)
            {
                summary += "\nCompleted tasks:\n";
                foreach (var t in completed)
                {
                    summary += $"   • {t.Title}\n";
                }
            }

            return summary;
        }

        public (string title, string description, DateTime? reminder) ParseTaskCommand(string input)
        {
            string title = "";
            string description = "";
            DateTime? reminder = null;

            // Extract reminder date (e.g., "remind me in 3 days")
            var dateMatch = Regex.Match(input, @"remind(?:er)?\s*(?:me)?\s*(?:in)?\s*(\d+)\s*(day|days|week|weeks|month|months)");
            if (dateMatch.Success)
            {
                int amount = int.Parse(dateMatch.Groups[1].Value);
                string unit = dateMatch.Groups[2].Value;
                if (unit.StartsWith("day")) reminder = DateTime.Now.AddDays(amount);
                else if (unit.StartsWith("week")) reminder = DateTime.Now.AddDays(amount * 7);
                else if (unit.StartsWith("month")) reminder = DateTime.Now.AddMonths(amount);
            }

            // Extract reminder date (e.g., "remind me on 2024-12-25")
            var dateMatch2 = Regex.Match(input, @"remind(?:er)?\s*(?:me)?\s*(?:on)?\s*(\d{4}-\d{2}-\d{2})");
            if (dateMatch2.Success)
            {
                if (DateTime.TryParse(dateMatch2.Groups[1].Value, out DateTime parsedDate))
                {
                    reminder = parsedDate;
                }
            }

            // Clean the input to get the task title
            string cleaned = Regex.Replace(input, @"(add|create|new)\s*(?:a\s*)?(task|reminder)", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"remind\s*(?:me)?\s*(?:in\s*\d+\s*(day|days|week|weeks|month|months))?", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"remind\s*(?:me)?\s*(?:on\s*\d{4}-\d{2}-\d{2})?", "", RegexOptions.IgnoreCase);
            title = cleaned.Trim();

            // If title contains a dash, split into title and description
            if (title.Contains(" - "))
            {
                string[] parts = title.Split(new[] { " - " }, StringSplitOptions.None);
                title = parts[0].Trim();
                description = parts.Length > 1 ? parts[1].Trim() : null;
            }

            return (title, description, reminder);
        }
    }
}//Code was guided by artificial inteligance deepseek 