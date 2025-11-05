using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Star Assassin/Loadout Skill Data")]
public class ConstellationSkillData : ScriptableObject
{
    public enum SkillCategory { Attack, Lure, Stealth, Movement }
    
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
}