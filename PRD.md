# Product Requirements Document (PRD)
## Song of the Stars - Night Assassin

**Version:** 1.0
**Date:** November 17, 2025
**Status:** In Development
**Platform:** PC (Unity 6.0.2)

---

## 1. Executive Summary

**Song of the Stars - Night Assassin** is a unique rhythm-based stealth assassination game that combines the precision timing of rhythm games with the strategic gameplay of stealth titles. Players must synchronize their movements and actions with musical beats to execute perfect assassinations while avoiding guard detection in a constellation-themed world.

### Key Innovation
Unlike traditional stealth games where timing is purely player-controlled, this game enforces rhythm-synchronized actions for both the player and AI enemies, creating a dance-like assassination experience where perfect timing is rewarded and mistakes are punished.

---

## 2. Product Vision & Goals

### 2.1 Vision Statement
To create an innovative gaming experience that challenges players to think strategically while maintaining perfect rhythm, offering a unique blend of tension from both stealth detection and rhythm precision.

### 2.2 Primary Goals
1. **Unique Gameplay Experience**: Establish a new subgenre by successfully merging rhythm and stealth mechanics
2. **Accessible Yet Deep**: Easy to learn rhythm mechanics with complex strategic depth
3. **Replayability**: Multiple difficulty levels and skill loadouts encourage experimentation
4. **Polish & Performance**: Maintain 60 FPS with precise audio-visual synchronization
5. **Content Scalability**: Modular architecture supporting easy addition of new skills, missions, and enemies

### 2.3 Success Criteria
- Players successfully understand and engage with rhythm-based controls within first 5 minutes
- Average mission completion time: 3-10 minutes (varies by difficulty)
- Player retention: Complete at least 3 missions on different difficulty levels
- Performance: Maintain <16ms frame time (60 FPS) on mid-range hardware
- Audio sync accuracy: <50ms latency between beat and visual feedback

---

## 3. Target Audience

### 3.1 Primary Audience
- **Rhythm Game Enthusiasts** (Age 16-35)
  - Familiar with timing-based mechanics
  - Enjoy precision challenges
  - Appreciate music-driven gameplay

- **Stealth Game Players** (Age 18-40)
  - Strategic thinkers who enjoy planning
  - Appreciate tension and risk/reward systems
  - Prefer non-violent or stylized assassination mechanics

### 3.2 Secondary Audience
- Indie game enthusiasts looking for unique mechanics
- Speedrunners attracted to skill-based optimization
- Casual players interested in accessible difficulty modes

### 3.3 User Personas

**Persona 1: "The Rhythm Master"**
- Age: 24, plays rhythm games regularly
- Wants: Perfect timing challenges, high skill ceiling
- Needs: Clear visual/audio feedback, tight input windows on hard modes

**Persona 2: "The Strategic Planner"**
- Age: 30, enjoys stealth games (Hitman, Dishonored)
- Wants: Multiple approach options, AI pattern prediction
- Needs: Clear guard sight lines, consistent AI behavior

**Persona 3: "The Casual Enjoyer"**
- Age: 22, plays games occasionally
- Wants: Fun, accessible experience without frustration
- Needs: Forgiving timing windows, clear tutorials, gradual difficulty curve

---

## 4. Core Features & Requirements

### 4.1 Rhythm System (CRITICAL - P0)

**4.1.1 Beat Synchronization**
- **Requirement**: All major gameplay actions must align with musical beats
- **Implementation**: RhythmSyncManager with BPM-based timing
- **Status**: ✅ Implemented
- **Technical Specs**:
  - BPM range: 60-180
  - Audio DSP synchronization (AudioSettings.dspTime)
  - Beat offset calibration support
  - Pre-calculated judgment windows for performance

