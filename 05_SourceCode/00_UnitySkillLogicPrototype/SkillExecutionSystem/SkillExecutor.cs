using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    [Header("사정거리 표시")]
    [SerializeField]
    private SkillRangeIndicator rangeIndicator;

    [Header("Targeting")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask enemyLayer;

    private readonly Collider[] hitBuffer = new Collider[32];

    private readonly HashSet<EnemyHealth> uniqueTargets = new HashSet<EnemyHealth>();

    private Vector3 lastAreaCenter;
    private float lastAreaRadius;

    private void Awake()
    {
        if(attackOrigin == null)
        {
            attackOrigin = transform;
        }

        if (rangeIndicator == null)
        {
            rangeIndicator = GetComponent<SkillRangeIndicator>();
        }
    }

    public bool TryExecute(SOSkill skill, out string resultMessage, out Transform targetTransform)
    {
        targetTransform = null;

        if (skill==null)
        {
            resultMessage = "SkillData가 없습니다.";
            return false;
        }

        switch (skill.skillType)
        {
            case SkillType.Normal:
                return ExecuteNormal(skill, out resultMessage, out targetTransform);
            case SkillType.Dot:
                return ExecuteDot(skill, out resultMessage, out targetTransform);
            case SkillType.Area:
                return ExecuteArea(skill, out resultMessage);
            default:
                resultMessage = $"지원하지 않는 스킬 타입: {skill.skillType}";
                return false;
        }
    }

    // 단일 공격 스킬 실행
    private bool ExecuteNormal(SOSkill skill, out string resultMessage, out Transform targetTransform)
    {
        targetTransform = null;
        EnemyHealth target = FindNearestTarget(skill.range);

        if(!ValidateTarget(target, skill.range, out resultMessage))
        {
            return false;
        }

        targetTransform = target.transform;
        target.TakeDamage(skill.damage);
        
        resultMessage = $"[단일 공격] {target.name}에게 {skill.damage}의 피해를 입혔습니다.";
        return true;
    }

    // 지속 피해 스킬 실행
    private bool ExecuteDot(SOSkill skill, out string resultMessage, out Transform targetTransform)
    {
        Vector3 areaCenter =
            attackOrigin.position + Vector3.up * 0.5f;

        targetTransform = null;

        EnemyHealth target = FindNearestTarget(skill.range);

        if(!ValidateTarget(target, skill.range, out resultMessage))
        {
            return false;
        }

        targetTransform = target.transform;
        DotStatusEffect dotEffect = target.GetComponent<DotStatusEffect>();

        if (dotEffect == null)
        {
            resultMessage = $"{target.name}에게 DotStatusEffect 컴포넌트가 없습니다.";
            return false;   
        }

        // 모든 실행 조건 확인 후 Effect 적용
        if (skill.damage > 0f)
        {
            target.TakeDamage(skill.damage);
        }

        dotEffect.ApplyDot(skill.dotDamagePerTick, skill.dotDuration, skill.dotInterval);

        // 1.5초 이후 범위 표시 해제 (기본 0.5초)
        rangeIndicator?.ShowCircle(areaCenter, skill.areaRadius, 1.5f);

        resultMessage = $"[지속 피해] {target.name}에게 지속 피해 적용: " +
            $"{skill.dotDamagePerTick}/tick, " +
            $"{skill.dotDuration}초";

        return true;
    }

    // 범위 공격 스킬 실행
    // Spin Attack은 적이 없어도 스킬 자체는 발동한 것으로 처리.
    // 따라서 적이 0명이어도 true로 반환함
    private bool ExecuteArea(SOSkill skill, out string resultMessage)
    {
        // 플레이어 주변 range 거리의 지점을 범위 공격 중심으로 사용
        Vector3 areaCenter = 
            attackOrigin.position + Vector3.up * 0.5f;

        rangeIndicator?.ShowCircle(areaCenter, skill.areaRadius);

        int hitCount = Physics.OverlapSphereNonAlloc(
            areaCenter, skill.areaRadius, hitBuffer, enemyLayer);

        uniqueTargets.Clear();

        for(int i=0; i < hitCount; i++)
        {
            Collider hitCollider = hitBuffer[i];

            if (hitCollider == null) continue;

            EnemyHealth target = hitBuffer[i].GetComponentInParent<EnemyHealth>();

            if(target==null || target.IsDead)
            {
                continue;
            }

            uniqueTargets.Add(target);
        }

        foreach(EnemyHealth target in uniqueTargets)
        {
            target.TakeDamage(skill.damage);
        }

        resultMessage = $"[범위 공격] {uniqueTargets.Count}명의 적에게 " +
            $"{skill.damage}의 범위 피해";

        lastAreaCenter = areaCenter;
        lastAreaRadius = skill.areaRadius;

        return true;
    }


    private EnemyHealth FindNearestTarget(float searchRadius)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            attackOrigin.position,
            searchRadius,
            hitBuffer,
            enemyLayer);

        EnemyHealth nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            EnemyHealth candidate =
                hitBuffer[i].GetComponentInParent<EnemyHealth>();

            if (candidate == null || candidate.IsDead)
                continue;

            float distanceSqr =
                (candidate.transform.position -
                 attackOrigin.position).sqrMagnitude;

            if (distanceSqr >= nearestDistanceSqr)
                continue;

            nearestDistanceSqr = distanceSqr;
            nearestTarget = candidate;
        }

        return nearestTarget;
    }

    private bool ValidateTarget(EnemyHealth target, float allowedRange, out string failureMessage)
    {
        if (target ==null)
        {
            failureMessage = "대상을 찾지 못했습니다.";
            return false;
        }

        if (target.IsDead)
        {
            failureMessage = $"{target.name}은 이미 사망한 대상입니다.";
            return false;
        }

        float distanceSqr = 
            (target.transform.position - attackOrigin.position).sqrMagnitude;

        float allowedRangeSqr = allowedRange * allowedRange;

        if (distanceSqr > allowedRangeSqr)
        {
            float distance = Mathf.Sqrt(distanceSqr);

            failureMessage = $"{target.name}은 공격 범위를 벗어났습니다. " +
                $"(거리: {distance:F2}, 허용 거리: {allowedRange:F2})";
            return false;
        }

        failureMessage = string.Empty;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if(attackOrigin == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(lastAreaCenter, lastAreaRadius);
    }

}
