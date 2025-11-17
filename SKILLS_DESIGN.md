# Constellation Skills Design Document
## Song of the Stars - Night Assassin

**Version**: 1.0
**Date**: November 17, 2025
**Approach**: Quality-Focused (8 Core Skills)

---

## Design Philosophy

### Core Principles
1. **Each skill feels unique** - Distinct gameplay impact
2. **Clear use cases** - Players understand when to use
3. **Balanced costs** - Risk/reward trade-offs
4. **Visual clarity** - Easy to understand effects
5. **Combo potential** - Skills synergize with each other

### Skill Distribution (4 Categories × 2 Skills)
```
Attack:  Capricorn Trap ✅ + Orion's Arrow 🆕
Lure:    Decoy ✅ + Gemini Clone 🆕
Stealth: Shadow Blend 🆕 + Andromeda's Veil 🆕
Movement: Pegasus Dash 🆕 + Aquarius Flow 🆕
```

---

## Skill Specifications

### ATTACK SKILLS

#### 1. Capricorn Trap (EXISTING ✅)
**Status**: Already implemented
**Category**: Attack
**Description**: Places a trap that stuns guards who step on it

**Stats**:
- Cooldown: 12 beats
- Focus Cost: 20
- Input Pattern: 1 (single beat)
- Effect Duration: Until triggered
- Range: Melee placement

**Use Case**: Zone control, blocking patrol routes

---

#### 2. Orion's Arrow 🆕
**Category**: Attack
**Constellation**: Orion (The Hunter)
**Theme**: Precise ranged elimination

**Description**:
Fires a star-shaped projectile that eliminates a single guard at range. Requires line of sight.

**Stats**:
- Cooldown: 16 beats
- Focus Cost: 30
- Input Pattern: 1-2 (two beats, rhythm sensitive)
- Range: 15 meters
- Damage: Instant elimination
- Projectile Speed: Fast (travels 15m in 0.5s)

**Mechanics**:
1. Player inputs 1-2 rhythm pattern
2. Cursor appears to select target guard
3. On confirmation, fires projectile
4. Projectile travels in straight line
5. Eliminates first guard hit
6. Blocked by obstacles

**Visual Effects**:
- Golden star projectile with trail
- Impact sparkle effect on hit
- Guard vanish animation

**Audio**:
- Whoosh sound on fire
- Twinkle on impact

**Balance Notes**:
- High focus cost prevents spam
- Long cooldown = strategic use only
- Line of sight requirement adds skill
- Single target only (no pierce)

**Use Cases**:
- Eliminate distant patrol guard
- Eliminate watchtower guard
- Clear path before moving
- Combo: Use after Decoy to reposition

**Counters**:
- Obstacles block shot
- High focus cost
- Long cooldown
- Alerts nearby guards if they see it

---

### LURE SKILLS

#### 3. Decoy (EXISTING ✅)
**Status**: Already implemented
**Category**: Lure
**Description**: Spawns a stationary decoy that attracts guard attention

**Stats**:
- Cooldown: 8 beats
- Focus Cost: 15
- Input Pattern: 2 (single beat)
- Duration: 20 beats
- Range: 5 meters placement

**Use Case**: Distract guards, create patrol gaps

---

#### 4. Gemini Clone 🆕
**Category**: Lure
**Constellation**: Gemini (The Twins)
**Theme**: Moving duplicate of yourself

**Description**:
Creates a semi-transparent clone that mirrors your last movement pattern, drawing guard attention.

**Stats**:
- Cooldown: 14 beats
- Focus Cost: 25
- Input Pattern: 2-2 (two beats, same key twice)
- Duration: 12 beats
- Movement Pattern: Repeats last 4 player movements

**Mechanics**:
1. Records player's last 4 directional inputs
2. Clone spawns at player position
3. Clone replays movement pattern in loop
4. Guards investigate clone (higher priority than static Decoy)
5. Clone fades after duration
6. Clone cannot be damaged

**Movement Examples**:
```
If player moved: Up → Right → Right → Down
Clone repeats: Up → Right → Right → Down → [loop]
```

**Visual Effects**:
- Semi-transparent player sprite (50% alpha, cyan tint)
- Shimmer/glitch effect while moving
- Star particles trail behind clone
- Fade-out on expiration

**Audio**:
- Clone spawn: Ethereal chime
- Clone footsteps: Quieter echoes
- Clone fade: Soft dissipation sound

