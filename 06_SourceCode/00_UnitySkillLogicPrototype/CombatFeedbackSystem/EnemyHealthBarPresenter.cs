using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthBarPresenter : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private RectTransform healthBarPrefab;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2, 0);

    [Header("Display")]
    [SerializeField] private bool hideWhenFull;

    private EnemyHealth enemyHealth;

    private RectTransform healthBarInstance;
    private Slider healthSlider;

    private Camera targetCamera;
    private bool shouldShow;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        targetCamera = Camera.main;
    }

    private void Start()
    {
        CreateHealthBar();

        enemyHealth.HealthChanged += OnHealthChanged;
        enemyHealth.Died += OnEnemyDied;

        RefreshHealthBar(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
    }

    private void LateUpdate()
    {
        UpdateHealthBarPosition();
    }

    private void CreateHealthBar()
    {
        if(healthBarPrefab==null)
        {
            Debug.LogError("Health bar prefab is not assigned.");
            enabled = false;
            return;
        }

        if(EnemyHealthBarRoot.Instance==null)
        {
            Debug.LogError("EnemyHealthBarRoot instance is not found in the scene.");
            enabled = false;
            return;
        }

        healthBarInstance = Instantiate(healthBarPrefab, EnemyHealthBarRoot.Instance.RectTransform);

        if (healthSlider == null)
        {
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
        }

        if(healthSlider ==null)
        {
            Debug.LogError("Slider component is not found in the health bar prefab.");
            enabled = false;
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.interactable = false;
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarInstance == null || targetCamera == null)
            return;

        Vector3 worldPosition = transform.position + worldOffset;

        Vector3 screenPosition = targetCamera.WorldToScreenPoint(worldPosition);

        // z가 0 이하이면 카메라 뒤에 있는 오브젝트
        bool isInFrontOfCamera = screenPosition.z > 0;

        healthBarInstance.gameObject.SetActive(isInFrontOfCamera && shouldShow);

        if (!isInFrontOfCamera) return;

        healthBarInstance.position = screenPosition;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        RefreshHealthBar(currentHealth, maxHealth);
    }

    private void RefreshHealthBar(float currentHealth, float maxHealth)
    {
        if (healthSlider == null) return;

        float healthRatio = maxHealth > 0f ? currentHealth / maxHealth : 0f;

        healthSlider.value = healthRatio;

        shouldShow = !enemyHealth.IsDead && (!hideWhenFull||healthRatio<1f);
    }

    private void OnEnemyDied()
    {
        shouldShow = false;

        if(healthBarInstance != null)
        {
            healthBarInstance.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if(healthBarInstance != null)
        {
            healthBarInstance.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if(enemyHealth!=null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
            enemyHealth.Died -= OnEnemyDied;
        }

        if(healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
        }
    }
}
