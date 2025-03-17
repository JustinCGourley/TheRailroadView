using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] GameObject healthBar;
    [SerializeField] Transform endPosition;
    float startScale;
    float maxHealth;
    float currentHealth;

    bool started = false;

    private void Start()
    {
        startScale = healthBar.transform.localScale.x;
        started = true;
    }

    private void FixedUpdate()
    {
        this.transform.forward = (this.transform.position - Camera.main.transform.position).normalized;
    }

    public void UpdateHealthBar(float health, float max)
    {
        if (!started) return;

        Vector3 newScale = healthBar.transform.localScale;
        newScale.x = startScale * (health / max);

        healthBar.transform.localScale = newScale;

        float sizeWidth = healthBar.GetComponent<SpriteRenderer>().bounds.size.x;

        Vector3 pos = endPosition.localPosition;
        pos.x += (sizeWidth / 2);

        healthBar.transform.localPosition = pos;

    }
}
