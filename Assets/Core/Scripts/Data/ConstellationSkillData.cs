using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewConstellationSkill", menuName = "Star Assassin/Constellation Skill Data")]
public class ConstellationSkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public string constellationName;
    
    [Header("Rhythm and Timing")]
    public List<KeyCode> rhythmPattern;
    public int cooldownBeats;
    
    [Header("Visuals & Sound")]
    public GameObject skillEffectPrefab;
}