**4.1.2 Timing Judgment**
- **Requirement**: Grade player inputs as Perfect/Great/Miss
- **Implementation**: RhythmPatternChecker with configurable windows
- **Status**: ✅ Implemented
- **Judgment Windows** (Normal difficulty):
  - Perfect: ±80ms from beat
  - Great: ±150ms from beat
  - Miss: Outside Great window
- **Difficulty Scaling**: Windows adjust per difficulty level

**4.1.3 Visual Feedback**
- **Requirement**: Clear visual indication of beat timing and judgment
- **Status**: ✅ Partial (BeatVisualizer exists, needs enhancement)
- **Needed Improvements**:
  - Beat line/metronome UI element
  - Judgment text with color coding
  - Visual pulse on beats
  - Anticipation indicators (upcoming beat warnings)

---

### 4.2 Player Mechanics (CRITICAL - P0)

**4.2.1 Movement System**
- **Requirement**: Grid-based movement synchronized to beats
- **Implementation**: PlayerController with beat-queued movement
- **Status**: ✅ Implemented
- **Features**:
  - Direction queuing (WASD input)
  - Beat-synchronized execution
  - Free movement mode (costs Focus)
  - Speed modifiers based on difficulty

**4.2.2 Assassination System**
- **Requirement**: Multiple assassination methods with range detection
- **Implementation**: PlayerAssassination component
- **Status**: ✅ Implemented
- **Types**:
  - Melee assassination (close range)
  - Ranged assassination (mid-range)
  - Charge attack mechanics
- **Detection**: Circle overlap + raycast validation

**4.2.3 Stealth System**
- **Requirement**: Stealth mode toggle affecting visibility and speed
- **Implementation**: PlayerStealth component
- **Status**: ✅ Implemented
- **Features**:
  - Stealth activation/deactivation
  - Speed multiplier (difficulty-based)
  - Visibility state tracking
  - Integration with Guard detection

**4.2.4 Skill System**
- **Requirement**: 4-slot loadout with cooldown management
- **Implementation**: SkillLoadoutManager + ConstellationSkillData
- **Status**: ✅ Core system implemented
- **Skill Categories**:
  1. Attack Skills (assassination abilities)
  2. Lure Skills (decoys, distractions)
  3. Stealth Skills (disguise, concealment)
  4. Movement Skills (mobility enhancements)
- **Cooldown**: Beat-based cooldown tracking
- **Input**: 1-3 button rhythm patterns per skill

---

### 4.3 Guard AI System (CRITICAL - P0)

**4.3.1 Behavior States**
- **Requirement**: Context-aware AI with state transitions
- **Implementation**: State pattern with polymorphic states
- **Status**: ✅ Implemented
- **States**:
  1. **Patrolling**: Routine patrol, beat-synchronized movement
  2. **Investigating**: Alert check at suspicious location (decoy)
  3. **Chasing**: Active pursuit with detection buildup
  4. **Stunned**: Temporary incapacitation

**4.3.2 Detection System**
- **Requirement**: Line-of-sight with probabilistic detection
- **Implementation**: ProbabilisticDetection algorithm
- **Status**: ✅ Implemented
- **Mechanics**:
  - Distance-based detection probability
  - Visibility factor calculations
  - Temporal buildup (gradual detection)
  - Difficulty-adjusted parameters
  - Decoy distraction system

**4.3.3 Patrol System**
- **Requirement**: Rhythm-synchronized patrol with waypoints
- **Implementation**: GuardRhythmPatrol with patrol points
- **Status**: ✅ Implemented
- **Features**:
  - Beat-synchronized movement
  - Waypoint-based patrol routes
  - Automatic loop/ping-pong patrol patterns

---

### 4.4 Resource Management (HIGH PRIORITY - P1)

**4.4.1 Focus System**
- **Requirement**: Resource gained from perfect timing, spent on special abilities
- **Implementation**: Focus tracking in PlayerController
- **Status**: ✅ Implemented
- **Mechanics**:
  - Max Focus: 100 points
  - Perfect timing: +10 Focus
  - Free movement cost: 50 Focus
  - Miss penalty: Alert increase
