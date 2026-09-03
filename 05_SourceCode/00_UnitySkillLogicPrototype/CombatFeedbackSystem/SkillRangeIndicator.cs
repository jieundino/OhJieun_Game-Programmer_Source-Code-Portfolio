using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillRangeIndicator : MonoBehaviour
{
    [SerializeField] private GameObject circleIndicator;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if(circleIndicator != null)
        {
            circleIndicator.SetActive(false);
        }
    }

    public void ShowCircle(Vector3 center, float radius, float duration = 0.5f)
    {
        if(circleIndicator == null)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        circleIndicator.SetActive(true);

        circleIndicator.transform.position = center + Vector3.up * 0.02f;

        float diameter = radius * 2f;

        circleIndicator.transform.localScale = 
            new Vector3(diameter, diameter, 1f);

        hideCoroutine = StartCoroutine(HideCircleAfter(duration));
    }

    private IEnumerator HideCircleAfter(float duration)
    {
        yield return new WaitForSeconds(duration);

        circleIndicator?.SetActive(false);
        hideCoroutine = null;
    }
}
