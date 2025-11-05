using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어가 미션에 가져갈 스킬 4가지를 관리합니다. (숫자키 1,2,3,4로 최종 변경됨)
/// </summary>
public class SkillLoadoutManager : MonoBehaviour
{
    [Header("▶ 장착 스킬 (미션 시작 전 설정)")]
    // KeyCode.Alpha1 ~ Alpha4로 연결될 스킬들
    public ConstellationSkillData stealthSkill;   
    public ConstellationSkillData lureSkill;      
    public ConstellationSkillData movementSkill;  
    public ConstellationSkillData attackSkill;    

    public Dictionary<KeyCode, ConstellationSkillData> activeSkills = new Dictionary<KeyCode, ConstellationSkillData>();

    void Awake()
    {
        activeSkills.Clear();
        // 스킬 키를 숫자키(Alpha1~4)로 변경
        if (stealthSkill != null) activeSkills.Add(KeyCode.Alpha1, stealthSkill);
        if (lureSkill != null) activeSkills.Add(KeyCode.Alpha2, lureSkill);
        if (movementSkill != null) activeSkills.Add(KeyCode.Alpha3, movementSkill);
        if (attackSkill != null) activeSkills.Add(KeyCode.Alpha4, attackSkill);
    }
}