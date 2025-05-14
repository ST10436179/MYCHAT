using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace EnhancedCybersecurityAdvisor
{
    // Delegates for response selection and sentiment modification
    public delegate bool ResponseSelector(string input, out string response, out string topicName);
    public delegate string SentimentResponseModifier(string baseResponse, string sentiment, string originalInput);

    class CyberSecurityAdvisor
    {
        // User profile data
        private string _userName;
        private DateTime _joinedAt;
        private int _questionsAsked;
        private List<string> _topicsViewed;
        private Dictionary<string, string> _userMemory; // Store preferences
        private List<string> _conversationHistory; // Track conversation
        private string _currentTopic; // Current conversation topic
        private Random _random; // For random responses

        // Audio files
        private readonly Dictionary<string, string> _audioFiles = new Dictionary<string, string>
        {
            { "welcome", "Welcome.wav" },
            { "introduction", "introduction.wav" }
        };

        // Knowledge base with multiple responses per topic
        private readonly Dictionary<string, (string Name, List<string> Keywords, List<string> MainResponses, List<string> Tips)> _topics =
            new Dictionary<string, (string Name, List<string> Keywords, List<string> MainResponses, List<string> Tips)>(StringComparer.OrdinalIgnoreCase);

        // Sentiment detection
        private readonly Dictionary<string, string> _sentimentKeywords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "worried", "worried" }, { "concerned", "worried" }, { "anxious", "worried" }, { "scared", "worried" },
            { "curious", "curious" }, { "interested", "curious" }, { "tell me more", "curious" },
            { "frustrated", "frustrated" }, { "annoyed", "frustrated" }, { "upset", "frustrated" },
            { "happy", "positive" }, { "glad", "positive" }, { "thanks", "positive" }, { "thank", "positive" }
        };

        private readonly Dictionary<string, string> _sentimentResponses = new Dictionary<string, string>
        {
            { "worried", "It's understandable to feel {0} about cybersecurity. Let's take it step by step: {1}" },
            { "curious", "I'm glad you're curious! Here's some info: {0}" },
            { "frustrated", "I hear your frustration. Let's clarify this: {0}" },
            { "positive", "Awesome to hear your enthusiasm! {0}" }
        };

        // Random responses for non-topic interactions
        private readonly Dictionary<string, List<string>> _randomResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "greeting", new List<string> {
                "Hello! How can I help you with cybersecurity today?",
                "Hi there! Ready to learn about staying safe online?",
                "Greetings! What cybersecurity topic are you interested in?",
                "Welcome! I'm here to answer your cybersecurity questions."
            }},
            { "farewell", new List<string> {
                "Stay safe online! Remember what we discussed about cybersecurity.",
                "Thank you for chatting about cybersecurity! Keep those digital defenses strong!",
                "It was great helping you with cybersecurity information. Remember to stay vigilant online!",
                "Until next time, keep your passwords strong and your personal information secure!"
            }},
            { "unknown", new List<string> {
                "I'm not sure I understand. Could you try rephrasing?",
                "That topic isn't in my knowledge base. Can I help with password safety, phishing, or privacy?",
                "I don't have info on that. Would you like to know about malware or VPNs?",
                "I'm not familiar with that. Try asking about social media security or public Wi-Fi."
            }}
        };

        // ASCII art logo
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

        public CyberSecurityAdvisor()
        {
            _questionsAsked = 0;
            _topicsViewed = new List<string>();
            _userMemory = new Dictionary<string, string>();
            _conversationHistory = new List<string>();
            _currentTopic = string.Empty;
            _random = new Random();
            InitializeTopics();
        }

        private void InitializeTopics()
        {
            // Password topic
            _topics.Add("password", (
                Name: "Password Safety",
                Keywords: new List<string> { "password", "passphrase", "pwd", "pass" },
                MainResponses: new List<string>
                {
                    "For strong passwords: use at least 12 characters, mix uppercase, lowercase, numbers, and symbols. Never reuse passwords across accounts, and consider using a password manager.",
                    "Create unique passwords with a mix of characters. Password managers can help you keep track of them securely.",
                    "Strong passwords are key to security. Use long, random combinations and avoid reusing them."
                },
                Tips: new List<string>
                {
                    "Consider using a passphrase like 'Purple-Horse-Battery-Staple-42!' for better security.",
                    "Enable two-factor authentication (2FA) for an extra layer of protection.",
                    "Change passwords regularly, especially for high-value accounts."
                }
            ));

            // Phishing topic
            _topics.Add("phishing", (
                Name: "Phishing Prevention",
                Keywords: new List<string> { "phishing", "scam", "email", "fraud", "suspicious" },
                MainResponses: new List<string>
                {
                    "Phishing attempts trick you into revealing sensitive information. Always verify the sender's email address, don't click suspicious links, and never share personal details.",
                    "Be cautious of emails asking for personal information. Scammers often pose as trusted organizations.",
                    "Phishing emails can look real. Check the sender and avoid clicking links in unsolicited messages."
                },
                Tips: new List<string>
                {
                    "Hover over links to see the actual URL before clicking.",
                    "Look for urgent language or threats, as these are common phishing tactics.",
                    "Check for spelling or grammar errors, common in phishing attempts."
                }
            ));

            // Safe browsing topic
            _topics.Add("browsing", (
                Name: "Safe Browsing",
                Keywords: new List<string> { "browsing", "browser", "internet", "web", "online" },
                MainResponses: new List<string>
                {
                    "For safe browsing: keep your browser updated, use HTTPS websites, be careful when downloading files, and avoid public Wi-Fi for sensitive tasks.",
                    "Stay secure online by ensuring websites use HTTPS and keeping your browser up to date.",
                    "Safe browsing means avoiding risky downloads and using secure connections like HTTPS."
                },
                Tips: new List<string>
                {
                    "Clear cookies and history regularly to protect your privacy.",
                    "Use ad blockers and privacy extensions to enhance security.",
                    "Check for HTTPS and a padlock icon before entering sensitive data."
                }
            ));

            // Malware topic
            _topics.Add("malware", (
                Name: "Malware Protection",
                Keywords: new List<string> { "malware", "virus", "trojan", "ransomware", "spyware" },
                MainResponses: new List<string>
                {
                    "To protect against malware: keep software updated, use reputable antivirus software, and avoid downloads from untrusted sources.",
                    "Malware can harm your device. Stay safe with antivirus software and careful downloading.",
                    "Prevent malware by updating your system and avoiding suspicious email attachments."
                },
                Tips: new List<string>
                {
                    "Run regular antivirus scans to catch potential threats.",
                    "Be cautious of files with extensions like .exe or .bat.",
                    "Back up data regularly to protect against ransomware."
                }
            ));

            // Data breach topic
            _topics.Add("data breach", (
                Name: "Data Breach Response",
                Keywords: new List<string> { "data breach", "breach", "hack", "leaked", "compromised" },
                MainResponses: new List<string>
                {
                    "If affected by a data breach: change passwords immediately, monitor accounts, and consider freezing your credit.",
                    "A data breach can expose your info. Act fast by updating passwords and checking accounts.",
                    "Post-breach, secure your accounts with new passwords and monitor for unusual activity."
                },
                Tips: new List<string>
                {
                    "Check if your email was compromised using Have I Been Pwned.",
                    "Consider a credit monitoring service after a breach.",
                    "Be vigilant about phishing attempts after a breach."
                }
            ));

            // VPN topic
            _topics.Add("vpn", (
                Name: "VPN Usage",
                Keywords: new List<string> { "vpn", "virtual private network", "proxy" },
                MainResponses: new List<string>
                {
                    "A VPN encrypts your internet connection, protecting your privacy, especially on public Wi-Fi.",
                    "Use a VPN to secure your data and hide your IP address on unsecured networks.",
                    "VPNs enhance privacy by encrypting your connection, ideal for public Wi-Fi."
                },
                Tips: new List<string>
                {
                    "Choose a reputable paid VPN, as free ones may log your data.",
                    "A VPN isn't full anonymity—combine it with good security habits.",
                    "Select a VPN with a no-logs policy for better privacy."
                }
            ));

            // Social media topic
            _topics.Add("social media", (
                Name: "Social Media Security",
                Keywords: new List<string> { "social media", "facebook", "twitter", "instagram", "tiktok", "linkedin" },
                MainResponses: new List<string>
                {
                    "Protect social media accounts with strong passwords, 2FA, and regular privacy setting reviews.",
                    "Secure your social media by using unique passwords and enabling two-factor authentication.",
                    "Keep your social media safe with strong security settings and careful sharing."
                },
                Tips: new List<string>
                {
                    "Review third-party app access to your accounts regularly.",
                    "Avoid quizzes or games that request profile access—they may collect data.",
                    "Limit personal details like birth dates or addresses."
                }
            ));

            // Privacy topic
            _topics.Add("privacy", (
                Name: "Online Privacy",
                Keywords: new List<string> { "privacy", "tracking", "cookies", "data protection" },
                MainResponses: new List<string>
                {
                    "Protect your online privacy by reviewing app permissions and using privacy-focused tools.",
                    "Online privacy starts with controlling what data you share and who can access it.",
                    "Stay private online by limiting data sharing and checking privacy settings."
                },
                Tips: new List<string>
                {
                    "Use privacy-focused browsers like Firefox or search engines like DuckDuckGo.",
                    "Regularly clear tracking cookies to reduce data collection.",
                    "Read privacy policies to understand data usage."
                }
            ));

            // Public Wi-Fi topic
            _topics.Add("public wifi", (
                Name: "Public Wi-Fi Safety",
                Keywords: new List<string> { "public wifi", "hotspot", "free wifi", "wireless" },
                MainResponses: new List<string>
                {
                    "Public Wi-Fi is risky. Use a VPN, avoid sensitive accounts, and verify network names.",
                    "Secure public Wi-Fi usage with a VPN and by sticking to HTTPS websites.",
                    "Protect yourself on public Wi-Fi by disabling auto-connect and using a VPN."
                },
                Tips: new List<string>
                {
                    "Always use HTTPS websites on public Wi-Fi.",
                    "Avoid banking or shopping on public networks without a VPN.",
                    "Turn off file sharing when using public Wi-Fi."
                }
            ));
        }

        public void Start()
        {
            DisplayLogo();
            PlayAudio("welcome");
            DisplayWelcomeMessage();
            
            _userName = GetUserName();
            _joinedAt = DateTime.Now;
            _userMemory["name"] = _userName;
            
            PlayAudio("introduction");
            DisplayTypingEffect($"\nNice to meet you, {_userName}! I'm your cybersecurity advisor.");
            DisplayTypingEffect("You can ask about cybersecurity topics like passwords, scams, or privacy, or type 'help' to see all topics.");
            DisplayTypingEffect("Type 'exit', 'quit', or 'bye' to end our session.");
            Console.WriteLine();

            RunConversationLoop();
        }

        private void DisplayLogo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(_logoArt);
            Console.ResetColor();

            Console.Write("Initializing system");
            for (int i = 0; i < 6; i++)
            {
                Console.Write(".");
                Thread.Sleep(250);
            }
            Console.WriteLine("\n");
        }

        private void DisplayWelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      Welcome to the Enhanced Cybersecurity Advisory System         ║");
            Console.WriteLine("║  Your personal guide to understanding digital security threats     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private string GetUserName()
        {
            string name = string.Empty;
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

        private void DisplayTypingEffect(string text)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(15);
            }
            Console.WriteLine();
        }

        private void DisplayBotResponse(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\nAdvisor> ");
            DisplayTypingEffect(message);
            Console.ResetColor();
        }

        private void DisplayUserPrompt()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{_userName}> ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void DisplayErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private void DisplayWarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private void DisplayHelpMenu()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n╔════════════════════════════════════════════════╗");
            Console.WriteLine("║             Available Topics                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var topic in _topics.Values)
            {
                Console.WriteLine($"• {topic.Name} (Keywords: {string.Join(", ", topic.Keywords)})");
            }
            Console.ResetColor();
            
            Console.WriteLine("\nType a topic name or use a related keyword to learn more.");
            Console.WriteLine("Type 'exit', 'quit', or 'bye' to end the session.");
        }

        private bool PlayAudio(string audioKey)
        {
            try
            {
                if (!_audioFiles.ContainsKey(audioKey))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Unknown audio key: {audioKey}]");
                    Console.ResetColor();
                    return false;
                }

                string audioPath = _audioFiles[audioKey];
                if (File.Exists(audioPath))
                {
                    using (var audioFile = new AudioFileReader(audioPath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Audio file '{audioPath}' not found]");
                    Console.ResetColor();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error playing audio: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        private string DetectSentiment(string input)
        {
            foreach (var sentiment in _sentimentKeywords)
            {
                if (input.ToLower().Contains(sentiment.Key))
                {
                    return sentiment.Value;
                }
            }
            return string.Empty;
        }

        private string ModifyResponseBySentiment(string baseResponse, string sentiment, string originalInput)
        {
            if (_sentimentResponses.ContainsKey(sentiment))
            {
                if (sentiment == "worried")
                {
                    string worryTerm = _sentimentKeywords.Keys
                        .FirstOrDefault(k => sentiment == "worried" && originalInput.ToLower().Contains(k)) ?? "worried";
                    return string.Format(_sentimentResponses[sentiment], worryTerm, baseResponse);
                }
                return string.Format(_sentimentResponses[sentiment], baseResponse);
            }
            return baseResponse;
        }

        private void CheckForUserInterests(string input)
        {
            string lowerInput = input.ToLower();
            if (lowerInput.Contains("interested in") || lowerInput.Contains("like to learn about"))
            {
                foreach (var topic in _topics.Keys)
                {
                    if (lowerInput.Contains(topic))
                    {
                        _userMemory["interest"] = topic;
                        DisplayBotResponse($"Great! I'll remember you're interested in {topic}.");
                        break;
                    }
                }
            }

            if (lowerInput.Contains("my name is"))
            {
                int index = lowerInput.IndexOf("my name is") + 10; // Length of "my name is"
                if (index < input.Length)
                {
                    string name = input.Substring(index).Trim();
                    // Remove trailing punctuation
                    if (name.EndsWith(".") || name.EndsWith("!") || name.EndsWith(","))
                    {
                        name = name.Substring(0, name.Length - 1);
                    }
                    // Validate name: must be non-empty and contain at least one letter
                    if (!string.IsNullOrWhiteSpace(name) && name.Any(char.IsLetter))
                    {
                        _userMemory["preferred_name"] = name;
                        DisplayBotResponse($"Got it, I'll call you {name}!");
                    }
                    else
                    {
                        DisplayBotResponse("Sorry, I couldn't catch your name. Could you say it again, like 'my name is Alex'?");
                    }
                }
                else
                {
                    DisplayBotResponse("Sorry, I couldn't catch your name. Could you say it again, like 'my name is Alex'?");
                }
            }
        }

        private string IncorporateMemory(string response)
        {
            if (_userMemory.ContainsKey("interest") && _random.Next(3) == 0)
            {
                string interest = _userMemory["interest"];
                return $"As someone interested in {interest}, you might like this: {response}";
            }

            if (_userMemory.ContainsKey("preferred_name") && _random.Next(4) == 0)
            {
                return $"{_userMemory["preferred_name"]}, {response}";
            }

            return response;
        }

        private bool TryGetResponse(string userInput, out string response, out string topicName)
        {
            response = string.Empty;
            topicName = string.Empty;
            userInput = userInput.ToLower();

            // Basic conversational inputs
            if (userInput.Contains("hello") || userInput.Contains("hi") || userInput == "hey")
            {
                response = _randomResponses["greeting"][_random.Next(_randomResponses["greeting"].Count)];
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
                response = "You can ask me about password safety, phishing, safe browsing, malware protection, VPNs, social media security, privacy, public Wi-Fi, and more!";
                return true;
            }

            // Handle memory-related questions
            if (userInput.Contains("what am i interested in") || userInput.Contains("what do i like"))
            {
                if (_userMemory.ContainsKey("interest"))
                {
                    response = $"You mentioned you're interested in {_userMemory["interest"]}. Want to learn more about it?";
                    return true;
                }
                response = "I don't know your interests yet! Tell me what you're interested in.";
                return true;
            }

            // Handle follow-up questions
            if (!string.IsNullOrEmpty(_currentTopic) &&
                (userInput.Contains("more") || userInput.Contains("explain") || userInput.Contains("tell me") || userInput.Contains("details")))
            {
                var topic = _topics[_currentTopic];
                response = $"More on {topic.Name}: {topic.MainResponses[_random.Next(topic.MainResponses.Count)]}\n\nAdditional tips:";
                foreach (var tip in topic.Tips)
                {
                    response += $"\n• {tip}";
                }
                topicName = topic.Name;
                return true;
            }

            // Delegate for response selection
            ResponseSelector selector = (string input, out string resp, out string tName) =>
            {
                resp = string.Empty;
                tName = string.Empty;
                foreach (var topic in _topics)
                {
                    if (topic.Value.Keywords.Any(k => input.Contains(k)))
                    {
                        resp = topic.Value.MainResponses[_random.Next(topic.Value.MainResponses.Count)] + "\n\nAdditional tips:";
                        foreach (var tip in topic.Value.Tips)
                        {
                            resp += $"\n• {tip}";
                        }
                        tName = topic.Value.Name;
                        return true;
                    }
                }
                return false;
            };

            if (selector(userInput, out response, out topicName))
            {
                return true;
            }

            // Handle unknown inputs
            response = _randomResponses["unknown"][_random.Next(_randomResponses["unknown"].Count)];
            return true;
        }

        private void IncrementQuestions()
        {
            _questionsAsked++;
        }

        private void AddTopicViewed(string topic)
        {
            if (!_topicsViewed.Contains(topic))
            {
                _topicsViewed.Add(topic);
            }
        }

        private string GetSessionSummary()
        {
            TimeSpan sessionDuration = DateTime.Now - _joinedAt;
            string interestSummary = _userMemory.ContainsKey("interest") 
                ? $"\n- Your main interest: {_userMemory["interest"]}" 
                : "";
            return $"Session Summary for {_userName}:\n" +
                   $"- Session duration: {sessionDuration.Minutes} minutes, {sessionDuration.Seconds} seconds\n" +
                   $"- Topics explored: {_topicsViewed.Count}\n" +
                   $"- Questions asked: {_questionsAsked}" + interestSummary;
        }

        private void RunConversationLoop()
        {
            bool continueChat = true;
            int invalidInputCount = 0;

            while (continueChat)
            {
                DisplayUserPrompt();
                string userInput = Console.ReadLine().Trim();
                Console.ResetColor();

                if (string.IsNullOrEmpty(userInput))
                {
                    DisplayWarningMessage("I didn't catch that. Could you please type something?");
                    invalidInputCount++;
                    if (invalidInputCount >= 3)
                    {
                        DisplayWarningMessage("Try typing 'help' to see topics or 'exit' to quit.");
                    }
                    continue;
                }

                if (userInput.Length > 200)
                {
                    DisplayWarningMessage("Input is too long. Please keep it under 200 characters.");
                    invalidInputCount++;
                    continue;
                }

                invalidInputCount = 0;
                _conversationHistory.Add(userInput);
                userInput = userInput.ToLower();

                if (userInput == "exit" || userInput == "quit" || userInput == "bye")
                {
                    HandleExit();
                    continueChat = false;
                    continue;
                }

                if (userInput == "help")
                {
                    DisplayHelpMenu();
                    continue;
                }

                // Check for user interests
                CheckForUserInterests(userInput);

                // Detect sentiment
                string sentiment = DetectSentiment(userInput);

                // Process input using delegate
                ResponseSelector processInput = TryGetResponse;
                if (processInput(userInput, out string response, out string topicName))
                {
                    _currentTopic = topicName;
                    if (!string.IsNullOrEmpty(sentiment))
                    {
                        SentimentResponseModifier modifier = ModifyResponseBySentiment;
                        response = modifier(response, sentiment, userInput);
                    }
                    response = IncorporateMemory(response);
                    DisplayBotResponse(response);
                    IncrementQuestions();
                    if (!string.IsNullOrEmpty(topicName))
                    {
                        AddTopicViewed(topicName);
                    }
                }

                Console.WriteLine();
            }
        }

        private void HandleExit()
        {
            DisplayBotResponse(_randomResponses["farewell"][_random.Next(_randomResponses["farewell"].Count)]);
            DisplayTypingEffect(GetSessionSummary());
            DisplayTypingEffect("\nStay safe online and remember to practice good cybersecurity habits!");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nPress any key to exit...");
            Console.ResetColor();
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            var advisor = new CyberSecurityAdvisor();
            advisor.Start();
        }
    }
}