using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    explosion
}

public class ControlledTowerProjectile : MonoBehaviour
{
    public Element element;
    [SerializeField] EffectType effectType;

    public void Shoot(ProjectileInfo projInfo, Vector3 pos)
    {
        List<Enemy> enemies;
        enemies = WaveManager.Instance.getEnemiesInRange(pos, projInfo.aoeRange);


        switch (effectType)
        {
            case EffectType.explosion:
                Debug.Log($"Creating explosion @{pos} w/ {element}");
                EffectsManager.Instance.CreateExplosionAt(pos, element);
                break;
        }


        foreach (Enemy enemy in enemies)
        {
            ElementManager.Instance.ApplyElement(pos, enemy.controller, projInfo, element);
        }
        ElementManager.Instance.ApplyElement(pos, null, projInfo, element);
    }
    
}
