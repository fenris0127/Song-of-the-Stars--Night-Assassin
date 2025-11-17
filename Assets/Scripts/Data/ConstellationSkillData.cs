using UnityEngine;

public enum SkillCategory { Attack, Lure, Stealth, Movement }

[CreateAssetMenu(fileName = "NewSkill", menuName = "Star Assassin/Loadout Skill Data")]
public class ConstellationSkillData : ScriptableObject
{

    [Header("★ Basic Info / 기본 정보")]
    public string skillName;
    [TextArea(2, 4)]
    public string description;
    public SkillCategory category;
    public string constellationName;
    public Sprite icon;

    [Header("♬ Rhythm & Timing / 리듬 및 타이밍")]
    [Tooltip("Number of inputs required (1-3) / 스킬 발동에 필요한 키 입력 횟수 (1~3회)")]
    public int inputCount;
    [Tooltip("Cooldown after use (in beats) / 스킬 사용 후 쿨타임 (비트 단위)")]
    public int cooldownBeats;
    [Tooltip("Focus cost to activate skill / 스킬 활성화 Focus 비용")]
    public float focusCost = 15f;

    [Header("▶ Effects / 이펙트")]
    public GameObject skillEffectPrefab;
    [Tooltip("Effect duration (in beats) / 이펙트 지속 시간(비트 단위)")]
    public int effectDurationBeats = 4;

    [Header("▶ Skill-Specific Stats")]
    [Tooltip("Range in meters (for ranged skills)")]
    public float range = 5f;
    [Tooltip("Speed multiplier (for movement skills)")]
    public float speedMultiplier = 1f;
    [Tooltip("Damage amount (for attack skills)")]
    public float damage = 0f;
    [Tooltip("Activation delay in beats")]
    public int activationDelayBeats = 0;

    [Header("▶ Audio")]
    public AudioClip activationSFX;
    public AudioClip loopSFX;
    public AudioClip endSFX;

    [Header("▶ Balance Notes")]
    [TextArea(3, 6)]
    public string balanceNotes;
}