- **UI**: Focus bar visualization

**4.4.2 Alert Level System**
- **Requirement**: Cumulative alert tracking affecting mission outcome
- **Implementation**: MissionManager alert level tracking
- **Status**: ✅ Implemented
- **Mechanics**:
  - Range: 0 to maxAlertLevel (default: 5)
  - Increased by: Missed beats, detection, loud actions
  - Mission failure: Alert reaches maximum
  - Reset: Per mission

---

### 4.5 Difficulty System (HIGH PRIORITY - P1)

**4.5.1 Difficulty Levels**
- **Requirement**: 4 distinct difficulty tiers with comprehensive parameter scaling
- **Implementation**: DifficultyManager with ScriptableObject settings
- **Status**: ✅ Implemented
- **Levels**:
  1. **Easy**: Wide timing windows, slow guards, high player speed
  2. **Normal**: Balanced gameplay
  3. **Hard**: Tight timing, aggressive guards, lower resources
  4. **Expert**: Unforgiving timing, maximum guard efficiency

**4.5.2 Scalable Parameters**
- **Rhythm Parameters**:
  - beatTolerance (judgment window size)
  - perfectTolerance (perfect judgment window)
- **Guard Parameters**:
  - viewDistance
  - fieldOfViewAngle
  - moveSpeed
  - detectionTime
- **Player Parameters**:
  - moveSpeed
  - stealthSpeedMultiplier
- **Skill Parameters**:
  - cooldownMultiplier

---

### 4.6 Mission System (HIGH PRIORITY - P1)

**4.6.1 Mission Structure**
- **Requirement**: Clear objectives with success/failure conditions
- **Implementation**: MissionManager
- **Status**: ✅ Core system implemented
- **Components**:
  - Target tracking system
  - Alert level management
  - Mission completion callbacks
  - Success/failure state transitions

**4.6.2 Mission Progression** (PLANNED - P2)
- **Requirement**: Multiple missions with increasing difficulty
- **Status**: ❌ Not implemented
- **Needed**:
  - Mission selection UI
  - Mission unlock progression
  - Per-mission leaderboards/statistics
  - Mission rating system (e.g., S/A/B/C ranks)

---

### 4.7 Persistence System (MEDIUM PRIORITY - P2)

**4.7.1 Save System**
- **Requirement**: JSON-based save/load for progress and settings
- **Implementation**: SaveSystem singleton
- **Status**: ✅ Implemented
- **Saved Data**:
  - Mission progress
  - Skill unlocks and loadouts
  - Player settings (volume, difficulty)
  - Statistics (missions completed, perfect judgments, etc.)
- **Features**:
  - Auto-save on quit
  - Manual save/load
  - Persistent across scenes

---

### 4.8 UI/UX System (MEDIUM PRIORITY - P2)

**4.8.1 HUD Elements**
- **Requirement**: Real-time gameplay information display
- **Implementation**: UIManager
- **Status**: ✅ Partial implementation
- **Elements**:
  - ✅ Skill icons with cooldown displays
  - ✅ Focus bar
  - ✅ Alert level indicator
  - ✅ Judgment text (Perfect/Great/Miss)
  - ❌ Beat timeline/metronome (NEEDED)
  - ❌ Combo counter (PLANNED)
  - ❌ Mission objectives overlay (PLANNED)

**4.8.2 Minimap**
- **Requirement**: Strategic overhead view with guard states
- **Implementation**: MinimapController
- **Status**: ✅ Implemented
- **Features**:
  - Real-time guard positions
  - Guard detection states (patrol/investigate/chase)
  - Player position indicator

**4.8.3 Menu Systems**
- **Implementation**: MainMenuManager
- **Status**: ✅ Basic implementation
- **Screens**:
  - Main menu (Start, Options, Quit)
  - Skill loadout selection
  - Settings (audio, difficulty)
  - Pause menu

---

