using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어가 미션에 가져갈 스킬 4가지(W, A, S, D)를 관리합니다.
/// </summary>
public class SkillLoadoutManager : MonoBehaviour
{
    [Header("▶ 장착 스킬 (미션 시작 전 설정)")]
    [Tooltip("Stealth(W), Lure(A), Movement(S), Attack(D)에 할당될 스킬")]
    public ConstellationSkillData stealthSkill;   // KeyCode.W
    public ConstellationSkillData lureSkill;      // KeyCode.A
    public ConstellationSkillData movementSkill;  // KeyCode.S
    public ConstellationSkillData attackSkill;    // KeyCode.D

    // 런타임에서 사용할 활성 스킬 맵
    public Dictionary<KeyCode, ConstellationSkillData> activeSkills = new Dictionary<KeyCode, ConstellationSkillData>();

    void Awake()
    {
        // 키 코드에 스킬 할당 및 딕셔너리 구성
        if (stealthSkill != null) activeSkills.Add(KeyCode.W, stealthSkill);
        if (lureSkill != null) activeSkills.Add(KeyCode.A, lureSkill);
        if (movementSkill != null) activeSkills.Add(KeyCode.S, movementSkill);
        if (attackSkill != null) activeSkills.Add(KeyCode.D, attackSkill);
    }
}