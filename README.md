## VRM ARKit Blendshapes Auto Setup

This tool automates VRM blendshape generation to speed up setup in Unity.

![Screenshot of VRM ARKit Blendshapes Auto Setup in Unity](https://i.imgur.com/A8cvxLU.png)

The **Avatar Name** field is used only to name your generated files:

![Screenshot of VRM ARKit Blendshapes Auto Setup in Unity](https://i.imgur.com/fKAfm4Y.png)

The **Blendshapes Object** field should reference an object with a **Skinned Mesh Renderer** that contains VRM visemes (A, E, I, O, U, …) and facial tracking blendshapes (ARKit or Unified Expressions).  
These blendshapes are usually located in the avatar's **Body** object, but this may vary by creator:

![Screenshot of Unity showing the object containing the Skinned Mesh Renderer with VRM visemes and facial tracking blendshapes](https://i.imgur.com/jP7YJJK.png)

The **Blendshapes List** includes all ARKit blendshapes plus VRM visemes on the left, with corresponding target blendshapes (either ARKit or Unified Expressions) on the right:

![Screenshot of VRM ARKit Blendshapes Auto Setup showing ARKit blendshapes and their corresponding target blendshapes](https://i.imgur.com/vRbIiM6.png)

### [Download](https://github.com/KogaNoodle/VRM_ARKit_Blendshape_Setup/releases)

---
## Credits
This script is based on [Tracer755 VRM Auto Setup](https://github.com/tracer755/VRC_VRM_BlendShape_Avatar_Setup).

---
## Notes
- Your model's blendshapes must follow VRChat / ARKit / UE naming conventions (case-insensitive).
- Please verify your avatar. Some models use unconventional blendshape names and may need to be edited to match the conventions mentioned above.
- For VSeeFace compatibility, all extra visemes must be present: `SIL`, `CH`, `DD`, `FF`, `KK`, `NN`, `PP`, `RR`, `SS`, `TH`.