using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using VrmAutoSetup.Editor.Models;

namespace VrmAutoSetup.Editor
{
    public class BlendshapeProcessor
    {
        private static readonly Regex EndsWithLR = new Regex(@"Left|Right|_[LR]$", RegexOptions.Compiled);
        
        private readonly Dictionary<int, string> _avatarBlendshapes = new Dictionary<int, string>();
        private GameObject _avatar;
        private SkinnedMeshRenderer _renderer;
        
        public void SetAvatar(GameObject avatar)
        {
            _avatar = avatar;
            _renderer = avatar?.GetComponent<SkinnedMeshRenderer>();
            LoadBlendshapes();
        }
        
        private void LoadBlendshapes()
        {
            _avatarBlendshapes.Clear();
            if (_renderer?.sharedMesh == null) return;
            
            var mesh = _renderer.sharedMesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                _avatarBlendshapes[i] = mesh.GetBlendShapeName(i);
            }
        }
        
        public int BlendshapeCount => _avatarBlendshapes.Count;
        
        public IReadOnlyDictionary<int, string> GetBlendshapeDict() => _avatarBlendshapes;
        
        public string GetAvatarName() => _avatar?.name ?? "";
        
        public List<BlendshapeResult> FindAllBlendshapes()
        {
            var results = new List<BlendshapeResult>();
            var processedIndices = new HashSet<int>();
            
            foreach (var arkitItem in ARKitBlendshapes.GetAll())
            {
                var candidates = arkitItem.AlternativeNames.ToList();
                
                string bestMatch = null;
                int bestIndex = -1;
                MatchQuality bestQuality = MatchQuality.None;
                float bestConfidence = 0f;
                
                foreach (string candidateName in candidates)
                {
                    foreach (var avatarBs in _avatarBlendshapes)
                    {
                        if (processedIndices.Contains(avatarBs.Key)) continue;
                        
                        var (matchedName, quality, confidence) = 
                            StringFuzzyMatcher.FindBestMatch(candidateName, new[] { avatarBs.Value });
                        
                        if (quality > bestQuality || (quality == bestQuality && confidence > bestConfidence))
                        {
                            bestMatch = avatarBs.Value;
                            bestIndex = avatarBs.Key;
                            bestQuality = quality;
                            bestConfidence = confidence;
                        }
                    }
                }
                
                if (bestMatch != null)
                {
                    processedIndices.Add(bestIndex);
                    
                    bool isCombo = !EndsWithLR.IsMatch(arkitItem.ARKitName) 
                                   && EndsWithLR.IsMatch(bestMatch);
                    
                    results.Add(new BlendshapeResult(
                        arkitItem.ARKitName, 
                        bestMatch, 
                        bestIndex,
                        bestQuality,
                        bestConfidence,
                        isCombo));
                }
            }
            
            return results;
        }
        
        public List<string> GetUnmatchedBlendshapes(List<BlendshapeResult> results)
        {
            var matchedIndices = new HashSet<int>(results.Where(r => r.IsMatched).Select(r => r.Index));
            var unmatched = new List<string>();
            
            foreach (var bs in _avatarBlendshapes)
            {
                if (!matchedIndices.Contains(bs.Key))
                {
                    unmatched.Add(bs.Value);
                }
            }
            
            return unmatched;
        }
        
        public int MatchedCount(List<BlendshapeResult> results) => 
            results.Count(r => r.IsMatched);
        
        public int FindMatchingIndex(string blendshapeName)
        {
            var match = _avatarBlendshapes.FirstOrDefault(
                bs => bs.Value.Equals(blendshapeName, StringComparison.OrdinalIgnoreCase));
            return match.Value != null ? match.Key : -1;
        }
    }
}