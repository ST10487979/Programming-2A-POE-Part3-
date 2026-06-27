using System;
using System.Collections.Generic;
using System.Text;

namespace MfanaBotCyberApplicationP3.Services
{
    public class MemoryServices
    {
        private Dictionary<string, string> conversationMemory;
        private string _name;
        private string _favoriteTopic;
        private string _lastTopic;
        private string _lastResponse;
        private int _memoryCount;

        public MemoryServices()
        {
            conversationMemory = new Dictionary<string, string>();
            _name = string.Empty;
            _favoriteTopic = string.Empty;
            _lastTopic = string.Empty;
            _lastResponse = string.Empty;
            _memoryCount = 0;
        }

        /// <summary>
        /// Remembers the user's name
        /// </summary>
        public void RememberName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _name = name.Trim();
                LogActivity($"Name remembered: {_name}");
            }
        }

        /// <summary>
        /// Gets the user's name
        /// </summary>
        public string GetName() => _name;

        /// <summary>
        /// Checks if the user has provided a name
        /// </summary>
        public bool HasName() => !string.IsNullOrWhiteSpace(_name);

        /// <summary>
        /// Remembers a topic the user is interested in
        /// </summary>
        public void RememberTopic(string topic)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                _favoriteTopic = topic.ToLower().Trim();
                _lastTopic = _favoriteTopic;
                LogActivity($"Topic remembered: {_favoriteTopic}");
            }
        }

        
        public string GetFavoriteTopic() => _favoriteTopic;

     
        public bool HasFavoriteTopic() => !string.IsNullOrWhiteSpace(_favoriteTopic);

        public void StoreConversationContext(string topic, string response)
        {
            if (!string.IsNullOrWhiteSpace(topic))
            {
                _lastTopic = topic;
            }
            if (!string.IsNullOrWhiteSpace(response))
            {
                _lastResponse = response;
            }

            string key = $"conversation_{DateTime.Now.Ticks}";
            conversationMemory[key] = $"{topic}|{response}";
            _memoryCount = conversationMemory.Count;
        }

        /// <summary>
        /// Gets the last topic discussed
        /// </summary>
        public string GetLastTopic() => _lastTopic;

        /// <summary>
        /// Gets the last response given
        /// </summary>
        public string GetLastResponse() => _lastResponse;

        /// <summary>
        /// Gets the number of stored memories
        /// </summary>
        public int GetMemoryCount() => _memoryCount;

        /// <summary>
        /// Gets a personalized greeting based on stored memory
        /// </summary>
        public string GetPersonalizedGreeting()
        {
            if (HasName() && HasFavoriteTopic())
            {
                return $"Yoh {_name}! I remember you're interested in {_favoriteTopic}. Want to learn more about that?";
            }
            else if (HasName())
            {
                return $"Welcome back {_name}! How can I help you stay safe online today?";
            }
            else
            {
                return "Yoh mfana! Whats your name? (Just tell me 'my name is [your name]')";
            }
        }

        /// <summary>
        /// Gets a summary of all stored memories
        /// </summary>
        public string GetMemorySummary()
        {
            if (conversationMemory.Count == 0 && string.IsNullOrEmpty(_name))
                return "No memories yet";

            var sb = new StringBuilder();
            sb.AppendLine("🧠 Memory Summary:");
            sb.AppendLine("━━━━━━━━━━━━━━━━");

            if (!string.IsNullOrEmpty(_name))
                sb.AppendLine($"👤 Name: {_name}");

            if (!string.IsNullOrEmpty(_favoriteTopic))
                sb.AppendLine($"📚 Interested in: {_favoriteTopic}");

            if (!string.IsNullOrEmpty(_lastTopic))
                sb.AppendLine($"💬 Last topic: {_lastTopic}");

            sb.AppendLine($"📝 {_memoryCount} items stored in memory");

            if (_memoryCount > 0)
            {
                sb.AppendLine("\nRecent memories:");
                int count = 0;
                // Get last 5 memories (most recent first)
                var keys = new List<string>(conversationMemory.Keys);
                keys.Reverse();

                foreach (string key in keys)
                {
                    if (count >= 5) break;
                    string value = conversationMemory[key];
                    string[] parts = value.Split('|');
                    string topic = parts.Length > 0 ? parts[0] : "unknown";
                    string response = parts.Length > 1 ? parts[1] : "";

                    sb.AppendLine($"   • Topic: {topic}");
                    if (!string.IsNullOrEmpty(response) && response.Length > 50)
                        sb.AppendLine($"     Response: {response.Substring(0, 50)}...");
                    else if (!string.IsNullOrEmpty(response))
                        sb.AppendLine($"     Response: {response}");
                    count++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clears all memory
        /// </summary>
        public void ClearMemory()
        {
            conversationMemory.Clear();
            _name = string.Empty;
            _favoriteTopic = string.Empty;
            _lastTopic = string.Empty;
            _lastResponse = string.Empty;
            _memoryCount = 0;
            LogActivity("Memory cleared");
        }

        /// <summary>
        /// Gets all memories as a dictionary (for display)
        /// </summary>
        public Dictionary<string, string> GetAllMemories()
        {
            return new Dictionary<string, string>(conversationMemory);
        }

        /// <summary>
        /// Finds memories related to a specific topic
        /// </summary>
        public List<string> FindMemoriesByTopic(string topic)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(topic))
                return results;

            string searchTerm = topic.ToLower().Trim();
            foreach (var kvp in conversationMemory)
            {
                if (kvp.Value.ToLower().Contains(searchTerm))
                {
                    results.Add($"{kvp.Key}: {kvp.Value}");
                }
            }
            return results;
        }

        /// <summary>
        /// Checks if the user has a specific memory
        /// </summary>
        public bool HasMemory(string key)
        {
            return conversationMemory.ContainsKey(key);
        }

        /// <summary>
        /// Gets a specific memory by key
        /// </summary>
        public string GetMemory(string key)
        {
            return conversationMemory.TryGetValue(key, out string value) ? value : null;
        }

        /// <summary>
        /// Saves memory to a file (optional - for persistence between sessions)
        /// </summary>
        public void SaveToFile(string filePath = "memory.json")
        {
            try
            {
                var memoryData = new
                {
                    Name = _name,
                    FavoriteTopic = _favoriteTopic,
                    LastTopic = _lastTopic,
                    LastResponse = _lastResponse,
                    Conversations = conversationMemory
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(memoryData, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(filePath, json);
                LogActivity($"Memory saved to {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving memory: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads memory from a file (optional - for persistence between sessions)
        /// </summary>
        public void LoadFromFile(string filePath = "memory.json")
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return;

                string json = System.IO.File.ReadAllText(filePath);
                var memoryData = Newtonsoft.Json.JsonConvert.DeserializeObject<MemorySaveData>(json);

                if (memoryData != null)
                {
                    _name = memoryData.Name ?? string.Empty;
                    _favoriteTopic = memoryData.FavoriteTopic ?? string.Empty;
                    _lastTopic = memoryData.LastTopic ?? string.Empty;
                    _lastResponse = memoryData.LastResponse ?? string.Empty;
                    conversationMemory = memoryData.Conversations ?? new Dictionary<string, string>();
                    _memoryCount = conversationMemory.Count;
                    LogActivity($"Memory loaded from {filePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading memory: {ex.Message}");
            }
        }

        // Helper class for JSON serialization
        private class MemorySaveData
        {
            public string Name { get; set; }
            public string FavoriteTopic { get; set; }
            public string LastTopic { get; set; }
            public string LastResponse { get; set; }
            public Dictionary<string, string> Conversations { get; set; }
        }

        // Helper method for logging
        private void LogActivity(string description)
        {
            try
            {
                // Optional: Also log to Database if available
                // DatabaseHelper.AddActivityLog(description);
                System.Diagnostics.Debug.WriteLine($"[Memory] {DateTime.Now}: {description}");
            }
            catch { }
        }

        // --- NEW NLP-ENHANCED METHODS ---

        /// <summary>
        /// Extracts key information from user input (NLP simulation)
        /// </summary>
        public MemoryExtraction ExtractMemoryFromInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new MemoryExtraction { Success = false };

            string lowerInput = input.ToLower();
            var extraction = new MemoryExtraction { Success = true };

            // Extract name: "my name is X" or "I am X" or "call me X"
            string[] namePatterns = { "my name is ", "i am ", "call me ", "name's ", "my name's " };
            foreach (string pattern in namePatterns)
            {
                int index = lowerInput.IndexOf(pattern);
                if (index >= 0)
                {
                    string name = input.Substring(index + pattern.Length).Trim();
                    // Clean up any trailing punctuation or extra words
                    name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z\s]", "");
                    if (!string.IsNullOrWhiteSpace(name) && name.Split(' ').Length <= 3)
                    {
                        extraction.Name = name.Trim();
                        break;
                    }
                }
            }

            // Extract topic: "interested in X" or "tell me about X" or "about X"
            string[] topicPatterns = {
                "interested in ", "tell me about ", "explain ",
                "about ", "teach me ", "learn about ", "info on "
            };
            foreach (string pattern in topicPatterns)
            {
                int index = lowerInput.IndexOf(pattern);
                if (index >= 0)
                {
                    string topic = input.Substring(index + pattern.Length).Trim();
                    topic = System.Text.RegularExpressions.Regex.Replace(topic, @"[?,.!]", "").Trim();
                    if (!string.IsNullOrWhiteSpace(topic) && topic.Split(' ').Length <= 5)
                    {
                        extraction.Topic = topic;
                        break;
                    }
                }
            }

            // If no topic found but input contains known keywords
            if (string.IsNullOrEmpty(extraction.Topic))
            {
                string[] topics = { "password", "phishing", "privacy", "scam", "wifi", "malware", "2fa", "security", "safe" };
                foreach (string topic in topics)
                {
                    if (lowerInput.Contains(topic))
                    {
                        extraction.Topic = topic;
                        break;
                    }
                }
            }

            // Extract sentiment keywords
            string[] sentimentPhrases = {
                "i feel ", "feeling ", "i'm ", "i am ",
                "makes me ", "getting ", "so "
            };
            foreach (string phrase in sentimentPhrases)
            {
                int index = lowerInput.IndexOf(phrase);
                if (index >= 0)
                {
                    string feeling = input.Substring(index + phrase.Length).Trim();
                    extraction.Sentiment = feeling.Split(' ')[0];
                    break;
                }
            }

            return extraction;
        }

        /// <summary>
        /// Stores extraction results in memory
        /// </summary>
        public void StoreExtraction(MemoryExtraction extraction)
        {
            if (extraction == null || !extraction.Success) return;

            if (!string.IsNullOrEmpty(extraction.Name))
            {
                RememberName(extraction.Name);
            }

            if (!string.IsNullOrEmpty(extraction.Topic))
            {
                RememberTopic(extraction.Topic);
            }

            if (!string.IsNullOrEmpty(extraction.Sentiment))
            {
                // Store sentiment as a memory
                string key = $"sentiment_{DateTime.Now.Ticks}";
                conversationMemory[key] = $"sentiment|{extraction.Sentiment}";
                _memoryCount = conversationMemory.Count;
            }
        }
    }

    /// <summary>
    /// Represents extracted memory data from user input
    /// </summary>
    public class MemoryExtraction
    {
        public bool Success { get; set; }
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Sentiment { get; set; }
        public string Action { get; set; } // e.g., "add task", "quiz", "help"
        public string Entity { get; set; } // e.g., "password", "phishing"
    }
}