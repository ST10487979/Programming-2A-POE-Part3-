using System;
using System.Collections.Generic;

namespace MfanaBotCyberApplicationP3.Services
{
    public class SentimentServices
    {
        private Dictionary<string, List<string>> sentimentKeywords;
        private Dictionary<string, string> sentimentEmojis;
        private string lastSentiment = "neutral";

        public SentimentServices()
        {
            LoadSentimentKeywords();
            LoadSentimentEmojis();
        }

        private void LoadSentimentKeywords()
        {
            sentimentKeywords = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["worried"] = new List<string>
                {
                    "worried", "scared", "anxious", "afraid", "concerned",
                    "nervous", "stress", "stressed", "panic", "fear",
                    "terrified", "frightened", "uneasy", "uncomfortable",
                    "threatened", "vulnerable", "unsafe", "danger"
                },
                ["happy"] = new List<string>
                {
                    "happy", "great", "awesome", "good", "excellent",
                    "wonderful", "fantastic", "amazing", "brilliant",
                    "lekker", "sharp", "enjoying", "love", "nice",
                    "cool", "sweet", "perfect", "excited", "glad",
                    "pleased", "satisfied", "thankful", "grateful"
                },
                ["curious"] = new List<string>
                {
                    "curious", "interesting", "interested", "wondering",
                    "learn", "want to know", "tell me", "explain",
                    "what is", "how does", "why", "when", "where",
                    "who", "which", "teach me", "show me", "guide me"
                },
                ["frustrated"] = new List<string>
                {
                    "frustrated", "annoyed", "angry", "irritated",
                    "upset", "disappointed", "tired of", "sick of",
                    "hate", "useless", "waste", "pointless", "difficult",
                    "confusing", "complicated", "hard", "tough", "struggle"
                },
                ["confused"] = new List<string>
                {
                    "confused", "don't understand", "unclear", "huh",
                    "what do you mean", "lost", "puzzled", "baffled",
                    "perplexed", "mystified", "unsure", "uncertain",
                    "not sure", "maybe", "perhaps", "might be"
                },
                ["thankful"] = new List<string>
                {
                    "thank", "thanks", "appreciate", "grateful",
                    "thankful", "blessed", "lucky", "fortunate",
                    "shot bru", "lekker", "cheers", "much appreciated"
                }
            };
        }//Code was guided by artificial inteligance deepseek 

        private void LoadSentimentEmojis()
        {
            sentimentEmojis = new Dictionary<string, string>
            {
                ["worried"] = "😟",
                ["happy"] = "😊",
                ["curious"] = "🤔",
                ["frustrated"] = "😤",
                ["confused"] = "😕",
                ["thankful"] = "🙏",
                ["neutral"] = "😐"
            };
        }

        
        /// Detects the sentiment of the user's input
        
        public string DetectSentiment(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "neutral";

            string lowerInput = input.ToLower();

            // Check each sentiment category
            foreach (var category in sentimentKeywords)
            {
                foreach (string keyword in category.Value)
                {
                    if (lowerInput.Contains(keyword))
                    {
                        lastSentiment = category.Key;
                        return category.Key;
                    }
                }
            }

            // If no sentiment detected, return neutral
            lastSentiment = "neutral";
            return "neutral";
        }

        
        /// Gets an emoji for the given sentiment
        
        public string GetSentimentEmoji(string sentiment)
        {
            if (string.IsNullOrWhiteSpace(sentiment))
                return sentimentEmojis["neutral"];

            sentiment = sentiment.ToLower();
            return sentimentEmojis.ContainsKey(sentiment)
                ? sentimentEmojis[sentiment]
                : sentimentEmojis["neutral"];
        }

        
        /// Gets the last detected sentiment
       
        public string GetLastSentiment() => lastSentiment;

        
        /// Gets an empathetic response based on sentiment
      
        public string GetEmpatheticResponse(string sentiment)
        {
            var empatheticResponses = new Dictionary<string, List<string>>
            {
                ["worried"] = new List<string>
                {
                    "I understand your concern mfana. Let me help you stay safe online.",
                    "It's natural to feel worried about online security. I'm here to guide you.",
                    "Don't stress bru! Let me break this down in a simple way for you."
                },
                ["happy"] = new List<string>
                {
                    "Lekker mfana! I'm glad you're in good spirits! 😊",
                    "Love your positive energy! Let's learn some cyber security tips!",
                    "Sharp sharp! Your good mood makes learning more fun!"
                },
                ["curious"] = new List<string>
                {
                    "Great question mfana! I love your curiosity about cyber security!",
                    "A curious mind is a safe mind! Let me explain that for you.",
                    "That's a lekker question! Here's what you need to know..."
                },
                ["frustrated"] = new List<string>
                {
                    "Yoh, I know cyber security can be frustrating. Let me make it simpler for you.",
                    "Take a breath bru. I'll explain this in the easiest way possible.",
                    "I hear you mfana. Let's tackle this together - one step at a time."
                },
                ["confused"] = new List<string>
                {
                    "No stress bru! Let me clarify that for you.",
                    "I understand it can be confusing. Let me explain it differently.",
                    "Let me break that down into simpler terms for you mfana."
                },
                ["thankful"] = new List<string>
                {
                    "Lekker mfana! Always happy to help you stay safe online! 🛡️",
                    "My pleasure bru! Stay safe out there!",
                    "You're welcome! That's what I'm here for! 🇿🇦"
                },
                ["neutral"] = new List<string>
                {
                    "How can I help you today mfana?",
                    "Ask me about passwords, phishing, or online safety!",
                    "I'm here to keep you safe online. What do you want to know?"
                }
            };

            if (empatheticResponses.ContainsKey(sentiment))
            {
                var responses = empatheticResponses[sentiment];
                Random random = new Random();
                return responses[random.Next(responses.Count)];
            }

            return "How can I help you today mfana?";
        }

        
        public SentimentAnalysis AnalyzeSentiment(string input)
        {
            string sentiment = DetectSentiment(input);
            string emoji = GetSentimentEmoji(sentiment);
            string empatheticResponse = GetEmpatheticResponse(sentiment);

            return new SentimentAnalysis
            {
                Sentiment = sentiment,
                Emoji = emoji,
                EmpatheticResponse = empatheticResponse,
                IsPositive = sentiment == "happy" || sentiment == "thankful",
                IsNegative = sentiment == "worried" || sentiment == "frustrated",
                IsNeutral = sentiment == "neutral"
            };
        }
    }

    
    /// Represents a sentiment analysis result
    
    public class SentimentAnalysis
    {
        public string Sentiment { get; set; }
        public string Emoji { get; set; }
        public string EmpatheticResponse { get; set; }
        public bool IsPositive { get; set; }
        public bool IsNegative { get; set; }
        public bool IsNeutral { get; set; }

        public override string ToString()
        {
            return $"{Emoji} {Sentiment}";
        }
    }
}//Code was guided by artificial inteligance deepseek 