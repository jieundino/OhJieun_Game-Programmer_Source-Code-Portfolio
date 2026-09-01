using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Normal,
    Dot,
    Area
}

[CreateAssetMenu(
    fileName ="NewSkill",
    menuName ="Skill/Skill Data")]
public class SOSkill : ScriptableObject
{
    [Header("Basic")]
    public SkillType skillType;
    public float coolTime;
    public string animationName;
    public Sprite icon;

    [Header("Damage")]
    public float damage;

    [Tooltip("단일 대상 및 지속 피해 스킬의 사용 가능 거리")]
    public float range = 5f;

    [Header("Area Skill")]
    [Tooltip("범위 공격의 반경")]
    public float areaRadius = 2.5f;

    [Header("Dot Skill")]
    [Tooltip("틱당 지속 피해")]
    public float dotDamagePerTick = 2f;

    [Tooltip("지속 피해 총 지속 시간")]
    public float dotDuration = 10f;

    [Tooltip("지속 피해 틱 간격")]
    public float dotInterval = 1f;
}