**Balance Notes**:
- More expensive than Decoy (25 vs 15 focus)
- Shorter duration (12 vs 20 beats)
- But more effective: moves naturally
- Guards investigate longer

**Use Cases**:
- Lead guards away from path
- Create false patrol route
- Bait guards into trap
- Buy time for assassination
- Combo: Clone distracts while you use Orion's Arrow

**Advanced Tactics**:
- Intentionally create looping patrol
- Use complex movement to maximize distraction time
- Combine with Shadow Blend: Clone moves, you hide

---

### STEALTH SKILLS

#### 5. Shadow Blend 🆕
**Category**: Stealth
**Constellation**: Cygnus (The Swan) - associated with shadows
**Theme**: Stillness becomes invisibility

**Description**:
Become invisible while standing completely still. Movement or actions break invisibility.

**Stats**:
- Cooldown: 10 beats
- Focus Cost: 20
- Input Pattern: 3-4 (two beats)
- Duration: Until movement or 16 beats max
- Activation Delay: 1 beat (prevents instant use)

**Mechanics**:
1. Player activates skill
2. After 1 beat delay, player becomes invisible IF stationary
3. Invisibility persists while not moving
4. Any movement/skill/action breaks invisibility
5. Max duration: 16 beats
6. Guards cannot detect invisible player

**Visual Effects**:
- Player sprite fades to 10% alpha
- Dark aura surrounds player
- Shadow particles swirl slowly
- Alert indicator when movement would break stealth

**Audio**:
- Activation: Soft shadow whisper
- Break stealth: Sharp "snap" sound

**Balance Notes**:
- Moderate focus cost
- Requires patience (standing still)
- Can't act while invisible
- Guards pass by harmlessly
- Perfect for letting patrols pass

**Use Cases**:
- Wait for patrol to pass
- Hide in corner during alert
- Time assassination after guards pass
- Emergency panic button
- Combo: Shadow Blend → wait → Pegasus Dash past

**Limitations**:
- Cannot move
- Cannot use skills
- Cannot assassinate
- Breaks on ANY input
- 1 beat delay prevents panic use

**Skill Expression**:
- Timing when to activate
- Positioning before activation
- Patience management
- Reading patrol patterns

---

#### 6. Andromeda's Veil 🆕
**Category**: Stealth
**Constellation**: Andromeda (The Chained Princess)
**Theme**: Freedom through complete invisibility

**Description**:
Complete invisibility for short duration. Can move and act freely but costs high focus.

**Stats**:
- Cooldown: 20 beats (LONG)
- Focus Cost: 40 (VERY HIGH)
- Input Pattern: 4-4-4 (three beats, all same)
- Duration: 6 beats
- Movement: Full speed allowed

**Mechanics**:
1. Player becomes completely invisible
2. Can move at full speed
3. Can use other skills
4. Can perform assassinations
5. Guards cannot detect at all
6. Duration: 6 beats only

**Visual Effects**:
- Player sprite becomes shimmer outline only
- Starlight particles follow movement
- Constellation symbol appears overhead
- Countdown timer visible to player

**Audio**:
- Activation: Magical "veil drop" sound
- Duration: Ethereal ambient hum
- Expiration: Soft chime warning
- Break: Fabric rustle

**Balance Notes**:
- VERY expensive (40 focus = 4 Perfect timings)
- Long cooldown prevents abuse
- Short duration = must use wisely
- Triple input pattern = harder to activate
- Ultimate escape/assassination tool

**Use Cases**:
- Emergency escape when detected
- Dash through heavily patrolled area
- Assassination in open area
- Rescue mission (grab objective, run)
- "Ultimate ability" feeling

**Tactical Depth**:
- Save for critical moments
- High risk if you fail to Perfect (no focus recovery)
- Coordinate with mission objectives
- Time with guard patterns

**Combo Potential**:
- Veil → Orion's Arrow → Escape
- Veil → Assassinate 2-3 guards rapidly
- Veil → Pegasus Dash through danger zone

---

### MOVEMENT SKILLS

#### 7. Pegasus Dash 🆕
**Category**: Movement
**Constellation**: Pegasus (The Winged Horse)
**Theme**: Swift repositioning

**Description**:
Instant teleport dash in facing direction. Covers 3 grid tiles instantly.

