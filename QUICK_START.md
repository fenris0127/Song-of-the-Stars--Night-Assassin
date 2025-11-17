# Quick Start Guide
## Song of the Stars - Night Assassin

**빠른 시작 가이드** - 30분 안에 프로젝트를 실행하세요!

---

## ⚡ 5분 안에 시작하기

### 1. Unity에서 프로젝트 열기
- Unity Hub에서 "Open" 클릭
- 이 폴더 선택
- Unity 버전: **6.0.2 이상**

### 2. 콘텐츠 자동 생성 (30초)
```
Unity 메뉴: Song of the Stars → Content → Initialize All Content
버튼 클릭: 🚀 GENERATE ALL CONTENT
대기: 30초
완료! ✅
```

### 3. 테스트 씬 열기
```
Assets/Scenes/TestScene.unity (곧 생성됨)
또는
File → New Scene
```

끝! 이제 개발 시작할 수 있습니다! 🎉

---

## 📁 프로젝트 구조

```
Song-of-the-Stars--Night-Assassin/
├── Assets/
│   ├── Data/                    # ScriptableObjects (스킬, 미션)
│   │   ├── Skills/              # 8개 스킬 데이터
│   │   └── Missions/            # 3개 미션 데이터
│   │
│   ├── Scripts/                 # 모든 C# 스크립트
│   │   ├── Data/                # 데이터 구조 및 생성 도구
│   │   ├── Systems/             # 게임 시스템 (8개 핵심 시스템)
│   │   ├── Skills/              # 스킬 관련 로직
│   │   ├── Missions/            # 미션 관련 로직
│   │   ├── Procedural/          # 절차적 생성
│   │   ├── Tools/               # 개발 도구
│   │   ├── UI/                  # UI 컨트롤러
│   │   └── Editor/              # Unity Editor 확장
│   │
│   ├── Art/                     # 아트 에셋
│   │   └── Icons/Skills/        # 스킬 아이콘 (8개 플레이스홀더)
│   │
│   ├── Audio/                   # 오디오 파일
│   │   ├── Music/               # 배경음악
│   │   └── SFX/                 # 효과음
│   │
│   └── UI/                      # UI 스프라이트
│       └── Sprites/             # UI 이미지들
│
├── Docs/                        # 문서 (PRD, 디자인 문서)
│   ├── PRD.md                   # 제품 요구사항 문서
│   ├── SKILLS_DESIGN.md         # 스킬 디자인
│   ├── MISSION_DESIGNS.md       # 미션 디자인
│   └── ARCHITECTURE.md          # 아키텍처 문서
│
└── 가이드들/
    ├── CONTENT_GENERATION_GUIDE.md  # 콘텐츠 생성 완벽 가이드
    ├── DATA_SETUP_GUIDE.md          # 데이터 설정 가이드
    ├── UI_ASSETS_GUIDE.md           # UI 에셋 요구사항
    └── QUICK_START.md (이 파일)     # 빠른 시작
```

---

## 🎯 핵심 기능들

### 자동 생성 시스템 ⚡
- **원클릭 콘텐츠 생성**: 8개 스킬 + 3개 미션 자동 생성
- **절차적 미션**: 랜덤 시드 기반 무한 미션
- **절차적 레벨**: BSP 알고리즘 기반 레벨 생성
- **데이터 자동 채우기**: 수동 입력 98% 절감

### 8대 핵심 시스템 🎮
1. **SettingsManager** - 모든 게임 설정 중앙 관리
2. **SaveLoadManager** - 3개 슬롯 저장/로드 시스템
3. **AudioManager** - 리듬 동기화 오디오 엔진
4. **PlayerStatsTracker** - 포괄적 통계 추적
5. **AchievementSystem** - 20+ 업적 시스템
6. **LeaderboardSystem** - 로컬 순위표
7. **DailyChallengeSystem** - 매일 새로운 도전
8. **ReplaySystem** - 게임플레이 녹화/재생

