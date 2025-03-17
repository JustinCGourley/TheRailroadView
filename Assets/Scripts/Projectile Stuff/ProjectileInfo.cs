using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileInfo", menuName = "ProjectileInfo")]
public class ProjectileInfo : ScriptableObject
{
    public float damage;
    public float speed;
    public float aoeRange;
    public bool doesArc;
    public bool isHoming;

    //modifiers for 'how much should the damage effect this attribute'
    public float basicMod;
    public float fireMod;
    public float waterMod;
    public float earthMod;
    public float airMod;
    public float lavaMod;
    public float steamMod;
    [Description("This is a multiplier of dmg * explosionMod = damage in AOE for explosion")]
    public float explosionMod;
    public float mudMod;
    public float iceMod;
    public float electricMod;

    public GameObject basic;
    public GameObject fire;
    public GameObject water;
    public GameObject earth;
    public GameObject air;
    public GameObject lava;
    public GameObject steam;
    public GameObject explosion;
    public GameObject mud;
    public GameObject ice;
    public GameObject electric;
    public GameObject GetProjectile(Element element)
    {
        switch (element)
        {
            case Element.fire:
                if (fire == null) Debug.LogWarning($"Projectile {this.name} has no fire OBJ");
                return (fire != null) ? fire : basic;
            case Element.water:
                if (water == null) Debug.LogWarning($"Projectile {this.name} has no water OBJ");
                return (water != null) ? water : basic;
            case Element.earth:
                if (earth == null) Debug.LogWarning($"Projectile {this.name} has no earth OBJ");
                return (earth != null) ? earth : basic;
            case Element.air:
                if (air == null) Debug.LogWarning($"Projectile {this.name} has no air OBJ");
                return (air != null) ? air : basic;
            case Element.lava:
                if (lava == null) Debug.LogWarning($"Projectile {this.name} has no lava OBJ");
                return (lava != null) ? lava : basic;
            case Element.steam:
                if (steam == null) Debug.LogWarning($"Projectile {this.name} has no steam OBJ");
                return (steam != null) ? steam : basic;
            case Element.explosion:
                if (explosion == null) Debug.LogWarning($"Projectile {this.name} has no explosion OBJ");
                return (explosion != null) ? explosion : basic;
            case Element.mud:
                if (mud == null) Debug.LogWarning($"Projectile {this.name} has no mud OBJ");
                return (mud != null) ? mud : basic;
            case Element.ice:
                if (ice == null) Debug.LogWarning($"Projectile {this.name} has no ice OBJ");
                return (ice != null) ? ice : basic;
            case Element.electric:
                if (electric == null) Debug.LogWarning($"Projectile {this.name} has no electric OBJ");
                return (electric != null) ? electric : basic;
            default: return basic;
        }
    }
}