### 4.9 Audio System (CRITICAL - P0)

**4.9.1 Music Synchronization**
- **Requirement**: Precise beat tracking with minimal latency
- **Implementation**: MusicManager with DSP-based timing
- **Status**: ✅ Implemented
- **Technical**:
  - AudioSettings.dspTime for precision
  - Beat offset calibration
  - BPM detection/manual setting

**4.9.2 Sound Effects**
- **Requirement**: Layered audio with volume controls
- **Implementation**: SFXManager
- **Status**: ✅ Implemented
- **Categories**:
  - Master volume
  - SFX volume
  - UI volume
  - Music volume
- **Features**: Persistent audio source management

---

### 4.10 Tutorial System (MEDIUM PRIORITY - P2)

**4.10.1 Onboarding**
- **Requirement**: Interactive tutorial teaching core mechanics
- **Implementation**: TutorialManager
- **Status**: ⚠️ Partial (needs content)
- **Tutorial Topics**:
  - Rhythm timing basics
  - Movement synchronization
  - Skill activation
  - Stealth mechanics
  - Guard detection awareness
  - Mission objectives

---

### 4.11 Performance & Optimization (HIGH PRIORITY - P1)

**4.11.1 Optimization Systems**
- **Status**: ✅ Implemented
- **Features**:
  - Pre-calculated judgment windows
  - Object pooling system
  - Guard LOD system
  - Manager caching (GameServices)
  - Non-alloc physics calls
  - DSP-based audio (no frame-based timing)

**4.11.2 Performance Targets**
- Frame rate: 60 FPS minimum
- Frame time: <16ms (99th percentile)
- Memory: <500MB RAM usage
- Load time: <3 seconds scene load

---

## 5. User Stories

### Epic 1: First-Time Player Experience
- **US-1.1**: As a new player, I want a clear tutorial so I can learn rhythm mechanics within 5 minutes
- **US-1.2**: As a new player, I want visual beat indicators so I know when to press buttons
- **US-1.3**: As a new player, I want forgiving Easy mode so I can enjoy the game without frustration
- **US-1.4**: As a new player, I want clear failure explanations so I understand my mistakes

### Epic 2: Core Gameplay Loop
- **US-2.1**: As a player, I want responsive rhythm input so my actions feel synchronized with music
- **US-2.2**: As a player, I want multiple skill loadouts so I can experiment with different playstyles
- **US-2.3**: As a player, I want predictable guard patterns so I can plan my approach
- **US-2.4**: As a player, I want stealth feedback so I know when I'm detected

### Epic 3: Skill Mastery
- **US-3.1**: As an experienced player, I want tight timing windows (Hard/Expert) so I feel challenged
- **US-3.2**: As an experienced player, I want complex skill combos so I can optimize my play
- **US-3.3**: As an experienced player, I want mission leaderboards so I can compete for perfect runs
- **US-3.4**: As an experienced player, I want detailed statistics so I can track my improvement

### Epic 4: Content Variety
- **US-4.1**: As a player, I want multiple missions with different layouts so gameplay stays fresh
- **US-4.2**: As a player, I want unlockable skills so I have progression goals
- **US-4.3**: As a player, I want different enemy types so I adapt my strategies
- **US-4.4**: As a player, I want environmental hazards/tools so I have more tactical options

### Epic 5: Replayability
- **US-5.1**: As a player, I want mission rankings (S/A/B/C) so I'm incentivized to replay
- **US-5.2**: As a player, I want challenge modifiers so I can increase difficulty for rewards
- **US-5.3**: As a player, I want achievements/unlockables so I have long-term goals
- **US-5.4**: As a player, I want speedrun timers so I can optimize my routes

---

## 6. Technical Requirements

### 6.1 Platform & Technology
- **Engine**: Unity 6.0.2.10f1
- **Platform**: PC (Windows/Mac/Linux)
- **Language**: C# (.NET Standard 2.1)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Target Resolution**: 1920x1080 (scalable)
- **Input**: Keyboard & Mouse (controller support planned)

