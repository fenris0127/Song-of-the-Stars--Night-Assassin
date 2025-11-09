using UnityEngine;

public enum SkillCategory { Attack, Lure, Stealth, Movement }

[CreateAssetMenu(fileName = "NewSkill", menuName = "Star Assassin/Loadout Skill Data")]
public class ConstellationSkillData : ScriptableObject
{
    
    [Header("★ 기본 정보")]
    public string skillName;
    public SkillCategory category;
    public Sprite icon; 

    [Header("♬ 리듬 및 타이밍")]
    [Tooltip("스킬 발동에 필요한 키 입력 횟수 (1~3회)")]
    public int inputCount; 
    [Tooltip("스킬 사용 후 쿨타임 (비트 단위)")]
    public int cooldownBeats;

    [Header("▶ 이펙트")]
    public GameObject skillEffectPrefab; 
    [Tooltip("이펙트(예: 데코이)의 지속 시간(비트 단위). PlayerController.ActivateIllusion에서 사용됩니다.")]
    public int effectDurationBeats = 4;
}