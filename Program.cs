using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace EnhancedCybersecurityAdvisor
{
    // Delegate for response selection
    public delegate string ResponseSelector(string topic);

    class CyberSecurityAdvisor
    {
        // User profile data with expanded memory capabilities
        private string _userName;
        private DateTime _joinedAt;
        private int _questionsAsked;
        private List<string> _topicsViewed;
        private Dictionary<string, string> _userMemory; // Expanded memory to store user preferences and information
        private List<string> _conversationHistory; // Track conversation for context awareness
        private string _currentTopic = string.Empty; // Track the current topic being discussed

        // Audio files
        private readonly Dictionary<string, string> _audioFiles = new Dictionary<string, string>
        {
            { "welcome", "Welcome.wav" },
            { "introduction", "introduction.wav" }
        };

        // Sentiment analysis dictionaries
        private readonly Dictionary<string, string> _positiveWords = new Dictionary<string, string>
        {
            { "good", "I'm glad to hear that!" },
            { "great", "That's wonderful!" },
            { "happy", "I'm happy to help you stay secure online!" },
            { "interested", "It's great that you're interested in cybersecurity!" },
            { "like", "I'm glad you like this information!" },
            { "thanks", "You're welcome! I'm here to help with any cybersecurity questions." },
            { "thank", "You're welcome! I'm here to help with any cybersecurity questions." },
            { "helpful", "I'm glad you find this helpful!" },
            { "excellent", "Thank you! I aim to provide excellent cybersecurity advice." }
        };

        private readonly Dictionary<string, string> _negativeWords = new Dictionary<string, string>
        {
            { "worried", "It's normal to feel concerned about cybersecurity. I'm here to help you understand how to stay safe." },
            { "scared", "Many people feel anxious about online threats. Let me help alleviate your concerns with some practical advice." },
            { "confused", "Cybersecurity can be confusing. Let me break it down for you in simpler terms." },
            { "difficult", "I understand cybersecurity can seem challenging. Let's take it step by step." },
            { "overwhelmed", "It's easy to feel overwhelmed by cybersecurity information. Let's focus on the basics first." },
            { "frustrating", "I understand your frustration. Cybersecurity doesn't have to be complicated - let me help simplify it." },
            { "scary", "Digital security can seem intimidating, but with a few simple practices, you can greatly improve your safety online." },
            { "hard", "Cybersecurity doesn't have to be hard. I'll help you understand the fundamental concepts." },
            { "afraid", "It's okay to be cautious about online security. That awareness is actually the first step to staying safe!" }
        };

        // Generic collection for random responses
        private readonly Dictionary<string, List<string>> _randomResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "greeting", new List<string> {
                "Hello! How can I help you with cybersecurity today?",
                "Hi there! Ready to learn about staying safe online?",
                "Greetings! What cybersecurity topic are you interested in?",
                "Welcome! I'm here to answer your cybersecurity questions."
            }},
            { "phishing", new List<string> {
                "Always verify the sender's email address before clicking any links or downloading attachments.",
                "Be suspicious of emails with urgent requests or threatening language - legitimate organizations don't pressure you this way.",
                "Hover over links before clicking to see the actual URL destination. If it looks suspicious, don't click!",
                "Banks and legitimate companies will never ask for sensitive information via email.",
                "When in doubt about an email, contact the supposed sender through official channels to verify its authenticity."
            }},
            { "password", new List<string> {
                "Use a unique password for each important account to prevent credential stuffing attacks.",
                "Consider using a password manager to generate and store complex passwords securely.",
                "The length of a password matters more than its complexity - aim for at least 12 characters.",
                "Passphrases (a string of random words) are often more secure and easier to remember than complex passwords.",
                "Enable two-factor authentication whenever possible for an additional layer of security."
            }},
            { "malware", new List<string> {
                "Keep your operating system and applications updated to patch security vulnerabilities.",
                "Only download software from official websites and app stores to reduce the risk of malware.",
                "Be cautious of email attachments, even from people you know - their accounts could be compromised.",
                "Use reputable antivirus software and keep it updated for the latest malware protection.",
                "Scan files before opening them, especially if they come from unfamiliar sources."
            }},
            { "browsing", new List<string> {
                "Look for HTTPS in the URL and a padlock icon to ensure the website is secure.",
                "Use private browsing mode when using public computers to prevent storing your session data.",
                "Clear your cookies and browsing history regularly to protect your privacy.",
                "Consider using a VPN when connecting to public Wi-Fi networks for encrypted communications.",
                "Be cautious about what information you share online - once it's out there, it's hard to remove."
            }},
            { "social_media", new List<string> {
                "Regularly review your privacy settings on all social media platforms.",
                "Be cautious about accepting friend or connection requests from people you don't know.",
                "Limit the personal information you share on social media profiles.",
                "Be aware that quizzes and games often collect your data for marketing purposes.",
                "Use strong, unique passwords for your social media accounts and enable two-factor authentication."
            }},
            { "farewell", new List<string> {
                "Stay safe online! Remember what we discussed about cybersecurity.",
                "Thank you for chatting about cybersecurity! Keep those digital defenses strong!",
                "It was great helping you with cybersecurity information. Remember to stay vigilant online!",
                "Until next time, keep your passwords strong and your personal information secure!"
            }},
            { "unknown", new List<string> {
                "I'm not sure I understand. Could you try rephrasing that?",
                "That topic isn't in my cybersecurity knowledge base. Can I help you with password safety, phishing, or safe browsing instead?",
                "I don't have information on that specific topic. Would you like to know about malware protection or data breach response?",
                "I'm not familiar with that question. Try asking about VPNs, social media security, or other cybersecurity topics."
            }}
        };

        // Knowledge base
        private readonly Dictionary<string, (string Name, List<string> Keywords, string MainResponse, List<string> Tips)> _topics =
            new Dictionary<string, (string, List<string>, string, List<string>)>(StringComparer.OrdinalIgnoreCase);

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

        // Constructor
        public CyberSecurityAdvisor()
        {
            _questionsAsked = 0;
            _topicsViewed = new List<string>();
            _userMemory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _conversationHistory = new List<string>();
            InitializeTopics();
        }

        private void InitializeTopics()
        {
            // Password topic
            _topics.Add("password", (
                Name: "Password Safety",
                Keywords: new List<string> { "password", "passwords", "pwd", "pass", "passphrase" },
                MainResponse: "For strong passwords: use at least 12 characters, mix uppercase, lowercase, numbers, and symbols. Never reuse passwords across accounts, and consider using a password manager.",
                Tips: new List<string>
                {
                    "Consider using a passphrase instead of a single word. For example, 'Purple-Horse-Battery-Staple-42!' is much stronger than 'P@ssw0rd'.",
                    "Enable two-factor authentication (2FA) whenever possible for an additional layer of security.",
                    "Change your passwords regularly, especially for high-value accounts like banking and email.",
                    "Use a trusted password manager to generate and store complex passwords securely."
                }
            ));

            // Phishing topic
            _topics.Add("phishing", (
                Name: "Phishing Prevention",
                Keywords: new List<string> { "phishing", "scam", "scams", "email", "fake", "fraud", "suspicious" },
                MainResponse: "Phishing attempts trick you into revealing sensitive information. Always verify the sender's email address, don't click suspicious links, and never provide personal information unless you're certain of the recipient's identity.",
                Tips: new List<string>
                {
                    "Hover over links before clicking to see the actual URL destination.",
                    "Be wary of urgent requests or threats that create pressure to act quickly.",
                    "Look for spelling and grammar errors, which are common in phishing attempts.",
                    "If an offer seems too good to be true, it probably is."
                }
            ));

            // Safe browsing topic
            _topics.Add("browsing", (
                Name: "Safe Browsing",
                Keywords: new List<string> { "browsing", "browser", "internet", "web", "website", "surf", "online" },
                MainResponse: "For safe browsing: keep your browser updated, use HTTPS websites, be careful when downloading files, don't use public Wi-Fi for sensitive transactions, and consider using a VPN for additional protection.",
                Tips: new List<string>
                {
                    "Clear your cookies and browsing history regularly to protect your privacy.",
                    "Use browser extensions like ad blockers and privacy tools to enhance security.",
                    "Check for HTTPS and a padlock icon in the address bar before entering sensitive information.",
                    "Consider using a privacy-focused browser for additional protection."
                }
            ));

            // Malware topic
            _topics.Add("malware", (
                Name: "Malware Protection",
                Keywords: new List<string> { "malware", "virus", "trojan", "ransomware", "spyware", "adware", "infection" },
                MainResponse: "To protect against malware: keep your software updated, use reputable antivirus software, don't download from untrusted sources, and be cautious of email attachments.",
                Tips: new List<string>
                {
                    "Regularly scan your system with anti-malware software.",
                    "Be especially cautious of files with extensions like .exe, .bat, or .scr.",
                    "Keep your operating system and applications updated with the latest security patches.",
                    "Back up your important data regularly to protect against ransomware attacks."
                }
            ));

            // Data breach topic
            _topics.Add("data breach", (
                Name: "Data Breach Response",
                Keywords: new List<string> { "data breach", "breach", "hack", "leaked", "stolen", "compromised", "identity theft" },
                MainResponse: "If you're affected by a data breach: change your passwords immediately, monitor your accounts for suspicious activity, and consider freezing your credit if personal information was compromised.",
                Tips: new List<string>
                {
                    "Use services like Have I Been Pwned to check if your data has been compromised.",
                    "Consider using a credit monitoring service after a major breach.",
                    "Be extra vigilant about phishing attempts following a data breach announcement.",
                    "Report suspicious activity on your accounts immediately to the service provider."
                }
            ));

            // VPN topic
            _topics.Add("vpn", (
                Name: "VPN Usage",
                Keywords: new List<string> { "vpn", "virtual private network", "proxy", "encrypted connection" },
                MainResponse: "A VPN (Virtual Private Network) encrypts your internet connection, helping protect your privacy and security, especially on public Wi-Fi networks. It can also mask your IP address and location.",
                Tips: new List<string>
                {
                    "Free VPNs often collect and sell your data - consider paying for a reputable service.",
                    "Even with a VPN, practice good security habits as they don't make you completely anonymous.",
                    "Choose a VPN provider with a strict no-logs policy for better privacy.",
                    "Use a VPN when connecting to public Wi-Fi networks to protect your data from eavesdroppers."
                }
            ));

            // Social media topic
            _topics.Add("social media", (
                Name: "Social Media Security",
                Keywords: new List<string> { "social media", "facebook", "twitter", "instagram", "tiktok", "linkedin", "snapchat" },
                MainResponse: "Protect your social media accounts by using strong passwords, enabling two-factor authentication, reviewing privacy settings regularly, and being careful about what personal information you share publicly.",
                Tips: new List<string>
                {
                    "Regularly review and remove third-party app access to your social media accounts.",
                    "Be wary of quizzes and games that request access to your profile - they may collect personal data.",
                    "Think twice before sharing personal details like your full birth date, address, or phone number.",
                    "Be cautious about accepting friend or connection requests from people you don't know."
                }
            ));

            // Privacy topic
            _topics.Add("privacy", (
                Name: "Online Privacy",
                Keywords: new List<string> { "privacy", "tracking", "cookies", "data collection", "anonymous", "incognito" },
                MainResponse: "Protecting your online privacy involves controlling what information you share, using privacy-focused tools and services, and understanding how companies collect and use your data.",
                Tips: new List<string>
                {
                    "Regularly review privacy settings on websites and apps you use.",
                    "Consider using privacy-focused browsers and search engines.",
                    "Read privacy policies before using new services, especially regarding data collection practices.",
                    "Use cookie blockers and ad blockers to reduce tracking across websites."
                }
            ));

            // Public Wi-Fi topic
            _topics.Add("public wifi", (
                Name: "Public Wi-Fi Safety",
                Keywords: new List<string> { "public wifi", "hotspot", "free wifi", "wireless", "cafe wifi", "hotel wifi" },
                MainResponse: "Public Wi-Fi networks are convenient but risky. Use a VPN, avoid accessing sensitive accounts, verify network names, and turn off file sharing and automatic connections for better security.",
                Tips: new List<string>
                {
                    "Always use HTTPS websites when on public Wi-Fi.",
                    "Avoid online banking or shopping on public networks unless using a VPN.",
                    "Disable automatic connections to prevent your device from joining unknown networks.",
                    "Consider using your mobile data instead of public Wi-Fi for sensitive activities."
                }
            ));
        }

        // Main program start
        public void Start()
        {
            DisplayLogo();
            PlayAudio("welcome");
            DisplayWelcomeMessage();
            
            _userName = GetUserName();
            _joinedAt = DateTime.Now;
            _userMemory.Add("name", _userName);
            
            PlayAudio("introduction");
            DisplayTypingEffect($"\nNice to meet you, {_userName}! I'm your cybersecurity advisor.");
            DisplayTypingEffect("You can ask me about various cybersecurity topics or type 'help' to see all available topics.");
            DisplayTypingEffect("Type 'exit', 'quit', or 'bye' when you're ready to end our session.");
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
                Console.WriteLine($"• {topic.Name}");
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

        // Randomly select a response from the collection for variety
        private string GetRandomResponse(string topic)
        {
            if (_randomResponses.ContainsKey(topic))
            {
                Random random = new Random();
                int index = random.Next(0, _randomResponses[topic].Count);
                return _randomResponses[topic][index];
            }
            return "I don't have specific information on that topic.";
        }

        // Advanced response method using sentiment detection
        private string GetAdvancedResponse(string userInput)
        {
            userInput = userInput.ToLower();
            
            // Store conversation for context
            _conversationHistory.Add(userInput);
            
            // Check for sentiment in the user input
            foreach (var word in _positiveWords.Keys)
            {
                if (userInput.Contains(word))
                {
                    return _positiveWords[word];
                }
            }
            
            foreach (var word in _negativeWords.Keys)
            {
                if (userInput.Contains(word))
                {
                    return _negativeWords[word];
                }
            }
            
            // Handle "more" requests (follow-up questions)
            if (userInput.Contains("more") && !string.IsNullOrEmpty(_currentTopic))
            {
                foreach (var topic in _topics)
                {
                    if (topic.Key.Equals(_currentTopic, StringComparison.OrdinalIgnoreCase))
                    {
                        // Use the ResponseSelector delegate for random responses
                        ResponseSelector selector = GetRandomResponse;
                        return selector(topic.Key.Replace(" ", "_"));
                    }
                }
            }
            
            // Check for memory-related questions
            if (userInput.Contains("what am i interested in") || userInput.Contains("what do i like"))
            {
                if (_userMemory.ContainsKey("interest"))
                {
                    return $"You previously mentioned that you're interested in {_userMemory["interest"]}. Would you like to learn more about it?";
                }
            }
            
            // Check for topic interests and store in memory
            if (userInput.Contains("interested in") || userInput.Contains("like to learn about"))
            {
                foreach (var topic in _topics)
                {
                    foreach (var keyword in topic.Value.Keywords)
                    {
                        if (userInput.Contains(keyword))
                        {
                            // Store user interest in memory
                            _userMemory["interest"] = topic.Value.Name.ToLower();
                            return $"Great! I'll remember that you're interested in {topic.Value.Name.ToLower()}. It's an important aspect of cybersecurity.";
                        }
                    }
                }
            }
            
            return null; // Return null if no advanced response is found
        }

        private bool TryGetResponse(string userInput, out string response, out string topicName)
        {
            response = string.Empty;
            topicName = string.Empty;
            userInput = userInput.ToLower();

            // Try to get an advanced response first (sentiment, memory, follow-up)
            string advancedResponse = GetAdvancedResponse(userInput);
            if (!string.IsNullOrEmpty(advancedResponse))
            {
                response = advancedResponse;
                return true;
            }

            // Basic greetings using random responses
            if (userInput.Contains("hello") || userInput.Contains("hi") || userInput == "hey")
            {
                response = GetRandomResponse("greeting");
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

            // Look for topic matches
            foreach (var topic in _topics)
            {
                if (topic.Value.Keywords.Any(k => userInput.Contains(k)))
                {
                    // Store the current topic for context and follow-up questions
                    _currentTopic = topic.Key;
                    
                    // Personalize response if user previously showed interest in this topic
                    string personalizedIntro = string.Empty;
                    if (_userMemory.ContainsKey("interest") && _userMemory["interest"].Equals(topic.Value.Name.ToLower()))
                    {
                        personalizedIntro = $"Since you mentioned you're interested in {topic.Value.Name.ToLower()}, here's some helpful information: ";
                    }
                    
                    response = $"{personalizedIntro}{topic.Value.MainResponse}\n\nAdditional tips:";
                    foreach (var tip in topic.Value.Tips)
                    {
                        response += $"\n• {tip}";
                    }
                    topicName = topic.Value.Name;
                    return true;
                }
            }

            // Handle unknown inputs
            response = GetRandomResponse("unknown");
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
            
            string interestSummary = string.Empty;
            if (_userMemory.ContainsKey("interest"))
            {
                interestSummary = $"\n- Your main interest: {_userMemory["interest"]}";
            }
            
            return $"Session Summary for {_userName}:\n" +
                   $"- Session duration: {sessionDuration.Minutes} minutes, {sessionDuration.Seconds} seconds\n" +
                   $"- Topics explored: {_topicsViewed.Count}\n" +
                   $"- Questions asked: {_questionsAsked}" +
                   interestSummary;
        }

        private void RunConversationLoop()
        {
            bool continueChat = true;

            while (continueChat)
            {
                DisplayUserPrompt();
                string userInput = Console.ReadLine().Trim();
                Console.ResetColor();

                if (string.IsNullOrEmpty(userInput))
                {
                    DisplayWarningMessage("I didn't catch that. Could you please type something?");
                    continue;
                }

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

                if (TryGetResponse(userInput, out string response, out string topicName))
                {
                    DisplayBotResponse(response);
                    IncrementQuestions();
                    if (!string.IsNullOrEmpty(topicName))
                    {
                        AddTopicViewed(topicName);
                    }
                }
                else
                {
                    // This should never happen now because TryGetResponse always returns true
                    // with the unknown response handler, but keeping as a fallback
                    DisplayBotResponse($"I'm not sure how to help with that, {_userName}. " +
                        "Try asking about a specific cybersecurity topic or type 'help' to see available topics.");
                }

                Console.WriteLine();
            }
        }

        private void HandleExit()
        {
            // Use random farewell message
            DisplayBotResponse(GetRandomResponse("farewell"));
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