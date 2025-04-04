// class UserProfile
//     {
//         public string Name { get; set; }
//         public DateTime JoinedAt { get; set; }
//         public int QuestionsAsked { get; set; }
//         public List<string> TopicsViewed { get; set; }

//         public UserProfile(string name)
//         {
//             Name = name;
//             JoinedAt = DateTime.Now;
//             QuestionsAsked = 0;
//             TopicsViewed = new List<string>();
//         }

//         public void IncrementQuestions()
//         {
//             QuestionsAsked++;
//         }

//         public void AddTopicViewed(string topic)
//         {
//             if (!TopicsViewed.Contains(topic))
//             {
//                 TopicsViewed.Add(topic);
//             }
//         }

//         public string GetSessionSummary()
//         {
//             TimeSpan sessionDuration = DateTime.Now - JoinedAt;
//             return $"Session Summary for {Name}:\n" +
//                    $"- Session duration: {sessionDuration.Minutes} minutes, {sessionDuration.Seconds} seconds\n" +
//                    $"- Topics explored: {TopicsViewed.Count}\n" +
//                    $"- Questions asked: {QuestionsAsked}";
//         }
//     }