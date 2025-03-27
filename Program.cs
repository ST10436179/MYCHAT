using System;
using System.Collections.Generic;
using System.Media;
using System.IO;
using System.Threading;
using NAudio.Wave;
// using System.Speech.Synthesis;
// using Google.Cloud.TextToSpeech.V1


namespace CybersecurityAwarenessBot
{
    class Program
    {
        // ASCII art for the bot logo
        static string asciiLogo = @"
  /$$$$$$            /$$                                  /$$$$$$    /$$     /$$                        /$$      
 /$$__  $$          | $$                                 /$$__  $$  | $$    | $$                       | $$      
| $$  \__/ /$$   /$$| $$$$$$$   /$$$$$$   /$$$$$$       | $$  \ $$ /$$$$$$ /$$$$$$   /$$$$$$   /$$$$$$$| $$   /$$
| $$      | $$  | $$| $$__  $$ /$$__  $$ /$$__  $$      | $$$$$$$$|_  $$_/|_  $$_/  |____  $$ /$$_____/| $$  /$$/
| $$      | $$  | $$| $$  \ $$| $$$$$$$$| $$  \__/      | $$__  $$  | $$    | $$     /$$$$$$$| $$      | $$$$$$/ 
| $$    $$| $$  | $$| $$  | $$| $$_____/| $$            | $$  | $$  | $$ /$$| $$ /$$/$$__  $$| $$      | $$_  $$ 
|  $$$$$$/|  $$$$$$$| $$$$$$$/|  $$$$$$$| $$            | $$  | $$  |  $$$$/|  $$$$/  $$$$$$$|  $$$$$$$| $$ \  $$
 \______/  \____  $$|_______/  \_______/|__/            |__/  |__/   \___/   \___/  \_______/ \_______/|__/  \__/
           /$$  | $$                                                                                             
          |  $$$$$$/                                                                                             
           \______/                                                                                              
";

