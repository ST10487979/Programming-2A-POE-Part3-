using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MfanaBotCyberApplicationP3.Services;

namespace MfanaBotCyberApplicationP3.Service
{
    public class ResponseServices
    {
        private Dictionary<string, List<string>> keywordResponses;
        private List<string> randomResponses;
        private Random random;
        private string lastResponse;
        private TaskServices taskService;
        private QuizServices quizService;
        private ActivityLogServices logService;

        public ResponseServices()
        {
            random = new Random();
            taskService = new TaskServices();
            quizService = new QuizServices();
            logService = new ActivityLogServices();
            LoadResponses();
            logService.LogAction("Bot started");
        }

        private void LoadResponses()
        {
            keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
                {
                    "For strong passwords bru: Use at least 12 characters with uppercase, lowercase, numbers, and symbols. Never reuse passwords!",
                    "A strong password is like a boomslang - unpredictable and dangerous to attackers! Use a mix of characters and a password manager like Bitwarden.",
                    "Here's a tip mfana: Use a passphrase like 'Purple Elephant Dancing!23' - easier to remember but hard to crack!"
                },
                ["phishing"] = new List<string>
                {
                    "Phishing is when scammers send fake emails pretending to be legit companies. In SA we see lots of fake SASSA and Capitec emails. Always check the sender!",
                    "Never click links in suspicious emails mfana! Hover over the link first to see where it really goes. If it looks dodgy, delete it.",
                    "Scammers create urgency - 'Your account will be closed!' Take a breath and verify by calling the company directly, not using numbers in the email."
                },
                ["privacy"] = new List<string>
                {
                    "Privacy is important mfana! Review your social media privacy settings. Don't share your location or birth date publicly.",
                    "Use different emails for different purposes. One for banking, one for social media, one for newsletters.",
                    "Be careful what you post online. Once something is on the internet, it's very hard to remove completely."
                },
                ["two factor"] = new List<string>
                {
                    "Two-Factor Authentication adds an extra lock to your accounts. Even if someone steals your password, they need your phone too!",
                    "Always turn on 2FA for email, banking, and social media. Use an authenticator app like Google Authenticator - it's safer than SMS."
                },
                ["malware"] = new List<string>
                {
                    "Malware is bad software that can steal your files. Install antivirus and don't download from sketchy websites mfana!",
                    "If your computer is running slow or showing popups, you might have malware. Run a scan with Windows Defender or Malwarebytes."
                },
                ["wifi"] = new List<string>
                {
                    "Public WiFi at malls and coffee shops is risky mfana! Avoid online banking on public networks. Use a VPN if you must connect.",
                    "Hackers can create fake WiFi networks called 'Evil Twins'. Always confirm the network name with staff before connecting."
                },
                ["update"] = new List<string>
                {
                    "Software updates are annoying but SO important mfana! They fix security holes. Turn on automatic updates.",
                    "Don't click 'Remind me later' forever. Hackers exploit unpatched software within days of updates being released."
                },
                ["encouragement"] = new List<string>
                {
                    "You're doing great mfana! Staying safe online is a journey, not a destination.",
                    "Lekker! You're already smarter than most people about cyber security.",
                    "Keep learning bru! The more you know, the safer you'll be.",
                    "Sharp sharp! You're becoming a cyber security expert!"
                }
            };

            randomResponses = new List<string>
            {
                "Yoh, I didn't quite understand that bru. Can you ask me about passwords, phishing, privacy, or safe browsing? I'm here to help mfana!",
                "Hmm, I'm not sure about that. Try asking me about passwords, phishing, privacy, or 2FA.",
                "I didn't catch that mfana. Ask me something about cyber security and I'll help you out!"
            };
        }

        public string GetResponse(string input, string sentiment = "neutral")
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Yoh, say something mfana!";

            input = input.ToLower().Trim();
            string response = "";

            // --- Check for Quiz commands ---
            if (input.Contains("quiz") || input.Contains("game") || input.Contains("test me"))
            {
                if (quizService.IsActive)
                    return quizService.GetCurrentQuestionText();
                logService.LogAction("User started quiz");
                return quizService.StartQuiz();
            }

            // --- Quiz answer handling ---
            if (quizService.IsActive)
            {
                string result = quizService.SubmitAnswer(input);
                if (!quizService.IsActive)
                    logService.LogAction("Quiz ended");
                return result;
            }

            // --- Check for Task commands ---
            if (input.Contains("add task") || input.Contains("create task") || input.Contains("new task"))
            {
                var (title, description, reminder) = taskService.ParseTaskCommand(input);
                if (string.IsNullOrWhiteSpace(title))
                    return "What task would you like to add mfana? Tell me something like 'Add task - Enable 2FA'";
                logService.LogAction($"Task added: {title}");
                return taskService.AddTask(title, description, reminder);
            }

