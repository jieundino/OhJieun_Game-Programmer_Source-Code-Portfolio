using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private int slotIndex;
    [SerializeField] private PlayerSkillController player;

    [SerializeField] private Image imgIcon;
    [SerializeField] private Image imgCool;

    private SkillRuntime runtime;

    private void Awake()
    {
        if(player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.GetComponent<PlayerSkillController>();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "PlayerSkillController를 찾을 수 없습니다.");

            gameObject.SetActive(false);
            return;
        }

        runtime = player.GetSkillRuntime(slotIndex);

        if (runtime == null)
        {
            Debug.LogError($"SlotIndex {slotIndex}에 해당하는 SkillRuntime을 찾을 수 없습니다.");
            gameObject.SetActive(false);
            return;
        }


        imgIcon.sprite = runtime.SkillData.icon;
        imgCool.fillAmount = runtime.CooldownRatio;
    }

    private void Update()
    {
        if (runtime == null)
            return;

        imgCool.fillAmount = runtime.CooldownRatio;
    }

    public void OnClicked()
    {
        player.TryActivateSkill(slotIndex);
    }

}
