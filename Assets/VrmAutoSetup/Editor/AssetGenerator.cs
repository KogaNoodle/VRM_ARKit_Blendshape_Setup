using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRM;
using VrmAutoSetup.Editor.Models;

namespace VrmAutoSetup.Editor
{
    public class AssetGenerator
    {
        private const string DefaultSaveLocation = "Assets/!VRM Blendshapes/";
        
        private readonly BlendshapeProcessor _processor;
        
        public AssetGenerator(BlendshapeProcessor processor)
        {
            _processor = processor;
        }
        
        public BlendShapeAvatar Generate(List<BlendshapeResult> results, string avatarName, GameObject avatar)
        {
            string clipsPath = Path.Combine(DefaultSaveLocation, $"{avatarName}_Clips");
            
            EnsureDirectoryExists(clipsPath);
            
            var clipAssets = new List<BlendShapeClip>();
            
            foreach (var result in results)
            {
                var clip = CreateBlendshapeClip(result, avatar);
                string path = Path.Combine(clipsPath, $"{result.ARKitKey}.asset");
                AssetDatabase.CreateAsset(clip, path);
                clipAssets.Add(clip);
            }
            
            var neutralClip = ScriptableObject.CreateInstance<BlendShapeClip>();
            neutralClip.Preset = BlendShapePreset.Neutral;
            neutralClip.BlendShapeName = "Neutral";
            AssetDatabase.CreateAsset(neutralClip, Path.Combine(clipsPath, "Neutral.asset"));
            AssetDatabase.Refresh();
            
            var avatarData = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            string avatarPath = Path.Combine(DefaultSaveLocation, $"{avatarName}_AvatarBlendShape.asset");
            AssetDatabase.CreateAsset(avatarData, avatarPath);
            
            avatarData.Clips = new List<BlendShapeClip>(clipAssets) { neutralClip };
            EditorUtility.SetDirty(avatarData);
            AssetDatabase.Refresh();
            
            return avatarData;
        }
        
        private BlendShapeClip CreateBlendshapeClip(BlendshapeResult result, GameObject avatar)
        {
            var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            clip.BlendShapeName = result.ARKitKey;
            
            foreach (var preset in NamePresets.GetAll())
            {
                if (preset.Name == result.ARKitKey)
                {
                    clip.Preset = preset.BlendShapePreset;
                    break;
                }
            }
            
            var bindings = new List<BlendShapeBinding>
            {
                new BlendShapeBinding
                {
                    RelativePath = avatar.name,
                    Index = result.Index,
                    Weight = 100f
                }
            };
            
            if (result.IsComboBlendshape)
            {
                bool isLeft = result.MatchedName.EndsWith("Left");
                string oppositeName = result.MatchedName.Replace(isLeft ? "Left" : "Right", 
                    isLeft ? "Right" : "Left");
                int oppositeIndex = _processor.FindMatchingIndex(oppositeName);
                
                if (oppositeIndex >= 0)
                {
                    bindings.Add(new BlendShapeBinding
                    {
                        RelativePath = avatar.name,
                        Index = oppositeIndex,
                        Weight = 100f
                    });
                }
            }
            
            clip.Values = bindings.ToArray();
            return clip;
        }
        
        private void EnsureDirectoryExists(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
        }
    }
}