### 8개 별자리 스킬 ⭐
1. **Capricorn Trap** (염소자리 덫) - 적 구속
2. **Orion's Arrow** (오리온의 화살) - 원거리 암살
3. **Leo Decoy** (사자자리 미끼) - 적 유인
4. **Gemini Clone** (쌍둥이자리 분신) - 분신 생성
5. **Shadow Blend** (그림자 은신) - 스텔스 강화
6. **Andromeda Veil** (안드로메다 베일) - 지역 투명화
7. **Pegasus Dash** (페가수스 대시) - 순간 이동
8. **Aquarius Flow** (물병자리 흐름) - 시간 감속

---

## 🛠️ Unity 메뉴 사용법

프로젝트를 Unity에서 열면 최상단 메뉴에 **"Song of the Stars"** 메뉴가 나타납니다:

### 📂 Content (콘텐츠 생성)
```
Song of the Stars/
└── Content/
    ├── Initialize All Content        # 🚀 원클릭 생성 창
    ├── 1. Generate Skills Only       # 스킬만 생성
    └── 2. Generate Missions Only     # 미션만 생성
```

**사용법**:
- 첫 실행 시 "Initialize All Content" 실행
- 모든 콘텐츠가 30초 안에 자동 생성됨

### 📊 Data (데이터 도구)
```
Song of the Stars/
└── Data/
    ├── Populate All Skill Data       # 8개 스킬 데이터 채우기
    ├── Populate All Mission Data     # 3개 미션 데이터 채우기
    ├── Generate Random Mission       # 랜덤 미션 생성
    └── ...
```

### 🔧 Tools (개발 도구)
```
Song of the Stars/
└── Tools/
    ├── Analyze Game Balance          # 밸런스 분석
    ├── Generate Level Layout         # 레벨 자동 생성
    ├── Generate Guard Patrols        # 경비 배치 생성
    └── ...
```

---

## 📖 단계별 개발 가이드

### Phase 1: 프로젝트 설정 (5분) ✅ 완료!
- [x] Unity 프로젝트 생성
- [x] 폴더 구조 생성
- [x] 코어 스크립트 작성
- [x] 에디터 도구 생성

### Phase 2: 콘텐츠 생성 (1분)
- [ ] `Song of the Stars → Content → Initialize All Content` 실행
- [ ] 생성 완료 확인
- [ ] Assets 폴더에서 생성된 파일 확인

### Phase 3: 테스트 씬 만들기 (10분)
- [ ] 새 씬 생성: `Assets/Scenes/TestScene.unity`
- [ ] GameManager 오브젝트 추가
- [ ] 핵심 시스템들 추가:
  - SettingsManager
  - AudioManager
  - SaveLoadManager
- [ ] 테스트용 플레이어 오브젝트 추가

### Phase 4: UI 프리팹 만들기 (15분)
- [ ] Canvas 생성
- [ ] PauseMenu 프리팹 생성
- [ ] HUD 프리팹 생성 (체력바, 포커스바)
- [ ] 타이밍 인디케이터 추가

### Phase 5: 첫 미션 테스트 (30분)
- [ ] Tutorial 미션 씬 생성
- [ ] 플레이어 컨트롤러 연결
- [ ] 스킬 시스템 테스트
- [ ] 리듬 시스템 테스트
- [ ] 미션 클리어 확인

---

## 🎨 에셋 교체하기

### 플레이스홀더 아이콘 교체
1. **최종 아이콘 준비** (256x256 PNG)
2. **같은 이름으로 저장**:
   - `CapricornTrap_Icon.png`
   - `OrionsArrow_Icon.png`
   - 등등...
3. **덮어쓰기**: `Assets/Art/Icons/Skills/`
4. Unity가 자동으로 업데이트! ✨

### 음악/효과음 추가
1. **README.txt 확인**: `Assets/Audio/Music/README.txt`
2. **필요한 파일 이름 확인**:
   - `Tutorial_Music.ogg` (100 BPM)
   - `Mission01_Music.ogg` (120 BPM)
   - 등등...
3. **파일 추가**: 정확한 이름으로 폴더에 드롭
4. Unity가 자동 인식!

---

## 🐛 문제 해결

