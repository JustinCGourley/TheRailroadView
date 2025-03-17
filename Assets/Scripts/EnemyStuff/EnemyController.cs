using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    EnemyMovement enemyMovement;
    GameManager gameManager;

    [HideInInspector] public EnemyInfo enemyInfo;

    public float startHealth;
    [Description("This is a multiplier that is used with the current day to add health on to the enemy with the initial health as the base")]
    public float healthScaling;

    public float health;

    private float maxHealth;
    [SerializeField] HealthBar healthBar;

    private float lastHitTime = -1;
    bool freezeSelf = false;
    float freezeTimeEnd;

    bool onFire = false;
    float fireTick = 0;
    int fireTicksTaken = 0;

    bool mudSlow = false;
    float mudSlowTick = 0;

    AIUnitController unitTarget;
    Element curElement;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find(Constants.GAMEOBJECT_GAMEMANAGER).GetComponent<GameManager>();
        enemyInfo = this.GetComponent<EnemyInfo>();
        enemyMovement = this.GetComponent<EnemyMovement>();
        enemyMovement.MovementStart();
        enemyMovement.UpdatePath();

        maxHealth = startHealth + (ProgressionManager.Instance.CurrentDay * healthScaling);
        health = maxHealth;
        healthBar.UpdateHealthBar(health, maxHealth);
    }

    // Update is called once per frame
    public void UpdateEnemy()
    {
        Move();

        if (unitTarget == null)
        {
            CheckForUnits();
        }
        else if ((this.transform.position - unitTarget.aiMovement.Position).magnitude <= Constants.ENEMY_HIT_ENEMY_DISTANCE)
        {
            HitUnit();
        }


        if (onFire && Time.time - fireTick >= Constants.ENEMY_HIT_ENEMY_DISTANCE)
        {
            FireBurn();
        }
        if (mudSlow && Time.time - mudSlowTick >= Constants.ENEMY_MUD_SLOW_TIME)
        {
            mudSlow = false;
        }
    }

    private void FireBurn()
    {
        health--;
        fireTick = Time.time;
        fireTicksTaken++;
        if (fireTicksTaken >= 5)
        {
            fireTicksTaken = 0;
            onFire = false;
        }
    }

    void Move()
    {
        if (freezeSelf)
        {
            if (Time.time >= freezeTimeEnd)
            {
                freezeSelf = false;
            }
        }
        else
        {
            enemyMovement.UpdateMove(mudSlow);
        }
    }

    private void CheckForUnits()
    {
        List<AIUnitController> units = gameManager.GetUnitsInRange(this.transform.position, Constants.ENEMY_NEAR_UNIT_DISTANCE);
        if (units.Count > 0)
        {
            unitTarget = units[Random.Range(0, units.Count)];
            enemyMovement.SeekTarget(unitTarget);
        }
    }

    private void HitUnit()
    {
        if (Time.time - lastHitTime >= Constants.ENEMY_HIT_UNIT_COOLDOWN)
        {
            lastHitTime = Time.time;
            unitTarget.TakeDamage(1); //dealing one damage
            if (unitTarget.gameObject == null) // unit has died
            {
                unitTarget = null;
            }
        }
    }

    public void AttackNexus()
    {
        gameManager.TakeDamage(enemyInfo.damage);
        EffectsManager.Instance.CreateExplosionAt(this.transform.position, Element.none);
        WaveManager.Instance.KillEnemy(this.gameObject);
    }

    public bool TakeDamage(float damage, Element element)
    {
        health -= damage;
        healthBar.UpdateHealthBar(health, maxHealth);

        if (element == Element.mud)
        {
            ActivateMud();
        }

        if (health <= 0)
        {
            WaveManager.Instance.KillEnemy(this.gameObject);
            return true;
        }
        return false;
    }

    /// <summary>
    /// apply elements to enemy from a projectile/whatever
    /// </summary>
    public void ApplyElementToSelf(Element element)
    {
        //skip if element is the same
        if (element == curElement) return;

        Element newElement = ElementManager.Instance.CheckElementCombo(curElement, element);
        if (newElement != curElement) 
        {

            //TODO: IN here should be where the enemy applies effects that happen when getting new elements applied

            if (newElement == Element.fire)
            {
                onFire = true;
                fireTicksTaken = fireTick > 0 ? fireTicksTaken - 1 : 0;
            }
            else if (newElement == Element.mud)
            {
                ActivateMud();
            }

            curElement = newElement;
        }
    }

    private void ActivateMud()
    {
        mudSlow = true;
        mudSlowTick = Time.time;
    }

    public void FreezeSelf(float amount)
    {
        freezeSelf = true;
        freezeTimeEnd = Time.time + amount;
    }

    public Vector3 GetShotPosition()
    {
        return this.enemyInfo.shootPos.transform.position;
    }

}