            if (input.Contains("complete task") || input.Contains("mark done"))
            {
                var match = Regex.Match(input, @"\d+");
                if (match.Success)
                {
                    int id = int.Parse(match.Value);
                    logService.LogAction($"Task completed: ID {id}");
                    return taskService.CompleteTask(id);
                }
                return "Tell me which task to complete by ID. Say 'complete task 1'";
            }

            if (input.Contains("delete task") || input.Contains("remove task"))
            {
                var match = Regex.Match(input, @"\d+");
                if (match.Success)
                {
                    int id = int.Parse(match.Value);
                    logService.LogAction($"Task deleted: ID {id}");
                    return taskService.DeleteTask(id);
                }
                return "Tell me which task to delete by ID. Say 'delete task 1'";
            }

            if (input.Contains("tasks") || input.Contains("task list") || input.Contains("show tasks"))
            {
                logService.LogAction("User viewed tasks");
                return taskService.GetTaskSummary();
            }

            // --- Activity Log commands ---
            if (input.Contains("activity log") || input.Contains("what have you done") || input.Contains("show log"))
            {
                logService.LogAction("User viewed activity log");
                return logService.GetRecentLogs(10);
            }

            if (input.Contains("full log") || input.Contains("show all"))
            {
                logService.LogAction("User viewed full log");
                return logService.GetFullLog();
            }

            // --- Clear command ---
            if (input.Contains("clear log"))
            {
                logService.LogAction("User cleared log");
                return "Log cleared mfana!";
            }

            // --- More tips / another tip ---
            if (input.Contains("more") || input.Contains("another") || input.Contains("another tip") ||
                input.Contains("tell me more") || input.Contains("explain more"))
            {
                logService.LogAction("User asked for more tips");
                return GetRandomResponse("encouragement");
            }

            // --- Thank you responses ---
            if (input.Contains("thank") || input.Contains("thanks") || input.Contains("shot bru"))
            {
                logService.LogAction("User thanked bot");
                return "Lekker mfana! Anytime! Stay safe online! 🛡️";
            }

            // --- Greetings ---
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey") || input.Contains("yow"))
            {
                logService.LogAction("User greeted bot");
                return "Howzit mfana! Ask me about password safety, phishing scams, privacy, or anything cyber security related!";
            }

            // --- Exit commands ---
            if (input.Contains("bye") || input.Contains("goodbye") || input.Contains("exit") || input.Contains("quit"))
            {
                logService.LogAction("User exited");
                return "Hlokomela mfana! Stay safe online and remember what you learned today! Sharp sharp! 👋";
            }

            // --- Check for keywords ---
            foreach (var kvp in keywordResponses)
            {
                if (input.Contains(kvp.Key))
                {
                    logService.LogAction($"User asked about: {kvp.Key}");
                    return GetKeywordResponse(kvp.Key);
                }
            }

            // --- Empathetic responses based on sentiment ---
            if (sentiment == "worried" || sentiment == "scared" || sentiment == "anxious")
            {
                logService.LogAction("User expressed worry/anxiety");
                return GetEmpatheticResponse("worried");
            }

            // --- Default fallback ---
            logService.LogAction($"Unrecognized query: {input}");
            return randomResponses[random.Next(randomResponses.Count)];
        }

        private string GetKeywordResponse(string keyword)
        {
            if (keywordResponses.ContainsKey(keyword))
            {
                var responses = keywordResponses[keyword];
                return responses[random.Next(responses.Count)];
            }
            return randomResponses[random.Next(randomResponses.Count)];
        }

        private string GetRandomResponse(string category)
        {
            if (keywordResponses.ContainsKey(category))
            {
                var responses = keywordResponses[category];
                return responses[random.Next(responses.Count)];
            }
            return randomResponses[random.Next(randomResponses.Count)];
        }

        private string GetEmpatheticResponse(string sentiment)
        {
            var empathetic = new Dictionary<string, List<string>>
            {
                ["worried"] = new List<string>
                {
                    "I understand your concern mfana. Let me help you stay safe. " + GetRandomResponse("password"),
                    "It's completely understandable to feel worried about online security mfana. Let me help you. " + GetRandomResponse("password"),
                    "Yoh, I know cyber security can be frustrating bru. Let me break it down simply for you. " + GetRandomResponse("password")
                },
                ["curious"] = new List<string>
                {
                    "Great question mfana! I'm glad you're curious about staying safe online. " + GetRandomResponse("password"),
                    "Lekker mfana! I'm happy to help. " + GetRandomResponse("password")
                }
            };

            if (empathetic.ContainsKey(sentiment))
            {
                var responses = empathetic[sentiment];
                return responses[random.Next(responses.Count)];
            }
            return randomResponses[random.Next(randomResponses.Count)];
        }

        public bool IsExitCommand(string input)
        {
            input = input.ToLower().Trim();
            return input.Contains("bye") || input.Contains("goodbye") ||
                   input.Contains("exit") || input.Contains("quit");
        }
    }
}