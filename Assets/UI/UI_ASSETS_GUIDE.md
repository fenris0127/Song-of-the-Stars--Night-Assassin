# UI Assets Guide
## Song of the Stars - Night Assassin

This guide describes all UI sprites, fonts, and assets needed for the game.

---

## 📁 Folder Structure

```
Assets/UI/
├── Sprites/
│   ├── Buttons/
│   ├── Panels/
│   ├── Icons/
│   ├── HUD/
│   ├── Timing/
│   └── Backgrounds/
├── Fonts/
└── Animations/
```

---

## 🎨 Required UI Sprites

### 1. **Buttons** (`Assets/UI/Sprites/Buttons/`)

#### Standard Button States (3 sprites per button):
- `Button_Normal.png` (256x64px)
- `Button_Hover.png` (256x64px)
- `Button_Pressed.png` (256x64px)
- `Button_Disabled.png` (256x64px)

**Style Guide:**
- Background: Dark blue/purple gradient (#1a1a2e → #0f0f1e)
- Border: Cyan glow (#00d4ff)
- Text: White (#ffffff) center-aligned

#### Special Buttons:
- `Button_Primary.png` - For main actions (green accent)
- `Button_Danger.png` - For destructive actions (red accent)
- `Button_Small.png` (128x48px) - For compact UI
- `Button_Icon.png` (64x64px) - Icon-only buttons

### 2. **Panels** (`Assets/UI/Sprites/Panels/`)

- `Panel_Background.png` (512x512px, 9-slice capable)
  - Dark semi-transparent (#0a0a15, 85% opacity)
  - Soft border glow

- `Panel_Header.png` (512x64px, 9-slice)
  - Gradient top (#1a1a2e → #0f0f1e)
  - Bottom separator line

- `Panel_Window.png` (1024x768px, 9-slice)
  - Main window background
  - Corner decorations

- `Panel_Tooltip.png` (256x128px, 9-slice)
  - Small tooltip background
  - Pointer/arrow edge

### 3. **HUD Elements** (`Assets/UI/Sprites/HUD/`)

#### Focus Bar:
- `FocusBar_Background.png` (256x32px)
- `FocusBar_Fill.png` (256x32px)
  - Gradient: Cyan (#00d4ff) to Blue (#0080ff)
  - Animated shimmer overlay
- `FocusBar_Border.png` (256x32px)

#### Health/Alert Bar:
- `AlertBar_Background.png` (256x32px)
- `AlertBar_Fill.png` (256x32px)
  - Gradient: Yellow (#ffff00) to Red (#ff0000)

#### Combo Counter:
- `ComboCounter_Background.png` (128x128px)
  - Circular design
- `ComboCounter_Glow.png` (128x128px)
  - Additive glow layer

### 4. **Timing Indicators** (`Assets/UI/Sprites/Timing/`)

Critical for rhythm gameplay!

- `Timing_Perfect.png` (256x256px)
  - Gold/cyan burst effect
  - "PERFECT" text integrated

- `Timing_Great.png` (256x256px)
  - Blue burst
  - "GREAT" text

- `Timing_Good.png` (256x256px)
  - Green effect
  - "GOOD" text

- `Timing_Miss.png` (256x256px)
  - Red X effect
  - "MISS" text

- `Beat_Indicator.png` (64x64px)
  - Pulsing circle
  - Used for beat visualization

- `Input_Window.png` (128x128px)
  - Perfect timing zone indicator
  - Shrinking/expanding animation

### 5. **Icons** (`Assets/UI/Sprites/Icons/`)

#### Generic Icons (32x32px):
- `Icon_Settings.png` - Gear
- `Icon_Save.png` - Floppy disk
- `Icon_Load.png` - Folder
- `Icon_Exit.png` - Door/arrow
- `Icon_Info.png` - Information (i)
- `Icon_Warning.png` - Exclamation mark
- `Icon_Success.png` - Checkmark
- `Icon_Locked.png` - Padlock
- `Icon_Unlocked.png` - Open padlock

#### Gameplay Icons (64x64px):
- `Icon_Focus.png` - Star/energy symbol
- `Icon_Stealth.png` - Eye with slash
- `Icon_Detection.png` - Alert eye
- `Icon_Objective.png` - Target crosshair
- `Icon_Score.png` - Trophy/star
- `Icon_Time.png` - Clock

#### Rank Icons (128x128px):
- `Rank_S.png` - Gold S with sparkles
- `Rank_A.png` - Silver A
- `Rank_B.png` - Bronze B
- `Rank_C.png` - Iron C
- `Rank_D.png` - Gray D
- `Rank_F.png` - Dark red F

### 6. **Backgrounds** (`Assets/UI/Sprites/Backgrounds/`)

- `Menu_Background.png` (1920x1080px)
  - Starry night sky
  - Constellation outlines
  - Parallax-ready (separate layers)

- `Mission_Background_Tutorial.png` (1920x1080px)
- `Mission_Background_01.png` (1920x1080px)
- `Mission_Background_02.png` (1920x1080px)

---

## 🔤 Fonts

### Primary Font: **Orbitron** (or similar sci-fi font)
Download: https://fonts.google.com/specimen/Orbitron

**Usage:**
- Titles: Bold, 48-72pt
- Headers: Medium, 32-48pt
- Body: Regular, 16-24pt
- Small text: Regular, 12-14pt

**Import Settings in Unity:**
- Character Set: Unicode
- Font Size: 128 (for crisp scaling)
- Font Rendering Mode: Smooth

### Secondary Font: **Roboto** (for readability)
Download: https://fonts.google.com/specimen/Roboto

**Usage:**
- Descriptions: Regular, 14-18pt
- Tooltips: Light, 12-14pt

---

## 🎭 Sprite Atlas Configuration

### Create Sprite Atlases for Performance:

1. **UI_Main_Atlas** (2048x2048)
   - All buttons
   - All panels
   - Common icons

2. **UI_HUD_Atlas** (1024x1024)
   - Focus bars
   - Alert bars
   - Combo counters
   - Timing indicators

3. **UI_Icons_Atlas** (512x512)
   - All small icons
   - Rank badges

**Unity Import Settings:**
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single (or Multiple for atlas sheets)
Pixels Per Unit: 100
Filter Mode: Bilinear
Max Size: 2048
Compression: RGBA Crunched
```

---

## 🌟 Color Palette

### Primary Colors:
```css
Dark Background:    #0a0a15
Secondary Dark:     #1a1a2e
Accent Cyan:        #00d4ff
Accent Blue:        #0080ff
Accent Purple:      #8b5cf6
```

### UI States:
```css
Normal:             #00d4ff (Cyan)
Hover:              #33e0ff (Lighter Cyan)
Pressed:            #0099cc (Darker Cyan)
Disabled:           #4a4a5e (Gray)
Success:            #10b981 (Green)
Warning:            #f59e0b (Orange)
Error:              #ef4444 (Red)
```

### Timing Feedback:
```css
Perfect:            #ffd700 (Gold) / #00d4ff (Cyan)
Great:              #3b82f6 (Blue)
Good:               #10b981 (Green)
Miss:               #ef4444 (Red)
```

---

## 📐 Standard Sizes

### Buttons:
- Large: 256x64px
- Medium: 192x48px
- Small: 128x40px
- Icon: 64x64px

### Panels:
- Full screen: 1920x1080px
- Large window: 1024x768px
- Medium panel: 512x512px
- Small tooltip: 256x128px

### Icons:
- Tiny: 16x16px (not recommended, use 32x32)
- Small: 32x32px
- Medium: 64x64px
- Large: 128x128px
- Rank/Badge: 256x256px

### HUD Elements:
- Bars: 256x32px (fill area: 250x28px)
- Circular: 128x128px
- Indicators: 64x64px

---

## 🎨 Creating Placeholder Assets

If you don't have final art, create simple placeholder sprites:

### Using Photoshop/GIMP:
1. Create new image with required dimensions
2. Fill with base color from palette
3. Add simple gradient
4. Add border/glow effect
5. Export as PNG with transparency

### Using Unity:
1. Create solid color texture
2. Apply UI shader
3. Use as temporary placeholder

### Using Free Tools:
- **Figma** (free web-based): https://figma.com
- **Pixlr** (free web-based): https://pixlr.com
- **GIMP** (free desktop): https://gimp.org

---

## 📦 Asset Organization

```
Assets/UI/
├── Sprites/
│   ├── Atlas_Main.spriteatlas
│   ├── Atlas_HUD.spriteatlas
│   └── Atlas_Icons.spriteatlas
├── Fonts/
│   ├── Orbitron-Bold.ttf
│   ├── Orbitron-Medium.ttf
│   ├── Orbitron-Regular.ttf
│   └── Roboto-Regular.ttf
└── Materials/
    ├── UI_Default.mat
    └── UI_Glow.mat
```

---

## ✅ Quick Start Checklist

### Minimum Viable UI (for testing):

- [ ] 3 button sprites (normal, hover, pressed)
- [ ] 1 panel background (9-slice)
- [ ] Focus bar (background + fill + border)
- [ ] 4 timing indicators (Perfect, Great, Good, Miss)
- [ ] Beat indicator sprite
- [ ] 6 rank icons (S, A, B, C, D, F)
- [ ] Primary font (Orbitron)
- [ ] 10 basic icons (settings, save, load, etc.)

**Time to create**: ~2-3 hours with basic tools

### Full Production UI:

- [ ] All button variations and states
- [ ] Complete panel set with decorations
- [ ] Animated HUD elements
- [ ] Particle effects for timing feedback
- [ ] Animated backgrounds
- [ ] Custom cursor sprites
- [ ] Loading screen assets
- [ ] Tutorial overlay graphics

**Time to create**: ~20-40 hours with artist

---

## 🔗 Recommended Free Asset Packs

If you want to speed up development:

1. **Kenney UI Pack** (Free)
   - https://kenney.nl/assets/ui-pack
   - 450+ UI elements
   - Clean, modern style

2. **Game Icons** (Free, CC BY 3.0)
   - https://game-icons.net
   - 4000+ SVG icons
   - Export as PNG at any size

3. **Google Fonts** (Free)
   - https://fonts.google.com
   - Orbitron, Roboto, Exo 2

---

## 🎬 Animation Guidelines

### Button Animations:
- Hover: Scale 1.0 → 1.05, 0.2s
- Press: Scale 1.0 → 0.95, 0.1s
- Glow pulse: 0.8s cycle

### Timing Indicators:
- Perfect: Scale burst 0.5 → 1.5, fade out 0.3s
- Beat pulse: Scale 1.0 → 1.2 → 1.0, match BPM

### Panel Transitions:
- Open: Fade + scale (0.8 → 1.0), 0.3s
- Close: Fade + scale (1.0 → 0.8), 0.2s

---

## 📝 Notes

- All sprites should have **transparency** (PNG format)
- Use **9-slice** for scalable panels and bars
- Keep **consistent padding** (8px, 16px, 32px)
- Design for **1920x1080** as base resolution
- Test at **1280x720** for minimum support
- Use **sprite atlases** to reduce draw calls
- Animate with **DOTween** or Unity Animator

---

**Last Updated**: 2025-01-17
**For**: Song of the Stars - Night Assassin
**Unity Version**: 6.0.2+
