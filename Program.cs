using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace EnhancedCybersecurityAdvisor
{
    // Main Program class
    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of the chatbot and run it
            CyberSecurityAdvisor advisor = new CyberSecurityAdvisor();
            advisor.Start();
        }
    }

    // Class to store user information - I learned about object-oriented design in class!
    class UserProfile
    {
        public string Name { get; set; }
        public DateTime JoinedAt { get; set; }
        public int QuestionsAsked { get; set; }
        public List<string> TopicsViewed { get; set; }

        // Constructor to initialize a new user profile
        public UserProfile(string name)
        {
            Name = name;
            JoinedAt = DateTime.Now; // Store when the session started
            QuestionsAsked = 0; // Start counting questions
            TopicsViewed = new List<string>(); // Create empty list for topics
        }

        // Method to increase question count
        public void IncrementQuestions()
        {
            QuestionsAsked++;
        }

        // Method to add a new topic to the viewed list (without duplicates)
        public void AddTopicViewed(string topic)
        {
            // Only add if we haven't seen this topic before
            if (!TopicsViewed.Contains(topic))
            {
                TopicsViewed.Add(topic);
            }
        }

        // Create a summary of the session
        public string GetSessionSummary()
        {
            // Calculate how long they used the program
            TimeSpan sessionDuration = DateTime.Now - JoinedAt;
            
            // Build a nice summary with stats
            return $"Session Summary for {Name}:\n" +
                   $"- Session duration: {sessionDuration.Minutes} minutes, {sessionDuration.Seconds} seconds\n" +
                   $"- Topics explored: {TopicsViewed.Count}\n" +
                   $"- Questions asked: {QuestionsAsked}";
        }
    }

    // Audio Manager class to handle sound effects - this was challenging to implement!
    class AudioManager
    {
        private readonly Dictionary<string, string> _audioFiles;

        public AudioManager()
        {
            // Store the audio file names in a dictionary for easy access
            _audioFiles = new Dictionary<string, string>
            {
                { "welcome", "Welcome.wav" },
                { "introduction", "introduction.wav" },
            };
        }

        public bool PlayAudio(string audioKey)
        {
            try
            {
                // Make sure the audio key exists
                if (!_audioFiles.ContainsKey(audioKey))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Unknown audio key: {audioKey}]");
                    Console.ResetColor();
                    return false;
                }

                string audioPath = _audioFiles[audioKey];

                // Check if file exists before trying to play it
                if (File.Exists(audioPath))
                {
                    // Used NAudio library from NuGet - nice discovery!
                    using (var audioFile = new AudioFileReader(audioPath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        // Wait until playback is complete
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    return true;
                }
                else
                {
                    // File not found warning
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Audio file '{audioPath}' not found]");
                    Console.ResetColor();
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Something went wrong with playback
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error playing audio: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
    }

    // Knowledge Base class - stores all the cybersecurity info I researched
    class KnowledgeBase
    {
        private readonly Dictionary<string, CyberTopic> _topics;

        public KnowledgeBase()
        {
            // Use a case-insensitive comparison so "Password" and "password" match
            _topics = new Dictionary<string, CyberTopic>(StringComparer.OrdinalIgnoreCase);
            InitializeTopics(); // Load all the topics
        }

        private void InitializeTopics()
        {
            // Password topic - these are important concepts from my security class!
            var passwordTopic = new CyberTopic("Password Safety");
            passwordTopic.AddKeyword("password");
            passwordTopic.SetMainResponse("For strong passwords: use at least 12 characters, mix uppercase, lowercase, numbers, and symbols. Never reuse passwords across accounts, and consider using a password manager.");
            passwordTopic.AddTip("Consider using a passphrase instead of a single word. For example, 'Purple-Horse-Battery-Staple-42!' is much stronger than 'P@ssw0rd'.");
            passwordTopic.AddTip("Enable two-factor authentication (2FA) whenever possible for an additional layer of security.");
            _topics.Add("password", passwordTopic);

            // Phishing topic - learned about this from recent news articles
            var phishingTopic = new CyberTopic("Phishing Prevention");
            phishingTopic.AddKeyword("phishing");
            phishingTopic.AddKeyword("scam");
            phishingTopic.AddKeyword("email");
            phishingTopic.SetMainResponse("Phishing attempts trick you into revealing sensitive information. Always verify the sender's email address, don't click suspicious links, and never provide personal information unless you're certain of the recipient's identity.");
            phishingTopic.AddTip("Hover over links before clicking to see the actual URL destination.");
            phishingTopic.AddTip("Be wary of urgent requests or threats that create pressure to act quickly.");
            _topics.Add("phishing", phishingTopic);

            // Safe browsing topic
            var browsingTopic = new CyberTopic("Safe Browsing");
            browsingTopic.AddKeyword("browsing");
            browsingTopic.AddKeyword("browser");
            browsingTopic.AddKeyword("internet");
            browsingTopic.AddKeyword("web");
            browsingTopic.SetMainResponse("For safe browsing: keep your browser updated, use HTTPS websites, be careful when downloading files, don't use public Wi-Fi for sensitive transactions, and consider using a VPN for additional protection.");
            browsingTopic.AddTip("Clear your cookies and browsing history regularly to protect your privacy.");
            browsingTopic.AddTip("Use browser extensions like ad blockers and privacy tools to enhance security.");
            _topics.Add("browsing", browsingTopic);

            // Malware topic - super important in today's world!
            var malwareTopic = new CyberTopic("Malware Protection");
            malwareTopic.AddKeyword("malware");
            malwareTopic.AddKeyword("virus");
            malwareTopic.AddKeyword("trojan");
            malwareTopic.AddKeyword("ransomware");
            malwareTopic.SetMainResponse("To protect against malware: keep your software updated, use reputable antivirus software, don't download from untrusted sources, and be cautious of email attachments.");
            malwareTopic.AddTip("Regularly scan your system with anti-malware software.");
            malwareTopic.AddTip("Be especially cautious of files with extensions like .exe, .bat, or .scr.");
            _topics.Add("malware", malwareTopic);

            // Data breach topic - this happened to my friend recently
            var dataBreachTopic = new CyberTopic("Data Breach Response");
            dataBreachTopic.AddKeyword("data breach");
            dataBreachTopic.AddKeyword("breach");
            dataBreachTopic.AddKeyword("hack");
            dataBreachTopic.AddKeyword("leaked");
            dataBreachTopic.SetMainResponse("If you're affected by a data breach: change your passwords immediately, monitor your accounts for suspicious activity, and consider freezing your credit if personal information was compromised.");
            dataBreachTopic.AddTip("Use services like Have I Been Pwned to check if your data has been compromised.");
            dataBreachTopic.AddTip("Consider using a credit monitoring service after a major breach.");
            _topics.Add("data breach", dataBreachTopic);

            // VPN topic - I use this myself when traveling
            var vpnTopic = new CyberTopic("VPN Usage");
            vpnTopic.AddKeyword("vpn");
            vpnTopic.AddKeyword("virtual private network");
            vpnTopic.SetMainResponse("A VPN (Virtual Private Network) encrypts your internet connection, helping protect your privacy and security, especially on public Wi-Fi networks. It can also mask your IP address and location.");
            vpnTopic.AddTip("Free VPNs often collect and sell your data - consider paying for a reputable service.");
            vpnTopic.AddTip("Even with a VPN, practice good security habits as they don't make you completely anonymous.");
            _topics.Add("vpn", vpnTopic);

            // Social media topic - added this for my final project
            var socialMediaTopic = new CyberTopic("Social Media Security");
            socialMediaTopic.AddKeyword("social media");
            socialMediaTopic.AddKeyword("facebook");
            socialMediaTopic.AddKeyword("twitter");
            socialMediaTopic.AddKeyword("instagram");
            socialMediaTopic.AddKeyword("tiktok");
            socialMediaTopic.SetMainResponse("Protect your social media accounts by using strong passwords, enabling two-factor authentication, reviewing privacy settings regularly, and being careful about what personal information you share publicly.");
            socialMediaTopic.AddTip("Regularly review and remove third-party app access to your social media accounts.");
            socialMediaTopic.AddTip("Be wary of quizzes and games that request access to your profile - they may collect personal data.");
            _topics.Add("social media", socialMediaTopic);
        }

        public bool TryGetResponse(string userInput, out string response, out string topicName)
        {
            response = string.Empty;
            topicName = string.Empty;
            userInput = userInput.ToLower();

            // Handle basic conversational inputs - makes it friendlier!
            if (userInput.Contains("hello") || userInput.Contains("hi") || userInput == "hey")
            {
                response = "Hello there! How can I help you with cybersecurity today?";
                return true;
            }
            if (userInput.Contains("how are you"))
            {
                response = "I'm functioning perfectly, thank you for asking! Ready to help you stay safe online.";
                return true;
            }
            if (userInput.Contains("what is your purpose") || userInput.Contains("what do you do"))
            {
                response = "My purpose is to help raise awareness about cybersecurity practices and answer your questions about staying safe online.";
                return true;
            }
            if (userInput.Contains("what can i ask you") || userInput.Contains("what can you do"))
            {
                response = "You can ask me about password safety, phishing, safe browsing, malware protection, VPNs, social media security, and general cybersecurity best practices!";
                return true;
            }

            // Find matching topics based on keywords
            foreach (var topic in _topics.Values)
            {
                if (topic.KeywordMatch(userInput))
                {
                    response = topic.GetFullResponse();
                    topicName = topic.Name;
                    return true;
                }
            }

            // No matches found
            return false;
        }

        // Return list of all available topics
        public List<string> GetAllTopicNames()
        {
            return _topics.Values.Select(t => t.Name).ToList();
        }
    }

    // Class for individual cybersecurity topics
    class CyberTopic
    {
        public string Name { get; private set; }
        private List<string> Keywords { get; set; }
        private string MainResponse { get; set; }
        private List<string> Tips { get; set; }

        // Constructor
        public CyberTopic(string name)
        {
            Name = name;
            Keywords = new List<string>();
            Tips = new List<string>();
        }

        // Add a keyword that triggers this topic
        public void AddKeyword(string keyword)
        {
            Keywords.Add(keyword.ToLower());
        }

        // Set the main explanation text
        public void SetMainResponse(string response)
        {
            MainResponse = response;
        }

        // Add additional tips
        public void AddTip(string tip)
        {
            Tips.Add(tip);
        }

        // Check if any keyword is in the input text
        public bool KeywordMatch(string input)
        {
            return Keywords.Any(k => input.Contains(k));
        }

        // Get complete response with tips
        public string GetFullResponse()
        {
            string response = MainResponse + "\n\nAdditional tips:";
            foreach (var tip in Tips)
            {
                response += $"\n• {tip}";
            }
            return response;
        }
    }

    // User Interface class - I spent a lot of time making this look good!
    class UserInterface
    {
        // ASCII art logo I created for the program - this took me forever to get right
        private readonly string _logoArt = @"
   _____      _                 _____                      _ _         
  / ____|    | |               / ____|                    (_) |        
 | |     _   | |__   ___ _ __| (___   ___  ___ _   _ _ __ _| |_ _   _ 
 | |    | | | | '_ \ / _ \ '__|\___ \ / _ \/ __| | | | '__| | __| | | |
 | |____| |_| | |_) |  __/ |  ____) |  __/ (__| |_| | |  | | |_| |_| |
  \_____\__, |_.__/ \___|_| |_____/ \___|\___|\__,_|_|  |_|\__|\__, |
         __/ |                                                   __/ |
        |___/                                                   |___/ 
           _       _       _                 
          / \   __| |_   _(_)___  ___  _ __ 
         / _ \ / _` \ \ / / / __|/ _ \| '__|
        / ___ \ (_| |\ V /| \__ \ (_) | |   
       /_/   \_\__,_| \_/ |_|___/\___/|_|   
";

        // Display the logo with a cool loading animation
        public void DisplayLogo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(_logoArt);
            Console.ResetColor();

            // Loading animation to make it look more professional
            Console.Write("Initializing system");
            for (int i = 0; i < 6; i++)
            {
                Console.Write(".");
                Thread.Sleep(250);  // Added small delay between dots
            }
            Console.WriteLine("\n");
        }

        // Welcome message with pretty box
        public void DisplayWelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      Welcome to the Enhanced Cybersecurity Advisory System         ║");
            Console.WriteLine("║  Your personal guide to understanding digital security threats     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        // Get user's name with validation
        public string GetUserName()
        {
            string name = string.Empty;
            
            // Keep asking until we get a valid name
            do
            {
                Console.Write("\nBefore we begin, please enter your name: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                name = Console.ReadLine().Trim();
                Console.ResetColor();

                if (string.IsNullOrEmpty(name))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Name cannot be empty. Please try again.");
                    Console.ResetColor();
                }
            } while (string.IsNullOrEmpty(name));

            return name;
        }

        // Cool typing effect I learned about - makes it look like computer is thinking
        public void DisplayTypingEffect(string text)
        {
            // This shows text character by character for a typing effect
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(15); // Made it a bit faster than my first attempt
            }
            Console.WriteLine();
        }

        // Display a message from the advisor with typing effect
        public void DisplayBotResponse(string message, string userName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\nAdvisor> ");
            DisplayTypingEffect(message);
            Console.ResetColor();
        }

        // Display the user's prompt with their name
        public void DisplayUserPrompt(string userName)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{userName}> ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        // Display error messages in red
        public void DisplayErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        // Display warning messages in yellow
        public void DisplayWarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        // Show available topics menu
        public void DisplayHelpMenu(List<string> topics)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n╔════════════════════════════════════════════════╗");
            Console.WriteLine("║             Available Topics                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            Console.ResetColor();

            // List all topics with bullet points
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var topic in topics)
            {
                Console.WriteLine($"• {topic}");
            }
            Console.ResetColor();
            
            Console.WriteLine("\nType a topic name or use a related keyword to learn more.");
            Console.WriteLine("Type 'exit', 'quit', or 'bye' to end the session.");
        }
    }

    // Main class that brings everything together - this was challenging to structure!
    class CyberSecurityAdvisor
    {
        private readonly UserInterface _ui;
        private readonly KnowledgeBase _knowledgeBase;
        private readonly AudioManager _audioManager;
        private UserProfile _userProfile;

        // Constructor to set up all components
        public CyberSecurityAdvisor()
        {
            _ui = new UserInterface();
            _knowledgeBase = new KnowledgeBase();
            _audioManager = new AudioManager();
        }

        // Start the program
        public void Start()
        {
            _ui.DisplayLogo();
            _audioManager.PlayAudio("welcome");
            _ui.DisplayWelcomeMessage();
            
            // Get user's name and create profile
            string userName = _ui.GetUserName();
            _userProfile = new UserProfile(userName);
            
            // Play introduction sound and show welcome message
            _audioManager.PlayAudio("introduction");
            _ui.DisplayTypingEffect($"\nNice to meet you, {userName}! I'm your cybersecurity advisor.");
            _ui.DisplayTypingEffect("You can ask me about various cybersecurity topics or type 'help' to see all available topics.");
            _ui.DisplayTypingEffect("Type 'exit', 'quit', or 'bye' when you're ready to end our session.");
            Console.WriteLine();

            // Start the main conversation loop
            RunConversationLoop();
        }

        // Main conversation loop
        private void RunConversationLoop()
        {
            bool continueChat = true;

            while (continueChat)
            {
                // Display prompt and get input
                _ui.DisplayUserPrompt(_userProfile.Name);
                string userInput = Console.ReadLine().Trim();
                Console.ResetColor();

                // Check for empty input
                if (string.IsNullOrEmpty(userInput))
                {
                    _ui.DisplayWarningMessage("I didn't catch that. Could you please type something?");
                    continue;
                }

                // Convert to lowercase for easier comparison
                userInput = userInput.ToLower();

                // Check for exit commands
                if (userInput == "exit" || userInput == "quit" || userInput == "bye")
                {
                    HandleExit();
                    continueChat = false;
                    continue;
                }

                // Check for help command
                if (userInput == "help")
                {
                    _ui.DisplayHelpMenu(_knowledgeBase.GetAllTopicNames());
                    continue;
                }

                // Try to find a response in our knowledge base
                if (_knowledgeBase.TryGetResponse(userInput, out string response, out string topicName))
                {
                    _ui.DisplayBotResponse(response, _userProfile.Name);
                    _userProfile.IncrementQuestions();
                    
                    // Record that this topic was viewed
                    if (!string.IsNullOrEmpty(topicName))
                    {
                        _userProfile.AddTopicViewed(topicName);
                    }
                }
                else
                {
                    // No matching topic found
                    _ui.DisplayBotResponse($"I'm not sure how to help with that, {_userProfile.Name}. " +
                        "Try asking about a specific cybersecurity topic or type 'help' to see available topics.", _userProfile.Name);
                }

                Console.WriteLine();
            }
        }

        // Handle exit from the program
        private void HandleExit()
        {
            _ui.DisplayBotResponse("Thank you for using the Enhanced Cybersecurity Advisor!", _userProfile.Name);
            _ui.DisplayTypingEffect(_userProfile.GetSessionSummary());
            _ui.DisplayTypingEffect("\nStay safe online and remember to practice good cybersecurity habits!");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nPress any key to exit...");
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}