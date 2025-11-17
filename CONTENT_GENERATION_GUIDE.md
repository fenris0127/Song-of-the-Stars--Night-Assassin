# Content Generation Guide
## Song of the Stars - Night Assassin

Complete step-by-step guide to generate all game content automatically.

---

## 📋 Overview

This project includes **automated content generation** tools that will create:

- ✅ **8 Constellation Skills** - Fully configured ScriptableObjects
- ✅ **3 Missions** - Tutorial + 2 playable missions
- ✅ **8 Placeholder Skill Icons** - Colored gradient circles
- ✅ **Audio Folder Structure** - Organized folders with README guides

**Total Time**: ~30 seconds (automatic)
**Manual Time Saved**: ~5 hours

---

## 🚀 Quick Start (ONE CLICK!)

### Method 1: Generate Everything at Once

1. **Open Unity Editor** (Unity 6.0.2+)

2. **Open Content Initializer Window**
   - Menu: `Song of the Stars` → `Content` → `Initialize All Content`

3. **Click "🚀 GENERATE ALL CONTENT"**
   - Wait 30 seconds
   - Done! ✅

That's it! Skip to the [Verification](#verification) section.

---

## 🔧 Method 2: Step-by-Step Generation

If you prefer to generate content piece by piece:

### Step 1: Generate Skills (8 assets)

**Option A: Using Content Initializer**
1. Menu: `Song of the Stars` → `Content` → `Initialize All Content`
2. Click **"1. Generate Skills"**
3. Wait for completion message

**Option B: Using Direct Menu**
1. Menu: `Song of the Stars` → `Content` → `1. Generate Skills Only`
2. Confirm dialog
3. Check `Assets/Data/Skills/`

**What Gets Created:**
```
Assets/Data/Skills/
├── CapricornTrap.asset
├── OrionsArrow.asset
├── LeoDecoy.asset
├── GeminiClone.asset
├── ShadowBlend.asset
├── AndromedaVeil.asset
├── PegasusDash.asset
└── AquariusFlow.asset
```

Each skill is **fully configured** with:
- Name, description, category
- Input count, cooldown, focus cost
- Range, speed, damage values
- Balanced for gameplay

---

### Step 2: Generate Missions (3 assets)

**Option A: Using Content Initializer**
1. Menu: `Song of the Stars` → `Content` → `Initialize All Content`
2. Click **"2. Generate Missions"**
3. Wait for completion

**Option B: Using Direct Menu**
1. Menu: `Song of the Stars` → `Content` → `2. Generate Missions Only`
2. Confirm dialog
3. Check `Assets/Data/Missions/`

**What Gets Created:**
```
Assets/Data/Missions/
├── Tutorial_FirstSteps.asset
├── Mission_01_SilentApproach.asset
└── Mission_02_NightMarket.asset
```

Each mission includes:
- Primary objectives (2-4)
- Optional objectives
- Difficulty settings
- Time limits
- BPM settings
- Rank thresholds

---

### Step 3: Generate Placeholder Icons (8 images)

**Using Content Initializer:**
1. Menu: `Song of the Stars` → `Content` → `Initialize All Content`
2. Click **"3. Generate Placeholder Icons"**
3. Wait for completion

**What Gets Created:**
```
Assets/Art/Icons/Skills/
├── CapricornTrap_Icon.png (128x128, gold gradient)
├── OrionsArrow_Icon.png (128x128, blue gradient)
├── LeoDecoy_Icon.png (128x128, orange gradient)
├── GeminiClone_Icon.png (128x128, purple gradient)
├── ShadowBlend_Icon.png (128x128, dark gradient)
├── AndromedaVeil_Icon.png (128x128, cyan gradient)
├── PegasusDash_Icon.png (128x128, red gradient)
└── AquariusFlow_Icon.png (128x128, teal gradient)
```

Icons are:
- 128x128 pixels
- Circular gradient design
- Color-coded by skill type
- Ready to use in UI

---

### Step 4: Setup Audio Structure

**Using Content Initializer:**
1. Menu: `Song of the Stars` → `Content` → `Initialize All Content`
2. Click **"4. Setup Audio Structure"**
3. Wait for completion

**What Gets Created:**
```
Assets/Audio/
├── Music/
│   └── README.txt (lists required music files)
├── SFX/
│   ├── Skills/
│   │   └── README.txt (skill sound requirements)
│   ├── Combat/
│   │   └── README.txt
│   ├── UI/
│   │   └── README.txt
│   ├── Ambient/
│   ├── Footsteps/
│   └── Environmental/
```

Each README.txt contains:
- Required audio file names
- Recommended formats (.ogg, .mp3, .wav)
- BPM requirements for music
- Import settings for Unity

---

## ✅ Verification

