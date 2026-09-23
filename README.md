# Void Protocol

![Void Protocol Banner](https://github.com/user-attachments/assets/c719d3ad-203f-45fa-9d25-ab125c9f7bf1)

[![Unity 6](https://img.shields.io/badge/Unity-6000.5.9f1-black.svg?style=flat&logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/URP-2D%20Renderer-blue.svg)](https://unity.com/features/universal-render-pipeline)
[![Playable Demo](https://img.shields.io/badge/Playable-DEMO-brightgreen.svg)](https://play.unity.com/en/games/919513d2-c1ce-41e1-8dd9-dbd44c508bbe/space-quest)

**Void Protocol** is a fast-paced 2D space action game developed in Unity 6. Navigate perilous asteroid fields, defeat hostile critters, withstand aggressive enemy waves, and challenge formidable bosses in deep space.

---

## 🎮 Playable Demo

<p><strong>Play the game directly in your browser:</strong><br>
👉 <a href="https://play.unity.com/en/games/919513d2-c1ce-41e1-8dd9-dbd44c508bbe/space-quest"><strong>https://play.unity.com/en/games/919513d2-c1ce-41e1-8dd9-dbd44c508bbe/space-quest</strong></a></p>

---

## 🕹️ Controls

| Action | Keyboard / Mouse | Xbox Controller |
| :--- | :--- | :--- |
| **Movement** | `W`, `A`, `S`, `D` / Arrow Keys | Left Analog Stick |
| **Attack** | Left Mouse Button / Right `Shift` | `A` Button |
| **Super Speed Move** | Right Mouse Button / `Spacebar` | `B` Button |
| **Pause / Unpause** | Middle Mouse Button / `Esc` | `X` Button |

---

## ✨ Features

- **Dynamic Combat:** Rapid-fire phaser weapons, bullet particle collisions, and responsive ship physics.
- **Boss Battles & Enemy Waves:** Procedural object spawner managing critters, hazards, and multi-stage boss encounters.
- **High-Performance Object Pooling:** Optimized pooled audio, bullet, and effect spawning using custom pooling architectures.
- **Universal Render Pipeline (URP 2D):** Modern 2D lighting, camera shake effects, screen flashes, and multi-layer parallax space backdrops.
- **Input System Integration:** Full cross-platform gamepad and keyboard/mouse mapping via Unity's new Input System.

---

## 📂 Project Structure

```
├── Assets/
│   ├── Animations/        # Ship, enemy, and VFX sprite animations
│   ├── Art/               # High-res sprites, textures, and parallax backgrounds
│   ├── Audio/             # Sound effects, ambient tracks, and audio mixer
│   ├── Editor/            # Build automation scripts
│   ├── Prefabs/           # Pre-configured game objects (Weapons, Enemies, UI)
│   ├── Resources/         # Dynamic materials and runtime assets
│   ├── Scenes/            # MainMenu, Level1, Level 1 Complete, GameOver
│   ├── Scripts/           # Modular C# game architecture (Managers, Controllers, Weapons)
│   ├── Settings/          # URP and project rendering configuration
│   └── TextMesh Pro/      # Game typography and SDF font assets
├── Packages/              # Unity 6 package manifest and lockfile
├── ProjectSettings/       # Engine, input, tags, physics, and build configurations
├── .gitattributes         # Text normalization
├── .gitignore             # Standard Unity ignore rules
└── README.md              # Project documentation
```

---

## 🚀 Getting Started

### Prerequisites
- [Unity 6](https://unity.com/download) (Recommended version: `6000.5.9f1` or later)
- Universal Render Pipeline (URP) support

### Setup & Play in Unity Editor
1. Clone this repository:
   ```bash
   git clone https://github.com/<your-username>/Void-Protocol.git
   ```
2. Open **Unity Hub** and click **Add** > **Add project from disk**.
3. Select this repository folder.
4. Launch the project using **Unity 6**.
5. In the Project window, navigate to `Assets/Scenes/` and double-click `MainMenu.unity`.
6. Press the **Play** button at the top of the editor.

---

## 📜 Credits & Acknowledgments

- **Base foundation:** Built on top of the tutorial foundation from [YouTube Tutorial](https://youtu.be/LD2gfUKkMD0).
- **Audio Assets:** Music tracks by JDSherbert (Nostalgia Music Pack).
- **Engine:** Built with [Unity 6](https://unity.com/).