### ❌ "Song of the Stars 메뉴가 안 보여요"
```
1. Unity 닫기
2. Library/ 폴더 삭제
3. Unity 다시 열기
4. 스크립트 컴파일 대기 (1-2분)
5. 메뉴 나타남!
```

### ❌ "콘텐츠 생성이 안 돼요"
```
1. Console 창 확인 (에러 메시지 확인)
2. Assets/Data/ 폴더가 있는지 확인
3. 스크립트 컴파일 에러 있는지 확인
4. Unity 재시작
```

### ❌ "아이콘이 검은색으로 나와요"
```
1. 아이콘 선택
2. Inspector에서 Texture Type 확인
3. "Sprite (2D and UI)"로 변경
4. Apply 클릭
```

---

## 📚 주요 문서들

| 문서 | 설명 | 언제 읽나요? |
|------|------|-------------|
| `PRD.md` | 게임 전체 디자인 | 큰 그림 이해할 때 |
| `CONTENT_GENERATION_GUIDE.md` | 콘텐츠 생성 완벽 가이드 | 콘텐츠 만들 때 |
| `DATA_SETUP_GUIDE.md` | 데이터 구조 상세 설명 | 데이터 커스텀할 때 |
| `UI_ASSETS_GUIDE.md` | UI 스프라이트 요구사항 | UI 만들 때 |
| `SKILLS_DESIGN.md` | 스킬 밸런스 & 디자인 | 스킬 수정할 때 |
| `MISSION_DESIGNS.md` | 미션 구조 & 목표 | 미션 만들 때 |

---

## 🎮 개발 워크플로우

### 매일 개발 시작할 때:
```bash
1. git pull                          # 최신 코드 받기
2. Unity 열기
3. Scene 로드
4. Play 눌러서 테스트
5. 개발 시작!
```

### 새 스킬 추가하기:
```
1. Assets/Data/Skills/ 에서 기존 스킬 복제
2. 이름 변경
3. Inspector에서 값 수정
4. 아이콘 추가
5. 테스트!
```

### 새 미션 추가하기:
```
1. Assets/Data/Missions/ 에서 기존 미션 복제
2. 이름 변경
3. Objectives 수정
4. BPM, 난이도 조정
5. 테스트!
```

---

## 🚀 다음 단계

### 즉시 할 수 있는 것:
- ✅ 콘텐츠 자동 생성 실행
- ✅ 생성된 스킬/미션 데이터 확인
- ✅ 플레이스홀더 아이콘 확인

### 곧 해야 할 것:
- [ ] 테스트 씬 만들기
- [ ] 플레이어 컨트롤러 구현
- [ ] UI 프리팹 만들기
- [ ] 첫 미션 테스트

### 나중에 할 것:
- [ ] 최종 아트 에셋 교체
- [ ] 음악/효과음 추가
- [ ] 더 많은 미션 추가
- [ ] 밸런스 조정

---

## 💡 개발 팁

### 💾 자주 저장하세요!
```
Unity: Ctrl+S (씬 저장)
Git: 의미있는 단위로 커밋
```

### 🧪 자주 테스트하세요!
```
작은 변경 → 즉시 Play → 확인 → 다음 작업
```

### 📊 밸런스 분석 사용하세요!
```
Song of the Stars → Tools → Analyze Game Balance
```

### 🎵 리듬 동기화 확인하세요!
```
AudioManager의 BPM 설정과 미션의 BPM이 일치하는지 확인!
```

---

## 🎉 축하합니다!

이제 **Song of the Stars - Night Assassin** 개발을 시작할 준비가 되었습니다!

**다음 할 일:**
1. Unity에서 프로젝트 열기
2. `Song of the Stars → Content → Initialize All Content` 실행
3. 생성된 콘텐츠 확인
4. 개발 시작!

**질문이 있으면:**
- 관련 가이드 문서 확인 (위 표 참고)
- 코드 주석 읽기 (모든 스크립트에 상세 주석)
- Unity Console 확인 (디버그 메시지 많음)

**행운을 빕니다!** 🌟

---

**마지막 업데이트**: 2025-01-17
**Unity 버전**: 6.0.2+
**예상 설정 시간**: 30분
