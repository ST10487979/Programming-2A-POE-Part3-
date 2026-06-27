using System;
using System.Collections.Generic;
using System.Linq;
using MfanaBotCyberApplicationP3.DataBase;

namespace MfanaBotCyberApplicationP3.Service
{
    public class QuizServices
    {
        private List<QuizQuestion> questions;
        private int currentIndex = 0;
        private int score = 0;
        private bool isActive = false;

        public QuizServices()
        {
            InitializeQuestions();
        }

        private void InitializeQuestions()
        {
            questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it" },
                    CorrectIndex = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects your information."
                },
                new QuizQuestion
                {
                    Question = "Which of these is a strong password?",
                    Options = new List<string> { "123456", "password", "P@ssw0rd!2024", "qwerty" },
                    CorrectIndex = 2,
                    Explanation = "A strong password uses a mix of uppercase, lowercase, numbers, and special characters."
                },
                new QuizQuestion
                {
                    Question = "True or False: It's safe to use public WiFi for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "Public WiFi is unsafe for banking. Hackers can intercept your data on unsecured networks."
                },
                new QuizQuestion
                {
                    Question = "What is 2FA (Two-Factor Authentication)?",
                    Options = new List<string> { "A type of virus", "An extra layer of security for your accounts", "A new phone model", "A social media platform" },
                    CorrectIndex = 1,
                    Explanation = "2FA adds a second verification step (like a code on your phone) to keep your accounts safer."
                },
                new QuizQuestion
                {
                    Question = "What is a common sign of a phishing email?",
                    Options = new List<string> { "Proper spelling and grammar", "A sense of urgency", "A friendly tone", "No attachments" },
                    CorrectIndex = 1,
                    Explanation = "Phishing emails often create urgency to trick you into acting without thinking."
                },
                new QuizQuestion
                {
                    Question = "True or False: Using the same password for all accounts is safe.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "Using the same password everywhere is dangerous. If one account is hacked, all are vulnerable."
                },
                new QuizQuestion
                {
                    Question = "What should you do if you think your computer has malware?",
                    Options = new List<string> { "Ignore it", "Run an antivirus scan", "Download more software", "Turn it off" },
                    CorrectIndex = 1,
                    Explanation = "Running an antivirus scan can detect and remove malware from your system."
                },
                new QuizQuestion
                {
                    Question = "Which of these is a safe browsing practice?",
                    Options = new List<string> { "Clicking suspicious ads", "Using HTTPS websites", "Downloading from unknown sources", "Entering passwords on popups" },
                    CorrectIndex = 1,
                    Explanation = "HTTPS websites encrypt your data. Always look for the padlock icon in the address bar."
                },
                new QuizQuestion
                {
                    Question = "True or False: You should regularly update your software.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 0,
                    Explanation = "Updates fix security holes. Hackers exploit outdated software to infect your device."
                },
                new QuizQuestion
                {
                    Question = "What is social engineering?",
                    Options = new List<string> { "A type of engineering job", "Manipulating people to reveal information", "Building social networks", "Writing code" },
                    CorrectIndex = 1,
                    Explanation = "Social engineering tricks people into sharing sensitive information through manipulation."
                },
                new QuizQuestion
                {
                    Question = "If you get a call saying your bank account is compromised, what should you do?",
                    Options = new List<string> { "Give them your details to help", "Call your bank using the number on your card", "Send a WhatsApp message", "Ignore it" },
                    CorrectIndex = 1,
                    Explanation = "Always verify by calling your bank directly using the official number on your card."
                },
                new QuizQuestion
                {
                    Question = "True or False: It's safe to open email attachments from unknown senders.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "Unknown attachments may contain malware. Only open attachments from trusted sources."
                }
            };
        }

        public bool IsActive => isActive;

        public string StartQuiz()
        {
            isActive = true;
            currentIndex = 0;
            score = 0;
            DatabaseHelper.AddActivityLog("Quiz started");
            return GetCurrentQuestionText();
        }

        public string GetCurrentQuestionText()
        {
            if (currentIndex >= questions.Count)
                return EndQuiz();

            var q = questions[currentIndex];
            string text = $"📝 Q{currentIndex + 1}: {q.Question}\n\n";
            for (int i = 0; i < q.Options.Count; i++)
            {
                text += $"   {(char)('A' + i)}) {q.Options[i]}\n";
            }
            return text;
        }

        public string SubmitAnswer(string input)
        {
            if (currentIndex >= questions.Count)
                return EndQuiz();

            int selectedIndex = -1;
            input = input.Trim().ToUpper();

            // Try to parse answer (A, B, C, D or number)
            if (input.Length == 1 && input[0] >= 'A' && input[0] <= 'D')
                selectedIndex = input[0] - 'A';
            else if (int.TryParse(input, out int num) && num >= 1 && num <= questions[currentIndex].Options.Count)
                selectedIndex = num - 1;

            if (selectedIndex == -1 || selectedIndex >= questions[currentIndex].Options.Count)
                return "Please enter A, B, C, or D for your answer.";

            var q = questions[currentIndex];
            bool isCorrect = selectedIndex == q.CorrectIndex;
            if (isCorrect) score++;

            string result = isCorrect ? "✅ Correct!" : $"❌ Incorrect. The answer was {q.Options[q.CorrectIndex]}.";
            result += $"\n📖 {q.Explanation}";

            currentIndex++;
            DatabaseHelper.AddActivityLog($"Quiz: Q{currentIndex} - {(isCorrect ? "Correct" : "Incorrect")}");

            if (currentIndex >= questions.Count)
                return result + "\n\n" + EndQuiz();

            return result + "\n\n" + GetCurrentQuestionText();
        }

        public string EndQuiz()
        {
            isActive = false;
            int total = questions.Count;
            double percentage = (double)score / total * 100;

            string feedback;
            if (percentage >= 90) feedback = "🌟 Excellent mfana! You're a cybersecurity pro! 🎉";
            else if (percentage >= 70) feedback = "👏 Great job! You know your cybersecurity well!";
            else if (percentage >= 50) feedback = "💪 Not bad! Keep learning to stay safe online!";
            else feedback = "📚 Keep learning mfana! Cybersecurity is important for everyone!";

            string summary = $"\n📊 Quiz Complete!\nScore: {score}/{total} ({percentage:F0}%)\n{feedback}";
            DatabaseHelper.AddActivityLog($"Quiz completed: {score}/{total}");
            return summary;
        }
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }
    }
}//Code was guided by artificial inteligance deepseek 