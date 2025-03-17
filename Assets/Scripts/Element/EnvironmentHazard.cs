using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EnvironmentHazard : MonoBehaviour
{
    public Element element;
    [Description("How long does it take before a damage tick occurs (in seconds)")]
    public float damageTick;
    [Description("How much damage is dealt per tick (or for things like ice, how long does the freeze last)")]
    public float damageAmount;

    private float curTick;

    List<EnemyController> enemies = new List<EnemyController>();


    public void UpdateHazard()
    {
        if (Time.time - curTick >= damageTick)
        {
            curTick = Time.time;
            foreach (EnemyController enemy in enemies)
            {
                //do damage to enemy or w/e
                switch (element)
                {
                    case Element.ice:
                        enemy.FreezeSelf(damageAmount);
                        break;
                    case Element.fire:
                        enemy.ApplyElementToSelf(Element.fire);
                        break;
                    case Element.lava:
                        enemy.ApplyElementToSelf(Element.fire);
                        enemy.TakeDamage(damageAmount, Element.fire);
                        break;
                    case Element.mud:
                        enemy.ApplyElementToSelf(Element.mud);
                        break;
                    case Element.water:
                        //wont do anything special just applies water element
                        enemy.ApplyElementToSelf(Element.water);
                        break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_ENEMY))
        {
            enemies.Add(collision.gameObject.GetComponent<EnemyController>());
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_ENEMY))
        {
            enemies.Remove(collision.gameObject.GetComponent<EnemyController>());
        }
    }
}