### 6.2 Architecture
- **Patterns**:
  - Singleton: Core managers (GameManager, SaveSystem)
  - State Pattern: Guard AI behavior
  - Event-Driven: Decoupled component communication
  - Service Locator: GameServices manager registry
  - ScriptableObject: Data-driven design for skills, difficulty
  - Component-Based: Unity MonoBehaviour architecture

### 6.3 Performance Requirements
| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| Frame Rate | 60 FPS | 45 FPS |
| Frame Time (avg) | <16ms | <22ms |
| Frame Time (99th) | <20ms | <30ms |
| Memory Usage | <400MB | <600MB |
| Scene Load Time | <2s | <5s |
| Audio Latency | <30ms | <50ms |

### 6.4 Code Quality Standards
- **Documentation**: XML comments for public APIs
- **Logging**: Comprehensive error and state change logging
- **Testing**: Unit tests for core systems (rhythm, AI states)
- **Code Style**: Unity C# conventions, region organization
- **Comments**: Korean for gameplay mechanics, English for technical docs

### 6.5 Dependencies
- **TextMesh Pro**: UI text rendering (Unity package)
- **Universal RP**: Rendering pipeline (Unity package)
- **Audio DSP**: Precise timing (Unity AudioSettings API)

---

## 7. Content Requirements

### 7.1 Skills (Constellation-Themed)

**Priority 1 (Minimum Viable Product)**
- ✅ Decoy (Lure skill)
- ✅ Capricorn Trap (Attack skill)
- ❌ 10 additional skills (2-3 per category)

**Priority 2 (Full Release)**
- 20+ unique skills
- Skill unlock progression system
- Skill upgrade tiers
- Skill combo synergies

### 7.2 Missions

**Priority 1 (MVP)**
- 1 tutorial mission (partially implemented)
- 3 core missions with varied layouts

**Priority 2 (Full Release)**
- 10-15 unique mission environments
- Mission difficulty variants
- Secret objectives/challenges
- Boss missions (unique enemy patterns)

### 7.3 Enemy Types

**Current**
- ✅ Basic Guard (patrol/investigate/chase/stunned)

**Planned (Priority 2)**
- Heavy Guard (slower, wider vision)
- Scout (faster, narrow vision)
- Stationary Watcher (360° vision, immobile)
- Dog Companion (enhanced detection)

### 7.4 Audio Content

**Music Tracks Needed**
- 3-5 unique background tracks (different BPMs)
- Tutorial track (slower BPM ~80)
- Action tracks (mid-high BPM 120-160)
- Menu/ambient music

**Sound Effects Needed**
- Footstep sounds (player, guards)
- Skill activation sounds (per skill)
- Detection alert sounds
- Judgment feedback (perfect/great/miss)
- Environmental sounds (ambient)
- UI sounds (button clicks, menu navigation)

---

## 8. Success Metrics & KPIs

### 8.1 Engagement Metrics
- **Mission Completion Rate**: >70% of players complete tutorial
- **Session Length**: Average 20-40 minutes per session
- **Missions Per Session**: 2-4 missions
- **Skill Experimentation**: Players try >5 different skill loadouts
- **Difficulty Progression**: >40% of players attempt Hard mode

### 8.2 Quality Metrics
- **Perfect Timing Rate**: 30-50% perfect judgments (Normal difficulty)
- **Detection Frequency**: Players detected 1-3 times per mission
- **Mission Failure Rate**: <30% failure on first attempt (Normal)
- **Retry Engagement**: >60% of players retry failed missions

### 8.3 Technical Metrics
- **Frame Rate Stability**: >95% of frames at target 60 FPS
- **Audio Sync Accuracy**: <30ms average latency
- **Crash Rate**: <0.1% sessions
- **Load Times**: <2s average scene load