**Stats**:
- Cooldown: 8 beats
- Focus Cost: 20
- Input Pattern: 1-1 (two beats, quick double-tap)
- Distance: 6 meters (3 grid tiles)
- Direction: Current facing direction
- Speed: Instant teleport

**Mechanics**:
1. Player inputs 1-1 pattern
2. Instantly teleports forward 3 tiles
3. Leaves particle trail
4. Can dash through obstacles? NO (blocked by walls)
5. Can dash through guards? NO (stops at collision)
6. Brief invulnerability window (0.2s)

**Visual Effects**:
- Wing particles at start position
- Dash trail of feathers/stars
- Landing impact sparkle
- Player sprite blur during dash

**Audio**:
- Whoosh with wing flaps
- Soft landing sound

**Balance Notes**:
- Low cooldown = frequent use
- Moderate focus cost
- Short distance (tactical, not escape)
- Can't phase through obstacles
- Rhythm input = skill-based

**Use Cases**:
- Quick reposition during combat
- Dodge guard line of sight
- Close gap for assassination
- Escape after alert
- Cross dangerous areas quickly

**Advanced Tactics**:
- Dash perpendicular to guard patrol
- Dash into cover
- Dash behind guard for backstab
- Chain with other movement (free move)

**Risk/Reward**:
- Can dash INTO danger if not careful
- No mid-dash cancel
- Requires spatial awareness
- Walls block dash (can waste cooldown)

---

#### 8. Aquarius Flow 🆕
**Category**: Movement
**Constellation**: Aquarius (The Water Bearer)
**Theme**: Fluid, flowing movement

**Description**:
Doubles movement speed for duration. Move like water flowing.

**Stats**:
- Cooldown: 12 beats
- Focus Cost: 15
- Input Pattern: 4 (single beat)
- Duration: 8 beats
- Speed Multiplier: 2x base movement speed

**Mechanics**:
1. Activate with single input
2. Movement speed doubles
3. Still synchronized to beats (but covers more distance)
4. Can combine with stealth (fast stealth)
5. Duration: 8 beats
6. No other penalties

**Visual Effects**:
- Blue water-like aura around player
- Motion blur trail
- Water droplet particles
- Gentle flowing animation on sprite

**Audio**:
- Activation: Water rush sound
- Duration: Gentle flowing ambient
- Footsteps: Faster pace
- Expiration: Water settle sound

**Balance Notes**:
- Low focus cost (cheapest movement skill)
- Simple activation (single input)
- Good duration (8 beats)
- Speed boost is significant but not OP
- Synergizes with stealth

**Use Cases**:
- Cover large distances quickly
- Outrun guard detection buildup
- Reach time-sensitive objectives
- Escape after assassination
- Speed through open areas

**Synergies**:
- Flow + Stealth = Fast invisible movement
- Flow + Dash = Ultra mobility
- Flow + Clone = Clone moves fast too (mirrors last movements)
- Flow at mission start = Fast positioning

**Skill Expression**:
- Route optimization
- Timing activation
- Combining with other skills
- Rhythm management at high speed

---

## Skill Synergy Matrix

### Combo Examples

**Aggressive Combos**:
1. **Gemini Clone → Orion's Arrow**
   - Clone distracts guards
   - Snipe distracted guard from range
   - Estimated: 55 focus total

2. **Aquarius Flow → Orion's Arrow**
   - Speed to vantage point
   - Snipe from new position
   - Estimated: 45 focus total

3. **Andromeda's Veil → Multi-Assassination**
   - Full invisibility
   - Eliminate 2-3 guards rapidly
   - Ultimate combo: 40 focus

**Defensive Combos**:
1. **Shadow Blend → Pegasus Dash**
   - Wait in hiding
   - Dash when safe moment arrives
   - Estimated: 40 focus total

2. **Gemini Clone → Shadow Blend**
   - Clone distracts
   - Hide in plain sight
   - Estimated: 45 focus total

**Mobility Combos**:
1. **Aquarius Flow → Pegasus Dash**
   - Speed boost + teleport
   - Ultra-fast repositioning
   - Estimated: 35 focus total

2. **Andromeda's Veil → Pegasus Dash**
   - Invisible teleport
   - Ultimate escape
   - Estimated: 60 focus total

---

## Focus Economy Analysis

