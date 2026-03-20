using System.Collections.Generic;

namespace VrmAutoSetup.Editor.Models
{
    public static class ARKitBlendshapes
    {
        private static readonly List<BlendshapeMapping> Mappings = new List<BlendshapeMapping>
        {
            new BlendshapeMapping("BrowDownLeft", "BrowDownLeft", "BrowLowererLeft", "BrowDown"),
            new BlendshapeMapping("BrowDownRight", "BrowDownRight", "BrowLowererRight", "BrowDown"),
            new BlendshapeMapping("BrowInnerUp", "BrowInnerUp", "BrowUp", "BrowInnerUpRight", "BrowInnerUpLeft"),
            new BlendshapeMapping("BrowOuterUpLeft", "BrowOuterUpLeft", "BrowUpLeft"),
            new BlendshapeMapping("BrowOuterUpRight", "BrowOuterUpRight", "BrowUpRight"),
            new BlendshapeMapping("CheekPuff", "CheekPuff", "CheekPuffRight", "CheekPuffLeft"),
            new BlendshapeMapping("CheekSquintLeft", "CheekSquintLeft", "CheekSquint"),
            new BlendshapeMapping("CheekSquintRight", "CheekSquintRight", "CheekSquint"),
            new BlendshapeMapping("EyeBlinkLeft", "EyeBlinkLeft", "EyeClosedLeft", "EyeClosed"),
            new BlendshapeMapping("EyeBlinkRight", "EyeBlinkRight", "EyeClosedRight", "EyeClosed"),
            new BlendshapeMapping("EyeLookDownLeft", "EyeLookDownLeft"),
            new BlendshapeMapping("EyeLookDownRight", "EyeLookDownRight"),
            new BlendshapeMapping("EyeLookInLeft", "EyeLookInLeft"),
            new BlendshapeMapping("EyeLookInRight", "EyeLookInRight"),
            new BlendshapeMapping("EyeLookOutLeft", "EyeLookOutLeft"),
            new BlendshapeMapping("EyeLookOutRight", "EyeLookOutRight"),
            new BlendshapeMapping("EyeLookUpLeft", "EyeLookUpLeft"),
            new BlendshapeMapping("EyeLookUpRight", "EyeLookUpRight"),
            new BlendshapeMapping("EyeSquintLeft", "EyeSquintLeft", "EyeSquint"),
            new BlendshapeMapping("EyeSquintRight", "EyeSquintRight", "EyeSquint"),
            new BlendshapeMapping("EyeWideLeft", "EyeWideLeft", "EyeWide"),
            new BlendshapeMapping("EyeWideRight", "EyeWideRight", "EyeWide"),
            new BlendshapeMapping("JawForward", "JawForward"),
            new BlendshapeMapping("JawLeft", "JawLeft"),
            new BlendshapeMapping("JawOpen", "JawOpen", "MouthOpen"),
            new BlendshapeMapping("JawRight", "JawRight"),
            new BlendshapeMapping("MouthClose", "MouthClose", "MouthClosed"),
            new BlendshapeMapping("MouthDimpleLeft", "MouthDimpleLeft", "MouthDimple"),
            new BlendshapeMapping("MouthDimpleRight", "MouthDimpleRight", "MouthDimple"),
            new BlendshapeMapping("MouthFrownLeft", "MouthFrownLeft", "MouthSadLeft", "MouthSad"),
            new BlendshapeMapping("MouthFrownRight", "MouthFrownRight", "MouthSadRight", "MouthSad"),
            new BlendshapeMapping("MouthFunnel", "MouthFunnel", "LipFunnel", "LipFunnelUpper", "LipFunnelLower", "LipFunnelUpperRight", "LipFunnelUpperLeft", "LipFunnelLowerRight", "LipFunnelLowerLeft"),
            new BlendshapeMapping("MouthLeft", "MouthLeft"),
            new BlendshapeMapping("MouthLowerDownLeft", "MouthLowerDownLeft", "MouthLowerDown"),
            new BlendshapeMapping("MouthLowerDownRight", "MouthLowerDownRight", "MouthLowerDown"),
            new BlendshapeMapping("MouthPressLeft", "MouthPressLeft", "MouthPress"),
            new BlendshapeMapping("MouthPressRight", "MouthPressRight", "MouthPress"),
            new BlendshapeMapping("MouthPucker", "MouthPucker", "LipPucker", "LipPuckerUpper", "LipPuckerLower", "LipPuckerUpperRight", "LipPuckerUpperLeft", "LipPuckerLowerRight", "LipPuckerLowerLeft"),
            new BlendshapeMapping("MouthRight", "MouthRight"),
            new BlendshapeMapping("MouthRollLower", "MouthRollLower", "LipSuckLower", "LipSuckLowerRight", "LipSuckLowerLeft", "LipSuck"),
            new BlendshapeMapping("MouthRollUpper", "MouthRollUpper", "LipSuckUpper", "LipSuckUpperRight", "LipSuckUpperLeft", "LipSuck"),
            new BlendshapeMapping("MouthShrugLower", "MouthShrugLower", "MouthRaiserLower"),
            new BlendshapeMapping("MouthShrugUpper", "MouthShrugUpper", "MouthRaiserUpper"),
            new BlendshapeMapping("MouthSmileLeft", "MouthSmileLeft", "MouthSmile"),
            new BlendshapeMapping("MouthSmileRight", "MouthSmileRight", "MouthSmile"),
            new BlendshapeMapping("MouthStretchLeft", "MouthStretchLeft", "MouthStretch"),
            new BlendshapeMapping("MouthStretchRight", "MouthStretchRight", "MouthStretch"),
            new BlendshapeMapping("MouthUpperUpLeft", "MouthUpperUpLeft", "MouthUpperUp"),
            new BlendshapeMapping("MouthUpperUpRight", "MouthUpperUpRight", "MouthUpperUp"),
            new BlendshapeMapping("NoseSneerLeft", "NoseSneerLeft", "NoseSneer"),
            new BlendshapeMapping("NoseSneerRight", "NoseSneerRight", "NoseSneer"),
            new BlendshapeMapping("TongueOut", "TongueOut"),
            
            // VRM Standard Emotions
            new BlendshapeMapping("Neutral", "Neutral", "Neutralize", "Rest", "vrc.v_neutral"),
            new BlendshapeMapping("Joy", "Joy", "Happy", "Smile", "vrc.v_joy"),
            new BlendshapeMapping("Angry", "Angry", "Mad", "Anger", "vrc.v_angry"),
            new BlendshapeMapping("Sorrow", "Sorrow", "Sad", "Upset", "vrc.v_sorrow"),
            new BlendshapeMapping("Fun", "Fun", "Excited", "Surprised", "vrc.v_fun"),
            
            // Visemes with vis_ prefix variants
            new BlendshapeMapping("A", "v_aa", "vrc.v_aa", "aa", "vis_aa"),
            new BlendshapeMapping("E", "v_e", "v_ee", "vrc.v_e", "vrc.v_ee", "e", "ee", "vis_e"),
            new BlendshapeMapping("I", "v_ih", "vrc.v_ih", "ih", "vis_ih"),
            new BlendshapeMapping("O", "v_oh", "vrc.v_oh", "oh", "vis_oh"),
            new BlendshapeMapping("U", "v_ou", "vrc.v_ou", "ou", "vis_ou"),
            new BlendshapeMapping("SIL", "v_sil", "vrc.v_sil", "sil", "vis_sil"),
            new BlendshapeMapping("CH", "v_ch", "vrc.v_ch", "ch", "vis_ch"),
            new BlendshapeMapping("DD", "v_dd", "vrc.v_dd", "dd", "vis_dd"),
            new BlendshapeMapping("FF", "v_ff", "vrc.v_ff", "ff", "vis_ff"),
            new BlendshapeMapping("KK", "v_kk", "vrc.v_kk", "kk", "vis_kk"),
            new BlendshapeMapping("NN", "v_nn", "vrc.v_nn", "nn", "vis_nn"),
            new BlendshapeMapping("PP", "v_pp", "vrc.v_pp", "pp", "vis_pp"),
            new BlendshapeMapping("RR", "v_rr", "vrc.v_rr", "rr", "vis_rr"),
            new BlendshapeMapping("SS", "v_ss", "vrc.v_ss", "ss", "vis_ss"),
            new BlendshapeMapping("TH", "v_th", "vrc.v_th", "th", "vis_th"),
            
            // Blink presets
            new BlendshapeMapping("Blink_L", "LeftBlink", "Blink", "EyeBlinkLeft", "EyeClosedLeft", "EyeClosed"),
            new BlendshapeMapping("Blink_R", "RightBlink", "Blink", "EyeBlinkRight", "EyeClosedRight", "EyeClosed"),
            new BlendshapeMapping("Blink", "Blink", "EyeBlinkLeft", "EyeClosedLeft", "EyeClosed", "EyeBlinkRight", "EyeClosedRight", "EyeClosed"),
        };

        public static IReadOnlyList<BlendshapeMapping> GetAll() => Mappings;
    }
}