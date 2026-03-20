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
    private static GUIStyle listItemStyle;
    private static GUIStyle dotStyle;
    private static GUIStyle categoryLabelStyle;
    private static GUIStyle btnStyle;
    private static GUIStyle overrideBtnStyleActive;
    private static GUIStyle overrideBtnStyleInactive;
    private static GUIStyle copyBtnStyle;

    // Row background textures
    private static Texture2D rowTexA;
    private static Texture2D rowTexB;
    private static Texture2D categoryTexture;

    // Icons
    private GUIContent infoIcon;
    private GUIContent targetIcon;
    private GUIContent statusIcon;
    private GUIContent clipboardIcon;

    bool showBsList = false;
    GameObject lastAvatar = null;
    string[] avatarBlendshapeNames = new string[0];
    Dictionary<string, BlendshapeMappingState> mappingStates = new Dictionary<string, BlendshapeMappingState>();

    Dictionary<int, string> avatarBlendshapes = new Dictionary<int, string>();

    // Validation: only blendshape names consisting of letters, digits, '_', '-', '.' are eligible for auto-matching
    private static readonly Regex ValidBlendshapeName = new Regex(@"^[a-zA-Z_.\-]+$", RegexOptions.Compiled);

    // Ordered category groups for the blendshape list
    private static readonly (string category, string[] keys)[] CategoryGroups = new[]
    {
        ("Brow", new[] { "BrowDownLeft", "BrowDownRight", "BrowInnerUp", "BrowOuterUpLeft", "BrowOuterUpRight" }),
        ("Eye", new[] { "EyeBlinkLeft", "EyeBlinkRight", "EyeLookDownLeft", "EyeLookDownRight", "EyeLookInLeft", "EyeLookInRight", "EyeLookOutLeft", "EyeLookOutRight", "EyeLookUpLeft", "EyeLookUpRight", "EyeSquintLeft", "EyeSquintRight", "EyeWideLeft", "EyeWideRight" }),
        ("Cheek", new[] { "CheekPuff", "CheekSquintLeft", "CheekSquintRight" }),
        ("Jaw", new[] { "JawForward", "JawLeft", "JawOpen", "JawRight" }),
        ("Mouth", new[] { "MouthClose", "MouthDimpleLeft", "MouthDimpleRight", "MouthFrownLeft", "MouthFrownRight", "MouthFunnel", "MouthLeft", "MouthLowerDownLeft", "MouthLowerDownRight", "MouthPressLeft", "MouthPressRight", "MouthPucker", "MouthRight", "MouthRollLower", "MouthRollUpper", "MouthShrugLower", "MouthShrugUpper", "MouthSmileLeft", "MouthSmileRight", "MouthStretchLeft", "MouthStretchRight", "MouthUpperUpLeft", "MouthUpperUpRight", "NoseSneerLeft", "NoseSneerRight", "TongueOut" }),
        ("Viseme", new[] { "A", "E", "I", "O", "U", "SIL", "CH", "DD", "FF", "KK", "NN", "PP", "RR", "SS", "TH" }),
        ("Blink", new[] { "Blink_L", "Blink_R", "Blink" }),
        ("Expressions", new[] { "Neutral", "Joy", "Angry", "Sorrow", "Fun" }),
    };

    // Dictionary based on ARKit Blendshapes and its respective UE counterparts
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
        { "Blink_L", new string[] { "LeftBlink", "EyeBlinkLeft", "EyeClosedLeft", "Blink", "EyeClosed" } },
        { "Blink_R", new string[] { "RightBlink", "EyeBlinkRight", "EyeClosedRight", "Blink", "EyeClosed" } },
        { "Blink", new string[] { "Blink", "EyeBlinkLeft", "EyeClosedLeft", "EyeBlinkRight", "EyeClosedRight", "EyeClosed" } },
        { "Neutral", new string[] { "Neutral", "Idle", "Rest", "vrc.v_neutral" } },
        { "Joy",     new string[] { "Joy", "Happy", "Smile", "vrc.v_joy" } },
        { "Angry",   new string[] { "Angry", "Mad", "Anger", "vrc.v_angry" } },
        { "Sorrow",  new string[] { "Sorrow", "Sad", "Upset", "vrc.v_sorrow" } },
        { "Fun",     new string[] { "Fun", "Excited", "Surprised", "vrc.v_fun" } },
    };

    LocalNamePreset[] NamePresets = new LocalNamePreset[] {
        new LocalNamePreset { name="A", blendShapePreset=BlendShapePreset.A},
        new LocalNamePreset { name = "E", blendShapePreset = BlendShapePreset.E },
        new LocalNamePreset { name = "I", blendShapePreset = BlendShapePreset.I },
        new LocalNamePreset { name = "O", blendShapePreset = BlendShapePreset.O },
        new LocalNamePreset { name = "U", blendShapePreset = BlendShapePreset.U },
        new LocalNamePreset { name = "Blink", blendShapePreset = BlendShapePreset.Blink },
        new LocalNamePreset { name = "Blink_R", blendShapePreset = BlendShapePreset.Blink_R },
        new LocalNamePreset { name = "Blink_L", blendShapePreset = BlendShapePreset.Blink_L },
        new LocalNamePreset { name = "Neutral", blendShapePreset = BlendShapePreset.Neutral },
        new LocalNamePreset { name = "Joy", blendShapePreset = BlendShapePreset.Joy },
        new LocalNamePreset { name = "Angry", blendShapePreset = BlendShapePreset.Angry },
        new LocalNamePreset { name = "Sorrow", blendShapePreset = BlendShapePreset.Sorrow },
        new LocalNamePreset { name = "Fun", blendShapePreset = BlendShapePreset.Fun },
    };

    // The 5 VRM expression keys (always generated as empty clips)
    private static readonly string[] ExpressionKeys = { "Neutral", "Joy", "Angry", "Sorrow", "Fun" };

    [MenuItem("Vtuber/Blendshapes Auto Setup")]
    public static void ShowWindow() {
        var window = GetWindow<ShapeKeyAutoSetup>("ARKit Setup");
        window.minSize = new Vector2(400, 550);
        window.Show();
    }

    private static Texture2D MakeSolidTex(Color col)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }

    private void InitializeStyles()
    {
        if (headerStyle != null) return;

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
            margin = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(2, 2, 2, 2)
        };

        dotStyle = new GUIStyle(EditorStyles.label) {
            richText = true,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(2, 2, 2, 2),
            padding = new RectOffset(0, 0, 0, 0)
        };

        categoryLabelStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 11,
            richText = true,
            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) },
            margin = new RectOffset(4, 4, 6, 2),
            padding = new RectOffset(4, 4, 2, 2)
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
            fixedWidth = 80
        };

        overrideBtnStyleInactive = new GUIStyle(GUI.skin.button) {
            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
            fixedWidth = 80
        };

        copyBtnStyle = new GUIStyle(GUI.skin.button) {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(8, 8, 8, 8),
            margin = new RectOffset(0, 0, 5, 5),
            alignment = TextAnchor.MiddleCenter
        };

        rowTexA = MakeSolidTex(new Color(0.16f, 0.16f, 0.16f, 1f));
        rowTexB = MakeSolidTex(new Color(0.19f, 0.19f, 0.19f, 1f));
        categoryTexture = MakeSolidTex(new Color(0.12f, 0.12f, 0.12f, 1f));

        infoIcon = EditorGUIUtility.IconContent("d_console.infoicon.sml");
        targetIcon = EditorGUIUtility.IconContent("d_AvatarSelector");
        statusIcon = EditorGUIUtility.IconContent("d_ViewToolOrbit");
        clipboardIcon = EditorGUIUtility.IconContent("Clipboard");
    }

    private void OnGUI()
    {
        try {
            InitializeStyles();

            SaveLocation = "Assets/!VRM Blendshapes/";

            GUILayout.Space(15);
            GUILayout.Label("ARKit Blendshape Auto Setup", headerStyle);
            GUILayout.Label("Seamlessly merge <b>ARKit facetracking</b> into your avatar with just a few clicks.", subtitleStyle);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);

            bool buttonEnabled = true;

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

                // Header Row
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.Space(8);
                GUILayout.Label(new GUIContent(" Detection Status", statusIcon.image), sectionHeaderStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label(showBsList ? "▼" : "▶", new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
                }, GUILayout.Width(20));
                GUILayout.Space(8);
                GUILayout.EndHorizontal();

                Rect headerRect = GUILayoutUtility.GetLastRect();
                headerRect.y -= 20;
                headerRect.height += 20;
                EditorGUIUtility.AddCursorRect(headerRect, MouseCursor.Link);

                if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition)) {
                    showBsList = !showBsList;
                    Event.current.Use();
                    GUI.FocusControl(null);
                }

                GUILayout.Space(5);

                // Info box (always visible)
                GUILayout.BeginHorizontal();
                GUILayout.Space(12);
                EditorGUILayout.HelpBox($"Detected {avatarBlendshapes.Count} total blendshapes. {validCount} are mapped for ARKit.", MessageType.Info);
                GUILayout.Space(12);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                // Collapsible blendshape list
                if (showBsList) {
                    // Legend (always visible while list is open, outside the scroll view)
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(16);
                    GUILayout.Label("<color=#888888>■</color> Empty", listItemStyle, GUILayout.ExpandWidth(false));
                    GUILayout.Space(12);
                    GUILayout.Label("<color=#88ff88>■</color> Matched", listItemStyle, GUILayout.ExpandWidth(false));
                    GUILayout.Space(12);
                    GUILayout.Label("<color=#4488ff>■</color> Guessed", listItemStyle, GUILayout.ExpandWidth(false));
                    GUILayout.Space(12);
                    GUILayout.Label("<color=#ffdd44>■</color> Overridden", listItemStyle, GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUI.skin.box, GUILayout.ExpandHeight(true));

                    string clipboard = "";
                    int rowIndex = 0;

                    foreach (var (category, keys) in CategoryGroups)
                    {
                        // --- Category separator ---
                        Rect catRect = EditorGUILayout.GetControlRect(false, 20f);
                        if (Event.current.type == EventType.Repaint && categoryTexture != null)
                            GUI.DrawTexture(catRect, categoryTexture);
                        GUI.Label(new Rect(catRect.x + 6, catRect.y + 2, catRect.width - 6, catRect.height),
                            $"<color=#888888><b>{category.ToUpper()}</b></color>", categoryLabelStyle);

                        foreach (string key in keys)
                        {
                            // All rows (ARKit, Blink, Expressions) use mappingStates
                            if (!mappingStates.TryGetValue(key, out var state)) continue;

                            bool isResolved = state.IsOverridden ? (state.OverrideSelectedIndex > 0) : state.HasMatch;

                            Rect rowRect = EditorGUILayout.GetControlRect(false, 22f);
                            Texture2D rowTex = (rowIndex % 2 == 0) ? rowTexA : rowTexB;
                            if (Event.current.type == EventType.Repaint && rowTex != null)
                                GUI.DrawTexture(rowRect, rowTex);

                            float x = rowRect.x + 4;
                            float rowY = rowRect.y + 1;
                            float rowH = rowRect.height - 2;

                            // Status dot: yellow=overridden, green=exact, blue=guessed, grey=empty
                            string dotColor = state.IsOverridden ? "#ffdd44"
                                : isResolved ? (state.IsGuessed ? "#4488ff" : "#88ff88")
                                : "#888888";
                            GUI.Label(new Rect(x, rowY, 16f, rowH), $"<color={dotColor}>■</color>", dotStyle);
                            x += 18f;

                            // Key label
                            string keyColor = isResolved ? "#dddddd" : "#888888";
                            GUI.Label(new Rect(x, rowY, 130f, rowH), $"<color={keyColor}><b>{state.ArkitKey}</b></color>", listItemStyle);
                            x += 132f;

                            // Override button width
                            float overrideBtnW = 80f;
                            float matchLabelW = rowRect.xMax - x - overrideBtnW - 8f;

                            // Match label or active popup
                            if (state.IsOverridden) {
                                state.OverrideSelectedIndex = EditorGUI.Popup(
                                    new Rect(x, rowY, matchLabelW, rowH),
                                    state.OverrideSelectedIndex,
                                    avatarBlendshapeNames
                                );
                            } else {
                                string matchColor = state.HasMatch ? (state.IsGuessed ? "#4488ff" : "#88ff88") : "#aaaaaa";
                                string matchText = state.HasMatch ? state.AutoMatchDisplay : "Empty";
                                GUI.Label(new Rect(x, rowY, matchLabelW, rowH),
                                    $"<color={matchColor}>{matchText}</color>", listItemStyle);
                            }
                            x += matchLabelW + 4f;

                            // Override toggle button
                            var oldBg = GUI.backgroundColor;
                            if (state.IsOverridden) GUI.backgroundColor = new Color(0.9f, 0.8f, 0.2f);
                            else GUI.backgroundColor = new Color(0.28f, 0.28f, 0.28f);

                            string btnLabel = state.IsOverridden ? "\u270e Active" : "Override";
                            GUIStyle btnSty = state.IsOverridden ? overrideBtnStyleActive : overrideBtnStyleInactive;

                            if (GUI.Button(new Rect(x, rowY, overrideBtnW, rowH), btnLabel, btnSty)) {
                                state.IsOverridden = !state.IsOverridden;
                                if (!state.IsOverridden) {
                                    state.OverrideSelectedIndex = state.HasMatch ? state.AutoMatchIndex1 + 1 : 0;
                                }
                            }
                            GUI.backgroundColor = oldBg;

                            if (isResolved) {
                                string resolvedName = state.IsOverridden
                                    ? avatarBlendshapeNames[state.OverrideSelectedIndex]
                                    : state.AutoMatchDisplay;
                                clipboard += $"[{state.ArkitKey}] => {resolvedName}\n";
                            }

                            rowIndex++;
                        }
                    }

                    EditorGUILayout.EndScrollView();
                    GUILayout.Space(12);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    // Full-width clipboard button
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    var oldColorCopy = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
                    GUIContent copyContent = new GUIContent(" Copy to Clipboard",
                        clipboardIcon != null ? clipboardIcon.image : null);
                    if (GUILayout.Button(copyContent, copyBtnStyle, GUILayout.ExpandWidth(true))) {
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
            GUIUtility.ExitGUI();
        }
    }

    private void RunGeneration() {
        bool hasOverrides = mappingStates.Values.Any(m => m.IsOverridden);
        if (hasOverrides) {
            if (!EditorUtility.DisplayDialog("Confirm Overrides",
                "You have manually overridden some blendshapes. Are you sure you want to proceed?",
                "Yes", "Cancel")) {
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
        // +5 for the expression clips
        float tempProgValue = (float)(0.8f / (validCount + ExpressionKeys.Length == 0 ? 1 : validCount + ExpressionKeys.Length));

        // --- Generate ARKit blendshape clips ---
        foreach (var kvp in mappingStates) {
            var state = kvp.Value;
            bool willGenerate = state.IsOverridden ? (state.OverrideSelectedIndex > 0) : state.HasMatch;
            if (!willGenerate) continue;

            progress += tempProgValue;
            EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Generating {state.ArkitKey}...", progress);
            BlendShape.Add(state.ArkitKey);

            var Clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            foreach (LocalNamePreset obj in NamePresets) {
                if (obj.name == state.ArkitKey) {
                    Clip.Preset = obj.blendShapePreset;
                }
            }

            string path = SaveLocation + $@"/{avatarName}_Clips/" + state.ArkitKey + ".asset";
            Clip.BlendShapeName = state.ArkitKey;

            if (state.IsOverridden) {
                int mappedIndex = state.OverrideSelectedIndex - 1;
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
        }

        // --- Generate VRM Expression clips (Neutral, Joy, Angry, Sorrow, Fun) ---
        // Skip expressions already generated with bindings by the mappingStates loop above
        foreach (string exprKey in ExpressionKeys) {
            if (mappingStates.TryGetValue(exprKey, out var exprState)) {
                bool alreadyGenerated = exprState.IsOverridden
                    ? exprState.OverrideSelectedIndex > 0
                    : exprState.HasMatch;
                if (alreadyGenerated) continue;
            }

            progress += tempProgValue;
            EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", $"Generating {exprKey}...", progress);

            BlendShape.Add(exprKey);
            var exprClip = ScriptableObject.CreateInstance<BlendShapeClip>();
            exprClip.BlendShapeName = exprKey;
            exprClip.Values = new VRM.BlendShapeBinding[0];

            foreach (LocalNamePreset obj in NamePresets) {
                if (obj.name == exprKey) {
                    exprClip.Preset = obj.blendShapePreset;
                }
            }

            string exprPath = SaveLocation + $@"/{avatarName}_Clips/" + exprKey + ".asset";
            AssetDatabase.CreateAsset(exprClip, exprPath);
        }

        // Avatar file
        var AvatarData = ScriptableObject.CreateInstance<BlendShapeAvatar>();
        AssetDatabase.CreateAsset(AvatarData, SaveLocation + "/" + avatarName + "_AvatarBlendShape.asset");

        // Inject all clips into avatar asset
        EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", "Finalizing VRM Avatar asset...", 0.9f);
        string path3 = SaveLocation + $"/{avatarName}_AvatarBlendShape.asset";
        StreamReader sr = new StreamReader(path3);
        string TmpData = sr.ReadToEnd();
        sr.Close();

        bool latch = true;
        TmpData = TmpData.Replace("  Clips: []", "  Clips:");

        foreach (var obj in BlendShape) {
            string metaPath = SaveLocation + $@"/{avatarName}_Clips/" + obj + ".asset.meta";
            if (File.Exists(metaPath)) {
                string[] lines = System.IO.File.ReadAllLines(metaPath);
                TmpData += $"{(!latch ? "\n" : "")}  - {{fileID:\"{Convert.ToInt64(lines[4].Split(':')[1].Trim())}\", guid: \"{lines[1].Split(' ')[1].Trim()}\", type: 2}}";
                latch = false;
            }
        }

        EditorUtility.DisplayProgressBar("Generating VRM Blendshapes", "Done!", 1f);
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

            for (int i = 0; i < shared_mesh.blendShapeCount; i++) {
                string bsName = shared_mesh.GetBlendShapeName(i);
                avatarBlendshapeNames[i + 1] = bsName;
                if (ValidBlendshapeName.IsMatch(bsName))
                    avatarBlendshapes[i] = bsName;
            }

            int matchCount = 0;
            var endsWithLR = new Regex(@"Left|Right|_[LR]$");

            // Each ARKit key independently searches the full blendshape list.
            // No claiming — the same avatar blendshape can match multiple keys,
            // which is correct because VRM BlendShapeClips can share mesh targets.
            foreach (KeyValuePair<string, string[]> ARKitItem in ARKitUE) {
                var state = new BlendshapeMappingState { ArkitKey = ARKitItem.Key };

                string bestMatchName = null;
                int bestMatchIndex = -1;
                float bestConfidence = 0f;
                bool isExact = false;

                // Pass 1 — Exact match (case-insensitive)
                foreach (string candidateName in ARKitItem.Value) {
                    foreach (var avatarBs in avatarBlendshapes) {
                        if (avatarBs.Value.Equals(candidateName, StringComparison.OrdinalIgnoreCase)) {
                            bestMatchName = avatarBs.Value;
                            bestMatchIndex = avatarBs.Key;
                            bestConfidence = 1.0f;
                            isExact = true;
                            goto PassDone;
                        }
                    }
                }

                // Pass 2 — Substring match (bidirectional)
                foreach (string candidateName in ARKitItem.Value) {
                    foreach (var avatarBs in avatarBlendshapes) {
                        bool sub = avatarBs.Value.IndexOf(candidateName, StringComparison.OrdinalIgnoreCase) >= 0
                                || candidateName.IndexOf(avatarBs.Value, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (sub) {
                            bestMatchName = avatarBs.Value;
                            bestMatchIndex = avatarBs.Key;
                            bestConfidence = 0.85f;
                            goto PassDone;
                        }
                    }
                }

                // Pass 3 — Fuzzy (Levenshtein ≥ 0.8): highest-scoring match
                foreach (string candidateName in ARKitItem.Value) {
                    foreach (var avatarBs in avatarBlendshapes) {
                        float score = VrmAutoSetup.Editor.StringFuzzyMatcher.CalculateSimilarity(candidateName, avatarBs.Value);
                        if (score >= 0.8f && score > bestConfidence) {
                            bestConfidence = score;
                            bestMatchName = avatarBs.Value;
                            bestMatchIndex = avatarBs.Key;
                        }
                    }
                }

                PassDone:;

                if (bestMatchIndex >= 0) {
                    state.HasMatch = true;
                    state.IsGuessed = !isExact;
                    state.AutoMatchIndex1 = bestMatchIndex;
                    state.OverrideSelectedIndex = bestMatchIndex + 1;

                    string displayName = bestMatchName;

                    // L/R combo partner detection (inline, no claiming)
                    if (!endsWithLR.IsMatch(ARKitItem.Key) && endsWithLR.IsMatch(bestMatchName)) {
                        bool isLeft = bestMatchName.EndsWith("Left");
                        string partnerName = bestMatchName.Replace(isLeft ? "Left" : "Right", isLeft ? "Right" : "Left");
                        var partnerBs = avatarBlendshapes.FirstOrDefault(bs =>
                            bs.Value.Equals(partnerName, StringComparison.OrdinalIgnoreCase));
                        if (partnerBs.Value != null) {
                            state.AutoMatchIndex2 = partnerBs.Key;
                            displayName += $", {partnerBs.Value}";
                        }
                    }

                    state.AutoMatchDisplay = displayName;
                    matchCount++;
                } else {
                    state.AutoMatchDisplay = "Empty";
                }

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
    public bool IsGuessed;
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
