# AI Agent Instructions for Song of the Stars: Night Assassin

## Project Overview
This is a rhythm-based stealth assassination game developed in Unity where players must sync their actions with the beat while using constellation-themed skills to complete missions.

## Core Architecture

### Key Components
1. **Rhythm System**
- `RhythmSyncManager`: Core beat tracking and timing system
- `RhythmPatternChecker`: Handles skill input timing, cooldowns, and Focus system
- See `Assets/Scripts/Rhythm/` for implementation details

2. **Skill System**
- `ConstellationSkillData`: ScriptableObject for defining skills (Attack, Lure, Stealth, Movement)
- `SkillLoadoutManager`: Manages player's active skills bound to keys 1-4
- See `Assets/Scripts/Player/SkillLoadoutManager.cs` for reference

3. **Guard AI**
- `GuardRhythmPatrol`: Rhythm-synchronized guard behavior and patrol patterns
- See `Assets/Scripts/Enemies/GuardRhythmPatrol.cs` for implementation

### Core Game Flow
1. Game state management through `GameManager` (MainMenu → InGame → Paused/MissionComplete/GameOver)
2. Player performs actions in sync with rhythm:
   - Perfect/Great/Miss timing judgments
   - Focus system rewards perfect timing
   - Skills require 1-3 rhythmic inputs to activate

## Development Patterns

### Difficulty System
- Four levels: Easy, Normal, Hard, Expert
- Settings in `DifficultySettings` class control:
  - Rhythm timing tolerances
  - Guard behavior parameters
  - Player attributes
  - Skill cooldowns
  - 데이터 분리 의무화: DifficultySettings 클래스 및 게임 밸런싱에 사용되는 모든 구성   데이터(예: 이동 거리, 속도, 쿨타임 값)는 ScriptableObject를 사용하여 코드 로직과 분리해야 합니다. 

### Save System
- JSON-based save system for:
  - Mission progress
  - Unlocked skills
  - Player settings
  - Statistics tracking
- See `SaveData` class in `Assets/Scripts/Core/SaveSystem.cs`

### UI Management
- Centralized through `UIManager` for:
  - Skill icons and cooldowns
  - Focus bar
  - Alert levels
  - Judgment text display

### Architectural Guardrails (AI 에이전트 필수 준수 사항)
- AI Agent는 코드를 생성하거나 수정할 때 다음의 아키텍처 원칙을 엄격하게 준수해야 합니다.
    1. 모듈성 및 SRP: **KISS(Keep It Simple, Stupid) 및 단일 책임 원칙(SRP)**을 사용하여, 생성하는 모든 C# 스크립트는 하나의 명확한 기능(예: Movement, Health, Detection)만 담당해야 합니다. 
    2. 디커플링 통신: 컴포넌트 간 또는 시스템 간 통신(예: 리듬 이벤트 전파, UI 업데이트 트리거)에는 C# Events 또는 UnityEvents를 사용하여 직접적인 참조와 결합도(Coupling)를 피해야 합니다.
    3. 상태 관리 패턴: GuardRhythmPatrol이나 PlayerController와 같이 복잡한 행동 로직을 가진 엔티티를 다룰 때는 State 패턴을 구현하여 상태 전환을 명확하고 확장 가능하게 관리해야 합니다.

## Common Operations

### Adding New Skills
1. Create ScriptableObject of type `ConstellationSkillData`
2. Configure skill parameters:
   - Category (Attack/Lure/Stealth/Movement)
   - Input count (1-3)
   - Cooldown (in beats)
3. Implement skill effects through prefabs

### Working with Rhythm
- All timed actions should sync through `RhythmSyncManager`
- Use `CheckJudgment()` for input timing validation
- Handle perfect/great/miss states appropriately

## Code Conventions
- Korean comments for gameplay mechanic descriptions
- English for technical documentation
- Use region tags for code organization
- Follow Unity's component-based architecture
- 디버깅 로깅: 잠재적 오류, 중요한 상태 변화(예: Alert Level 상승, Skill 발동 성공/실패)가 발생하는 지점에는 반드시 **로그(Debug.Log 또는 Debug.LogWarning)**를 포함하여 디버깅을 용이하게 합니다.
- 통합 지침 제공: 생성된 C# 스크립트의 본문 후에는, 해당 스크립트를 Unity Hierarchy의 어떤 GameObject에 연결하고, Editor에서 어떤 설정을 해야 하는지에 대한 명확한 단계별 지침을 schematic하게 설명해야 합니다.
- 출력 형식: 생성된 코드는 완전해야 하며, 간결하고 명확한 주석을 포함해야 합니다.

## Key Integration Points
- Player → Guard interactions through `PlayerAssassination`
- Rhythm → Gameplay sync via `RhythmSyncManager`
- UI ↔ Game state through event system
- Save system ↔ Game progress via `SaveData`