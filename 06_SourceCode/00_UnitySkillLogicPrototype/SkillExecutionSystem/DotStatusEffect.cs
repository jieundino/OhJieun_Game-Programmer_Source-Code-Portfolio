using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class DotStatusEffect : MonoBehaviour
{
    private EnemyHealth health;
    private Coroutine dotCoroutine;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    public void ApplyDot(float damagePerTick, float duration, float interval)
    {
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
        }

        dotCoroutine = StartCoroutine(DotCoroutine(damagePerTick, duration, interval));
    }

    private IEnumerator DotCoroutine(float damagePerTick, float duration, float interval)
    {
        if (damagePerTick <= 0f || duration <= 0f || interval <= 0f)
        {
            dotCoroutine = null;
            yield break;
        }

        int tickCount = Mathf.FloorToInt(duration / interval);

        for (int i = 0; i < tickCount; i++)
        {
            yield return new WaitForSeconds(interval);

            if (health.IsDead)
                break;

            health.TakeDamage(damagePerTick);
        }

        dotCoroutine = null;
    }
}
