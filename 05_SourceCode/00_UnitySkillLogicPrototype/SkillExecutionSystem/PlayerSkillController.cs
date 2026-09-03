using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerSkillController
//→ 스킬에 대응하는 SkillRuntime 탐색
//→ IsReady 검사
//→ 실제 스킬 실행
//→ 실행 성공 시 StartCooldown()
//→ Update에서 매 프레임 Tick()

public class PlayerSkillController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private SkillExecutor skillExecutor;

    [SerializeField] private SOSkill[] equippedSkills;

    private readonly List<SkillRuntime> skillRuntimes = new List<SkillRuntime>();

    Animator anim;

    private bool isUsingSkill;
    public bool IsUsingSkill => isUsingSkill;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (skillExecutor == null)
        {
            skillExecutor = GetComponent<SkillExecutor>();
        }

        InitializeSkills();
    }

    private void Update()
    {
        UpdateCooldowns();

        if(isUsingSkill)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryActivateSkill(0);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            TryActivateSkill(1);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            TryActivateSkill(2);
        }
    }

    private void InitializeSkills()
    {
        if (equippedSkills == null)
        {
            Debug.LogWarning("EquippedSkills가 설정되지 않았습니다.");
            return;
        }

        skillRuntimes.Clear();

        foreach (SOSkill skill in equippedSkills)
        {
            if (skill == null)
            {
                skillRuntimes.Add(null);
                continue;
            }

            skillRuntimes.Add(new SkillRuntime(skill));
        }
    }

    private void UpdateCooldowns()
    {
        foreach(SkillRuntime runtime in skillRuntimes)
        {
            runtime?.Tick(Time.deltaTime);
        }
    }

    public SkillRuntime GetSkillRuntime(int slotIndex)
    {
        if ((slotIndex<0||slotIndex>=skillRuntimes.Count))
        {
            Debug.LogWarning($"유효하지 않은 스킬 슬롯입니다: {slotIndex}");
            return null;
        }
        return skillRuntimes[slotIndex];
    }

    public void TryActivateSkill(int skillIndex)
    {
        SkillRuntime runtime = GetSkillRuntime(skillIndex);

        if(runtime == null)
        {
            Debug.Log($"스킬 슬롯 {skillIndex + 1}이 비어 있습니다.");
            return;
        }

        if(isUsingSkill)
        {
            Debug.Log("[Skill Failed] 스킬 사용 중에는 다른 스킬을 사용할 수 없습니다.");
            return;
        }

        if (!runtime.IsReady)
        {
            Debug.Log($"{runtime.SkillData.name} 쿨타임: " +
                $"{runtime.RemainingCooldown:F1}초");

            return;
        }

        bool succeeded = ActivateSkill(runtime.SkillData);

        if (succeeded)
        {
            runtime.StartCooldown();
        }
    }

    public bool ActivateSkill(SOSkill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[Skill Failed] SkillData 없음.");
            return false;
        }

        if(skillExecutor == null)
        {
            Debug.LogWarning("[Skill Failed] SkillExecutor 없음.");
            return false;
        }

        bool succeeded = skillExecutor.TryExecute(
            skill,
            out string resultMessage, 
            out Transform targetTransform);

        if (!succeeded)
        {
            Debug.Log($"[Skill Failed] {skill.name} / " +
                $"{resultMessage}");
            return false;
        }

        if (targetTransform != null)
        {
            playerMovement.FaceTargetInstant(targetTransform);
        }

        if(!string.IsNullOrEmpty(skill.animationName))
        {
            isUsingSkill = true;

            anim.CrossFade(skill.animationName, 0.05f, 0);
        }

        Debug.Log($"[Skill Success] " +
            $"Type: {skill.skillType}, " +
            $"Skill: {skill.name}, " +
            $"Result: {resultMessage}");

        return true;
    }

    // 스킬 애니메이션 종료 프레임에 이벤트 호출
    public void OnSkillAnimationEnd()
    {
        isUsingSkill = false;
    }

}
