using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MfanaBotCyberApplicationP3.Service;
using MfanaBotCyberApplicationP3.Services;

namespace MfanaBotCyberApplicationP3
{
    public partial class MainWindow : Window
    {
        private ResponseServices responseService;
        private MemoryServices memoryService;
        private SentimentServices sentimentService;
        private AudioServices audioService;
        private TaskServices taskService;
        private QuizServices quizService;
        private ActivityLogServices logService;
        private ObservableCollection<ChatMessage> messages;
        private DispatcherTimer typingTimer;
        private string currentTopic;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            LoadAsciiArt();
            SetupEventHandlers();
            PlayVoiceGreeting();
            AddWelcomeMessage();
            RefreshTaskList();
            RefreshLogDisplay();
        }

        private void InitializeServices()
        {
            responseService = new ResponseServices();
            memoryService = new MemoryServices();
            sentimentService = new SentimentServices();
            audioService = new AudioServices();
            taskService = new TaskServices();
            quizService = new QuizServices();
            logService = new ActivityLogServices();
            messages = new ObservableCollection<ChatMessage>();
        }

        private void LoadAsciiArt()
        {
            string art = @"
    ╔══════════════════════════════════════════╗
    ║         MFANA SECURITY BOT               ║
    ║    Keeping South Africa safe online      ║
    ╚══════════════════════════════════════════╝";
            AsciiArt.Text = art;
        }

        private void SetupEventHandlers()
        {
            SendButton.Click += SendButton_Click;
            ClearButton.Click += ClearButton_Click;
            MessageInput.KeyDown += MessageInput_KeyDown;
            AddTaskBtn.Click += AddTaskBtn_Click;
            StartQuizBtn.Click += StartQuizBtn_Click;
            SubmitAnswerBtn.Click += SubmitAnswerBtn_Click;
            QuizAnswerInput.KeyDown += QuizAnswerInput_KeyDown;
            RefreshLogBtn.Click += RefreshLogBtn_Click;
            FullLogBtn.Click += FullLogBtn_Click;

            ChatTabBtn.Click += (s, e) => SwitchView("Chat");
            TasksTabBtn.Click += (s, e) => SwitchView("Tasks");
            QuizTabBtn.Click += (s, e) => SwitchView("Quiz");
            LogTabBtn.Click += (s, e) => SwitchView("Log");

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (MessagesList != null && MessagesList.ItemsSource == null)
                MessagesList.ItemsSource = messages;
            MessageInput.Focus();
            UpdateMemoryDisplay();
        }

        private void SwitchView(string view)
        {
            ChatView.Visibility = view == "Chat" ? Visibility.Visible : Visibility.Collapsed;
            TasksView.Visibility = view == "Tasks" ? Visibility.Visible : Visibility.Collapsed;
            QuizView.Visibility = view == "Quiz" ? Visibility.Visible : Visibility.Collapsed;
            LogView.Visibility = view == "Log" ? Visibility.Visible : Visibility.Collapsed;

            if (view == "Tasks") RefreshTaskList();
            if (view == "Log") RefreshLogDisplay();

            // Update button styles
            ResetTabButtons();
            switch (view)
            {
                case "Chat": ChatTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")); break;
                case "Tasks": TasksTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")); break;
                case "Quiz": QuizTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")); break;
                case "Log": LogTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")); break;
            }
        }

        private void ResetTabButtons()
        {
            var buttons = new[] { ChatTabBtn, TasksTabBtn, QuizTabBtn, LogTabBtn };
            foreach (var btn in buttons)
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
        }

        private void PlayVoiceGreeting()
        {
            try { audioService.PlayGreeting(); } catch { }
        }

        private void AddWelcomeMessage()
        {
            string greeting = memoryService.GetPersonalizedGreeting();
            AddBotMessage(greeting);
        }

