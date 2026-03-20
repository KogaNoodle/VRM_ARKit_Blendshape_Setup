using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
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
    private static GUIStyle headerStyle;
    private static GUIStyle subtitleStyle;
    private static GUIStyle sectionHeaderStyle;
    private static GUIStyle foldoutHeaderStyle;
    private static GUIStyle listItemStyle;
    private static GUIStyle btnStyle;
    private static GUIStyle overrideBtnStyleActive;
    private static GUIStyle overrideBtnStyleInactive;
    private static GUIStyle copyBtnStyle;
    
    // Icons
    private GUIContent infoIcon;
    private GUIContent targetIcon;
    private GUIContent statusIcon;

    bool showBsList = false;
    GameObject lastAvatar = null;
    string[] avatarBlendshapeNames = new string[0];
    Dictionary<string, BlendshapeMappingState> mappingStates = new Dictionary<string, BlendshapeMappingState>();

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

    LocalNamePreset[] NamePresets = new LocalNamePreset[] { new LocalNamePreset { name="A", blendShapePreset=BlendShapePreset.A}, new LocalNamePreset { name = "E", blendShapePreset = BlendShapePreset.E }, new LocalNamePreset { name = "I", blendShapePreset = BlendShapePreset.I }, new LocalNamePreset { name = "O", blendShapePreset = BlendShapePreset.O }, new LocalNamePreset { name = "U", blendShapePreset = BlendShapePreset.U }, new LocalNamePreset { name = "Blink", blendShapePreset = BlendShapePreset.Blink }, new LocalNamePreset { name = "Blink_R", blendShapePreset = BlendShapePreset.Blink_R }, new LocalNamePreset { name = "Blink_L", blendShapePreset = BlendShapePreset.Blink_L } };
    
    [MenuItem("Vtuber/Blendshapes Auto Setup")]
    public static void ShowWindow() {
        var window = GetWindow<ShapeKeyAutoSetup>("ARKit Setup");
        window.minSize = new Vector2(400, 550);
        window.Show();
    }

    private void InitializeStyles()
    {
        if (headerStyle != null) return; // Already initialized

        headerStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.85f, 0.85f, 0.85f) },
            margin = new RectOffset(0, 0, 10, 5)
        };

        subtitleStyle = new GUIStyle(EditorStyles.label) {
            fontSize = 12,
            richText = true,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.55f, 0.55f, 0.55f) },
            margin = new RectOffset(0, 0, 0, 15)
        };

        sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 14,
            richText = true,
            normal = { textColor = new Color(0.85f, 0.85f, 0.85f) },
            margin = new RectOffset(0, 0, 0, 5)
        };

        listItemStyle = new GUIStyle(EditorStyles.label) {
            richText = true,
            fontSize = 12,
            margin = new RectOffset(5, 5, 2, 2)
        };

        btnStyle = new GUIStyle(GUI.skin.button) {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(20, 20, 20, 20)
        };

        overrideBtnStyleActive = new GUIStyle(GUI.skin.button) {
            normal = { textColor = Color.black },
            fontStyle = FontStyle.Bold,
            fixedWidth = 75
        };

        overrideBtnStyleInactive = new GUIStyle(GUI.skin.button) {
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
            fixedWidth = 75
        };

        copyBtnStyle = new GUIStyle(GUI.skin.button) {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(10, 10, 10, 10)
        };

        // Load built-in Unity icons
        infoIcon = EditorGUIUtility.IconContent("d_console.infoicon.sml");
        targetIcon = EditorGUIUtility.IconContent("d_AvatarSelector");
        statusIcon = EditorGUIUtility.IconContent("d_ViewToolOrbit");
    }

    private void OnGUI()
    {
        try {
            InitializeStyles();

            SaveLocation = "Assets/!VRM Blendshapes/";

            GUILayout.Space(15);
            GUILayout.Label("ARKit Blendshape Auto Setup", headerStyle);
            GUILayout.Label("Seamlessly merge <b>ARKit facetracking</b> into your avatar with just a few clicks.", subtitleStyle);
            
            // Native horizontal line separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);

            bool buttonEnabled = true;
            
            // Ensure nice label alignment like the reference image
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 120f;

            // --- SECTION 1: Basic Information ---
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            GUILayout.Label(new GUIContent(" Basic Information", infoIcon.image), sectionHeaderStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            avatarName = EditorGUILayout.TextField("Avatar Name", avatarName);
            GUILayout.Space(12);
            GUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(avatarName)) {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Space(12);
                EditorGUILayout.HelpBox("Please give the avatar a name to proceed.", MessageType.Error);
                GUILayout.Space(12);
                GUILayout.EndHorizontal();
                buttonEnabled = false;
            }
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            // --- SECTION 2: Target Setup ---
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            GUILayout.Label(new GUIContent(" Target Setup", targetIcon.image), sectionHeaderStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            try {
                Avatar = EditorGUILayout.ObjectField("Blendshapes Object", Avatar, typeof(GameObject), true) as GameObject;
                
                if (Avatar != lastAvatar) {
                    lastAvatar = Avatar;
                    DetectBlendshapes();
                }

            } catch (UnityEngine.ExitGUIException) {
                // Unity internal
            } catch (System.Exception e) {
                if (e is ArgumentException) GUIUtility.ExitGUI(); else throw;
            }
            GUILayout.Space(12);
            GUILayout.EndHorizontal();

            if (Avatar == null) {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Space(12);
                EditorGUILayout.HelpBox("Please select an object containing your blendshapes (Usually 'Body').", MessageType.Warning);
                GUILayout.Space(12);
                GUILayout.EndHorizontal();
                buttonEnabled = false;
            } else if (blendShapeCount == 0 && mappingStates.Count == 0) {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Space(12);
                EditorGUILayout.HelpBox("The selected object has no valid Facetracking Blendshapes.", MessageType.Error);
                GUILayout.Space(12);
                GUILayout.EndHorizontal();
                buttonEnabled = false;
            }
            
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            // --- SECTION 3: Detection Status ---
            if (Avatar != null && mappingStates.Count > 0) {
                int validCount = mappingStates.Values.Count(m => m.IsOverridden ? m.OverrideSelectedIndex > 0 : m.HasMatch);
                
                EditorGUILayout.BeginVertical("HelpBox", GUILayout.ExpandHeight(showBsList));
                
                // Header Row for Section 3
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                
                // Icon + Label
                GUILayout.Label(new GUIContent(" Detection Status", statusIcon.image), sectionHeaderStyle);
                
                GUILayout.FlexibleSpace();
                
                // Right-aligned custom arrow
                GUILayout.Label(showBsList ? "▼" : "▶", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } }, GUILayout.Width(20));
                GUILayout.Space(8);
                
                GUILayout.EndHorizontal();

                // Make the entire header row clickable
                Rect headerRect = GUILayoutUtility.GetLastRect();
                headerRect.y -= 20; 
                headerRect.height += 20;
                EditorGUIUtility.AddCursorRect(headerRect, MouseCursor.Link);
                
                if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition)) {
                    showBsList = !showBsList;
                    Event.current.Use(); 
                    GUI.FocusControl(null); // Remove focus to prevent ghost highlighting
                }

                GUILayout.Space(5);

                // Information Box (Always Visible)
                GUILayout.BeginHorizontal();
                GUILayout.Space(12);
                EditorGUILayout.HelpBox($"Detected {avatarBlendshapes.Count} total blendshapes. {validCount} are mapped for ARKit.", MessageType.Info);
                GUILayout.Space(12);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);

                // Blendshapes List (Collapsible)
                if (showBsList) {
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box, GUILayout.ExpandHeight(true));
                    
                    string clipboard = "";
                    foreach (var kvp in mappingStates) {
                        var state = kvp.Value;
                        GUILayout.BeginHorizontal();
                        
                        bool isResolved = state.IsOverridden ? (state.OverrideSelectedIndex > 0) : state.HasMatch;
                        string colorHex = isResolved ? "#88ff88" : "#aaaaaa";
                        GUILayout.Label($"<color={colorHex}><b>{state.ArkitKey}</b></color>", listItemStyle, GUILayout.Width(130));
                        
                        GUI.enabled = state.IsOverridden;
                        state.OverrideSelectedIndex = EditorGUILayout.Popup(state.OverrideSelectedIndex, avatarBlendshapeNames, GUILayout.ExpandWidth(true));
                        GUI.enabled = true;
                        
                        bool prevOverride = state.IsOverridden;
                        
                        var oldColorOverride = GUI.backgroundColor;
                        if (state.IsOverridden) {
                            GUI.backgroundColor = new Color(0.9f, 0.8f, 0.2f); // Yellowish
                        }
                        bool overrideClicked = GUILayout.Button("Override", state.IsOverridden ? overrideBtnStyleActive : overrideBtnStyleInactive);
                        GUI.backgroundColor = oldColorOverride;
                        
                        if (overrideClicked) {
                            state.IsOverridden = !state.IsOverridden;
                            if (!state.IsOverridden) {
                                state.OverrideSelectedIndex = state.HasMatch ? state.AutoMatchIndex1 + 1 : 0;
                            }
                        }

                        GUILayout.EndHorizontal();

                        if (isResolved) {
                            string resolvedName = state.IsOverridden ? avatarBlendshapeNames[state.OverrideSelectedIndex] : state.AutoMatchDisplay;
                            clipboard += $"[{state.ArkitKey}] => {resolvedName}\n";
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.Space(12);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    var oldColorCopy = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                    if (GUILayout.Button("Copy to Clipboard", copyBtnStyle)) {
                        GUIUtility.systemCopyBuffer = clipboard;
                        EditorUtility.DisplayDialog("Copied", "Blendshape list copied to clipboard!", "OK");
                        GUIUtility.ExitGUI();
                    }
                    GUI.backgroundColor = oldColorCopy;
                    GUILayout.Space(12);
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                } // end if (showBsList)
                EditorGUILayout.EndVertical();
            } else {
                blendShapeCount = 0;
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;

            if (buttonEnabled) {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("All checks passed! You are ready to generate your ARKit assets.", MessageType.Info);
            }

            // Generate Button
            GUI.enabled = buttonEnabled;
            var oldColor2 = GUI.backgroundColor;
            GUI.backgroundColor = buttonEnabled ? new Color(0.2f, 0.6f, 0.9f) : Color.grey;
            var setupBtn = GUILayout.Button("Generate Assets", btnStyle);
            GUI.backgroundColor = oldColor2;
            GUI.enabled = true;

            if (setupBtn) {
                RunGeneration();
            }
            
            this.Repaint();
        } catch (ArgumentException) {
            // Suppress the layout mismatch exception that happens during Hot Reload
            GUIUtility.ExitGUI();
        }
    }

    private void RunGeneration() {
        bool hasOverrides = mappingStates.Values.Any(m => m.IsOverridden);
        if (hasOverrides) {
            if (!EditorUtility.DisplayDialog("Confirm Overrides", "You have manually overridden some blendshapes. Are you sure you want to proceed?", "Yes", "Cancel")) {
                return;
            }
        }

        progress = 0f;
        EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", "Starting...", progress);
        var shared_mesh = Avatar.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        List<string> BlendShape = new List<string>();

        string clipsFolder = SaveLocation + $@"/{avatarName}_Clips";
        if (!Directory.Exists(clipsFolder)) {
            Directory.CreateDirectory(clipsFolder);
        } else {
            Directory.Delete(clipsFolder, true);
            Directory.CreateDirectory(clipsFolder);
        }

        string avatarAssetPath = Directory.GetCurrentDirectory() + @"/" + SaveLocation + "/" + avatarName + "_AvatarBlendShape.asset";
        if (File.Exists(avatarAssetPath)) {
            File.Delete(avatarAssetPath);
            File.Delete(avatarAssetPath + ".meta");
        }
        AssetDatabase.Refresh();

        int validCount = mappingStates.Values.Count(m => m.IsOverridden ? m.OverrideSelectedIndex > 0 : m.HasMatch);
        float tempProgValue = (float)(0.8f / (validCount == 0 ? 1 : validCount));
        int i = 0;

        foreach (var kvp in mappingStates) {
            var state = kvp.Value;
            bool willGenerate = state.IsOverridden ? (state.OverrideSelectedIndex > 0) : state.HasMatch;
            if (!willGenerate) continue;

            progress += tempProgValue;
            EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Generating {state.ArkitKey}...", progress);
            BlendShape.Add(state.ArkitKey);
            
            var Clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            foreach(LocalNamePreset obj in NamePresets) {
                if(obj.name == state.ArkitKey) {
                    Clip.Preset = obj.blendShapePreset;
                }
            }

            string path = SaveLocation + $@"/{avatarName}_Clips/" + state.ArkitKey + ".asset";
            Clip.BlendShapeName = state.ArkitKey;
            
            if (state.IsOverridden) {
                int mappedIndex = state.OverrideSelectedIndex - 1; // -1 for "None" offset
                var Data = new VRM.BlendShapeBinding {
                    Weight = 100,
                    RelativePath = Avatar.name,
                    Index = mappedIndex
                };
                Clip.Values = new VRM.BlendShapeBinding[] { Data };
            } else {
                var Data1 = new VRM.BlendShapeBinding {
                    Weight = 100,
                    RelativePath = Avatar.name,
                    Index = state.AutoMatchIndex1
                };

                if (state.AutoMatchIndex2 != -1) {
                    var Data2 = new VRM.BlendShapeBinding {
                        Weight = 100,
                        RelativePath = Avatar.name,
                        Index = state.AutoMatchIndex2
                    };
                    Clip.Values = new VRM.BlendShapeBinding[] { Data1, Data2 };
                } else {
                    Clip.Values = new VRM.BlendShapeBinding[] { Data1 };
                }
            }

            AssetDatabase.CreateAsset(Clip, path);
            i++;
        }

        // Avatar file
        var AvatarData = ScriptableObject.CreateInstance<BlendShapeAvatar>();
        AssetDatabase.CreateAsset(AvatarData, SaveLocation + "/" + avatarName + "_AvatarBlendShape.asset");

        // Neutral clip
        EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Generating Neutral shape...", progress += tempProgValue);
        BlendShape.Insert(0, "Neutral");
        var Clip2 = ScriptableObject.CreateInstance<BlendShapeClip>();
        Clip2.Preset = BlendShapePreset.Neutral;
        Clip2.BlendShapeName = "Neutral";
        AssetDatabase.CreateAsset(Clip2, SaveLocation + $"/{avatarName}_Clips/Neutral.asset");
        AssetDatabase.Refresh();

        // Blend Shape Avatar Injection
        EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Finalizing VRM Avatar asset...", 0.9f);
        string path3 = SaveLocation + $"/{avatarName}_AvatarBlendShape.asset";
        StreamReader sr = new StreamReader(path3);
        string TmpData = sr.ReadToEnd();
        sr.Close();
        
        bool latch = true;
        TmpData = TmpData.Replace("  Clips: []", "  Clips:");

        foreach (var obj in BlendShape) {
            string metaPath = SaveLocation + $@"/{avatarName}_Clips/" + obj + ".asset.meta";
            if(File.Exists(metaPath)) {
                string[] lines = System.IO.File.ReadAllLines(metaPath);
                TmpData += $"{(!latch ? "\n" : "")}  - {{fileID:\"{Convert.ToInt64(lines[4].Split(':')[1].Trim())}\", guid: \"{lines[1].Split(' ')[1].Trim()}\", type: 2}}";
                latch = false;
            }
        }

        EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Done!", 1f);
        StreamWriter streamWriter = new StreamWriter(path3);
        streamWriter.Write(TmpData);
        streamWriter.Close();
        AssetDatabase.Refresh();

        UnityEngine.Debug.Log($"Successfully created {BlendShape.Count} VRM keys for avatar: {avatarName}");
        EditorGUIUtility.PingObject(AvatarData);
        EditorUtility.ClearProgressBar();
    }

    private void DetectBlendshapes()
    {
        mappingStates.Clear();
        avatarBlendshapes.Clear();
        blendShapeCount = 0;

        if (Avatar == null)
        {
            avatarBlendshapeNames = new string[0];
            return;
        }

        try
        {
            var smr = Avatar.GetComponent<SkinnedMeshRenderer>();
            if (smr == null || smr.sharedMesh == null)
            {
                avatarBlendshapeNames = new string[0];
                return;
            }

            var shared_mesh = smr.sharedMesh;
            avatarBlendshapeNames = new string[shared_mesh.blendShapeCount + 1];
            avatarBlendshapeNames[0] = "None";

            for(int i = 0; i < shared_mesh.blendShapeCount; i++) {
                string bsName = shared_mesh.GetBlendShapeName(i);
                avatarBlendshapeNames[i + 1] = bsName;
                avatarBlendshapes[i] = bsName;
            }

            int matchCount = 0;
            var endsWithLR = new Regex(@"Left|Right|_[LR]$");

            foreach (KeyValuePair<string, string[]> ARKitItem in ARKitUE) {
                var state = new BlendshapeMappingState { ArkitKey = ARKitItem.Key };
                string blendshapeMatch = null;

                foreach (string blendshape in ARKitItem.Value) {
                    var bsCheck = avatarBlendshapes.FirstOrDefault(bs => bs.Value.ToLower() == blendshape.ToLower());

                    if (bsCheck.Value != null) {
                        blendshapeMatch = blendshape;
                        state.HasMatch = true;
                        state.AutoMatchIndex1 = bsCheck.Key;
                        state.OverrideSelectedIndex = bsCheck.Key + 1;

                        if (!endsWithLR.IsMatch(ARKitItem.Key) && endsWithLR.IsMatch(bsCheck.Value)) {
                            bool isLeft = bsCheck.Value.EndsWith("Left");
                            string partner = blendshape.Replace(isLeft ? "Left" : "Right", isLeft ? "Right" : "Left");
                            blendshapeMatch += $", {partner}";
                            
                            var bsCheck2 = avatarBlendshapes.FirstOrDefault(bs => bs.Value.ToLower() == partner.ToLower());
                            if (bsCheck2.Value != null) {
                                state.AutoMatchIndex2 = bsCheck2.Key;
                            }
                        }
                        matchCount++;
                        break;
                    }
                }

                state.AutoMatchDisplay = state.HasMatch ? blendshapeMatch : "Not Found";
                mappingStates.Add(ARKitItem.Key, state);
            }

            blendShapeCount = matchCount;
        }
        catch
        {
            avatarBlendshapeNames = new string[0];
            blendShapeCount = 0;
        }
    }
}

public class BlendshapeMappingState
{
    public string ArkitKey;
    public bool HasMatch;
    public string AutoMatchDisplay;
    public int AutoMatchIndex1 = -1;
    public int AutoMatchIndex2 = -1;
    public bool IsOverridden;
    public int OverrideSelectedIndex = 0;
}

public class ClipValue
{
    public string RelativePath = "Body";
    public int Index;
    public float Weight;
}

class LocalNamePreset
{
    public string name;
    public BlendShapePreset blendShapePreset;
}