### Focus Costs Summary
```
Cheapest → Most Expensive:

15 focus: Decoy (Lure), Aquarius Flow (Movement)
20 focus: Capricorn Trap (Attack), Shadow Blend (Stealth), Pegasus Dash (Movement)
25 focus: Gemini Clone (Lure)
30 focus: Orion's Arrow (Attack)
40 focus: Andromeda's Veil (Stealth) - ULTIMATE
```

### Focus Generation
- Max Focus: 100
- Perfect timing: +10 focus
- Great timing: +5 focus (if implemented)
- Miss: -15 focus

**Example Scenarios**:
- 4 Perfect timings = 40 focus = Andromeda's Veil
- 2 Perfect timings = 20 focus = Most basic skills
- 3 Perfect timings = 30 focus = Orion's Arrow

**Skill Spam Prevention**:
With new 15 focus base cost (from balance changes), players must:
- Earn focus through good rhythm
- Choose skills wisely
- Cannot spam skills endlessly

---

## Balancing Guidelines

### Cooldown Philosophy
- **Short (8-10 beats)**: Frequent tactical use
- **Medium (12-14 beats)**: Strategic timing required
- **Long (16-20 beats)**: Save for key moments

### Focus Cost Philosophy
- **Low (15-20)**: Can use multiple times per mission
- **Medium (25-30)**: Requires good rhythm performance
- **High (40+)**: Ultimate abilities, rare use

### Input Pattern Philosophy
- **Single (1 beat)**: Easy activation, more powerful cooldown/cost
- **Double (2 beats)**: Skill check, standard abilities
- **Triple (3 beats)**: Hard activation, powerful effects

---

## Implementation Checklist

### Per Skill Requirements
- [ ] ScriptableObject data asset
- [ ] Skill behavior script
- [ ] VFX prefab (activation, duration, end)
- [ ] SFX (activation, loop, end)
- [ ] Icon sprite (128x128)
- [ ] In-game testing
- [ ] Balance tuning

### Integration Points
- [ ] SkillLoadoutManager: Add new skills to pool
- [ ] RhythmPatternChecker: Handle new skill categories
- [ ] PlayerController: Movement skill handlers
- [ ] PlayerStealth: Stealth skill handlers
- [ ] PlayerAssassination: Attack skill handlers

---

## Testing Plan

### Skill-Specific Tests

**Orion's Arrow**:
- [ ] Projectile travels straight
- [ ] Eliminates guard on hit
- [ ] Blocked by obstacles
- [ ] Cursor targeting works
- [ ] Focus cost applied correctly

**Gemini Clone**:
- [ ] Records movement correctly
- [ ] Loops movement pattern
- [ ] Guards investigate clone
- [ ] Clone fades after duration
- [ ] Movement recording edge cases

**Shadow Blend**:
- [ ] Invisibility on stillness
- [ ] Breaks on movement
- [ ] Guards ignore invisible player
- [ ] Max duration enforced
- [ ] 1 beat delay works

**Andromeda's Veil**:
- [ ] Full invisibility works
- [ ] Can move while invisible
- [ ] Can use skills while invisible
- [ ] Duration countdown accurate
- [ ] High focus cost prevents spam

**Pegasus Dash**:
- [ ] Instant teleport works
- [ ] Distance correct (3 tiles)
- [ ] Blocked by obstacles
- [ ] Invulnerability window works
- [ ] Double-tap input responsive

**Aquarius Flow**:
- [ ] Speed doubles correctly
- [ ] Duration accurate
- [ ] Visual effects sync
- [ ] Compatible with stealth
- [ ] Movement still beat-synced

### Combo Tests
- [ ] Clone + Arrow combo
- [ ] Blend + Dash combo
- [ ] Flow + Veil combo
- [ ] All skills usable in sequence

### Balance Tests
- [ ] Focus economy feels fair
- [ ] Cooldowns prevent spam
- [ ] No single skill dominates
- [ ] All skills have use cases
- [ ] Skill progression feels rewarding

---

## Next Steps

1. **Implement Skill Scripts** (6 new skills)
2. **Create ScriptableObject Assets**
3. **Design VFX Prefabs** (placeholder or final)
4. **Integrate with RhythmPatternChecker**
5. **Create Skill Icons** (can be placeholder)
6. **Balance Testing**
7. **Documentation for Unity Setup**

**Estimated Time**: 8-10 days total (1-1.5 days per skill with testing)

---

**Document Version**: 1.0
**Total Skills**: 8 (2 existing + 6 new)
**Focus**: Quality over Quantity
**Status**: Design Complete, Ready for Implementation
