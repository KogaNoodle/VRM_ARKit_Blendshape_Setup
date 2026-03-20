namespace VrmAutoSetup.Editor.Models
{
    public class BlendshapeMapping
    {
        public string ARKitName;
        public string[] AlternativeNames;
        
        public BlendshapeMapping(string arkitsName, params string[] alternatives)
        {
            ARKitName = arkitsName;
            AlternativeNames = alternatives;
        }
    }
}