### 8.4 Player Satisfaction
- **Tutorial Clarity**: >80% of players understand mechanics
- **Rhythm Feedback**: >75% report satisfying timing feedback
- **Difficulty Balance**: >70% rate difficulty as "just right"
- **Overall Enjoyment**: >75% positive sentiment

---

## 9. Release Roadmap

### Phase 1: MVP (Current Focus) - Q1 2026
**Goal**: Playable vertical slice with core mechanics

**Deliverables**:
- ✅ Core rhythm system
- ✅ Player movement and assassination
- ✅ Guard AI with states
- ✅ Difficulty system
- ✅ Basic UI/HUD
- ⚠️ Tutorial mission (needs polish)
- ❌ 2 additional core missions
- ❌ Beat timeline UI
- ❌ 10-12 unique skills
- ❌ Audio content (3 music tracks, core SFX)

**Acceptance Criteria**:
- 3 completable missions
- 4 difficulty levels functional
- 60 FPS on mid-range hardware
- <50ms audio latency
- Complete tutorial experience

---

### Phase 2: Content Expansion - Q2 2026
**Goal**: Increase content variety and replayability

**Deliverables**:
- 5 additional missions
- 10 additional skills
- Mission ranking system (S/A/B/C)
- Statistics and leaderboards
- Achievement system
- 2 new enemy types
- Additional audio tracks
- Controller support

**Acceptance Criteria**:
- 8+ unique missions
- 20+ skills available
- Full progression system
- Persistent leaderboards

---

### Phase 3: Polish & Balance - Q3 2026
**Goal**: Refinement based on playtesting feedback

**Deliverables**:
- Difficulty rebalancing
- UI/UX improvements
- Visual effects enhancements
- Performance optimizations
- Bug fixes and stability
- Accessibility features
- Localization support (3+ languages)

**Acceptance Criteria**:
- <0.1% crash rate
- >80% positive playtest feedback
- All difficulty modes balanced
- Accessibility options implemented

---

### Phase 4: Launch & Post-Launch - Q4 2026
**Goal**: Public release and ongoing support

**Deliverables**:
- Steam release
- Marketing materials
- Community tools (mod support)
- Post-launch content roadmap
- Live support and patches

**Success Criteria**:
- Successful launch with >1000 sales in first month
- >75% positive Steam reviews
- Active community engagement
- DLC/expansion planning initiated

---

## 10. Risks & Mitigation Strategies

### 10.1 Technical Risks

**Risk 1: Audio Synchronization Issues**
- **Impact**: High - Core mechanic failure
- **Probability**: Medium
- **Mitigation**:
  - Use AudioSettings.dspTime (already implemented)
  - Extensive testing on various hardware
  - Audio calibration tool for players
  - Fallback visual-only rhythm mode

**Risk 2: Performance on Lower-End Hardware**
- **Impact**: Medium - Limits audience
- **Probability**: Medium
- **Mitigation**:
  - LOD systems (already implemented for guards)
  - Object pooling (implemented)
  - Scalable graphics settings
  - Performance profiling throughout development

**Risk 3: Input Latency**
- **Impact**: High - Affects rhythm precision
- **Probability**: Low
- **Mitigation**:
  - Input buffering system
  - Latency compensation settings
  - Raw input API usage
  - Regular latency testing

---

### 10.2 Design Risks

**Risk 4: Difficulty Balancing**
- **Impact**: High - Player frustration or boredom
- **Probability**: High
- **Mitigation**:
  - Extensive playtesting across skill levels
  - Difficulty system already highly configurable
  - Data-driven balance (ScriptableObjects)
  - Iterative tuning based on telemetry

**Risk 5: Learning Curve Too Steep**
- **Impact**: High - Player abandonment
- **Probability**: Medium
- **Mitigation**:
  - Comprehensive tutorial system
  - Easy mode with very forgiving timing
  - Progressive difficulty introduction
  - Clear visual feedback systems
  - Practice mode without failure

