namespace VrmAutoSetup.Editor.Models
{
    public enum MatchQuality { None, Fuzzy, Substring, Exact }
    
    public class BlendshapeResult
    {
        public string ARKitKey;
        public string MatchedName;
        public int Index;
        public bool IsComboBlendshape;
        public MatchQuality Quality;
        public float ConfidenceScore;
        public bool IsMatched => Quality != MatchQuality.None;
        
        public BlendshapeResult(string arkitKey, string matchedName, int index, 
            MatchQuality quality, float confidenceScore, bool isCombo = false)
        {
            ARKitKey = arkitKey;
            MatchedName = matchedName;
            Index = index;
            IsComboBlendshape = isCombo;
            Quality = quality;
            ConfidenceScore = confidenceScore;
        }
        
        public static BlendshapeResult Unmatched(string arkitKey)
        {
            return new BlendshapeResult(arkitKey, null, -1, MatchQuality.None, 0f, false);
        }
    }
}