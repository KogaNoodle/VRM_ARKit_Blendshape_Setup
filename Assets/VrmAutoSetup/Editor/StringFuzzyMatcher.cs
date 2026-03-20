using System;
using System.Collections.Generic;
using System.Linq;
using VrmAutoSetup.Editor.Models;

namespace VrmAutoSetup.Editor
{
    public static class StringFuzzyMatcher
    {
        public static (string matchedName, MatchQuality quality, float confidence) FindBestMatch(
            string target, 
            IEnumerable<string> candidates)
        {
            var candidateList = candidates.ToList();
            
            // Stage 1: Exact match (case-insensitive) - fast path
            var exact = candidateList.FirstOrDefault(c => 
                c.Equals(target, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return (exact, MatchQuality.Exact, 1.0f);
            
            // Stage 2: Substring matching
            var substring = candidateList.FirstOrDefault(c => 
                c.Contains(target, StringComparison.OrdinalIgnoreCase) ||
                target.Contains(c, StringComparison.OrdinalIgnoreCase));
            if (substring != null) return (substring, MatchQuality.Substring, 0.85f);
            
            // Stage 3: Levenshtein distance (75% threshold)
            string bestFuzzy = null;
            float bestScore = 0.75f;
            
            foreach (var candidate in candidateList)
            {
                float score = CalculateSimilarity(target, candidate);
                if (score >= bestScore && (bestFuzzy == null || score > bestScore))
                {
                    bestScore = score;
                    bestFuzzy = candidate;
                }
            }
            
            if (bestFuzzy != null) return (bestFuzzy, MatchQuality.Fuzzy, bestScore);
            
            return (null, MatchQuality.None, 0f);
        }
        
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        
        private static float CalculateSimilarity(string s1, string s2)
        {
            int distance = LevenshteinDistance(s1.ToLower(), s2.ToLower());
            int maxLen = Math.Max(s1.Length, s2.Length);
            return maxLen == 0 ? 1.0f : 1.0f - (float)distance / maxLen;
        }
        
        private static int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];
            
            for (int i = 0; i <= s1.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) d[0, j] = j;
            
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            
            return d[s1.Length, s2.Length];
        }
    }
}