### Check 1: Skills Created

1. Navigate to `Assets/Data/Skills/`
2. Should see 8 `.asset` files
3. Click on any skill asset
4. Inspector should show all fields populated

**Example: OrionsArrow.asset**
```
Skill Name: Orion's Arrow
Category: Attack
Focus Cost: 30
Cooldown Beats: 16
Range: 15
Damage: 999 (instant kill)
```

### Check 2: Missions Created

1. Navigate to `Assets/Data/Missions/`
2. Should see 3 `.asset` files
3. Click on any mission
4. Verify objectives are listed

**Example: Tutorial_FirstSteps.asset**
```
Mission Name: First Steps
Difficulty: Tutorial
BPM: 100
Objectives: 4 primary objectives
```

### Check 3: Icons Created

1. Navigate to `Assets/Art/Icons/Skills/`
2. Should see 8 `.png` files
3. Select any icon
4. Preview should show colored circle

**Import Settings:**
- Texture Type: Sprite (2D and UI)
- Max Size: 256

### Check 4: Audio Folders

1. Navigate to `Assets/Audio/`
2. Should see 7 folders
3. Each folder has a README.txt
4. Open README files to see requirements

---

## 🔗 Linking Assets to Systems

### After content is generated, you need to assign it:

### 1. Link Skills to Skill System

**Create a SkillDatabase ScriptableObject:**

1. Right-click in Project
2. Create → Song of the Stars → Skill Database
3. Name it: `AllSkills_Database`
4. In Inspector, set size to 8
5. Drag all 8 skill assets into the array

**Assign to GameManager:**
```csharp
// In your game initialization
SkillDatabase skillDB = Resources.Load<SkillDatabase>("AllSkills_Database");
```

### 2. Link Missions to Mission System

**Create a Mission List:**

Create a `MissionManager` prefab with:
- Drag Tutorial mission to slot 0
- Drag Mission 01 to slot 1
- Drag Mission 02 to slot 2

Or use a ScriptableObject list:
```csharp
[CreateAssetMenu]
public class MissionList : ScriptableObject
{
    public List<MissionData> missions;
}
```

### 3. Link Icons to Skills

**Automatic (Recommended):**

Create a script that auto-assigns icons by name matching:

```csharp
// AutoAssignIcons.cs (Editor script)
[MenuItem("Song of the Stars/Tools/Auto-Assign Skill Icons")]
static void AutoAssignIcons()
{
    string[] guids = AssetDatabase.FindAssets("t:ConstellationSkillData");

    foreach (string guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        ConstellationSkillData skill = AssetDatabase.LoadAssetAtPath<ConstellationSkillData>(path);

        // Find matching icon
        string iconName = skill.name + "_Icon";
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/Icons/Skills/{iconName}.png");

        if (icon != null)
        {
            skill.icon = icon;
            EditorUtility.SetDirty(skill);
        }
    }

    AssetDatabase.SaveAssets();
    Debug.Log("Icons auto-assigned!");
}
```

**Manual:**

1. Open each skill asset
2. Drag corresponding icon to "Icon" field
3. Save

---

## 🎨 Replacing Placeholder Assets

### Replace Placeholder Icons

When you have final art:

1. Create new icons with same names:
   - `CapricornTrap_Icon.png`
   - `OrionsArrow_Icon.png`
   - etc.

2. **Overwrite** the placeholder files in `Assets/Art/Icons/Skills/`

3. Unity will auto-update all references! ✨

**Recommended Icon Specs:**
- Size: 256x256 or 512x512
- Format: PNG with transparency
- Style: Match the constellation theme
- Include subtle glow/star effects

### Add Audio Files

Follow the README.txt guides in each audio folder:

**Example: Assets/Audio/Music/README.txt**
```
Required music files:
- Tutorial_Music.ogg (100 BPM)
- Mission01_Music.ogg (120 BPM)
- Mission02_Music.ogg (130 BPM)
```

1. Create or source music tracks
2. Name them **exactly** as specified
3. Drop into the folder
4. Unity will auto-detect

**Import Settings:**
- Load Type: Streaming (for music)
- Compression: Vorbis
- Quality: 0.7-1.0

---

## 🎯 Testing Generated Content

### Test 1: Skill Instantiation

```csharp
// Create test scene
public class SkillTest : MonoBehaviour
{
    public ConstellationSkillData testSkill;

    void Start()
    {
        Debug.Log($"Skill: {testSkill.skillName}");
        Debug.Log($"Cost: {testSkill.focusCost}");
        Debug.Log($"Cooldown: {testSkill.cooldownBeats} beats");
    }
}
```

1. Create empty GameObject
2. Add SkillTest script
3. Drag any skill asset to "Test Skill"
4. Play scene
5. Check Console for output

