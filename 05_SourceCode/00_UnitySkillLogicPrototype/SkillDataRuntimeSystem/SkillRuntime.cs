using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillRuntime
{
    public SOSkill SkillData { get; }

    public float RemainingCooldown { get; private set; }

    public bool IsReady => RemainingCooldown <= 0f;

    public float CooldownRatio
    {
        get
        {
            if (SkillData.coolTime <= 0f)
            {
                return 0f;
            }

            return RemainingCooldown / SkillData.coolTime;
        }
    }

    public SkillRuntime(SOSkill skillData)
    {
        SkillData = skillData;
        RemainingCooldown = 0f;
    }

    public void StartCooldown()
    {
        RemainingCooldown = Mathf.Max(0f, SkillData.coolTime);
    }

    public void Tick(float deltaTime)
    {
        if (RemainingCooldown <= 0f)
        {
            return;
        }

        RemainingCooldown = Mathf.Max(0f, 
            RemainingCooldown - deltaTime);
    }
}
