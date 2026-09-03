using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0f;

    public event Action<float, float> HealthChanged; // (currentHealth, maxHealth)
    public event Action Died;

    Animator anim;
    private Collider[] colliders;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        anim = GetComponent<Animator>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        CurrentHealth = Mathf.Max(CurrentHealth-damage, 0f);

        HealthChanged?.Invoke(
            CurrentHealth,
            maxHealth
        );

        Debug.Log(
            $"[{name}] {damage} 피해 / " +
            $"남은 체력: {CurrentHealth}");

        if (IsDead)
            Die();
    }

    private void Die()
    {
        Debug.Log($"[{name}] 사망");

        foreach(Collider col in colliders)
        {
            col.enabled = false;
        }

        Died?.Invoke();

        if(anim != null)
            anim.Play("Die");
    }

    public void HideInScene()
    {
        // 1초 후 비활성화 하는 코루틴 시작
        StartCoroutine(HideAfterDelay(1f));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
