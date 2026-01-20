using System.Text.RegularExpressions;

namespace MyRssClient.Data.Ranking {
    public class Tokenizer {
        public static List<string> Tokenize(string text) =>
            [.. Regex.Matches(text.ToLower(), @"\b\w+\b").Select(match => match.Value).Distinct() ];
    }
}
