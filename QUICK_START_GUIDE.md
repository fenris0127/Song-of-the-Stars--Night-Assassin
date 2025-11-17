# Quick Start Guide
## Song of the Stars - Night Assassin

Complete setup guide to get the game running in Unity Editor.

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Initial Setup](#initial-setup)
3. [Scene Setup](#scene-setup)
4. [Creating Your First Mission](#creating-your-first-mission)
5. [Testing the Game](#testing-the-game)
6. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required
- **Unity 6.0.2 or later**
- **Git** (for cloning repository)
- **Text Editor** (VS Code recommended)

### Recommended
- **Unity 6.0.23f1** (tested version)
- **Visual Studio 2022** (for C# debugging)

---

## Initial Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Song-of-the-Stars--Night-Assassin
```

### 2. Open in Unity

1. Open **Unity Hub**
2. Click **Add** → **Add project from disk**
3. Navigate to the cloned repository folder
4. Select and open the project
5. Wait for Unity to import all assets (~2-5 minutes)

### 3. Verify Package Dependencies

The project should auto-import these packages:
- **2D Sprite** (built-in)
- **TextMesh Pro** (optional, for better text rendering)
- **Input System** (if using new input)

If any are missing:
1. Go to **Window** → **Package Manager**
2. Search for the missing package
3. Click **Install**

---

## Scene Setup

### Step 1: Create Bootstrap Scene

The **GameBootstrap** scene initializes all core systems.

1. **Create new scene**: `File` → `New Scene` → `2D`
2. **Save as**: `Assets/Scenes/Bootstrap.unity`
3. **Create Bootstrap GameObject**:
   - Right-click Hierarchy → `Create Empty`
   - Name it `"GameBootstrap"`
   - Add component: `GameBootstrap` script
   - Configure:
     ```
     Startup Scene Name: "MainMenu"
     Target FPS: 60
     DSP Buffer Size: 512
     Master Volume: 0.8
     ```

4. **Add to Build Settings**:
   - `File` → `Build Settings`
   - Drag `Bootstrap.unity` to top of scene list (index 0)

### Step 2: Create Main Menu Scene

1. **Create new scene**: `2D`
2. **Save as**: `Assets/Scenes/MainMenu.unity`
3. **Add UI Canvas**:
   - Right-click Hierarchy → `UI` → `Canvas`
   - Set Render Mode: `Screen Space - Overlay`
   - Add `Canvas Scaler`:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920x1080`

4. **Add Main Menu UI** (simple version):
   - Create Button: "Start Tutorial"
   - Add `OnClick` event:
     ```csharp
     SceneTransitionManager.Instance.LoadScene("Tutorial_Courtyard")
     ```

5. **Add Scene Transition Manager**:
   - Create Empty GameObject: `"SceneTransitionManager"`
   - Add component: `SceneTransitionManager`
   - Configure:
     ```
     Fade Duration: 0.5
     Fade Color: Black
     Show Loading Screen: true
     Minimum Loading Time: 1.0
     ```

### Step 3: Create Tutorial Scene

Follow the tutorial mission specification from `MISSION_DESIGNS.md`.

1. **Create new scene**: `2D`
2. **Save as**: `Assets/Scenes/Tutorial_Courtyard.unity`

3. **Add Core GameObjects**:

   **Camera Setup:**
   ```
   Main Camera
   ├─ Camera (Orthographic, Size: 5)
   └─ Audio Listener
   ```

   **Game Managers:**
   ```
   --- GameManagers ---
   ├─ RhythmSyncManager
   ├─ EnhancedMissionManager
   ├─ StatisticsManager
   ├─ DifficultyManager
   ├─ VFXManager
   └─ ObjectPoolManager
   ```

   **Player:**
   ```
   Player
   ├─ SpriteRenderer (assign player sprite)
   ├─ Collider2D
   ├─ PlayerController
   ├─ FocusManager
   ├─ RhythmPatternChecker
   ├─ CapricornTrapSkill (tutorial skill)
   └─ (Add other skill components as unlocked)
   ```

   **UI:**
   ```
   --- UI ---
   ├─ Canvas
   │   ├─ BeatTimelineUI
   │   ├─ BeatVisualizer
   │   ├─ MissionObjectivesUI
   │   └─ TutorialUI
   └─ TutorialController
   ```

4. **Add Tutorial Controller**:
   - Create Empty GameObject: `"TutorialController"`
   - Add component: `TutorialController`
   - Enable: `Enable Tutorial`, `Always Show Beat Visual`
   - Reference `TutorialUI` in inspector

5. **Place Tutorial Elements**:
   - Create waypoint: Tag as `"Tutorial_Waypoint_1"`
   - Create practice dummy: Tag as `"TutorialDummy"`
   - Add `GuardRhythmPatrol` component to dummy

---

## Creating Your First Mission

### Step 1: Create MissionData ScriptableObject

1. **In Project window**:
   - Right-click `Assets/Data/Missions/`
   - `Create` → `Song of the Stars` → `Missions` → `Mission Data`
   - Name: `Tutorial_FirstSteps`

2. **Configure Mission Info**:
   ```
   Mission ID: "tutorial_first_steps"
   Mission Name: "First Steps"
   Difficulty: Tutorial
   Music BPM: 100
   Time Limit: 0 (None)
   Max Alert Level: 5
   Experience Reward: 50
   ```

3. **Add Primary Objectives**:

   Click `+` under Primary Objectives, add 4 objectives:

   **Objective 1: Learn Movement**
   ```
   Description: "Move to the marked location"
   Type: Reach
   Target Tag: "Tutorial_Waypoint_1"
   Target Count: 1
   ```

   **Objective 2: Practice Timing**
   ```
   Description: "Hit 5 Perfect rhythm inputs"
   Type: Custom
   Target Tag: "" (leave empty)
   Target Count: 5
   ```

   **Objective 3: Use First Skill**
   ```
   Description: "Activate Capricorn Trap (Press 1 on beat)"
   Type: Custom
   Target Tag: ""
   Target Count: 1
   ```

   **Objective 4: Eliminate Target**
   ```
   Description: "Eliminate the practice dummy"
   Type: Eliminate
   Target Tag: "TutorialDummy"
   Target Count: 1
   ```

4. **Add Scripted Events** (optional):

   **Welcome Dialog:**
   ```
   Event ID: "welcome"
   Trigger Type: OnMissionStart
   Event Type: ShowDialog
   Dialog Text: "Feel the rhythm of the stars..."
   ```

5. **Configure Skill Loadout**:
   - Assign `CapricornTrapSkill` data to Slot 0
   - Leave other slots empty

### Step 2: Assign Mission to Manager

1. **Select EnhancedMissionManager** in Tutorial scene
2. **Drag `Tutorial_FirstSteps`** to `Current Mission` field
3. **Save scene**

---

## Testing the Game

### Quick Test Workflow

1. **Play from Bootstrap**:
   - Open `Bootstrap.unity`
   - Press **Play** in Unity Editor
   - Should auto-load Main Menu

2. **Start Tutorial**:
   - Click "Start Tutorial" button
   - Should fade out → load tutorial → fade in
   - Tutorial UI should appear with first step

3. **Test Tutorial Flow**:
   - Follow on-screen prompts
   - Move with **WASD/Arrow Keys**
   - Watch beat visualization
   - Press **1** on beat to use skill
   - Approach dummy and press **Space** to eliminate

4. **Check Console**:
   - Should see: "Tutorial started"
   - Should see: "Tutorial step completed: ..."
   - Should see: "Mission objective completed: ..."
   - Should see: "Tutorial completed!"

### Test Rhythm Timing

1. **Open RhythmSyncManager inspector**
2. **Enable Gizmos** in Game view
3. **Play scene**
4. **Watch for**:
   - Beat counter incrementing
   - Beat visualization pulsing
   - Timeline markers scrolling
   - Perfect/Great judgment feedback

### Test Skill System

1. **Ensure player has Focus**:
   - Perfect inputs generate Focus
   - Check blue Focus bar in UI

2. **Press 1 on beat**:
   - Should spawn Capricorn Trap
   - Should consume 15 Focus
   - Should show cooldown (12 beats)

3. **Test other skills** (if unlocked):
   - Press 2, 3, 4 on beat
   - Verify VFX and functionality

---

## Configuration Files

### Create ScriptableObject Configs

If not already created, generate these in `Assets/Data/`:

**PlayerConfig:**
```
Create → Song of the Stars → Config → Player Config
- Move Distance: 1.0
- Move Speed: 5.0
- Assassination Range: 1.5
- Free Move Focus Cost: 5.0
```

**FocusConfig:**
```
Create → Song of the Stars → Config → Focus Config
- Base Focus Cost Per Skill: 15
- Perfect Combo Cooldown Multiplier: 0.67
- Focus Decay Per Second: 2.0
```

**RhythmConfig:**
```
Create → Song of the Stars → Config → Rhythm Config
- Perfect Tolerance: 0.04 (±40ms)
- Beat Tolerance: 0.08 (±80ms)
```

**DifficultySettings:**
```
Create → Song of the Stars → Config → Difficulty Settings
- Guard Detection Range Multiplier: 1.0
- Guard Vision Cone Angle: 90
- Max Alert Level: 10
```

---

## Troubleshooting

### Common Issues

**Problem: "NullReferenceException on GameBootstrap"**
- **Solution**: Ensure all managers are assigned in Bootstrap inspector
- Check that `SceneTransitionManager` prefab exists

**Problem: "Beat timing feels off"**
- **Solution**:
  - Check DSP Buffer Size = 512 in GameBootstrap
  - Verify Music BPM matches MissionData
  - Enable "Show Beat Gizmos" in RhythmSyncManager

**Problem: "Skills don't activate"**
- **Solution**:
  - Check Focus > skill cost (default 15)
  - Verify skill is assigned to loadout slot
  - Ensure input is ON THE BEAT (Perfect or Great)
  - Check cooldown hasn't expired yet

**Problem: "Tutorial doesn't start"**
- **Solution**:
  - Verify `TutorialController.enableTutorial = true`
  - Check `TutorialUI` is assigned in inspector
  - Ensure tutorial steps are populated (should auto-create)

**Problem: "Scene won't load"**
- **Solution**:
  - Check scene is added to Build Settings
  - Verify scene name matches exactly (case-sensitive)
  - Check SceneTransitionManager instance exists

**Problem: "Player can't move"**
- **Solution**:
  - Check PlayerController is enabled
  - Verify Input System settings
  - Check for physics colliders blocking movement
  - Ensure player isn't in a disabled state

### Debug Tools

**Enable Debug Logging:**
```csharp
// In any manager script
Debug.Log("Custom debug message");
```

**Unity Console Filters:**
- Click **Collapse** to group similar messages
- Use **Search** to filter (e.g., "Mission", "Tutorial", "Rhythm")

**Performance Profiling:**
1. Open **Profiler**: `Window` → `Analysis` → `Profiler`
2. Enable **Deep Profile** for detailed breakdown
3. Check frame times:
   - Target: <16.6ms (60 FPS)
   - UI should be <5ms
   - Skills should be <2ms

**Rhythm Timing Debug:**
```csharp
// Add to RhythmPatternChecker.cs in CheckInput()
Debug.Log($"Input offset: {offset * 1000f:F2}ms, Judgment: {judgment}");
```

### Performance Tips

1. **Object Pooling**:
   - VFX uses ObjectPoolManager (zero GC)
   - Configure pool sizes in VFXManager inspector

2. **Physics Optimization**:
   - Use `Physics2D.queriesHitTriggers = false`
   - Limit collision layers in LayerMask

3. **Audio Settings**:
   - DSP Buffer: 512 (balance latency vs performance)
   - Sample Rate: 48000 Hz

---

## Next Steps

Once you have the tutorial working:

1. **Create More Missions**:
   - Follow `MISSION_DESIGNS.md`
   - Mission 1: "Silent Approach" (Easy)
   - Mission 2: "Night Market" (Normal)

2. **Implement Remaining Skills**:
   - All 8 skills are coded and ready
   - Create ScriptableObject data for each
   - Add to Player prefab as components

3. **Build Level Layouts**:
   - Use Tilemap for environment
   - Place guard waypoints
   - Add hiding spots and obstacles

4. **Polish UI**:
   - Post-mission summary screen
   - Progression menu
   - Achievement notifications

5. **Add Content**:
   - Create skill icons (128x128px PNG)
   - Source VFX prefabs (32 total)
   - Source music tracks (80-140 BPM)
   - Record SFX (22 audio files)

---

## Resources

### Documentation
- **PRD.md** - Complete product requirements
- **PLAN.md** - 12-month development roadmap
- **SKILLS_DESIGN.md** - 8 skill specifications
- **MISSION_DESIGNS.md** - Mission templates and 3 designed missions
- **UNITY_SETUP_GUIDE.md** - Detailed Unity Editor integration

### Code Reference
- **GameBootstrap.cs** - Game initialization
- **EnhancedMissionManager.cs** - Mission lifecycle
- **TutorialController.cs** - Tutorial flow
- **RhythmSyncManager.cs** - Beat synchronization

### Unity Learn
- [2D Game Kit](https://learn.unity.com/project/2d-game-kit)
- [Audio Mixing](https://learn.unity.com/tutorial/audio-mixing)
- [ScriptableObjects](https://learn.unity.com/tutorial/introduction-to-scriptable-objects)

---

## Getting Help

### Common Questions

**Q: How do I add a new skill?**
A: See `SKILLS_DESIGN.md` for complete skill creation workflow.

**Q: How do I balance difficulty?**
A: Adjust `DifficultySettings` ScriptableObject. Test with different timing windows.

**Q: How do I create custom objectives?**
A: Extend `ObjectiveType` enum and add handling in `EnhancedMissionManager.NotifyObjectiveProgress()`.

**Q: How do I change the rhythm timing?**
A: Edit `RhythmConfig` ScriptableObject. Perfect = ±40ms, Great = ±80ms.

### Contact & Support

- **Documentation Issues**: Check UNITY_SETUP_GUIDE.md
- **Code Questions**: Read inline code comments (all scripts fully documented)
- **Design Questions**: Review PRD.md and PLAN.md

---

## Quick Reference Commands

### Git Commands
```bash
# Pull latest changes
git pull origin main

# Create feature branch
git checkout -b feature/my-feature

# Commit changes
git add .
git commit -m "Description of changes"
git push origin feature/my-feature
```

### Unity Shortcuts
```
Play/Stop:      Ctrl+P (Cmd+P on Mac)
Pause:          Ctrl+Shift+P
Step Frame:     Ctrl+Alt+P
Focus Scene:    F
Frame Selected: F (with object selected)
```

---

**Status**: Quick Start Guide Complete ✅
**Last Updated**: 2025-11-17
**Unity Version**: 6.0.2+

Happy developing! 🌟
