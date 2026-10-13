# Zen Garden VR

A relaxing virtual reality Japanese zen garden for Meta Quest, built in Unity. Rake patterns into the sand and decorate the garden with stones and plants.

## Features

- **Sand raking.** The rake tip raycasts onto the sand and stamps into a heightmap `RenderTexture`. A Shader Graph sand material turns that heightmap into grooves in real time, and strokes are interpolated so they draw as continuous lines instead of dots.
- **Garden placement.** Use the in-world menu and controller laser to pick stones, plants and a bridge, then place them on the sand. Includes undo, clear all, and a button to smooth the sand again.
- **Ambient audio.** Background zen ambience plus sand and gravel footstep sounds.

## My code

| Path | What it does |
| --- | --- |
| `Assets/Scripts/RakeTip.cs` | Raycasts from the rake tip to the sand and sends hit UVs to the painter |
| `Assets/Scripts/RakePainter.cs` | Stamps into the heightmap render texture; clear/reset |
| `Assets/Scripts/GardenPlacementController.cs` | Laser pointer, item selection, placement and undo |
| `Assets/Scripts/GardenPlacementMenu.cs` | Builds the in-world placement menu UI |
| `Assets/Scripts/SandUI.cs` | Connects UI buttons to the sand reset |
| `Assets/Shaders/Stamp.shader` | Brush stamp shader used to paint the heightmap |
| `Assets/Materials/SandShader.shadergraph` | Sand surface shader driven by the heightmap |
| `Assets/README_SandRaking.md` | Setup notes for the raking system |

## Requirements

- Unity **6000.3.7f1** (Unity 6) with Android Build Support
- Universal Render Pipeline (URP)
- Meta Quest headset
- Meta XR SDK and OpenXR. Unity installs these automatically from `Packages/manifest.json`.

## Third-party assets

This repo contains only my own work. The assets below were made by others, and their licenses don't allow redistributing them, so **they aren't in this repo**. To run the project, add them yourself **before opening it in Unity for the first time**. That way Unity keeps the existing references in the scene.

### Asset packs

Import these through the Unity Package Manager (*My Assets*) or by double-clicking the `.unitypackage`. Keep the default import location.

| Asset | Import location | Used for |
| --- | --- | --- |
| Seyeonjeong Pavilion environment pack | `Assets/SeyeonjeongPavilion/` | Pavilion, trees, plants, rocks, landscape |
| Stone Wall - Photo Based PBR | `Assets/Submission/Stone Wall - Photo Based PBR/` | Stone wall materials |

### Individual files

Download these and put them at the exact path shown, with the same file name. The matching `.meta` files are already in the repo.

| File | Source | Used for |
| --- | --- | --- |
| `Assets/Sounds/quietphase-ambient-zen-489706.mp3` | [Pixabay](https://pixabay.com/) by *quietphase* | Ambient music |
| `Assets/Sounds/dragon-studio-footsteps-on-gravel-2-397986.mp3` | [Pixabay](https://pixabay.com/) by *Dragon Studio* | Footsteps |
| `Assets/Sounds/dragon-studio-sand-transition-451861.mp3` | [Pixabay](https://pixabay.com/) by *Dragon Studio* | Sand sound effect |
| `Assets/Sounds/freesound_community-walking-in-sand-36204.mp3` | [Pixabay](https://pixabay.com/) by *freesound_community* | Walking in sand |
| `Assets/Materials/uploads_files_4742161_rake.fbx` | Free 3D model (rake) | Rake model |
| `Assets/Materials/0016-beach-sand-texture-seamless.jpg` | Free seamless sand texture | Sand material |

If any of these are missing, the project still opens, but the affected objects will show missing meshes, materials, or sounds.

## Getting started

1. Clone this repository.
2. Add the third-party assets listed above.
3. Open the project in Unity Hub with Unity 6000.3.7f1.
4. Open `Assets/Scenes/MainScene.unity`.
5. Switch the build target to Android, connect your Quest, and use **Build and Run**. You can also press Play with Quest Link.

## Author

Kjell Berghmans, Howest
