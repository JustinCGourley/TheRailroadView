using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Element
{
    none, 
    fire,
    water,
    earth,
    air,
    steam,
    lava,
    explosion,
    mud,
    ice,
    electric
}
public class ElementManager : MonoBehaviour
{
    public static ElementManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    // ------- singleton stuff ^^ --------------------



    public Element CheckElementCombo(List<Element> elements)
    {
        if (elements.Contains(Element.none))
        {
            return (elements[0] == Element.none) ? elements[1] : elements[0];
        }
        if (elements.Contains(Element.fire) && elements.Contains(Element.water))
        {
            return Element.steam;
        }
        if (elements.Contains(Element.fire) && elements.Contains(Element.earth))
        {
            return Element.lava;
        }
        if (elements.Contains(Element.fire) && elements.Contains(Element.air))
        {
            return Element.explosion;
        }
        if (elements.Contains(Element.water) && elements.Contains(Element.earth))
        {
            return Element.mud;
        }
        if (elements.Contains(Element.water) && elements.Contains(Element.air))
        {
            return Element.ice;
        }
        if (elements.Contains(Element.earth) && elements.Contains(Element.air))
        {
            return Element.electric;
        }

        return Element.none;
    }

    public Element CheckElementCombo(Element element1, Element element2)
    {
        return CheckElementCombo(new List<Element>() { element1, element2 });
    }

    public void ApplyElement(Vector3 pos, EnemyController enemyController, ProjectileInfo projectileInfo, Element element)
    {
        pos.y = 0;
        switch (element)
        {
            case Element.air:
                if (enemyController != null)
                {
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.airMod, element);
                }
                break;
            case Element.water:
                if (enemyController != null)
                {
                    enemyController.ApplyElementToSelf(Element.water);
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.waterMod, element);
                }
                EnvironmentManager.Instance.SpawnPool(pos, Element.water);
                break;
            case Element.fire:
                if (enemyController != null)
                {
                    enemyController.ApplyElementToSelf(Element.fire);
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.fireMod, element);
                }
                EnvironmentManager.Instance.SpawnPool(pos, Element.fire);
                break;
            case Element.earth:
                if (enemyController != null)
                {
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.earthMod, element);
                }
                break;
            case Element.steam:
                if (enemyController != null)
                {
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.steamMod, element);
                }
                EnvironmentManager.Instance.SpawnPool(pos, Element.steam);
                break;
            case Element.lava:
                if (enemyController != null)
                {
                    enemyController.ApplyElementToSelf(Element.fire);
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.lavaMod, element);
                }
                EnvironmentManager.Instance.SpawnPool(pos, Element.lava);
                break;
            case Element.ice:
                if (enemyController != null)
                {
                    enemyController.ApplyElementToSelf(Element.ice);
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.iceMod, element);
                    enemyController.FreezeSelf(0.5f);
                }
                EnvironmentManager.Instance.SpawnPool(pos, Element.ice);
                break;
            case Element.electric:

                WaveManager.Instance.DamageEnemiesInRange(this.transform.position, Constants.PROJECTILE_ELECTRIC_DEFAULT_RANGE, projectileInfo.damage, element, Mathf.FloorToInt(projectileInfo.electricMod));
                break;
            case Element.explosion:

                WaveManager.Instance.DamageEnemiesInRange(this.transform.position, projectileInfo.damage * projectileInfo.explosionMod, projectileInfo.damage, element);
                break;
            case Element.mud:
                if (enemyController != null)
                {
                    enemyController.ApplyElementToSelf(Element.mud);
                    enemyController.TakeDamage(projectileInfo.damage * projectileInfo.mudMod, element);
                }
                EnvironmentManager.Instance.SpawnPool(pos, Element.mud);
                break;
            case Element.none:
                if (enemyController != null)
                {
                    enemyController.TakeDamage(projectileInfo.damage, element);
                }
                break;
        }
    }

}


//base
//fire - water - earth - air
//others
// steam - lava - explosion - mud - ice - electric

// fire + water = steam/cloud
// fire + earth = lava
// fire + air = explosion
// water + earth = mud
// water + air = ice
// earth + air = electric