        // Predefined responses for common questions
        static Dictionary<string, string> responses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "hello", "Hello there! How can I help you with cybersecurity today?" },
            { "hi", "Hi there! I'm here to chat about cybersecurity. What would you like to know?" },
            { "how are you", "I'm functioning perfectly, thank you for asking! Ready to help you stay safe online." },
            { "what is your purpose", "My purpose is to help raise awareness about cybersecurity practices and answer your questions about staying safe online." },
            { "what can i ask you about", "You can ask me about password safety, phishing, safe browsing, malware protection, and general cybersecurity best practices!" },
            { "password safety", "Great question! For strong passwords: use at least 12 characters, mix uppercase, lowercase, numbers, and symbols. Never reuse passwords across accounts, and consider using a password manager." },
            { "phishing", "Phishing attempts trick you into revealing sensitive information. Always verify the sender's email address, don't click suspicious links, and never provide personal information unless you're certain of the recipient's identity." },
            { "safe browsing", "For safe browsing: keep your browser updated, use HTTPS websites, be careful when downloading files, don't use public Wi-Fi for sensitive transactions, and consider using a VPN for additional protection." },
            { "malware", "To protect against malware: keep your software updated, use reputable antivirus software, don't download from untrusted sources, and be cautious of email attachments." },
            { "data breach", "If you're affected by a data breach: change your passwords immediately, monitor your accounts for suspicious activity, and consider freezing your credit if personal information was compromised." },
            { "bye", "Goodbye! Stay safe online!" },
            { "exit", "Exiting the Cybersecurity Awareness Bot. Remember to stay vigilant online!" },
            { "quit", "Closing the Cybersecurity Awareness Bot. Keep your digital life secure!" }
        };

        static string userName;

        static void Main(string[] args)
        {
            PlayGreeting();
            DisplayLogo();
            GetUserName();
            RunChatbot();
        }

        static void PlayGreeting()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Playing welcome message...");
                Console.ResetColor();

                string audioPath = "Welcome.wav";

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
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Voice greeting would play here - Place 'welcome_greeting.wav' in the application directory]");
                    Console.ResetColor();
                    Thread.Sleep(1500);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error playing greeting: {ex.Message}");
                Console.ResetColor();
                Thread.Sleep(1500);
            }
        }


        static void DisplayLogo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(asciiLogo);
            Console.ResetColor();

            // Simulate a loading effect for visual appeal
            Console.Write("Loading");
            for (int i = 0; i < 5; i++)
            {
                Console.Write(".");
                Thread.Sleep(300);
            }
            Console.WriteLine("\n");
        }

        // static void GetUserName()
        // {
        //     Console.ForegroundColor = ConsoleColor.Cyan;
        //     Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        //     Console.WriteLine("║ Welcome to the Cybersecurity Awareness Chatbot!          ║");
        //     Console.WriteLine("║ I'm here to help you learn about staying safe online.    ║");
        //     Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        //     Console.ResetColor();

        //     do
        //     {
        //         Console.Write("\nBefore we begin, what's your name? ");
        //         Console.ForegroundColor = ConsoleColor.Yellow;
        //         userName = Console.ReadLine().Trim();
        //         Console.ResetColor();

        //         if (string.IsNullOrEmpty(userName))
        //         {
        //             Console.ForegroundColor = ConsoleColor.Red;
        //             Console.WriteLine("I didn't catch that. Please enter your name.");
        //             Console.ResetColor();
        //         }
        //     } while (string.IsNullOrEmpty(userName));

        //     // Simulate typing effect
        //     TypeLine($"\nNice to meet you, {userName}! I'm excited to chat with you about cybersecurity.");
        //     TypeLine("You can ask me about topics like password safety, phishing, safe browsing, or just say 'exit' when you're done.");
        //     Console.WriteLine();
        // }

        static void GetUserName()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ Welcome to the Cybersecurity Awareness Chatbot!          ║");
            Console.WriteLine("║ I'm here to help you learn about staying safe online.    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            do
            {
                Console.Write("\nBefore we begin, what's your name? ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                userName = Console.ReadLine().Trim();
                Console.ResetColor();

                if (string.IsNullOrEmpty(userName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("I didn't catch that. Please enter your name.");
                    Console.ResetColor();
                }
            } while (string.IsNullOrEmpty(userName));

            try
            {
                string introductionPath = "introduction.wav";

                if (File.Exists(introductionPath))
                {
                    using (var audioFile = new AudioFileReader(introductionPath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[Audio file '{introductionPath}' not found]");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error playing nice audio: {ex.Message}");
                Console.ResetColor();
            }
            TypeLine($"\nNice to meet you, {userName}! I'm excited to chat with you about cybersecurity.");
            TypeLine("You can ask me about topics like password safety, phishing, safe browsing, or just say 'exit' when you're done.");
            Console.WriteLine();
        }

        // static void SpeakText(string text)
        // {
        //     using (var synthesizer = new SpeechSynthesizer())
        //     {
        //         synthesizer.Volume = 100; // 0...100
        //         synthesizer.Rate = 1; // -10...10

        //         // Speak the text
        //         synthesizer.SelectVoice("Microsoft David Desktop");

        //         synthesizer.Speak(text);
        //     }
        // }

        static void RunChatbot()
        {
            bool continueChat = true;

            while (continueChat)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{userName}> ");
                Console.ForegroundColor = ConsoleColor.White;
                string userInput = Console.ReadLine().Trim().ToLower();
                Console.ResetColor();

                if (string.IsNullOrEmpty(userInput))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("I didn't catch that. Could you please type something?");
                    Console.ResetColor();
                    continue;
                }

                if (new[] { "exit", "quit", "bye" }.Contains(userInput))
                {
                    string exitResponse = responses.ContainsKey(userInput) ? responses[userInput] : "Goodbye! Stay safe online!";
                    Console.ForegroundColor = ConsoleColor.Green;
                    TypeLine($"\nBot> {exitResponse}");
                    Console.ResetColor();
                    continueChat = false;
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nBot> ");

                // More flexible response matching
                bool foundResponse = false;
                string[] keywords = {
            "hello", "hi", "how are you", "purpose", "ask you about",
            "password", "phishing", "safe browsing", "malware", "data breach"
        };

                foreach (string keyword in keywords)
                {
                    if (userInput.Contains(keyword))
                    {
                        // Find the corresponding response
                        var matchingResponse = responses.FirstOrDefault(r =>
                            r.Key.ToLower() == keyword || userInput.Contains(r.Key.ToLower()));

                        if (!string.IsNullOrEmpty(matchingResponse.Value))
                        {
                            TypeLine(matchingResponse.Value);
                            foundResponse = true;
                            break;
                        }
                    }
                }

                // If no matching response was found
                if (!foundResponse)
                {
                    TypeLine($"I'm not sure how to respond to that, {userName}. Could you try another cybersecurity-related question? " +
                           "You can ask me about password safety, phishing, safe browsing, or malware protection.");
                }

                Console.ResetColor();
                Console.WriteLine();
            }
        }


        // Method to simulate typing effect for more natural conversation
        static void TypeLine(string line)
        {
            foreach (char c in line)
            {
                Console.Write(c);
                Thread.Sleep(20); // Adjust the speed as needed
            }
            Console.WriteLine();
        }
    }

}
