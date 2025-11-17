# Unity Setup Guide
## Song of the Stars - Night Assassin
**Skill System Integration**

This guide walks through setting up all 8 constellation skills in the Unity Editor after the code implementation is complete.

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Creating ScriptableObject Assets](#creating-scriptableobject-assets)
3. [Player GameObject Setup](#player-gameobject-setup)
4. [VFX Prefab Creation](#vfx-prefab-creation)
5. [Audio Setup](#audio-setup)
6. [Testing Each Skill](#testing-each-skill)
7. [Skill Loadout Configuration](#skill-loadout-configuration)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### ✅ Completed Code Implementation
All skill scripts are implemented:
- `OrionsArrowSkill.cs` + `SkillProjectile.cs`
- `GeminiCloneSkill.cs` + `CloneController.cs`
- `ShadowBlendSkill.cs`
- `AndromedaVeilSkill.cs`
- `PegasusDashSkill.cs`
- `AquariusFlowSkill.cs`

### Required Unity Packages
- TextMeshPro (for UI)
- 2D Sprite (for visuals)
- Universal Render Pipeline (recommended for VFX)

---

## Creating ScriptableObject Assets

### Step 1: Create ConstellationSkillData Assets

Navigate to: `Assets/Data/Skills/`

Create 8 ScriptableObject assets (Right-click → Create → Song of the Stars → Constellation Skill):

#### 1. Capricorn Trap (Existing Attack)
```
Skill Name: Capricorn Trap
Category: Attack
Input Count: 1
Cooldown Beats: 8
Description: "Place a celestial trap that stuns guards"
Icon: [Assign trap icon]
VFX Prefab: [Assign trap VFX]
```

#### 2. Orion's Arrow (NEW Attack)
```
Skill Name: Orion's Arrow
Category: Attack
Input Count: 2
Cooldown Beats: 16
Description: "Fire a star projectile for ranged elimination"
Icon: [Assign arrow icon - bow constellation]
VFX Prefab: [Assign arrow trail VFX]
```

#### 3. Decoy (Existing Lure)
```
Skill Name: Decoy
Category: Lure
Input Count: 1
Cooldown Beats: 10
Description: "Create a stationary illusion to distract guards"
Icon: [Assign decoy icon]
VFX Prefab: [Assign illusion VFX]
```

#### 4. Gemini Clone (NEW Lure)
```
Skill Name: Gemini Clone
Category: Lure
Input Count: 2
Cooldown Beats: 12
Description: "Summon a moving clone that patrols and attracts guards"
Icon: [Assign twin constellation icon]
VFX Prefab: [Assign clone spawn VFX]
```

#### 5. Shadow Blend (NEW Stealth)
```
Skill Name: Shadow Blend
Category: Stealth
Input Count: 1
Cooldown Beats: 8
Description: "Become nearly invisible while stationary"
Icon: [Assign shadow icon]
VFX Prefab: [Assign shadow merge VFX]
```

#### 6. Andromeda's Veil (NEW Stealth)
```
Skill Name: Andromeda's Veil
Category: Stealth
Input Count: 3
Cooldown Beats: 16
Description: "Full invisibility with slow movement"
Icon: [Assign veil/galaxy icon]
VFX Prefab: [Assign shimmer VFX]
```

#### 7. Pegasus Dash (NEW Movement)
```
Skill Name: Pegasus Dash
Category: Movement
Input Count: 2
Cooldown Beats: 10
Description: "Teleport dash forward"
Icon: [Assign winged horse icon]
VFX Prefab: [Assign dash trail VFX]
```

#### 8. Aquarius Flow (NEW Movement)
```
Skill Name: Aquarius Flow
Category: Movement
Input Count: 3
Cooldown Beats: 12
Description: "Gain 2x movement speed"
Icon: [Assign water bearer icon]
VFX Prefab: [Assign flow aura VFX]
```

---

## Player GameObject Setup

### Step 2: Add Skill Components to Player

1. **Select Player GameObject** in Hierarchy
2. **Add Components** (Add Component → Scripts → SongOfTheStars.Skills):
   - `OrionsArrowSkill`
   - `GeminiCloneSkill`
   - `ShadowBlendSkill`
   - `AndromedaVeilSkill`
   - `PegasusDashSkill`
   - `AquariusFlowSkill`

### Step 3: Configure Each Skill Component

#### OrionsArrowSkill Configuration
```
Inspector Settings:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▶ Projectile Settings
  - Projectile Prefab: [Create and assign, see below]
  - Spawn Offset: (0.5, 0)

▶ Targeting
  - Auto Target: ✓
  - Auto Target Range: 15

▶ VFX & SFX
  - Activation VFX Prefab: [Arrow charge effect]
  - Fire SFX: [Arrow whoosh sound]

▶ Layer Settings
  - Target Mask: Guard
  - Obstacle Mask: Obstacles, Walls
```

**Projectile Prefab Setup:**
1. Create empty GameObject named "Orion_Arrow_Projectile"
2. Add components:
   - SpriteRenderer (assign arrow sprite)
   - Rigidbody2D (Gravity Scale: 0, Continuous detection)
   - CircleCollider2D (Is Trigger: ✓, Radius: 0.2)
   - SkillProjectile script
3. Configure SkillProjectile:
   ```
   Speed: 30
   Max Distance: 15
   Pierce: false
   Damage: 100
   Impact VFX Prefab: [Star burst effect]
   Destroy VFX Prefab: [Fade particles]
   Obstacle Mask: Obstacles, Walls
   Target Mask: Guard
   ```
4. Save as prefab in `Assets/Prefabs/Skills/`

#### GeminiCloneSkill Configuration
```
Inspector Settings:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▶ Clone Prefab
  - Clone Prefab: [Create and assign, see below]

▶ Spawn Settings
  - Spawn Offset: (2, 0)
  - Spawn In Facing Direction: ✓
  - Max Active Clones: 1

▶ Clone Configuration
  - Movement Pattern: BackAndForth
  - Clone Move Speed: 3
  - Clone Patrol Distance: 4
  - Clone Lifetime Beats: 8

▶ VFX & SFX
  - Activation VFX Prefab: [Twin sparkle effect]
  - Spawn SFX: [Ethereal chime]

▶ Layer Settings
  - Guard Mask: Guard
```

**Clone Prefab Setup:**
1. Duplicate Player GameObject (or create new sprite object)
2. Rename to "Gemini_Clone"
3. Add CloneController component
4. Configure CloneController:
   ```
   Movement Pattern: BackAndForth
   Move Speed: 3
   Patrol Distance: 4
   Pause Duration: 0.5
   Lifetime Beats: 8
   Guard Proximity Threshold: 1.5
   Transparency: 0.6
   Spawn VFX Prefab: [Clone appear effect]
   Despawn VFX Prefab: [Clone fade effect]
   Guard Mask: Guard
   ```
5. Adjust SpriteRenderer:
   - Color: Light blue tint (R:0.7, G:0.7, B:1, A:0.6)
6. Save as prefab

#### ShadowBlendSkill Configuration
```
Inspector Settings:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▶ Stealth Settings
  - Blend Alpha: 0.2
  - Duration Beats: 8
  - Movement Breaks Blend: ✓

▶ Detection Settings
  - Detection Range Multiplier: 0.3

▶ VFX & SFX
  - Activation VFX Prefab: [Shadow merge particles]
  - Break VFX Prefab: [Shadow disperse particles]
  - End VFX Prefab: [Shadow fade particles]
  - Activation SFX: [Whoosh sound]
  - Break SFX: [Crack sound]
```

#### AndromedaVeilSkill Configuration
```
Inspector Settings:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▶ Stealth Settings
  - Veil Alpha: 0.05
  - Duration Beats: 8
  - Move Speed Multiplier: 0.6

▶ Detection Settings
  - Detection Range Multiplier: 0.1
  - Vision Cone Multiplier: 0.5

▶ VFX & SFX
  - Activation VFX Prefab: [Galaxy shimmer burst]
  - End VFX Prefab: [Shimmer fade]
  - Veil Particles Prefab: [Floating star particles]
  - Activation SFX: [Mystical chime]
  - End SFX: [Ethereal fade]
  - Veil Ambient Loop: [Soft wind/space ambience]
```

#### PegasusDashSkill Configuration
```
Inspector Settings:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▶ Dash Settings
  - Dash Distance: 5
  - Can Pass Through Obstacles: ✓
  - Max Obstacle Thickness: 1

▶ Collision Settings
  - Solid Obstacle Mask: Obstacles, Walls
  - Destination Check Radius: 0.5

▶ VFX & SFX
  - Start VFX Prefab: [Wing burst particles]
  - End VFX Prefab: [Landing impact]
  - Trail VFX Prefab: [Feather trail]
  - Dash SFX: [Whoosh + wings]
  - Blocked SFX: [Thud sound]

▶ Camera
  - Instant Camera Follow: ✓
```

#### AquariusFlowSkill Configuration
```
Inspector Settings:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
▶ Speed Boost Settings
  - Speed Multiplier: 2
  - Duration Beats: 8

▶ Visual Effects
  - Flow Alpha: 0.9
  - Flow Tint: (R:0.7, G:0.9, B:1, A:1)
  - Flow Trail Prefab: [Water trail particles]
  - Flow Aura Prefab: [Wavy aura particles]

▶ VFX & SFX
  - Activation VFX Prefab: [Water burst]
  - End VFX Prefab: [Water splash]
  - Activation SFX: [Flowing water]
  - End SFX: [Splash]
  - Flow Ambient Loop: [Gentle stream]

▶ Bonus Effects
  - Reduce Dash Cooldown: ✓
  - Dash Cooldown Reduction: 0.25
```

---

## VFX Prefab Creation

### Quick VFX Templates

For rapid prototyping, create simple particle systems:

#### Generic Impact VFX
```
GameObject: "Impact_VFX"
Components:
  - Particle System
    Duration: 0.5
    Start Lifetime: 0.3-0.5
    Start Speed: 2-5
    Start Size: 0.2-0.5
    Emission: Burst 15-30 particles
    Shape: Sphere, Radius: 0.5
    Color over Lifetime: White → Transparent
    Size over Lifetime: 1 → 0
```

#### Trail VFX
```
GameObject: "Trail_VFX"
Components:
  - Trail Renderer
    Time: 0.3
    Width: 0.2 → 0
    Color: Gradient (skill-specific color)
    Material: [Particle additive material]
```

#### Aura VFX
```
GameObject: "Aura_VFX"
Components:
  - Particle System
    Looping: ✓
    Duration: 1
    Start Lifetime: 1-2
    Start Speed: 0.5
    Emission: 10 particles/sec
    Shape: Sphere, Radius: 1
    Color: Skill-specific with alpha 0.5
```

### Skill-Specific Color Schemes

- **Orion's Arrow**: Gold/Yellow (#FFD700)
- **Gemini Clone**: Cyan (#00FFFF)
- **Shadow Blend**: Dark Purple (#4B0082)
- **Andromeda's Veil**: Magenta/Purple (#8B00FF)
- **Pegasus Dash**: White/Light Blue (#ADD8E6)
- **Aquarius Flow**: Light Blue/Turquoise (#40E0D0)

---

## Audio Setup

### Required SFX Categories

1. **Activation Sounds** (8 files)
   - Short, distinctive sound for each skill
   - 0.1-0.3 seconds duration
   - Format: .wav or .ogg

2. **Impact/Hit Sounds** (3 files)
   - Arrow impact
   - Dash landing
   - Clone spawn/despawn

3. **Ambient Loops** (2 files)
   - Andromeda's Veil ambient (mystical)
   - Aquarius Flow ambient (water)
   - Looping, 2-4 seconds

4. **Feedback Sounds** (3 files)
   - Skill blocked/failed
   - Movement break (Shadow Blend)
   - Effect end

### Audio Importing
1. Place audio files in `Assets/Audio/Skills/`
2. Import Settings:
   - Load Type: Decompress On Load (for SFX)
   - Load Type: Streaming (for ambient loops)
   - Preload Audio Data: ✓ (for SFX)

---

## Testing Each Skill

### Test Checklist

Use the Context Menu test functions in each skill component:

#### Orion's Arrow
```
✓ Right-click OrionsArrowSkill → Test Fire Arrow
  - Verify projectile spawns
  - Check auto-targeting to nearest guard
  - Confirm guard is eliminated on hit
  - Check VFX plays at impact
  - Verify SFX plays
```

#### Gemini Clone
```
✓ Right-click GeminiCloneSkill → Test Spawn Clone
  - Verify clone spawns at offset
  - Check clone patrols back and forth
  - Confirm guards are attracted to clone
  - Verify clone despawns after duration
  - Test max active clones limit
```

#### Shadow Blend
```
✓ Right-click ShadowBlendSkill → Test Activate
  - Verify player becomes transparent (alpha 0.2)
  - Check effect persists while stationary
  - Move player → verify effect breaks
  - Check VFX plays on activation and break
```

#### Andromeda's Veil
```
✓ Right-click AndromedaVeilSkill → Test Activate
  - Verify player becomes nearly invisible (alpha 0.05)
  - Check movement speed reduced to 60%
  - Verify particles follow player
  - Check ambient audio loop plays
  - Confirm duration expires correctly
```

#### Pegasus Dash
```
✓ Right-click PegasusDashSkill → Test Dash
  - Verify teleport to target position
  - Test dash into wall (should stop before)
  - Test dash through thin obstacle (< 1m)
  - Check trail VFX appears
  - Verify SFX plays
```

#### Aquarius Flow
```
✓ Right-click AquariusFlowSkill → Test Activate
  - Verify speed increases to 2x
  - Check player tint changes to blue
  - Confirm particles follow player
  - Verify ambient audio loop plays
  - Test duration (8 beats)
```

### Integration Testing

#### Test Skill Loadout System
1. Open SkillLoadoutManager
2. Assign 4 skills to slots:
   - Slot 1 (Key 1): Orion's Arrow
   - Slot 2 (Key 2): Gemini Clone
   - Slot 3 (Key 3): Shadow Blend
   - Slot 4 (Key 4): Pegasus Dash
3. Play game
4. Press 1-4 to activate skills
5. Verify:
   - Skills consume focus (check Focus bar)
   - Cooldowns apply (check skill UI icons)
   - Perfect combo reduces cooldowns by 33%

#### Test Focus System
```
✓ Perfect timing → +10 focus
✓ Skill activation → -15 to -35 focus
✓ Miss judgment → -15 focus
✓ Focus max: 100
```

---

## Skill Loadout Configuration

### Default Loadouts

#### Tutorial Loadout
```
Slot 1: Capricorn Trap (Attack)
Slot 2: Decoy (Lure)
Slot 3: [Empty - unlocked in Mission 1]
Slot 4: [Empty - unlocked in Mission 2]
```

#### Mission 1-2 Loadout
```
Slot 1: Orion's Arrow (Attack)
Slot 2: Gemini Clone (Lure)
Slot 3: Shadow Blend (Stealth)
Slot 4: Pegasus Dash (Movement)
```

#### Mission 3+ Loadout (All Skills Unlocked)
```
Player chooses 4 from:
- Capricorn Trap, Orion's Arrow (Attack)
- Decoy, Gemini Clone (Lure)
- Shadow Blend, Andromeda's Veil (Stealth)
- Pegasus Dash, Aquarius Flow (Movement)
```

---

## Troubleshooting

### Common Issues

#### "Skill component not found on Player!"
**Solution**: Ensure all 6 skill components are added to Player GameObject.

#### Projectile doesn't move
**Solution**: Check SkillProjectile has Rigidbody2D with Gravity Scale: 0.

#### Clone doesn't patrol
**Solution**: Verify CloneController.Initialize() is called with valid facing direction.

#### Skills don't consume focus
**Solution**: Check RhythmPatternChecker.focusCostPerSkill is set (default: 15).

#### VFX doesn't appear
**Solution**:
1. Check VFXManager is in scene
2. Verify ObjectPoolManager is set up
3. Assign VFX prefabs in skill inspector

#### No sound plays
**Solution**:
1. Check SFXManager is in scene
2. Verify AudioSource on SFXManager
3. Assign audio clips in skill inspector

---

## Performance Checklist

After setup, verify performance:

```
✓ All skills use Object Pooling (check ObjectPoolManager)
✓ Particle systems have Max Particles limit (< 100)
✓ Audio clips are compressed
✓ No skills allocate GC during activation
✓ Frame time impact < 2ms per skill activation
```

---

## Next Steps

After Unity setup is complete:

1. **Create Skill Icons** (8 constellation-themed icons)
2. **Design VFX** (polish particle effects)
3. **Source Audio** (professional SFX)
4. **Mission Integration** (add skill unlock progression)
5. **Balance Testing** (adjust cooldowns, focus costs)
6. **Tutorial Update** (teach new skill mechanics)

---

## Quick Reference: File Locations

```
Code:
  Assets/Scripts/Skills/
    ├── OrionsArrowSkill.cs
    ├── SkillProjectile.cs
    ├── GeminiCloneSkill.cs
    ├── CloneController.cs
    ├── ShadowBlendSkill.cs
    ├── AndromedaVeilSkill.cs
    ├── PegasusDashSkill.cs
    └── AquariusFlowSkill.cs

ScriptableObjects:
  Assets/Data/Skills/
    ├── Capricorn_Trap.asset
    ├── Orion_Arrow.asset
    ├── Decoy.asset
    ├── Gemini_Clone.asset
    ├── Shadow_Blend.asset
    ├── Andromeda_Veil.asset
    ├── Pegasus_Dash.asset
    └── Aquarius_Flow.asset

Prefabs:
  Assets/Prefabs/Skills/
    ├── Orion_Arrow_Projectile.prefab
    ├── Gemini_Clone.prefab
    └── [VFX prefabs...]

Audio:
  Assets/Audio/Skills/
    ├── SFX/
    └── Ambient/
```

---

**Setup Time Estimate**: 3-4 hours for complete integration and testing

**Status**: Ready for Unity Editor configuration ✅
