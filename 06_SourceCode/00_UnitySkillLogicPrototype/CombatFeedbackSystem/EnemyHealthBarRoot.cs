using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBarRoot : MonoBehaviour
{
    public static EnemyHealthBarRoot Instance { get; private set; }

    public RectTransform RectTransform => transform as RectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
