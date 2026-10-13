# Sand Raking: setup notes

1. Create a RenderTexture asset to use as the sand heightmap:
   - Resolution 1024x1024 (or 2048 for more detail if you need it)
   - Format: R8 or RFloat (if available). Make sure "Read/Write" is enabled if you need CPU readback.

2. Create a new Material `Stamp` and set its shader to `Hidden/Stamp` (this repo's shader).
   - Assign a circular white brush texture to the material's `_BrushTex`.

3. Create the sand material (Shader Graph recommended):
   - Sample the heightmap RenderTexture in the shader graph as `_HeightMap`.
   - Use the height value to perturb normals (generate normals from height via Sobel or sample offsets) and optionally apply a small vertex displacement along the mesh normal.
   - For URP: create a Universal Shader Graph and plug the computed normal into the Normal input and the displaced vertex into Position (or use a Vertex Position node).

4. Scene setup:
   - Add a plane mesh for the sand, UVs must be 0..1 across the surface.
   - Assign the sand material and set the `_HeightMap` texture to the RenderTexture you created.
   - Add a collider to the sand plane (MeshCollider, convex not required) and put it on a dedicated `Sand` layer.

5. Rake setup:
   - Place the `RakeTip` script on the rake tip transform (a child GameObject at the end of the rake).
   - Assign `sandLayer` to the layer you used for the plane.
   - Assign the `RakePainter` component (create an empty GameObject and attach `RakePainter`) and set the `heightmap`, `stampMaterial`, and `brushTexture`.
   - (Optional) Press Play and ensure the rake tip calls `RakePainter.StampAt()` when contacting the sand plane.

6. Clear/reset:
   - To clear the sand, simply clear the RenderTexture to neutral value (e.g., 0.5) using `Graphics.Blit` or `RenderTexture.active` and `GL.Clear`.
   - Example: `Graphics.Blit(clearTexture, heightmap);` or a small helper function in `RakePainter`.
   - This project includes a `Clear(float value=0f)` method on `RakePainter` and a `SandUI.ClearSand()` helper script to wire to a UI button.

UI hookup:

- Create an empty GameObject (e.g., `SandUI`) and attach the `SandUI` script.
- Assign your `RakePainter` instance to the `painter` field and choose `clearValue` (0=flat, 0.5=neutral).
- On your Flexalon/Unity button, add an OnClick event and drag the `SandUI` GameObject, then select `SandUI.ClearSand()`.