**Risk 6: Repetitive Gameplay**
- **Impact**: Medium - Low replayability
- **Probability**: Medium
- **Mitigation**:
  - Diverse skill loadouts (20+ skills planned)
  - Varied mission layouts
  - Multiple enemy types
  - Challenge modifiers
  - Ranking/leaderboard systems

---

### 10.3 Content Risks

**Risk 7: Insufficient Content for Launch**
- **Impact**: High - Poor reviews, short playtime
- **Probability**: Medium
- **Mitigation**:
  - Prioritized feature list (P0/P1/P2)
  - Modular content pipeline
  - Early content production alongside systems
  - Scalable mission designer tools

**Risk 8: Music Licensing/Original Composition Costs**
- **Impact**: Medium - Budget constraints
- **Probability**: Low
- **Mitigation**:
  - Early budget allocation for audio
  - Royalty-free music alternatives
  - Procedural/adaptive music systems
  - In-house composition if feasible

---

### 10.4 Market Risks

**Risk 9: Niche Audience**
- **Impact**: Medium - Limited sales potential
- **Probability**: Medium
- **Mitigation**:
  - Clear marketing emphasizing unique mechanics
  - Target both rhythm and stealth communities
  - Streamable/shareable gameplay moments
  - Free demo to lower entry barrier
  - Community building before launch

**Risk 10: Competition from Similar Titles**
- **Impact**: Low - Genre is relatively unexplored
- **Probability**: Low
- **Mitigation**:
  - Emphasize unique rhythm-stealth hybrid
  - Build strong core mechanics (first-mover advantage)
  - Regular content updates post-launch
  - Community engagement and feedback

---

## 11. Open Questions & Decisions Needed

### 11.1 Design Decisions
1. **Should guards also fail rhythm checks?**
   - Currently guards move perfectly on beat
   - Could add randomized "mistakes" for realism
   - Decision needed: Balance vs realism trade-off

2. **Skill unlock progression: linear or branching?**
   - Linear: All skills unlock in sequence
   - Branching: Player chooses skill trees
   - Decision needed: Complexity vs player agency

3. **Mission structure: linear or open-ended?**
   - Linear: Story-driven mission sequence
   - Open: Select missions in any order
   - Decision needed: Narrative vs freedom

### 11.2 Technical Decisions
4. **Online features?**
   - Global leaderboards
   - Ghost data/replays
   - Asynchronous competition
   - Decision needed: Scope and infrastructure requirements

5. **Modding support?**
   - Allow custom missions/skills
   - Level editor tool
   - Decision needed: Development effort vs community longevity

### 11.3 Content Decisions
6. **Narrative integration?**
   - Currently minimal story
   - Add cutscenes/dialogue?
   - Environmental storytelling?
   - Decision needed: Budget and scope

7. **Boss missions design?**
   - Unique enemy types requiring special strategies
   - Rhythm pattern changes mid-fight
   - Decision needed: Design complexity and development time

---

## 12. Dependencies & Assumptions

### 12.1 Dependencies
- Unity 6.0.2 stability and continued support
- TextMesh Pro for UI rendering
- Universal Render Pipeline performance
- Audio DSP API reliability
- Platform (Steam) approval and integration

### 12.2 Assumptions
- Players have basic familiarity with either rhythm or stealth games
- Target hardware: Mid-range PC (GTX 1060 equivalent or better)
- Players use headphones or speakers (audio-dependent gameplay)
- Single-player experience (no multiplayer requirements)
- Premium purchase model (not F2P)

---

## 13. Appendices

