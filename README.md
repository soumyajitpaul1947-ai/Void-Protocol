# Void Protocol

![Void Protocol Banner](https://github.com/user-attachments/assets/c719d3ad-203f-45fa-9d25-ab125c9f7bf1)

[![Unity 6](https://img.shields.io/badge/Unity-6000.5.9f1-black.svg?style=flat&logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/URP-2D%20Renderer-blue.svg)](https://unity.com/features/universal-render-pipeline)
[![Author](https://img.shields.io/badge/Author-Soumyajit%20Paul-blueviolet.svg)](#credits--acknowledgments)

**Void Protocol** is a fast-paced 2D space action game built in Unity 6. Developed by Soumyajit Paul on top of the foundational architecture from Code Laboratory, Void Protocol transforms the mechanics into an endless survival space marathon featuring escalating boss encounters, deep-space parallax visuals, dynamic buff progression, and custom combat systems.

---

## 🕹️ Controls

| Action | Keyboard / Mouse | Xbox Controller |
| :--- | :--- | :--- |
| **Movement** | `W`, `A`, `S`, `D` / Arrow Keys | Left Analog Stick |
| **Phaser Attack** | Left Mouse Button / Right `Shift` | `A` Button |
| **Super Speed Boost** | Right Mouse Button / `Spacebar` | `B` Button |
| **Pause / Unpause** | Middle Mouse Button / `Esc` / `P` | `X` Button |

---

## ✨ Features

- **Endless Survival Marathon:** Time-based survival scaling (`survivalTime`), tracking total distance traversed, live HUD timers, and dynamic difficulty multipliers.
- **Escalating Multi-Tier Boss Battles:**
  - **Boss 1 (45s):** Heavy charging dreadnought.
  - **Boss 2 (100s):** Accelerated interceptor with amplified endurance.
  - **Boss 3 (150s):** High-tier titan initiating infinite loop cycles upon defeat.
- **Damage & Invulnerability System:** Ship health calibrated to a 3-hit destruction model with 1.5s post-hit invulnerability blinking and tight physical hull colliders.
- **Random Power-Up Progression:** Defeating hostile critters grants a 35% chance to drop temporary buffs:
  - **Repair (+1 Health)**
  - **Double Shot (Dual parallel phaser beams)**
  - **Rapid Fire (Accelerated projectile rate)**
  - **Temporary Invulnerability Shield**
- **Performance-Tuned Architecture:** Custom object pooling for phasers, explosions, and audio; Universal Render Pipeline (URP 2D) dynamic lighting; multi-layer parallax space backdrops.
- **Post-Game Rank Grading:** Automated grading system awarding Rank S+ (Apex), Rank S, Rank A, Rank B, or Rank C based on survival time and total score.

---

## 📂 Project Structure

```
Void-Protocol/
├── Assets/
│   ├── Animations/        # Ship, enemy, and VFX sprite animations
│   ├── Art/               # High-res sprites, textures, and parallax backgrounds
│   ├── Audio/             # Sound effects, ambient tracks, and audio mixer
│   ├── Editor/            # Build automation scripts (BuildScript.cs)
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

## 🚀 How to Play / Build

### Prerequisites
- [Unity 6](https://unity.com/download) (Recommended: `6000.5.9f1` or later)
- Universal Render Pipeline (URP 2D)

### Play in Unity Editor
1. Clone or download this repository:
   ```bash
   git clone https://github.com/<your-username>/Void-Protocol.git
   ```
2. Open **Unity Hub** and click **Add > Add project from disk**.
3. Select the `Void-Protocol` folder.
4. Launch the project in **Unity 6**.
5. In the Project tab, open `Assets/Scenes/MainMenu.unity` and click **Play**.

### Build Standalone Executable (.exe)
In Unity Editor:
1. Open the project.
2. Select **File > Build Profiles** (or click **BuildScript > Build Windows**).
3. Unity will compile and generate `Build/VoidProtocol.exe`.

---

## 📜 Credits & Acknowledgments

- **Game Creator & Developer:** Soumyajit Paul
- **Tutorial Foundation:** Original tutorial concepts by [Code Laboratory](https://youtu.be/LD2gfUKkMD0).
- **Audio Assets:** Music tracks by JDSherbert (Nostalgia Music Pack).
- **Engine:** Built with [Unity 6](https://unity.com/).