### Test 2: Mission Loading

```csharp
public class MissionTest : MonoBehaviour
{
    public MissionData testMission;

    void Start()
    {
        Debug.Log($"Mission: {testMission.missionName}");
        Debug.Log($"Objectives: {testMission.primaryObjectives.Count}");
        Debug.Log($"BPM: {testMission.musicBPM}");
    }
}
```

Same process as skill test.

### Test 3: Icon Display

1. Create Canvas
2. Add Image component
3. Drag skill icon to Image → Source Image
4. Play scene
5. Should see the icon displayed

---

## 🔧 Troubleshooting

### ❌ "Menu item not found"

**Problem**: Can't find `Song of the Stars` menu

**Solution**:
1. Close Unity
2. Delete `Library/` folder
3. Reopen Unity (will regenerate)
4. Wait for script compilation
5. Menu should appear

### ❌ "Script compilation errors"

**Problem**: Red errors in Console

**Solution**:
1. Check all scripts are in correct folders
2. Ensure `SkillDataPopulator.cs` is in `Assets/Scripts/Data/`
3. Ensure `MissionDataPopulator.cs` is in `Assets/Scripts/Data/`
4. Ensure `ContentInitializer.cs` is in `Assets/Scripts/Editor/`
5. Let Unity recompile

### ❌ "No skills created"

**Problem**: Menu runs but no assets appear

**Solution**:
1. Check Console for error messages
2. Ensure `Assets/Data/Skills/` folder exists
3. Run `Song of the Stars/Content/1. Generate Skills Only`
4. Check Project window and click Refresh

### ❌ "Icons are black/missing"

**Problem**: Icon textures appear black

**Solution**:
1. Select icon in Project
2. Check Import Settings:
   - Texture Type: **Sprite (2D and UI)**
   - sRGB: **Enabled**
3. Click Apply
4. Icons should appear correctly

---

## 📊 Content Summary

After running all generation:

| Asset Type | Count | Location | Time Saved |
|------------|-------|----------|------------|
| Skills | 8 | Assets/Data/Skills/ | ~2 hours |
| Missions | 3 | Assets/Data/Missions/ | ~3 hours |
| Icons | 8 | Assets/Art/Icons/Skills/ | ~30 mins |
| Audio Folders | 7 | Assets/Audio/ | ~15 mins |
| **TOTAL** | **26 items** | - | **~5.75 hours** |

---

## 🎓 Next Steps

After content generation is complete:

1. ✅ **Verify all assets** (see Verification section above)

2. ✅ **Link assets to systems**
   - Create SkillDatabase
   - Create MissionList
   - Auto-assign icons

3. ✅ **Create test scene**
   - Add GameManager
   - Add SkillSystem
   - Add MissionController
   - Test skill activation

4. ✅ **Replace placeholders**
   - Create/source final icons
   - Add music tracks
   - Add SFX files

5. ✅ **Build first level**
   - Use Tutorial mission data
   - Create level layout
   - Test gameplay loop

---

## 📚 Additional Resources

### Related Documentation:
- `DATA_SETUP_GUIDE.md` - Detailed data structure info
- `UI_ASSETS_GUIDE.md` - Complete UI sprite requirements
- `SKILLS_DESIGN.md` - Skill design specifications
- `MISSION_DESIGNS.md` - Mission design details

### Unity Menus:
- `Song of the Stars/Content/` - All content generation
- `Song of the Stars/Data/` - Data population tools
- `Song of the Stars/Tools/` - Utility scripts

### Asset Folders:
- `Assets/Data/` - All ScriptableObject data
- `Assets/Art/` - Visual assets (icons, sprites)
- `Assets/Audio/` - Audio files (music, SFX)
- `Assets/UI/` - UI sprites and fonts

---

## ✨ Pro Tips

### Tip 1: Batch Operations
Run all content generation at once for consistency:
```
Song of the Stars → Content → Initialize All Content → Generate All
```

### Tip 2: Version Control
After generating content:
```bash
git add Assets/Data Assets/Art Assets/Audio
git commit -m "Generate initial game content"
```

### Tip 3: Backup Before Regenerating
If you modify generated assets and want to regenerate:
1. Duplicate modified assets
2. Run generation (overwrites)
3. Manually merge changes

### Tip 4: Custom Content
To add more skills/missions:
1. Duplicate existing asset
2. Rename
3. Modify values
4. Add to SkillDatabase/MissionList

---

**Total Generation Time**: ~30 seconds
**Time Saved vs Manual**: ~5-6 hours
**Efficiency Gain**: 98%

**Ready to generate content!** 🚀

---

**Last Updated**: 2025-01-17
**For**: Song of the Stars - Night Assassin
**Unity Version**: 6.0.2+