### Appendix A: Technical Architecture Diagram
```
┌─────────────────────────────────────────────┐
│           GAME MANAGER (Core)               │
│  - Game State Management                    │
│  - Scene Flow Control                       │
└─────────────────┬───────────────────────────┘
                  │
        ┌─────────┴─────────┐
        │   GAME SERVICES   │ (Manager Registry)
        └─────────┬─────────┘
                  │
    ┌─────────────┼─────────────┐
    │             │             │
┌───▼────┐   ┌───▼────┐   ┌───▼────┐
│RHYTHM  │   │MISSION │   │DIFFICULTY│
│MANAGER │   │MANAGER │   │MANAGER  │
└───┬────┘   └───┬────┘   └────────┘
    │            │
    │    ┌───────┴────────┐
    │    │                │
┌───▼────▼───┐      ┌────▼─────┐
│  PLAYER    │      │  GUARDS  │
│ Controller │◄─────┤ RhythmAI │
│ - Movement │      │ - States │
│ - Skills   │      │ - Detect │
│ - Assassin │      └──────────┘
└────────────┘
    │
┌───▼────────┐
│ UI MANAGER │
│ - HUD      │
│ - Minimap  │
└────────────┘
```

### Appendix B: Skill Categories & Examples
**Attack Skills**
- Direct Assassination Strike
- Ranged Throwable (star, dagger)
- Charge Attack
- AOE Stun

**Lure Skills**
- ✅ Decoy (implemented)
- Sound Distraction
- Illusory Clone
- Bait Trap

**Stealth Skills**
- Invisibility (temporary)
- Shadow Blend (hide in shadows)
- Noise Masking
- Scent Concealment

**Movement Skills**
- Dash/Blink
- Wall Climb
- Slow Time
- Speed Boost

### Appendix C: Guard State Transition Diagram
```
    START
      │
      ▼
┌───────────┐
│ PATROLLING│◄─────────────┐
└─────┬─────┘              │
      │                    │
      │ (Decoy detected)   │ (Investigation complete)
      ▼                    │
┌──────────────┐           │
│INVESTIGATING │───────────┘
└──────┬───────┘
       │
       │ (Player spotted)
       ▼
┌───────────┐
│  CHASING  │──────────┐
└─────┬─────┘          │
      │                │ (Lost target)
      │ (Stunned)      │
      ▼                │
┌───────────┐          │
│  STUNNED  │          │
└─────┬─────┘          │
      │                │
      │ (Timer expires)│
      └────────────────┴─────► PATROLLING
```

### Appendix D: Difficulty Parameter Table
| Parameter | Easy | Normal | Hard | Expert |
|-----------|------|--------|------|--------|
| Beat Tolerance | 200ms | 150ms | 100ms | 60ms |
| Perfect Tolerance | 120ms | 80ms | 50ms | 30ms |
| Guard View Distance | 5m | 7m | 9m | 11m |
| Guard FOV Angle | 60° | 90° | 110° | 130° |
| Guard Move Speed | 0.8x | 1.0x | 1.3x | 1.5x |
| Player Move Speed | 1.3x | 1.0x | 0.9x | 0.8x |
| Skill Cooldown | 0.7x | 1.0x | 1.3x | 1.5x |
| Detection Time | 3.0s | 2.0s | 1.2s | 0.8s |
| Max Alert Level | 7 | 5 | 4 | 3 |

### Appendix E: Glossary
- **BPM**: Beats Per Minute, determines rhythm speed
- **DSP Time**: Digital Signal Processing time, high-precision audio timing
- **Focus**: Player resource gained from perfect timing
- **Judgment**: Timing grade (Perfect/Great/Miss)
- **Beat Window**: Time range for input validation
- **Alert Level**: Mission failure metric from detection/mistakes
- **Constellation Skills**: Celestial-themed ability system
- **State Machine**: Pattern for guard AI behavior management
- **LOD**: Level of Detail, rendering optimization technique

---

## 14. Document Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-11-17 | AI Product Team | Initial PRD creation based on codebase analysis |

---

## 15. Approval & Sign-off

**Product Owner**: _________________ Date: _______

**Technical Lead**: _________________ Date: _______

**Creative Director**: _________________ Date: _______

---

**END OF DOCUMENT**