        // --- Chat Methods ---
        private void SendButton_Click(object sender, RoutedEventArgs e) => SendUserMessage();
        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                e.Handled = true;
                SendUserMessage();
            }
        }

        private void SendUserMessage()
        {
            string userInput = MessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(userInput)) return;

            AddUserMessage(userInput);
            MessageInput.Clear();

            if (userInput.ToLower().Contains("my name is"))
                ExtractAndRememberName(userInput);

            string sentiment = sentimentService.DetectSentiment(userInput);
            UpdateSentimentDisplay(sentiment);
            DetectTopicInterest(userInput);

            if (responseService.IsExitCommand(userInput))
            {
                AddBotMessage("Hlokomela mfana! Stay safe online! 👋");
                var closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                closeTimer.Tick += (s, e) => Close();
                closeTimer.Start();
                return;
            }

            string response = responseService.GetResponse(userInput, sentiment);
            if (string.IsNullOrWhiteSpace(response))
                response = "Yoh, I'm having a brain freeze. Can you repeat that? Try 'password' or 'phishing'.";

            if (memoryService.HasName() && !string.IsNullOrEmpty(memoryService.GetFavoriteTopic()))
            {
                if (userInput.ToLower().Contains("tell me") || userInput.ToLower().Contains("explain"))
                    response = $"As someone interested in {memoryService.GetFavoriteTopic()}, {response.ToLower()}";
            }

            currentTopic = DetectTopic(userInput);
            memoryService.StoreConversationContext(currentTopic, response);
            AddBotMessageWithTypingEffect(response);
            UpdateMemoryDisplay();
            ScrollToBottom();
        }

        private void ExtractAndRememberName(string input)
        {
            string lower = input.ToLower();
            int nameIndex = lower.IndexOf("my name is") + 10;
            if (nameIndex > 9 && nameIndex < input.Length)
            {
                string name = input.Substring(nameIndex).Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    memoryService.RememberName(name);
                    AddBotMessage($"Lekker to meet you {name} mfana! 🇿🇦");
                }
            }
        }

        private void DetectTopicInterest(string input)
        {
            string lower = input.ToLower();
            string[] topics = { "password", "phishing", "privacy", "scam", "wifi", "malware", "2fa" };
            foreach (string topic in topics)
                if (lower.Contains(topic)) { memoryService.RememberTopic(topic); break; }
        }

        private string DetectTopic(string input)
        {
            string lower = input.ToLower();
            if (lower.Contains("password")) return "password";
            if (lower.Contains("phish") || lower.Contains("scam")) return "phishing";
            if (lower.Contains("privacy")) return "privacy";
            if (lower.Contains("wifi")) return "wifi";
            if (lower.Contains("malware")) return "malware";
            return "general";
        }

        private void AddUserMessage(string message)
        {
            messages.Add(new ChatMessage { Text = message, IsUser = true });
            ScrollToBottom();
        }

        private void AddBotMessage(string message)
        {
            messages.Add(new ChatMessage { Text = message, IsUser = false });
            ScrollToBottom();
        }

        private void AddBotMessageWithTypingEffect(string fullMessage)
        {
            var chatMessage = new ChatMessage { Text = "", IsUser = false };
            messages.Add(chatMessage);
            int currentIndex = 0;
            typingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) };
            typingTimer.Tick += (s, e) =>
            {
                if (currentIndex < fullMessage.Length)
                {
                    chatMessage.Text += fullMessage[currentIndex];
                    currentIndex++;
                    ScrollToBottom();
                }
                else typingTimer.Stop();
            };
            typingTimer.Start();
        }

        private void ScrollToBottom() => MessagesScrollViewer.ScrollToBottom();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            messages.Clear();
            AddBotMessage("Conversation cleared mfana! How can I help you today?");
            UpdateMemoryDisplay();
        }

        // --- Sentiment/Memory Display ---
        private void UpdateSentimentDisplay(string sentiment)
        {
            string emoji = sentimentService.GetSentimentEmoji(sentiment);
            if (SentimentLabel != null)
            {
                SentimentLabel.Text = $"{emoji} {sentiment}";
                if (sentiment == "worried")
                    SentimentLabel.Foreground = new SolidColorBrush(Colors.OrangeRed);
                else if (sentiment == "happy")
                    SentimentLabel.Foreground = new SolidColorBrush(Colors.LightGreen);
                else
                    SentimentLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
            }
        }

        private void UpdateMemoryDisplay()
        {
            int count = memoryService.GetMemoryCount();
            if (MemoryServes != null)
            {
                MemoryServes.Text = $"{count} item{(count != 1 ? "s" : "")} remembered";
                MemoryServes.Foreground = count > 0 ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Gray);
                if (count > 0) MemoryServes.ToolTip = memoryService.GetMemorySummary();
            }
        }

        // --- Task Methods ---
        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            string title = TaskTitleInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title mfana!");
                return;
            }
            DateTime? reminder = TaskReminderPicker.SelectedDate;
            string response = taskService.AddTask(title, null, reminder);
            MessageBox.Show(response);
            TaskTitleInput.Clear();
            TaskReminderPicker.SelectedDate = null;
            RefreshTaskList();
            AddBotMessage(response);
        }

        private void RefreshTaskList()
        {
            var tasks = taskService.GetAllTasks();
            var panel = new StackPanel();
            foreach (var task in tasks)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A3A")),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 5, 0, 5)
                };
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var text = new TextBlock
                {
                    Text = $"{task.Title}" + (task.ReminderDate.HasValue ? $" 📅 {task.ReminderDate.Value.ToShortDateString()}" : "") +
                           (task.IsCompleted ? " ✅" : ""),
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(text, 0);

                var completeBtn = new Button
                {
                    Content = "✓",
                    Width = 35,
                    Height = 30,
                    Background = new SolidColorBrush(task.IsCompleted ? Colors.Gray : (Color)ColorConverter.ConvertFromString("#4ECDC4")),
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(5, 0, 5, 0),
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Tag = task.Id
                };
                completeBtn.Click += (s, e) =>
                {
                    var btn = (Button)s;
                    int id = (int)btn.Tag;
                    string response = taskService.CompleteTask(id);
                    MessageBox.Show(response);
                    RefreshTaskList();
                    AddBotMessage(response);
                };
                Grid.SetColumn(completeBtn, 1);

                var deleteBtn = new Button
                {
                    Content = "✕",
                    Width = 35,
                    Height = 30,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B")),
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Tag = task.Id
                };
                deleteBtn.Click += (s, e) =>
                {
                    var btn = (Button)s;
                    int id = (int)btn.Tag;
                    string response = taskService.DeleteTask(id);
                    MessageBox.Show(response);
                    RefreshTaskList();
                    AddBotMessage(response);
                };
                Grid.SetColumn(deleteBtn, 2);

                grid.Children.Add(text);
                grid.Children.Add(completeBtn);
                grid.Children.Add(deleteBtn);
                border.Child = grid;
                panel.Children.Add(border);
            }
            TasksList.ItemsSource = panel.Children;
        }

        // --- Quiz Methods ---
        private void StartQuizBtn_Click(object sender, RoutedEventArgs e)
        {
            string question = quizService.StartQuiz();
            QuizQuestionLabel.Text = question;
            QuizScoreLabel.Text = $"Score: 0";
            QuizAnswerInput.Focus();
        }

        private void SubmitAnswerBtn_Click(object sender, RoutedEventArgs e)
        {
            string input = QuizAnswerInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            string result = quizService.SubmitAnswer(input);
            QuizQuestionLabel.Text = result;
            QuizAnswerInput.Clear();

            if (!quizService.IsActive)
            {
                QuizScoreLabel.Text = "Quiz Complete!";
                QuizAnswerInput.IsEnabled = false;
                StartQuizBtn.IsEnabled = true;
            }
        }

        private void QuizAnswerInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SubmitAnswerBtn_Click(sender, e);
        }

        // --- Log Methods ---
        private void RefreshLogBtn_Click(object sender, RoutedEventArgs e) => RefreshLogDisplay();
        private void RefreshLogDisplay() => LogDisplay.Text = logService.GetRecentLogs(10);

        private void FullLogBtn_Click(object sender, RoutedEventArgs e)
        {
            LogDisplay.Text = logService.GetFullLog();
            FullLogBtn.Content = "📋 Show Recent";
            FullLogBtn.Click -= FullLogBtn_Click;
            FullLogBtn.Click += (sender2, e2) =>
            {
                RefreshLogDisplay();
                FullLogBtn.Content = "📋 Show All";
            };
        }
    }
}