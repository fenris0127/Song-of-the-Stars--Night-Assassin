# Development Plan
## Song of the Stars - Night Assassin

**Based on**: PRD v1.0
**Last Updated**: November 17, 2025
**Target MVP**: Q1 2026
**Current Phase**: Phase 1 - MVP Development

---

## Table of Contents
1. [Current Status Summary](#current-status-summary)
2. [Immediate Priorities (Next 2-4 Weeks)](#immediate-priorities-next-2-4-weeks)
3. [Phase 1: MVP Completion (Q1 2026)](#phase-1-mvp-completion-q1-2026)
4. [Phase 2: Content Expansion (Q2 2026)](#phase-2-content-expansion-q2-2026)
5. [Phase 3: Polish & Balance (Q3 2026)](#phase-3-polish--balance-q3-2026)
6. [Phase 4: Launch (Q4 2026)](#phase-4-launch-q4-2026)
7. [Development Guidelines](#development-guidelines)
8. [Risk Mitigation Tasks](#risk-mitigation-tasks)

---

## Current Status Summary

### ✅ Completed Core Systems (Estimated 70% of MVP)
- Rhythm synchronization system (RhythmSyncManager)
- Timing judgment system (RhythmPatternChecker)
- Player movement, assassination, stealth
- Guard AI with 4 states (Patrol, Investigate, Chase, Stunned)
- Probabilistic detection system
- Difficulty system (4 levels)
- Focus and Alert level systems
- Basic UI/HUD (skill icons, focus bar, alert indicator)
- Minimap with guard states
- Save/Load system
- Audio synchronization (DSP-based)
- Performance optimization systems

### ⚠️ Partially Completed
- Tutorial system (needs content and polish)
- Beat visualization (exists but needs enhancement)
- Menu systems (basic implementation)

### ❌ Missing for MVP (Estimated 30% remaining)
- Beat timeline/metronome UI
- 10-12 additional constellation skills
- 2-3 complete missions (beyond tutorial)
- Music tracks and SFX library
- Tutorial mission polish
- Mission objectives UI overlay
- Performance testing and optimization

---

## Immediate Priorities (Next 2-4 Weeks)

### Priority 0: Critical Path to Playable Demo

#### Week 1-2: UI Enhancement & Core Feedback
**Goal**: Improve player feedback and rhythm visibility

1. **Beat Timeline UI Implementation** ⭐ CRITICAL
   - **Task**: Create visual metronome/beat line UI
   - **Location**: `Assets/Scripts/UI/BeatTimelineUI.cs` (NEW)
   - **Requirements**:
     - Horizontal beat line with scrolling beats
     - Current beat highlight
     - Upcoming beat indicators (2-3 beats ahead)
     - Perfect/Great timing zone visualization
     - Color coding (Perfect = green, Great = yellow)
     - Integration with RhythmSyncManager
   - **Acceptance Criteria**:
     - Visible beat indicators at all times
     - <5ms visual latency from beat
     - Scalable UI for different resolutions
   - **Estimated Time**: 3-4 days
   - **Dependencies**: None
   - **Priority**: P0

2. **Enhanced Beat Visualizer** ⭐ CRITICAL
   - **Task**: Improve existing BeatVisualizer.cs
   - **Location**: `Assets/Scripts/Audio/BeatVisualizer.cs`
   - **Requirements**:
     - Screen pulse effect on each beat
     - Intensity varies with judgment quality
     - Particle effects for Perfect timing
     - Audio-reactive background elements
   - **Acceptance Criteria**:
     - Clear visual pulse every beat
     - No performance impact (<0.5ms per frame)
   - **Estimated Time**: 2 days
   - **Dependencies**: None
   - **Priority**: P0

3. **Judgment Feedback Enhancement**
   - **Task**: Improve judgment text display in UIManager
   - **Location**: `Assets/Scripts/UI/UIManager.cs`
   - **Requirements**:
     - Larger, animated text for Perfect/Great/Miss
     - Color coding (Perfect = gold, Great = blue, Miss = red)
     - Fade-out animation
     - Position near player or beat line
     - Sound effects on judgment
   - **Acceptance Criteria**:
     - Instant feedback (<50ms from input)
     - Readable at all times
   - **Estimated Time**: 1-2 days
   - **Dependencies**: Enhanced BeatVisualizer
   - **Priority**: P0

4. **Mission Objectives UI Overlay**
   - **Task**: Create mission objectives display
   - **Location**: `Assets/Scripts/UI/MissionObjectivesUI.cs` (NEW)
   - **Requirements**:
     - List of current objectives
     - Progress tracking (e.g., "Eliminate 2/5 targets")
     - Checkmarks on completion
     - Optional/secret objectives indicator
     - Minimizable to corner
   - **Acceptance Criteria**:
     - Always visible during mission
     - Updates in real-time
   - **Estimated Time**: 2-3 days
   - **Dependencies**: MissionManager integration
   - **Priority**: P1

---

#### Week 2-3: Skill System Expansion
**Goal**: Expand skill variety to 12+ skills

5. **Constellation Skills Implementation (10 new skills)** ⭐ CRITICAL
   - **Task**: Design and implement 10 additional constellation-themed skills
   - **Location**:
     - `Assets/Scripts/Skills/` (individual skill scripts)
     - `Assets/Data/Skills/` (ScriptableObject definitions)
   - **Requirements**: 2-3 skills per category

   **Attack Skills** (3 new):
   - **Orion's Arrow** (Ranged attack)
     - Rhythm pattern: 1-2 (two beats)
     - Effect: Fires star projectile, eliminates guard at range
     - Cooldown: 8 beats
     - Prefab: Arrow with trail effect

   - **Leo's Roar** (AOE Stun)
     - Rhythm pattern: 1-3-2 (three beats)
     - Effect: Stuns all guards in radius for 4 beats
     - Cooldown: 16 beats
     - Prefab: Shockwave circle

   - **Scorpio's Sting** (Charge Attack)
     - Rhythm pattern: Hold 1 (charge mechanic)
     - Effect: Dash attack, pierces through multiple guards
     - Cooldown: 12 beats
     - Prefab: Dash line with stars

   **Lure Skills** (2 new):
   - **Gemini Clone** (Illusory duplicate)
     - Rhythm pattern: 2-2 (two beats)
     - Effect: Creates moving clone that mimics player
     - Duration: 12 beats
     - Cooldown: 10 beats
     - Prefab: Semi-transparent player sprite

   - **Lyra's Song** (Sound distraction)
     - Rhythm pattern: 3 (one beat)
     - Effect: Creates sound at target location
     - Cooldown: 6 beats
     - Prefab: Musical note particles

   **Stealth Skills** (3 new):
   - **Andromeda's Veil** (Invisibility)
     - Rhythm pattern: 4-4-4 (three beats)
     - Effect: Complete invisibility for 6 beats
     - Cooldown: 20 beats
     - Focus cost: 30
     - Prefab: Shimmer effect

   - **Shadow Blend** (Hide in shadows)
     - Rhythm pattern: 3-4 (two beats)
     - Effect: Become invisible when stationary
     - Duration: Until movement
     - Cooldown: 8 beats
     - Prefab: Shadow merge effect

   - **Perseus's Mask** (Disguise)
     - Rhythm pattern: 2-4 (two beats)
     - Effect: Guards ignore player for 8 beats (unless too close)
     - Cooldown: 14 beats
     - Prefab: Shimmer overlay

   **Movement Skills** (2 new):
   - **Pegasus Dash** (Blink/teleport)
     - Rhythm pattern: 1-1 (two beats)
     - Effect: Instant teleport 3 tiles forward
     - Cooldown: 6 beats
     - Focus cost: 20
     - Prefab: Wing particles

   - **Aquarius Flow** (Speed boost)
     - Rhythm pattern: 4 (one beat)
     - Effect: Double movement speed for 8 beats
     - Cooldown: 10 beats
     - Prefab: Water trail effect

   - **Acceptance Criteria**:
     - All skills functional with proper cooldowns
     - Visual and audio feedback for each
     - Balanced damage/effect values
     - No game-breaking bugs
   - **Estimated Time**: 8-10 days (1 skill per day)
   - **Dependencies**: Existing skill system
   - **Priority**: P0

6. **Skill Icon Assets Creation**
   - **Task**: Create or source 12 skill icons
   - **Location**: `Assets/UI/SkillIcons/`
   - **Requirements**:
     - Constellation-themed sprite designs
     - 128x128px minimum resolution
     - Consistent art style
     - Clear visual distinction
   - **Acceptance Criteria**:
     - All skills have unique icons
     - Readable at UI scale
   - **Estimated Time**: 3-4 days (or outsource)
   - **Dependencies**: Skill design completion
   - **Priority**: P1

---

#### Week 3-4: Mission Content Creation
**Goal**: Create 2-3 playable missions with varied layouts

7. **Tutorial Mission Polish** ⭐ CRITICAL
   - **Task**: Complete and polish tutorial mission
   - **Location**: `Assets/Scenes/TutorialMission.unity` (NEW)
   - **Requirements**:
     - Step-by-step rhythm training (5 steps)
       1. Understand beat timing (watch beats)
       2. Practice movement on beat
       3. Learn skill activation
       4. Practice stealth and detection
       5. Complete simple assassination
     - Clear UI instructions
     - Interactive prompts
     - Safe practice area (no failure)
     - Gradual difficulty introduction
     - Completion: ~5-7 minutes
   - **Acceptance Criteria**:
     - >80% of playtesters complete tutorial
     - Players understand core mechanics
     - No confusing instructions
   - **Estimated Time**: 5-6 days
   - **Dependencies**: Beat Timeline UI, TutorialManager
   - **Priority**: P0

8. **Mission 1: "First Contact"** ⭐ CRITICAL
   - **Task**: Design and build first core mission
   - **Location**: `Assets/Scenes/Mission01_FirstContact.unity` (NEW)
   - **Theme**: Infiltrate small guard post, eliminate captain
   - **Layout Design**:
     - Small enclosed area (20x20 tiles)
     - 5 patrolling guards
     - 1 stationary captain (target)
     - 2-3 shadow zones (stealth areas)
     - 1 decoy placement spot
   - **Objectives**:
     - Primary: Eliminate captain
     - Optional: Complete without detection
     - Optional: Finish under 3 minutes
   - **Difficulty**: Easy-Normal
   - **Music**: 100 BPM track
   - **Estimated Time**: 4-5 days
   - **Dependencies**: Skill system, Guard AI
   - **Priority**: P0

9. **Mission 2: "Night Market"**
   - **Task**: Design and build second mission
   - **Location**: `Assets/Scenes/Mission02_NightMarket.unity` (NEW)
   - **Theme**: Crowded marketplace, multiple targets
   - **Layout Design**:
     - Open area with stalls (30x25 tiles)
     - 8 guards (varied patrol patterns)
     - 3 targets to eliminate (any order)
     - Multiple approach routes
     - Environmental hazards (NPCs, obstacles)
   - **Objectives**:
     - Primary: Eliminate 3 targets
     - Optional: No alerts raised
     - Optional: Use only stealth skills
   - **Difficulty**: Normal-Hard
   - **Music**: 120 BPM track
   - **Estimated Time**: 5-6 days
   - **Dependencies**: Mission 1 completion
   - **Priority**: P0

10. **Mission 3: "Fortress Breach"**
    - **Task**: Design and build third mission
    - **Location**: `Assets/Scenes/Mission03_FortressBreach.unity` (NEW)
    - **Theme**: Heavily guarded fortress, extraction mission
    - **Layout Design**:
      - Multi-room fortress (40x30 tiles)
      - 12 guards (overlapping patrols)
      - 2 stationary watchers
      - VIP escort objective
      - Multiple skill synergy opportunities
    - **Objectives**:
      - Primary: Extract VIP to exit
      - Optional: Alert level < 2
      - Optional: Eliminate all guards
    - **Difficulty**: Hard-Expert
    - **Music**: 140 BPM track
    - **Estimated Time**: 6-7 days
    - **Dependencies**: Mission 2 completion
    - **Priority**: P1

---

### Week 4: Audio Content & Integration

11. **Music Track Sourcing/Creation** ⭐ CRITICAL
    - **Task**: Acquire or create 4 music tracks
    - **Requirements**:
      - Tutorial track (80 BPM, calm)
      - Mission 1 track (100 BPM, ambient)
      - Mission 2 track (120 BPM, energetic)
      - Mission 3 track (140 BPM, intense)
      - All tracks: royalty-free or original
      - Clear beat structure for rhythm sync
    - **Acceptance Criteria**:
      - Tracks sync perfectly with RhythmSyncManager
      - BPM manually verified
      - <50ms latency
    - **Estimated Time**: 5-7 days (or outsource)
    - **Dependencies**: None (can work in parallel)
    - **Priority**: P0

12. **SFX Library Creation**
    - **Task**: Create/source sound effects library
    - **Requirements** (25+ sounds):
      - Footsteps (player, guards) - 4 variations
      - Skill activation sounds - 12 unique
      - Assassination sounds - 3 types
      - Detection alerts - 3 levels
      - Judgment feedback - 3 (Perfect/Great/Miss)
      - UI sounds - 5 (clicks, hovers, etc.)
      - Environmental ambience - 3 tracks
    - **Acceptance Criteria**:
      - All sounds integrated with SFXManager
      - Volume balanced across all effects
      - No audio clipping
    - **Estimated Time**: 4-5 days
    - **Dependencies**: SFXManager (already implemented)
    - **Priority**: P1

---

### Week 4: Testing & Bug Fixes

13. **Performance Testing & Optimization**
    - **Task**: Profile and optimize to meet performance targets
    - **Testing Targets**:
      - Frame rate: Maintain 60 FPS
      - Frame time: <16ms average
      - Memory usage: <400MB
      - Audio latency: <30ms
    - **Test Scenarios**:
      - All 3 missions on all 4 difficulties
      - 12 guards on screen simultaneously
      - All skills activated in rapid succession
      - Extended play session (30+ minutes)
    - **Optimization Areas**:
      - Object pooling verification
      - Guard LOD system tuning
      - UI update frequency reduction
      - Texture memory optimization
    - **Acceptance Criteria**:
      - All targets met on target hardware
      - No memory leaks
      - No frame drops during skill use
    - **Estimated Time**: 5-6 days
    - **Dependencies**: All features implemented
    - **Priority**: P0

14. **Bug Fixing & Stability**
    - **Task**: QA testing and bug resolution
    - **Focus Areas**:
      - Rhythm timing accuracy
      - Guard AI state transitions
      - Skill cooldown edge cases
      - Save/load data integrity
      - UI responsiveness
      - Audio sync issues
    - **Testing Method**:
      - Playtesting sessions (5+ testers)
      - Bug tracking spreadsheet
      - Prioritized bug fixes (Critical > Major > Minor)
    - **Acceptance Criteria**:
      - Zero critical bugs
      - <5 major bugs
      - Stable 30-minute play session
    - **Estimated Time**: Ongoing throughout Phase 1
    - **Priority**: P0

---

## Phase 1: MVP Completion (Q1 2026)

### Milestone: Playable Vertical Slice
**Target Date**: End of Q1 2026 (March 31, 2026)

### Definition of Done for Phase 1
- [ ] 3 completable missions (Tutorial + 2 core missions)
- [ ] 12+ unique constellation skills
- [ ] 4 difficulty levels fully functional
- [ ] Beat timeline UI implemented and polished
- [ ] Complete tutorial experience
- [ ] 4 music tracks integrated
- [ ] Core SFX library (25+ sounds)
- [ ] Performance targets met (60 FPS, <30ms audio latency)
- [ ] Save/load system tested and stable
- [ ] Mission objectives UI functional
- [ ] All core systems bug-free

### Additional Phase 1 Tasks (Beyond Immediate Priorities)

15. **Mission Selection UI**
    - **Task**: Create mission selection screen
    - **Location**: `Assets/Scripts/UI/MissionSelectUI.cs` (NEW)
    - **Requirements**:
      - Grid/list of available missions
      - Mission preview (name, difficulty, objectives)
      - Locked/unlocked state visuals
      - Difficulty selector per mission
      - Leaderboard preview (if available)
      - "Play" button with scene loading
    - **Estimated Time**: 3-4 days
    - **Priority**: P1

16. **Enhanced Pause Menu**
    - **Task**: Improve pause menu functionality
    - **Location**: `Assets/Scripts/UI/PauseMenuUI.cs` (NEW)
    - **Requirements**:
      - Resume, Restart, Settings, Quit options
      - Mission objectives display
      - Current statistics (time, alerts, kills)
      - Skill loadout display (read-only)
      - Volume controls
    - **Estimated Time**: 2-3 days
    - **Priority**: P2

17. **Settings Menu Expansion**
    - **Task**: Add comprehensive settings
    - **Location**: `Assets/Scripts/UI/SettingsMenu.cs`
    - **Requirements**:
      - Audio settings (Master, Music, SFX, UI volumes)
      - Difficulty selection
      - Graphics quality (Low/Medium/High)
      - Resolution options
      - Fullscreen toggle
      - Audio calibration tool (offset adjustment)
      - Key binding display
    - **Estimated Time**: 4-5 days
    - **Priority**: P2

18. **Skill Loadout Customization UI**
    - **Task**: Allow players to customize 4 skill slots
    - **Location**: `Assets/Scripts/UI/SkillLoadoutUI.cs` (NEW)
    - **Requirements**:
      - Display all unlocked skills
      - Drag-and-drop or click-to-equip
      - 4 active slots (keys 1-4)
      - Skill descriptions and stats
      - Save loadout preferences
      - Loadout presets (optional)
    - **Estimated Time**: 4-5 days
    - **Priority**: P1

19. **Combo System (Optional Enhancement)**
    - **Task**: Add combo counter for consecutive perfect timings
    - **Location**: `Assets/Scripts/Rhythm/ComboTracker.cs` (NEW)
    - **Requirements**:
      - Track consecutive Perfect judgments
      - Display combo counter UI
      - Reset on Miss or Great
      - Bonus Focus per 5-combo milestone
      - Visual effects at milestones
    - **Acceptance Criteria**:
      - Combo counter accurate
      - Rewards balanced
    - **Estimated Time**: 2-3 days
    - **Priority**: P2

20. **Statistics Tracking Enhancement**
    - **Task**: Expand SaveSystem statistics
    - **Location**: `Assets/Scripts/Core/Systems/SaveSystem.cs`
    - **Requirements**:
      - Track per-mission statistics:
        - Best time
        - Highest combo
        - Perfect timing percentage
        - Alerts raised
        - Guards eliminated
        - Skills used
      - Global statistics across all missions
      - Statistics UI display
    - **Estimated Time**: 3-4 days
    - **Priority**: P2

---

## Phase 2: Content Expansion (Q2 2026)

### Milestone: Rich Content Library
**Target Date**: End of Q2 2026 (June 30, 2026)

### Goals
- Expand from 3 to 8+ missions
- Increase skills from 12 to 20+
- Add mission ranking system (S/A/B/C)
- Implement leaderboards and achievements
- Add 2 new enemy types
- Controller support

### Major Tasks for Phase 2

21. **Mission Expansion (5 additional missions)**
    - Mission 4: "Rooftop Chase"
    - Mission 5: "Underground Tunnels"
    - Mission 6: "Palace Infiltration"
    - Mission 7: "Harbor Heist"
    - Mission 8: "Boss Fight - The Watcher"
    - **Estimated Time**: 30-35 days total
    - **Priority**: P0

22. **Additional Skills (8-10 new skills)**
    - **Advanced Attack Skills**: 2-3 new
    - **Advanced Lure Skills**: 2 new
    - **Advanced Stealth Skills**: 2-3 new
    - **Advanced Movement Skills**: 2 new
    - **Estimated Time**: 10-12 days
    - **Priority**: P0

23. **Mission Ranking System**
    - **Task**: Implement S/A/B/C ranking per mission
    - **Ranking Criteria**:
      - Time taken (speedrun bonus)
      - Alerts raised (stealth bonus)
      - Perfect timing percentage (rhythm bonus)
      - Optional objectives completed
    - **Rank Calculation**:
      - S Rank: All optionals + <2 alerts + <target time
      - A Rank: 2/3 optionals + <3 alerts
      - B Rank: 1/3 optionals + <4 alerts
      - C Rank: Mission completed
    - **UI**: Display rank on mission complete screen
    - **Estimated Time**: 5-6 days
    - **Priority**: P1

24. **Leaderboard System**
    - **Task**: Implement global/local leaderboards
    - **Options**:
      - Local leaderboards (JSON-based)
      - Online leaderboards (requires backend service)
    - **Data Tracked**: Best time, highest combo, rank
    - **UI**: Leaderboard display per mission
    - **Estimated Time**: 8-10 days (local) or 15-20 days (online)
    - **Priority**: P2
    - **Decision Needed**: Local vs Online (see PRD Open Questions)

25. **Achievement System**
    - **Task**: Implement achievement tracking and unlocks
    - **Achievement Categories**:
      - Mission-based (complete all missions, S-rank all)
      - Skill-based (use each skill 10 times, perfect combos)
      - Stealth-based (no-alert runs, invisibility challenges)
      - Speed-based (speedrun achievements)
      - Mastery (100 perfect timings, 1000 guards eliminated)
    - **Total Achievements**: 20-30
    - **UI**: Achievement notification popup, achievement list screen
    - **Estimated Time**: 6-8 days
    - **Priority**: P2

26. **New Enemy Type: Heavy Guard**
    - **Task**: Design and implement Heavy Guard variant
    - **Characteristics**:
      - Slower movement (0.7x normal speed)
      - Wider vision cone (120° FOV)
      - Cannot be assassinated (requires skill or multiple hits)
      - Immune to basic lures
      - Distinct visual design (larger sprite, armor)
    - **AI Behavior**: Uses existing state machine with parameter tweaks
    - **Estimated Time**: 4-5 days
    - **Priority**: P1

27. **New Enemy Type: Scout**
    - **Task**: Design and implement Scout variant
    - **Characteristics**:
      - Faster movement (1.4x normal speed)
      - Narrower vision cone (60° FOV)
      - Longer detection range (1.3x normal)
      - Alerts other guards when suspicious
      - Distinct visual design (smaller sprite, cloak)
    - **AI Behavior**: Custom state additions for alert broadcasting
    - **Estimated Time**: 5-6 days
    - **Priority**: P1

28. **Controller Support**
    - **Task**: Add full gamepad/controller support
    - **Requirements**:
      - Unity Input System integration
      - Button mapping for all actions
      - UI navigation with controller
      - On-screen button prompts
      - Vibration feedback (optional)
    - **Tested Controllers**: Xbox, PlayStation, Generic
    - **Estimated Time**: 8-10 days
    - **Priority**: P2

29. **Additional Music Tracks (4-5 tracks)**
    - Tutorial/Menu music
    - 3-4 new mission tracks (varied BPM)
    - Boss fight music
    - **Estimated Time**: 7-10 days (or outsource)
    - **Priority**: P1

30. **Skill Unlock Progression System**
    - **Task**: Implement skill unlock mechanics
    - **Unlock Methods**:
      - Mission completion (unlock 2 skills per mission)
      - Achievement rewards
      - XP/level system (if implemented)
    - **Starting Skills**: 4 basic skills unlocked
    - **UI**: Skill unlock notifications, skill tree display
    - **Estimated Time**: 5-6 days
    - **Priority**: P1
    - **Decision Needed**: Linear vs branching unlock (see PRD Open Questions)

---

## Phase 3: Polish & Balance (Q3 2026)

### Milestone: Production-Ready Game
**Target Date**: End of Q3 2026 (September 30, 2026)

### Goals
- Difficulty rebalancing based on playtest data
- UI/UX refinements
- Visual effects enhancements
- Performance optimizations
- Accessibility features
- Localization (3+ languages)

### Major Tasks for Phase 3

31. **Extensive Playtesting & Balance Iteration**
    - **Task**: Conduct 3 rounds of playtesting with 20+ testers
    - **Focus Areas**:
      - Difficulty curve across missions
      - Skill balance (usage rates, effectiveness)
      - Guard AI challenge level
      - Rhythm timing window tuning
    - **Data Collection**:
      - Heatmaps of player deaths
      - Skill usage statistics
      - Mission completion rates per difficulty
      - Perfect timing rates
    - **Iteration**: Adjust ScriptableObject values based on data
    - **Estimated Time**: 15-20 days (throughout Phase 3)
    - **Priority**: P0

32. **UI/UX Polish Pass**
    - **Task**: Professional UI design overhaul
    - **Improvements**:
      - Consistent visual style across all screens
      - Smooth transitions and animations
      - Improved readability (font sizes, contrast)
      - Tutorial tooltips
      - Loading screens with tips
      - Victory/defeat screen polish
      - Screen shake and particle effects
    - **Estimated Time**: 10-12 days
    - **Priority**: P1

33. **Visual Effects Enhancement**
    - **Task**: Add polish VFX throughout game
    - **VFX Additions**:
      - Skill activation effects (per skill)
      - Assassination impact effects
      - Detection visual indicators (guard vision cones)
      - Beat pulse screen effects
      - Particle systems for environment
      - Death animations for guards
    - **Integration**: VFXManager expansion
    - **Estimated Time**: 8-10 days
    - **Priority**: P1

34. **Audio Polish**
    - **Task**: Final audio mixing and mastering
    - **Requirements**:
      - Balance all SFX volumes
      - Music ducking during important moments
      - Spatial audio for guard footsteps
      - Ambient environmental sounds per mission
      - Audio filters for stealth mode
    - **Estimated Time**: 5-6 days
    - **Priority**: P2

35. **Performance Optimization Round 2**
    - **Task**: Final optimization pass for all hardware tiers
    - **Targets**:
      - Low-end: 30 FPS stable
      - Mid-range: 60 FPS stable
      - High-end: 120+ FPS
    - **Optimizations**:
      - Texture atlasing
      - Mesh batching
      - Occlusion culling
      - Audio source pooling
      - Reduced draw calls
    - **Estimated Time**: 6-8 days
    - **Priority**: P1

36. **Accessibility Features**
    - **Task**: Implement accessibility options
    - **Features**:
      - Colorblind modes (3 variants)
      - Visual-only rhythm mode (for hearing impaired)
      - Audio-only cues (for visually impaired)
      - Adjustable UI scale
      - High contrast mode
      - Slow-motion practice mode
    - **Estimated Time**: 8-10 days
    - **Priority**: P2

37. **Localization (3 Languages)**
    - **Task**: Add multi-language support
    - **Languages**: English (default), Korean, Japanese
    - **Scope**:
      - All UI text
      - Tutorial instructions
      - Mission briefings
      - Skill descriptions
      - Menu strings
    - **Tool**: Unity Localization package
    - **Estimated Time**: 10-12 days (includes translation)
    - **Priority**: P2

38. **Bug Bash & Stability**
    - **Task**: Final bug fixing sprint
    - **Process**:
      - Company-wide bug bash (all team members test)
      - External QA testing
      - Edge case testing
      - Crash reporting implementation
    - **Target**: <0.1% crash rate
    - **Estimated Time**: 10-15 days
    - **Priority**: P0

39. **Documentation & Help System**
    - **Task**: Create in-game help and external documentation
    - **Deliverables**:
      - In-game help menu (controls, mechanics)
      - Online wiki/guide
      - Video tutorials (optional)
      - Skill reference guide
      - FAQ
    - **Estimated Time**: 6-8 days
    - **Priority**: P2

---

## Phase 4: Launch (Q4 2026)

### Milestone: Public Release
**Target Date**: Q4 2026 (October-December 2026)

### Goals
- Steam release preparation
- Marketing and promotion
- Community building
- Post-launch support planning

### Major Tasks for Phase 4

40. **Steam Integration**
    - **Task**: Implement Steamworks SDK features
    - **Features**:
      - Steam achievements
      - Steam leaderboards
      - Steam Cloud saves
      - Workshop support (for future modding)
      - Trading cards (optional)
    - **Estimated Time**: 8-10 days
    - **Priority**: P0

41. **Marketing Materials Creation**
    - **Task**: Create promotional assets
    - **Deliverables**:
      - Gameplay trailer (2-3 minutes)
      - Screenshots (10-15 high-quality)
      - GIFs for social media
      - Press kit
      - Steam store page description
      - Key art
    - **Estimated Time**: 10-15 days (or outsource)
    - **Priority**: P0

42. **Demo Version**
    - **Task**: Create free demo build
    - **Content**: Tutorial + First 2 missions
    - **Restrictions**: Skill progression limited
    - **Purpose**: Marketing and player acquisition
    - **Estimated Time**: 4-5 days
    - **Priority**: P1

43. **Launch Trailer**
    - **Task**: Professional launch trailer production
    - **Length**: 60-90 seconds
    - **Content**: Showcase unique mechanics, variety, polish
    - **Estimated Time**: 7-10 days (or outsource)
    - **Priority**: P0

44. **Community Building**
    - **Task**: Establish community presence
    - **Platforms**:
      - Discord server
      - Reddit community
      - Twitter/X account
      - Development blog
    - **Content**: Dev updates, behind-the-scenes, playtesting opportunities
    - **Estimated Time**: Ongoing from Phase 3
    - **Priority**: P2

45. **Beta Testing Program**
    - **Task**: Closed beta with 50-100 players
    - **Duration**: 2-4 weeks before launch
    - **Goals**: Final bug discovery, balance feedback, testimonials
    - **Estimated Time**: Planning 3-5 days, execution 14-28 days
    - **Priority**: P1

46. **Post-Launch Content Plan**
    - **Task**: Plan DLC and updates
    - **Potential Content**:
      - New mission packs (5 missions per DLC)
      - New skill categories
      - New enemy types
      - Challenge modes
      - Endless/survival mode
    - **Estimated Time**: 5-7 days planning
    - **Priority**: P2

47. **Launch Day Preparation**
    - **Task**: Final checklist and launch readiness
    - **Checklist**:
      - Build submission to Steam
      - Store page finalized
      - Press outreach completed
      - Social media posts scheduled
      - Launch day support plan
      - Patch 1.0.1 prepared (day-one fixes)
    - **Estimated Time**: 5-10 days
    - **Priority**: P0

---

## Development Guidelines

### Code Standards
1. **Architecture Principles** (from .github/copilot-instructions.md):
   - Maintain modularity (KISS, SRP)
   - Use C# Events for communication
   - State Pattern for complex entities
   - ScriptableObjects for data separation
   - Region tags for organization

2. **Performance Requirements**:
   - Profile before optimizing
   - Measure impact of changes
   - Use PerformanceMonitor for tracking
   - No premature optimization

3. **Testing Requirements**:
   - Unit tests for rhythm logic
   - Integration tests for AI states
   - Playtest each mission on all difficulties
   - Audio sync testing on multiple machines

### Version Control
- **Branch Strategy**: Feature branches for major tasks
- **Commit Frequency**: Daily commits minimum
- **Commit Messages**: Clear, descriptive (Korean acceptable)
- **Code Review**: Self-review before merge

### Asset Management
- **Naming Convention**: PascalCase for prefabs, camelCase for scripts
- **Organization**: Follow existing folder structure
- **File Size**: Keep textures <2MB, audio <5MB
- **Prefab Variants**: Use for skill variations

### Documentation
- **Code Comments**: Korean for gameplay, English for technical
- **XML Documentation**: All public APIs
- **README Updates**: Document new systems
- **Change Log**: Maintain version history

---

## Risk Mitigation Tasks

### Technical Risks

**Risk 1: Audio Sync Issues**
- **Mitigation Task**: Create audio calibration tool
  - Allow players to adjust offset (-200ms to +200ms)
  - Visual feedback for calibration
  - Save calibration per machine
  - **Priority**: P1
  - **Estimated Time**: 3-4 days

**Risk 2: Performance Degradation**
- **Mitigation Task**: Continuous performance monitoring
  - Weekly performance profiling
  - Automated performance tests
  - Performance budget alerts
  - **Priority**: P0
  - **Ongoing**: Throughout development

**Risk 3: Input Latency**
- **Mitigation Task**: Input buffering enhancement
  - Extend input buffer to 3 beats
  - Visual feedback for buffered inputs
  - Testing on various input devices
  - **Priority**: P1
  - **Estimated Time**: 2-3 days

### Design Risks

**Risk 4: Difficulty Imbalance**
- **Mitigation Task**: Difficulty analytics dashboard
  - Track completion rates per difficulty
  - Heatmaps of failure points
  - Automated balance suggestions
  - **Priority**: P1
  - **Estimated Time**: 5-6 days

**Risk 5: Steep Learning Curve**
- **Mitigation Task**: Enhanced tutorial system
  - Multiple tutorial levels (basic, intermediate, advanced)
  - Practice mode with no consequences
  - In-game tooltips and hints
  - **Priority**: P0
  - **Estimated Time**: Already planned in Week 1-2

**Risk 6: Repetitive Gameplay**
- **Mitigation Task**: Mission variety analysis
  - Ensure each mission has unique mechanics
  - Daily/weekly challenge modes
  - Randomized guard placements (optional)
  - **Priority**: P2
  - **Estimated Time**: Integrated into mission design

### Content Risks

**Risk 7: Insufficient Content**
- **Mitigation Task**: Content production pipeline
  - Mission creation tools/templates
  - Skill template system
  - Outsource asset creation if needed
  - **Priority**: P1
  - **Estimated Time**: Tools creation 5-7 days

**Risk 8: Audio Budget**
- **Mitigation Task**: Royalty-free asset library curation
  - Research free music sources (FreePD, OpenGameArt)
  - Procedural music generation (optional)
  - Budget allocation for professional tracks
  - **Priority**: P1
  - **Estimated Time**: 3-4 days research

---

## Success Metrics Tracking

### Development Metrics (Track Weekly)
- **Velocity**: Story points completed per week
- **Bug Count**: Open bugs by severity
- **Code Coverage**: Unit test coverage percentage
- **Build Success Rate**: Percentage of successful builds
- **Performance**: Frame time and memory usage trends

### Playtest Metrics (Track Per Playtest Round)
- **Tutorial Completion**: % of players completing tutorial
- **Mission Completion**: % completion per mission per difficulty
- **Perfect Timing Rate**: Average across all players
- **Session Length**: Average playtime
- **Player Feedback Scores**: 1-5 scale for enjoyment, difficulty, clarity

### Quality Gates (Must Pass Before Release)
- [ ] Zero critical bugs
- [ ] <5 major bugs
- [ ] 60 FPS on target hardware (100% of time)
- [ ] <30ms audio latency (95% of time)
- [ ] >80% tutorial completion rate (playtest)
- [ ] >70% positive feedback score (playtest)
- [ ] All 8 missions completable on all difficulties
- [ ] All 20+ skills functional and balanced

---

## Dependency Graph

```
Critical Path (Must Complete in Order):
Beat Timeline UI → Tutorial Mission → Mission 1 → Mission 2 → Mission 3 → Phase 1 Complete

Parallel Tracks (Can Develop Simultaneously):
Track A: Skills (Tasks 5-6) → Skill Loadout UI (Task 18)
Track B: Audio (Tasks 11-12) → Audio Polish (Task 34)
Track C: UI (Tasks 1-4) → UI/UX Polish (Task 32)
Track D: Missions (Tasks 7-10) → Mission Expansion (Task 21)

Phase Dependencies:
Phase 1 → Phase 2 → Phase 3 → Phase 4 (Sequential)
```

---

## Estimated Timeline Summary

| Phase | Duration | Key Deliverables | Total Effort (Days) |
|-------|----------|------------------|---------------------|
| **Immediate Priorities** | 2-4 weeks | Beat UI, Skills, Missions, Audio | 50-60 days |
| **Phase 1 Completion** | Q1 2026 (3 months) | MVP with 3 missions, 12+ skills | 90-120 days |
| **Phase 2** | Q2 2026 (3 months) | 8+ missions, 20+ skills, rankings | 100-130 days |
| **Phase 3** | Q3 2026 (3 months) | Polish, balance, accessibility | 80-110 days |
| **Phase 4** | Q4 2026 (3 months) | Marketing, launch, post-launch | 60-80 days |
| **Total** | ~12 months | Full game release | 390-500 days |

**Note**: Effort days assume single full-time developer. Adjust for team size and parallel development.

---

## Priority Legend
- **⭐ P0 (Critical)**: Must complete for MVP, blocks other work
- **P1 (High Priority)**: Essential for quality MVP
- **P2 (Medium Priority)**: Important but can be deferred
- **P3 (Low Priority)**: Nice-to-have, post-launch consideration

---

## Next Steps (Immediate Actions)

1. **Start Beat Timeline UI** (Task 1) - Highest priority, unblocks playtesting
2. **Design 10 Constellation Skills** (Task 5) - Begin design docs before implementation
3. **Source Music Tracks** (Task 11) - Long lead time, start immediately
4. **Plan Tutorial Mission** (Task 7) - Create detailed design doc

**Recommended Daily Workflow**:
- Morning: Core feature implementation (3-4 hours)
- Afternoon: Asset integration and testing (2-3 hours)
- Evening: Bug fixing and documentation (1-2 hours)

**Weekly Milestones**:
- Week 1: Beat Timeline UI complete
- Week 2: 5 skills implemented
- Week 3: Tutorial mission playable
- Week 4: Mission 1 complete + performance testing

---

**END OF PLAN**

*This plan is a living document. Update weekly based on progress, blockers, and changing priorities.*
