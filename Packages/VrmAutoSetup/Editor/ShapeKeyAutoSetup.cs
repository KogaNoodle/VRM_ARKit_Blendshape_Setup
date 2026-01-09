using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRM;

public class ShapeKeyAutoSetup : EditorWindow
{
    //Made by Tracer755, Ritual neo was the one who requested this tools construction.
    //Forked by Koga Noodle, to generate the Blendshape files no matter what facetracking type the model has, adapting it to ARKit.

    GameObject Avatar = null;
    string SaveLocation = "";
    string avatarName = "";
    Vector2 scrollPos;
    int blendShapeCount = 0;
    float progress = 0f;
    private static Texture2D validTexture;
    private static Texture2D invalidTexture;
    private static Texture2D infoTexture;
    private static GUIStyle validStyle;
    private static GUIStyle invalidStyle;
    private static GUIStyle infoStyle;
    bool showBsList = false;

    Dictionary<int, string> avatarBlendshapes = new Dictionary<int, string>();
    // Dictionary based on ARKit Blendshapes and it's respective UE counterparts
    Dictionary<string, string[]> ARKitUE = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase){
        { "BrowDownLeft", new string[] { "BrowDownLeft", "BrowLowererLeft", "BrowDown" } },
        { "BrowDownRight", new string[] { "BrowDownRight", "BrowLowererRight", "BrowDown" } },
        { "BrowInnerUp", new string[] { "BrowInnerUp", "BrowUp", "BrowInnerUpRight", "BrowInnerUpLeft" } },
        { "BrowOuterUpLeft", new string[] { "BrowOuterUpLeft", "BrowUpLeft" } },
        { "BrowOuterUpRight", new string[] { "BrowOuterUpRight", "BrowUpRight" } },
        { "CheekPuff", new string[] { "CheekPuff", "CheekPuffRight", "CheekPuffLeft" } },
        { "CheekSquintLeft", new string[] { "CheekSquintLeft", "CheekSquint" } },
        { "CheekSquintRight", new string[] { "CheekSquintRight", "CheekSquint" } },
        { "EyeBlinkLeft", new string[] { "EyeBlinkLeft", "EyeClosedLeft", "EyeClosed" } },
        { "EyeBlinkRight", new string[] { "EyeBlinkRight", "EyeClosedRight", "EyeClosed" } },
        { "EyeLookDownLeft", new string[] { "EyeLookDownLeft" } },
        { "EyeLookDownRight", new string[] { "EyeLookDownRight" } },
        { "EyeLookInLeft", new string[] { "EyeLookInLeft" } },
        { "EyeLookInRight", new string[] { "EyeLookInRight" } },
        { "EyeLookOutLeft", new string[] { "EyeLookOutLeft" } },
        { "EyeLookOutRight", new string[] { "EyeLookOutRight" } },
        { "EyeLookUpLeft", new string[] { "EyeLookUpLeft" } },
        { "EyeLookUpRight", new string[] { "EyeLookUpRight" } },
        { "EyeSquintLeft", new string[] { "EyeSquintLeft", "EyeSquint" } },
        { "EyeSquintRight", new string[] { "EyeSquintRight", "EyeSquint" } },
        { "EyeWideLeft", new string[] { "EyeWideLeft", "EyeWide" } },
        { "EyeWideRight", new string[] { "EyeWideRight", "EyeWide" } },
        { "JawForward", new string[] { "JawForward" } },
        { "JawLeft", new string[] { "JawLeft" } },
        { "JawOpen", new string[] { "JawOpen", "MouthOpen" } },
        { "JawRight", new string[] { "JawRight" } },
        { "MouthClose", new string[] { "MouthClose", "MouthClosed" } },
        { "MouthDimpleLeft", new string[] { "MouthDimpleLeft", "MouthDimple" } },
        { "MouthDimpleRight", new string[] { "MouthDimpleRight", "MouthDimple" } },
        { "MouthFrownLeft", new string[] { "MouthFrownLeft", "MouthSadLeft", "MouthSad" } },
        { "MouthFrownRight", new string[] { "MouthFrownRight", "MouthSadRight", "MouthSad" } },
        { "MouthFunnel", new string[] { "MouthFunnel", "LipFunnel", "LipFunnelUpper", "LipFunnelLower", "LipFunnelUpperRight", "LipFunnelUpperLeft", "LipFunnelLowerRight", "LipFunnelLowerLeft" } },
        { "MouthLeft", new string[] { "MouthLeft" } },
        { "MouthLowerDownLeft", new string[] { "MouthLowerDownLeft", "MouthLowerDown" } },
        { "MouthLowerDownRight", new string[] { "MouthLowerDownRight", "MouthLowerDown" } },
        { "MouthPressLeft", new string[] { "MouthPressLeft", "MouthPress" } },
        { "MouthPressRight", new string[] { "MouthPressRight", "MouthPress" } },
        { "MouthPucker", new string[] { "MouthPucker", "LipPucker", "LipPuckerUpper", "LipPuckerLower", "LipPuckerUpperRight", "LipPuckerUpperLeft", "LipPuckerLowerRight", "LipPuckerLowerLeft" } },
        { "MouthRight", new string[] { "MouthRight" } },
        { "MouthRollLower", new string[] { "MouthRollLower", "LipSuckLower", "LipSuckLowerRight", "LipSuckLowerLeft", "LipSuck" } },
        { "MouthRollUpper", new string[] { "MouthRollUpper", "LipSuckUpper", "LipSuckUpperRight", "LipSuckUpperLeft", "LipSuck" } },
        { "MouthShrugLower", new string[] { "MouthShrugLower", "MouthRaiserLower" } },
        { "MouthShrugUpper", new string[] { "MouthShrugUpper", "MouthRaiserUpper" } },
        { "MouthSmileLeft", new string[] { "MouthSmileLeft", "MouthSmile" } },
        { "MouthSmileRight", new string[] { "MouthSmileRight", "MouthSmile" } },
        { "MouthStretchLeft", new string[] { "MouthStretchLeft", "MouthStretch" } },
        { "MouthStretchRight", new string[] { "MouthStretchRight", "MouthStretch" } },
        { "MouthUpperUpLeft", new string[] { "MouthUpperUpLeft", "MouthUpperUp" } },
        { "MouthUpperUpRight", new string[] { "MouthUpperUpRight", "MouthUpperUp" } },
        { "NoseSneerLeft", new string[] { "NoseSneerLeft", "NoseSneer" } },
        { "NoseSneerRight", new string[] { "NoseSneerRight", "NoseSneer" } },
        { "TongueOut", new string[] { "TongueOut" } },
        { "A", new string[] { "v_aa", "vrc.v_aa", "aa" } },
        { "E", new string[] { "v_e", "v_ee", "vrc.v_e", "vrc.v_ee", "e", "ee" } },
        { "I", new string[] { "v_ih", "vrc.v_ih", "ih" } },
        { "O", new string[] { "v_oh", "vrc.v_oh", "oh" } },
        { "U", new string[] { "v_ou", "vrc.v_ou", "ou" } },
        { "SIL", new string[] { "v_sil", "vrc.v_sil", "sil" } },
        { "CH", new string[] { "v_ch", "vrc.v_ch", "ch" } },
        { "DD", new string[] { "v_dd", "vrc.v_dd", "dd" } },
        { "FF", new string[] { "v_ff", "vrc.v_ff", "ff" } },
        { "KK", new string[] { "v_kk", "vrc.v_kk", "kk" } },
        { "NN", new string[] { "v_nn", "vrc.v_nn", "nn" } },
        { "PP", new string[] { "v_pp", "vrc.v_pp", "pp" } },
        { "RR", new string[] { "v_rr", "vrc.v_rr", "rr" } },
        { "SS", new string[] { "v_ss", "vrc.v_ss", "ss" } },
        { "TH", new string[] { "v_th", "vrc.v_th", "th" } },
        { "Blink_L", new string[] { "LeftBlink", "Blink", "EyeBlinkLeft", "EyeClosedLeft", "EyeClosed" } },
        { "Blink_R", new string[] { "RightBlink", "Blink", "EyeBlinkRight", "EyeClosedRight", "EyeClosed" } },
        { "Blink", new string[] { "Blink", "EyeBlinkLeft", "EyeClosedLeft", "EyeClosed", "EyeBlinkRight", "EyeClosedRight", "EyeClosed" } }
    };

    NamePreset[] NamePresets = new NamePreset[] { new NamePreset { name="A", blendShapePreset=BlendShapePreset.A}, new NamePreset { name = "E", blendShapePreset = BlendShapePreset.E }, new NamePreset { name = "I", blendShapePreset = BlendShapePreset.I }, new NamePreset { name = "O", blendShapePreset = BlendShapePreset.O }, new NamePreset { name = "U", blendShapePreset = BlendShapePreset.U }, new NamePreset { name = "Blink", blendShapePreset = BlendShapePreset.Blink }, new NamePreset { name = "Blink_R", blendShapePreset = BlendShapePreset.Blink_R }, new NamePreset { name = "Blink_L", blendShapePreset = BlendShapePreset.Blink_L } };
    [MenuItem("Tools/VRM/Blendshapes Auto Setup")]
    
    public static void ShowWindow() {
        GetWindow(typeof(ShapeKeyAutoSetup));
    }

    private void OnGUI()
    {
        SaveLocation = "Assets/!VRM Blendshapes/";
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.richText = true;
        style.wordWrap = true;
        if (validTexture == null) {
            validTexture = MakeTex(1, 1, new Color(0.0f, 0.57f, 0.57f, 1.0f));
            validStyle = new GUIStyle();
            validStyle.normal.background = validTexture;
            validStyle.wordWrap = true;
            validStyle.richText = true;
            validStyle.alignment = TextAnchor.MiddleCenter;
        }

        if (invalidTexture == null) {
            invalidTexture = MakeTex(1, 1, new Color(0.57f, 0.0f, 0.0f, 1.0f));
            invalidStyle = new GUIStyle();
            invalidStyle.normal.background = invalidTexture;
            invalidStyle.wordWrap = true;
            invalidStyle.richText = true;
            invalidStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        if (infoTexture == null) {
            infoTexture = MakeTex(1, 1, new Color(0.0f, 0.42f, 0.85f, 1.0f));
            infoStyle = new GUIStyle();
            infoStyle.normal.background = infoTexture;
            infoStyle.wordWrap = true;
            infoStyle.richText = true;
            infoStyle.alignment = TextAnchor.MiddleCenter;
        }

        bool buttonEnabled = true;

        EditorGUILayout.LabelField("<size=16><b>VRM ARKit Blendshapes Auto Setup</b></size>", style);
        EditorGUILayout.LabelField("\n\n", EditorStyles.whiteLabel);
        EditorGUILayout.BeginVertical(infoStyle);
        GUILayout.Label("<size=12> ℹ️  <b>Avatar Name</b> will be used to name the files generated.</size>", style);
        EditorGUILayout.EndVertical();
        avatarName = EditorGUILayout.TextField("Avatar Name", avatarName);

        if (avatarName == "") {
            EditorGUILayout.BeginVertical(invalidStyle);
            GUILayout.Label("<size=12><b> ⚠  Please give the avatar a name</b></size>", style);
            EditorGUILayout.EndVertical();
            buttonEnabled = false;
        }
        EditorGUILayout.LabelField("\n\n", EditorStyles.whiteLabel);
        
        try {
            EditorGUILayout.BeginVertical(infoStyle);
            GUILayout.Label("<size=12> ℹ️  <b>Blendshapes Object</b> should contain the Facetracking Blendshapes. Usually it's the <b>\"Body\"</b> object in your avatar, but it may be different from model to model.</size>", style);
            EditorGUILayout.EndVertical();
            Avatar = EditorGUILayout.ObjectField("Blendshapes Object", Avatar, typeof(GameObject), true) as GameObject;

        if (Avatar == null) {
            EditorGUILayout.BeginVertical(invalidStyle);
            GUILayout.Label("<size=12><b> ⚠  Please select an object</b></size>", style);
            EditorGUILayout.EndVertical();
            buttonEnabled = false;
        } else if (blendShapeCount == 0) {
            EditorGUILayout.BeginVertical(invalidStyle);
            GUILayout.Label("<size=12><b> ⚠  Please select an object with valid Facetracking Blendshapes</b></size>", style);
            EditorGUILayout.EndVertical();
            buttonEnabled = false;
        }
        } catch (UnityEngine.ExitGUIException) {
            //Here so unity dosn't freak out
        } catch (System.Exception e) {
            throw (e);
        }

        EditorGUILayout.LabelField("\n\n", EditorStyles.boldLabel);
        
        if (buttonEnabled) {
            EditorGUILayout.BeginVertical(validStyle);
            GUILayout.Label($"<size=12><b> ✔  You are all set!</b></size>", style);
            EditorGUILayout.EndHorizontal();
        }
        
        GUI.enabled = buttonEnabled;
        var setupBtn = GUILayout.Button("Generate!");
        GUI.enabled = true;
        EditorGUILayout.LabelField("\n\n", EditorStyles.boldLabel);

        var endsWithLR = new Regex(@"Left|Right|_[LR]$");

        if (Avatar != null) {
            try {
                var shared_mesh = Avatar.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                int count = 0;
                string tmpMsg = "";
                string clipboard = "";
                bool latch = true;

                for(int i = 0; i < shared_mesh.blendShapeCount; i++) {
                    avatarBlendshapes[i] = shared_mesh.GetBlendShapeName(i);
                }

                foreach (KeyValuePair<string, string[]> ARKitItem in ARKitUE) {
                    string blendshapeMatch = null;
                    foreach (string blendshape in ARKitItem.Value) {
                        var bsCheck = avatarBlendshapes.FirstOrDefault(bs => bs.Value.ToLower() == blendshape.ToLower());

                        if (bsCheck.Value != null) {
                            blendshapeMatch = blendshape;
                            if (!endsWithLR.IsMatch(ARKitItem.Key) && endsWithLR.IsMatch(bsCheck.Value)) {
                                bool isLeft = bsCheck.Value.EndsWith("Left");
                                blendshapeMatch += $", {blendshape.Replace(isLeft ? "Left" : "Right", isLeft ? "Right" : "Left")}";
                            }
                            count++;
                            break;
                        }
                    }

                    tmpMsg += latch ? "" : $"<b>[{ARKitItem.Key}]</b> ➡ {blendshapeMatch}\n";
                    clipboard += latch ? "" : $"[{ARKitItem.Key}] => {blendshapeMatch}\n";
                    latch = false;
                }

                EditorGUILayout.BeginVertical(infoStyle);
                GUILayout.Label($"<size=15><b> ℹ️  {shared_mesh.blendShapeCount}</b></size> BlendShapes detected which <size=15><b>{count}</b></size> are valid VRM shapes.", style);
                EditorGUILayout.EndVertical();

                showBsList = EditorGUILayout.Foldout(showBsList, $"Blendshapes List ({count})", true);

                if (showBsList) {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                    var copyBtn = GUILayout.Button("Copy list to clipboard!");
                    GUILayout.Label($"<size=12>{tmpMsg}</size>", style);
                    EditorGUILayout.EndScrollView();

                    if (copyBtn) {
                        GUIUtility.systemCopyBuffer = clipboard;
                        EditorUtility.DisplayDialog(
                            "Confirm Action", // Title
                            "Blendshape list was succesfully copied to your clipboard!", // Message
                            "OK"
                        );
                    }
                }

                blendShapeCount = count;
            } catch {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                GUILayout.Label("No valid blend shapes detected on current avatar", EditorStyles.boldLabel);
                EditorGUILayout.EndScrollView();
                blendShapeCount = 0;
            }
        } else {
            blendShapeCount = 0;
        }

        if (setupBtn) {
            progress = 0f;
            EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", "Starting", progress);
            var shared_mesh = Avatar.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            List<string> BlendShape = new List<string>();

            if (!Directory.Exists(SaveLocation + $@"/{avatarName}_Clips")) {
                Directory.CreateDirectory(SaveLocation + $@"/{avatarName}_Clips");
            } else {
                Directory.Delete(SaveLocation + $@"/{avatarName}_Clips", true);
                Directory.CreateDirectory(SaveLocation + $@"/{avatarName}_Clips");
            }
            //check for a avatar file that already exitsts
            if (File.Exists(Directory.GetCurrentDirectory() + @"/" + SaveLocation + "/" + avatarName + "_AvatarBlendShape.asset")) {
                File.Delete(Directory.GetCurrentDirectory() + @"/" + SaveLocation + "/" + avatarName + "_AvatarBlendShape.asset");
                File.Delete(Directory.GetCurrentDirectory() + @"/" + SaveLocation + "/" + avatarName + "_AvatarBlendShape.asset.meta");
            }
            AssetDatabase.Refresh();
            //Generate Clips
            float tempProgValue = (float)(.8 / blendShapeCount);
            int i = 0;

            foreach (KeyValuePair<string, string[]> ARKitItem in ARKitUE) {
                foreach (string blendshape in ARKitItem.Value) {                
                    var bsCheck = avatarBlendshapes.FirstOrDefault(bs => bs.Value.ToLower() == blendshape.ToLower());

                    if (bsCheck.Value != null) {
                        bool isComboBs = !endsWithLR.IsMatch(ARKitItem.Key) && endsWithLR.IsMatch(bsCheck.Value);
                        progress += tempProgValue;
                        EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Generating {ARKitItem.Key}", progress);
                        BlendShape.Add(ARKitItem.Key);
                        var Clip = ScriptableObject.CreateInstance<BlendShapeClip>();
                        foreach(NamePreset obj in NamePresets) {
                            if(obj.name == ARKitItem.Key) {
                                Clip.Preset = obj.blendShapePreset;
                            }
                        }
                        string path = SaveLocation + $@"/{avatarName}_Clips/" + ARKitItem.Key + ".asset";
                        Clip.BlendShapeName = ARKitItem.Key;
                        var Data = new VRM.BlendShapeBinding();
                        Data.Weight = 100;
                        Data.RelativePath = Avatar.name;
                        Data.Index = bsCheck.Key;
                        var array = new VRM.BlendShapeBinding[isComboBs ? 2 : 1];
                        array[0] = Data;

                        if (isComboBs) {
                            bool isLeft = bsCheck.Value.EndsWith("Left");
                            var bsCheck2 = avatarBlendshapes.FirstOrDefault(bs => bs.Value.ToLower() == blendshape.Replace(isLeft ? "Left" : "Right", isLeft ? "Right" : "Left").ToLower());

                            if (bsCheck.Value != null) {
                                var Data2 = new VRM.BlendShapeBinding();
                                Data2.Weight = 100;
                                Data2.RelativePath = Avatar.name;
                                Data2.Index = bsCheck2.Key;
                                array[1] = Data2;
                            }
                        }

                        Clip.Values = array;
                        AssetDatabase.CreateAsset(Clip, path);
                        i++;
                        break;
                    }
                }
            }

            //Generate the avatar blendshape file before the assetdatabase refresh
            var AvatarData = ScriptableObject.CreateInstance<BlendShapeAvatar>();
            AssetDatabase.CreateAsset(AvatarData, SaveLocation + "/" + avatarName + "_AvatarBlendShape.asset");

            //Generate a neutral clip
            EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Generating Neural shape", progress += tempProgValue);
            BlendShape.Insert(0, "Neutral");
            var Clip2 = ScriptableObject.CreateInstance<BlendShapeClip>();
            Clip2.Preset = BlendShapePreset.Neutral;
            Clip2.BlendShapeName = "Neutral";
            AssetDatabase.CreateAsset(Clip2, SaveLocation + $"/{avatarName}_Clips/Neutral.asset");
            AssetDatabase.Refresh();

            //Add clips to a Blend Shape Avatar
            EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Adding shapes to VRM avatar file", .9f);
            List<string> guids = new List<string>();
            string path3 = SaveLocation + $"/{avatarName}_AvatarBlendShape.asset";
            StreamReader sr = new StreamReader(path3);
            string TmpData = sr.ReadToEnd();
            sr.Close();
            bool latch = true;
            TmpData = TmpData.Replace("  Clips: []", "  Clips:");

            foreach (var obj in BlendShape) {
                string[] lines = System.IO.File.ReadAllLines(SaveLocation + $@"/{avatarName}_Clips/" + obj + ".asset.meta");
                TmpData += $"{(!latch ? "\n" : "")}  - {{fileID:\"{Convert.ToInt64(lines[4].Split(':')[1].Trim())}\", guid: \"{lines[1].Split(' ')[1].Trim()}\", type: 2}}";
                latch = false;
            }

            EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Finishing up", 1f);
            StreamWriter streamWriter = new StreamWriter(path3);
            streamWriter.Write(TmpData);
            streamWriter.Close();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Sucessfully created: {BlendShape.Count} VRM keys for avatar: {avatarName}");
            EditorGUIUtility.PingObject(AvatarData);
            EditorUtility.ClearProgressBar();
        }
        this.Repaint();
    }

    // Helper function to create a solid color Texture2D
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
public class ClipValue
{
    public string RelativePath = "Body";
    public int Index;
    public float Weight;
}
class NamePreset
{
    public string name;
    public BlendShapePreset blendShapePreset;
}