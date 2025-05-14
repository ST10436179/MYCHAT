Enhanced Cybersecurity Advisor
Overview
The Enhanced Cybersecurity Advisor is a console-based chatbot developed in C# using .NET 9.0. It educates users on cybersecurity topics such as password safety, phishing prevention, and online privacy. The chatbot features a user-friendly interface, audio playback, sentiment detection, and user memory to personalize interactions, fulfilling the requirements of Part 2, including Step 7: Code Optimization.
Features

Keyword Recognition: Responds to cybersecurity topics (e.g., "password", "phishing") with multiple random responses.
Conversation Flow: Supports greetings, farewells, follow-up questions, and a help menu.
User Memory: Stores user name and interests (e.g., "my name is Alex", "interested in privacy") for personalized responses.
Sentiment Detection: Tailors responses based on user sentiment (e.g., "worried", "curious").
UI/UX: Includes ASCII art, colored text, typing effects, and initialization animation.
Audio Playback: Plays welcome and introduction audio using NAudio 2.2.1.
Error Handling: Manages empty inputs, long inputs (>200 characters), and invalid names.
Optimization: Uses generic collections (Dictionary, List), delegates for modularity, and efficient algorithms (O(1) lookups).

Prerequisites

.NET SDK: Version 9.0
NAudio: Version 2.2.1 (included via NuGet)
Audio Files: Welcome.wav and introduction.wav (PCM, 16-bit, 44.1kHz) in bin/Debug/net9.0


AFTER FETCHING FILES RUN / INSTALL:
dotnet add package NAudio
