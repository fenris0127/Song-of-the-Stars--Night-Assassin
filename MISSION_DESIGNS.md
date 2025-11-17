
# Mission Designs
## Song of the Stars - Night Assassin

Complete mission specifications for the first 3 missions.

---

## Table of Contents
1. [Mission 0: Tutorial - "First Steps"](#mission-0-tutorial---first-steps)
2. [Mission 1: "Silent Approach"](#mission-1-silent-approach)
3. [Mission 2: "Night Market"](#mission-2-night-market)
4. [Mission Template](#mission-template)

---

## Mission 0: Tutorial - "First Steps"

### Mission Info
```yaml
Mission ID: tutorial_first_steps
Display Name: "First Steps"
Difficulty: Tutorial
Scene: Tutorial_Courtyard
Music BPM: 100
Time Limit: None
Max Alert Level: 5
```

### Briefing
"Welcome, Night Assassin. This training courtyard will teach you the fundamentals of rhythm-based stealth. Master the beat, and the shadows will become your ally."

### Primary Objectives
1. **Learn Movement** (Reach)
   - Description: "Move to the marked location using arrow keys"
   - Type: Reach
   - Target: Tutorial_Waypoint_1
   - Count: 1

2. **Practice Timing** (Custom)
   - Description: "Hit 5 Perfect rhythm inputs"
   - Type: Custom (tracked by tutorial system)
   - Target Count: 5

3. **Use First Skill** (Custom)
   - Description: "Activate Capricorn Trap (Press 1 on beat)"
   - Type: Custom
   - Target Count: 1

4. **Eliminate Target** (Eliminate)
   - Description: "Eliminate the practice dummy"
   - Type: Eliminate
   - Target Tag: TutorialDummy
   - Count: 1

### Optional Objectives
- **Perfect Student** (Custom)
  - Description: "Complete tutorial without missing a beat"
  - Type: Stealth
  - Max Detections: 0

### Skill Loadout
**Available Skills:**
- Capricorn Trap (Attack - 1 beat)

**Default Loadout:**
- Slot 1: Capricorn Trap

### Scripted Events
1. **Welcome Dialog** (OnMissionStart)
   - Type: ShowDialog
   - Text: "Feel the rhythm of the stars. Each beat is an opportunity."

2. **First Perfect** (OnObjectiveComplete, index: 1)
   - Type: ShowDialog
   - Text: "Excellent! Perfect timing grants Focus energy."

3. **Skill Unlocked** (OnObjectiveComplete, index: 2)
   - Type: ShowDialog
   - Text: "Skills are powerful but require Focus. Use them wisely."

### Rewards
- Experience: 50 XP
- Unlocked Skills: Decoy (Lure)

### Level Layout
```
Tutorial Courtyard (Small):
- Starting area with movement markers
- 2 stationary practice dummies
- 1 patrol dummy (slow)
- Safe zones marked on floor
- Beat visualization always visible
```

### Design Notes
- **Focus**: Teach core rhythm mechanics
- **Pacing**: 3-5 minutes
- **Difficulty**: No failure - tutorial can't be failed, only retried
- **UI**: All tutorial prompts visible, hand-holding
- **Music**: Simple 100 BPM track, clear beat

---

## Mission 1: "Silent Approach"

### Mission Info
```yaml
Mission ID: mission_01_silent_approach
Display Name: "Silent Approach"
Difficulty: Easy
Scene: Mission_01_Courtyard
Music BPM: 110
Time Limit: None (180s for S-rank)
Max Alert Level: 10
```

### Briefing
"Your first real contract. Lord Malvern's estate guards a valuable artifact in the east courtyard. Eliminate the two patrol captains and reach the artifact vault. The city watch is on high alert - stay in the shadows."

### Primary Objectives
1. **Eliminate Captain Thorne** (Eliminate)
   - Description: "Eliminate the patrol captain in the west wing"
   - Type: Eliminate
   - Target Tag: Captain_Thorne
   - Count: 1

2. **Eliminate Captain Vale** (Eliminate)
   - Description: "Eliminate the patrol captain in the east wing"
   - Type: Eliminate
   - Target Tag: Captain_Vale
   - Count: 1

3. **Reach the Vault** (Reach)
   - Description: "Reach the artifact vault"
   - Type: Reach
   - Target Tag: Vault_Entrance
   - Count: 1

### Optional Objectives
1. **Ghost Protocol** (Stealth)
   - Description: "Complete mission without being detected"
   - Type: Stealth
   - Max Detections: 0

2. **Speed Run** (Survive)
   - Description: "Complete in under 120 seconds"
   - Type: Survive
   - Duration: 120 (inverse - must complete before time)

3. **Perfectionist** (Custom)
   - Description: "Maintain 100% Perfect timing (no Great or Miss)"
   - Type: Custom

### Skill Loadout
**Available Skills:**
- Capricorn Trap (Attack)
- Decoy (Lure)
- Orion's Arrow (Attack - NEW)

**Default Loadout:**
- Slot 1: Capricorn Trap
- Slot 2: Decoy
- Slot 3: (Empty - player choice)

### Scripted Events
1. **Mission Brief** (OnMissionStart)
   - Type: ShowDialog
   - Text: "Two targets, one vault. Remember your training."

2. **First Captain Down** (OnObjectiveComplete, index: 0)
   - Type: ShowDialog
   - Text: "One down. Stay focused on the rhythm."
   - Trigger: SpawnGuards (reinforcement)
   - Spawn Count: 2
   - Spawn Point Tag: ReinforcementSpawn

3. **Alert Level 5** (OnAlertLevel, level: 5)
   - Type: ShowDialog
   - Text: "Guards are getting suspicious. Lower your profile!"

4. **Vault Reached** (OnObjectiveComplete, index: 2)
   - Type: CameraFocus
   - Focus on vault entrance (cinematic moment)

### Rewards
- Experience: 100 XP
- Bonus per Optional: +50 XP
- Unlocked Skills: Gemini Clone (Lure), Shadow Blend (Stealth)

### Level Layout
```
Malvern Estate Courtyard (Medium):
┌─────────────────────────────────┐
│  [Start]                   [NW] │
│     ↓                        │  │
│  Gardens ←──→ Central Plaza  │  │
│     ↓                        ↓  │
│  [Captain Thorne] ←──→ [Captain Vale]
│                               │  │
│                          [Vault]│
└─────────────────────────────────┘

Key Features:
- 2 main patrol routes (West & East)
- Central plaza (high guard density)
- Garden path (stealth route, longer)
- 8 regular guards + 2 captains
- 3 hiding spots (bushes)
- 2 elevated positions (balconies)
```

### Guard Behavior
**Captain Thorne (West)**
- Patrol: West wing, 3-point route
- Vision: 120° cone, 8 unit range
- Alert Level: Calls +2 guards on detection

**Captain Vale (East)**
- Patrol: East wing, 4-point route
- Vision: 120° cone, 8 unit range
- Alert Level: Calls +2 guards on detection

**Regular Guards (6)**
- Patrol: Fixed routes, 2-3 points
- Vision: 90° cone, 6 unit range
- Alert Level: Investigate on sound

### Design Notes
- **Focus**: Introduce planning and route selection
- **Pacing**: 5-8 minutes for first playthrough
- **Difficulty**: Easy - generous timing windows, forgiving detection
- **Skills**: Encourage experimentation with 2 loadout slots
- **Replay Value**: Optional objectives require different strategies
- **Music**: 110 BPM, slightly faster than tutorial

---

## Mission 2: "Night Market"

### Mission Info
```yaml
Mission ID: mission_02_night_market
Display Name: "Night Market"
Difficulty: Normal
Scene: Mission_02_Market
Music BPM: 120
Time Limit: 300s
Max Alert Level: 10
```

### Briefing
"The night market is alive with activity - perfect cover, or perfect trap. Your target is the merchant prince Hassan, who trades in stolen constellation artifacts. He's surrounded by mercenary guards. Eliminate Hassan and recover the 3 stolen star maps before the market closes at dawn."

### Primary Objectives
1. **Eliminate Hassan** (Eliminate)
   - Description: "Eliminate the merchant prince Hassan"
   - Type: Eliminate
   - Target Tag: Target_Hassan
   - Count: 1

2. **Recover Star Maps** (Collect)
   - Description: "Recover the 3 stolen star maps"
   - Type: Collect
   - Item ID: StarMap
   - Count: 3

3. **Escape the Market** (Reach)
   - Description: "Reach the extraction point"
   - Type: Reach
   - Target Tag: ExtractionPoint
   - Count: 1

### Optional Objectives
1. **Unseen Phantom** (Stealth)
   - Description: "Complete without being spotted (Alert Level 0)"
   - Type: Stealth
   - Max Detections: 0

2. **Merchant of Death** (Eliminate)
   - Description: "Eliminate all 4 mercenary lieutenants"
   - Type: Eliminate
   - Target Tag: Mercenary_Lieutenant
   - Count: 4
   - Secret: Yes (revealed after first lieutenant killed)

3. **Speed Demon** (Survive)
   - Description: "Complete in under 180 seconds"
   - Type: Survive
   - Duration: 180

### Skill Loadout
**Available Skills:**
- Capricorn Trap (Attack)
- Orion's Arrow (Attack)
- Decoy (Lure)
- Gemini Clone (Lure)
- Shadow Blend (Stealth)
- Pegasus Dash (Movement - NEW)

**Default Loadout:**
- Slot 1: Orion's Arrow
- Slot 2: Gemini Clone
- Slot 3: Shadow Blend
- Slot 4: Pegasus Dash

### Scripted Events
1. **Market Opening** (OnMissionStart)
   - Type: ShowDialog
   - Text: "The market is crowded. Use the chaos to your advantage."
   - Type: ChangeMusic (fade in)

2. **Hassan Alerted** (OnAlertLevel, level: 3)
   - Type: ShowDialog
   - Text: "Hassan is fleeing! Don't let him escape!"
   - Type: SpawnGuards
   - Spawn Count: 4
   - Spawn Point: Hassan_Guards

3. **First Map Collected** (OnObjectiveComplete, index: 1, progress: 1)
   - Type: ShowDialog
   - Text: "Star map secured. Two more to find."

4. **Hassan Eliminated** (OnObjectiveComplete, index: 0)
   - Type: ShowDialog
   - Text: "Target eliminated. Recover the maps and extract."
   - Type: SetObjective (reveal secret objective)

5. **Time Warning** (OnTimer, 240s)
   - Type: ShowDialog
   - Text: "60 seconds until market closes!"

### Rewards
- Experience: 200 XP
- Bonus per Optional: +75 XP
- Unlocked Skills: Andromeda's Veil (Stealth), Aquarius Flow (Movement)

### Level Layout
```
Night Market District (Large):

┌──────────────────────────────────┐
│ [North Gate - Extraction Point]  │
├──────────────────────────────────┤
│  Spice Stalls    Weapon Stalls   │
│    [Map 1]          [Map 2]      │
│        ↓               ↓          │
│  Market Plaza ←──→ Silk Quarter  │
│      [Hassan]                     │
│        ↓                          │
│  Underground Vault  [Start]       │
│    [Map 3]                        │
└──────────────────────────────────┘

Key Features:
- Multiple vertical levels (street, rooftops, underground)
- Crowd NPCs (provide cover, can be spooked)
- Market stalls (hiding spots, destructible)
- 12 mercenary guards + 4 lieutenants + Hassan
- 5 star map locations (3 required, 2 decoys)
- Time-based patrol changes
```

### Guard Behavior
**Hassan (Target)**
- Behavior: Stationary in plaza, flees if alerted
- Flee Route: Underground vault → secret exit
- Guards: 4 personal bodyguards (follow Hassan)
- Vision: 360° (always alert), 6 unit range

**Mercenary Lieutenants (4)**
- Patrol: Fixed high-value areas
- Vision: 120° cone, 9 unit range
- Alert Level: +3 on detection, coordinate with others
- Equipment: Better armor (requires 2 hits or skill)

**Regular Mercenaries (12)**
- Patrol: Market routes, some stationary
- Vision: 90° cone, 7 unit range
- Alert Level: +1 on detection

**Crowd NPCs (30+)**
- Behavior: Random wander, flee when alerted
- Effect: Provide visual cover but can reveal player if spooked

### Environmental Hazards
- **Market Stalls**: Can be knocked over (creates noise, blocks paths)
- **Crowd Panic**: High alert causes stampede (chaotic cover)
- **Time Progression**: Guards change routes every 60s
- **Light Sources**: Can be extinguished with skills

### Design Notes
- **Focus**: Multi-objective management, time pressure
- **Pacing**: 8-12 minutes, encourages speed
- **Difficulty**: Normal - tighter timing, smarter guards
- **Skills**: Requires full loadout mastery (4 skills)
- **Replay Value**: High - multiple routes, secret objectives
- **Music**: 120 BPM, energetic market theme
- **Dynamic Elements**: Crowd AI, time-based changes

---

## Mission Template

Use this template for designing new missions:

```yaml
### Mission Info
Mission ID: mission_XX_name
Display Name: "Mission Name"
Difficulty: [Tutorial/Easy/Normal/Hard]
Scene: Scene_Name
Music BPM: [80-140]
Time Limit: [seconds or None]
Max Alert Level: [1-10]

### Briefing
[2-3 sentences describing mission context and goals]

### Primary Objectives
1. [Objective Name] ([Type])
   - Description: "[Player-facing text]"
   - Type: [Eliminate/Reach/Collect/Survive/Stealth/etc]
   - Target: [Tag or ID]
   - Count: [Number]

### Optional Objectives
1. [Optional Name] ([Type])
   - Description: "[Player-facing text]"
   - Secret: [Yes/No]
   - Type & parameters...

### Skill Loadout
Available Skills: [List all unlocked skills]
Default Loadout: [4 recommended skills]

### Scripted Events
1. [Event Name] (Trigger: [Type])
   - Event Type: [ShowDialog/SpawnGuards/etc]
   - Parameters...

### Rewards
- Experience: [Base XP]
- Bonus per Optional: [XP]
- Unlocked Skills: [List]

### Level Layout
[ASCII map or description]
- Key locations
- Guard positions
- Hiding spots
- Routes

### Guard Behavior
[Describe each guard type/captain]

### Design Notes
- Focus: [What skills/mechanics does this teach?]
- Pacing: [Expected completion time]
- Difficulty: [Key challenges]
- Replay Value: [What encourages replaying?]
```

---

## Mission Progression Arc

### Skill Introduction Curve
```
Tutorial:
  - Capricorn Trap ✓

Mission 1:
  - Decoy, Orion's Arrow ✓

Mission 2:
  - Gemini Clone, Shadow Blend, Pegasus Dash ✓

Mission 3:
  - Andromeda's Veil, Aquarius Flow ✓

All 8 skills unlocked by Mission 3!
```

### Complexity Progression
```
Tutorial:    1 objective,  2 guards,  No fail condition
Mission 1:   3 objectives, 10 guards, Alert system
Mission 2:   3 objectives, 17 guards, Time limit + Crowds
Mission 3+:  4+ objectives, 20+ guards, Environmental hazards
```

### Difficulty Scaling
```
Tutorial: Perfect window ±50ms, No penalties
Mission 1: Perfect window ±40ms, Forgiving detection
Mission 2: Perfect window ±40ms, Normal detection
Mission 3: Perfect window ±35ms, Strict detection
```

---

## Implementation Checklist

For each mission, create:

### Unity Editor Tasks
- [ ] Scene layout and geometry
- [ ] Guard waypoint paths
- [ ] Hiding spot placement
- [ ] Lighting and atmosphere
- [ ] Collectible item placement
- [ ] Trigger zones for objectives
- [ ] Spawn points for events

### ScriptableObject Assets
- [ ] MissionData asset
- [ ] Primary objective definitions
- [ ] Optional objective definitions
- [ ] Scripted event definitions
- [ ] Skill loadout configuration

### Testing
- [ ] All objectives completable
- [ ] Optional objectives challenging but fair
- [ ] Scripted events trigger correctly
- [ ] Guard AI behavior appropriate
- [ ] Time limits balanced
- [ ] Music sync feels good
- [ ] Replay value exists

### Polish
- [ ] Mission briefing audio/VO
- [ ] Objective complete SFX
- [ ] Environmental ambient sound
- [ ] Victory/failure screen
- [ ] Stats tracking (time, detection, perfects)

---

## Estimated Development Time

Per Mission:
- Design: 2-4 hours
- Level Layout (Unity): 4-8 hours
- Guard Placement & AI: 2-4 hours
- Objective Scripting: 2-3 hours
- Testing & Balance: 3-5 hours
- Polish: 2-4 hours

**Total per mission: 15-28 hours**

With 3 missions: **45-84 hours of mission content development**

---

**Status**: Mission designs complete - ready for Unity implementation
**Next**: Create MissionData ScriptableObject assets in Unity Editor
