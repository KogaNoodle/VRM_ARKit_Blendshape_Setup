using System.Collections.Generic;
using VRM;
using VrmAutoSetup.Editor.Models;

namespace VrmAutoSetup.Editor
{
    public static class NamePresets
    {
        private static readonly NamePreset[] Presets = new[]
        {
            new NamePreset { Name = "A", BlendShapePreset = BlendShapePreset.A },
            new NamePreset { Name = "E", BlendShapePreset = BlendShapePreset.E },
            new NamePreset { Name = "I", BlendShapePreset = BlendShapePreset.I },
            new NamePreset { Name = "O", BlendShapePreset = BlendShapePreset.O },
            new NamePreset { Name = "U", BlendShapePreset = BlendShapePreset.U },
            new NamePreset { Name = "Blink", BlendShapePreset = BlendShapePreset.Blink },
            new NamePreset { Name = "Blink_R", BlendShapePreset = BlendShapePreset.Blink_R },
            new NamePreset { Name = "Blink_L", BlendShapePreset = BlendShapePreset.Blink_L },
        };
        
        public static IEnumerable<NamePreset> GetAll() => Presets;
    }
}