using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnitController : AIController
{
    AIMoveTo aiMoveTo;

    [SerializeField] GameObject selectedObj;

    Vector3 targetLoc = Vector3.negativeInfinity;
    float donePathingDistance;
    float arrivedDistance;
    bool donePathing = false;
    Enemy targetEnemy = null;
    float attackTimer = -1;

    Transform isFollowing = null;

    int health;

    bool isLeader;
    public bool IsLeader { get { return isLeader; } }
    public FlockGroup flock;
    AIUnitController leader;
    public AIUnitController Leader { get { return leader; } }
    public List<AIUnitController> units;
    public Building_UnitTower tower;
    public Element element = Element.none;

    [SerializeField] int maxHealth;
    [SerializeField] int damage;
    [SerializeField] float range;
    [SerializeField] float accuracy;
    [SerializeField] float attackRange;
    [SerializeField] float attackSpeed; // how many seconds before it can attack again
    [SerializeField] bool isRanged;
    [SerializeField] ProjectileInfo projectile;
    [SerializeField] HealthBar healthBar;

    bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        aiMovement = this.GetComponent<AIMovement>();
        aiManager = GameObject.Find(Constants.GAMEOBJECT_AIMANAGER).GetComponent<AIManager>();
        aiMoveTo = new AIMoveTo();
        info.job = new Job(Vector3.zero, JobType.work_building, null, 0);

        donePathingDistance = 1;
        arrivedDistance = 0.2f;

        health = maxHealth;
        healthBar.UpdateHealthBar(health, maxHealth);

        initialized = true;
    }

    // Update is called once per frame
    public override void UpdateController()
    {
        if (!initialized) return;
        //check for target if we dont have one
        if (targetEnemy == null || targetEnemy.obj == null)
        {
            CheckInRangeOfEnemy();
        }


        //deal with getting to and attacking target
        if (targetEnemy != null)
        {
            if ((this.transform.position - targetEnemy.obj.transform.position).magnitude > range)
            {
                aiMovement.MoveToPosition(targetEnemy.obj.transform.position, range);
            }
            else
            {
                AttackEnemy();
            }
        }
        //listen to commands
        else if (isLeader && !targetLoc.Equals(Vector3.negativeInfinity))
        {
            float distance = (targetLoc - aiMovement.Position).magnitude;
            //pathing to target
            if (distance > arrivedDistance)
            {
                if (distance > donePathingDistance)
                {
                    aiMoveTo.WorkJob(this, aiMovement);
                }
                else if (!donePathing)
                {
                    donePathing = true;
                    //aiMovement.MoveToPosition(targetLoc); I THINK THIS IS JUST FOR UNIT CONTROLLER MOVING THAT LASET LITTLE BIT AT THE END, BUT LETS NOT HAVE THAT
                }
            }
            //at target
            else
            {
                targetLoc = Vector3.negativeInfinity;
            }
        }
        else if (!isLeader && isFollowing != null)
        {
            if ((this.transform.position - isFollowing.position).magnitude > Constants.AI_UNIT_FOLLOW_ARRIVED_DISTANCE)
            {
                aiMovement.MoveToPosition(isFollowing.position);
            }
        }

        aiMovement.UpdateMovement();
    }

    private void CheckInRangeOfEnemy()
    {
        List<Enemy> enemies = WaveManager.Instance.getEnemiesInRange(this.transform.position, attackRange);
        UtilityGizmo.Instance.DrawSphere(this.transform.position, range, new Color(0.3f, 0.3f, 0.3f, 0.05f));
        if (enemies.Count > 0)
        {
            targetEnemy = enemies[Random.Range(0, enemies.Count - 1)];

        }
        else
        {
            targetEnemy = null;
        }
    }

    public void MoveToClickPos(Vector3 clickPos)
    {
        if (!isLeader)
        {
            Debug.LogError("AIUnitController - Trying to move a non-leader??");
        }

        targetLoc = clickPos;
        info.job.jobPos = clickPos; 
        aiMoveTo.StartJob(this, aiMovement);

        for (int i = 0; i < units.Count; i++)
        {
            units[i].SetFollow(flock.GetPosition(i + 1));
        }
    }

    public void SetFollow(Transform follow)
    {
        isFollowing = follow;
    }

    public void StopFollow()
    {
        isFollowing = null;
    }

    private void AttackEnemy()
    {
        if (Time.time - attackTimer >= attackSpeed)
        {
            attackTimer = Time.time;

            if (isRanged)
            {
                FireProjectile(targetEnemy.obj);
            }
            else
            {
                bool done = targetEnemy.controller.TakeDamage(damage, this.element);

                if (done || targetEnemy.obj == null)
                {
                    targetEnemy = null;
                }
            }
        }
    }

    private void FireProjectile(GameObject target)
    {
        GameObject proj = Instantiate(projectile.GetProjectile(element));
        proj.transform.position = this.transform.position;

        TowerProjectile towerProjectile = proj.GetComponent<TowerProjectile>();
        //TODO - change this dir by some amount to aim up a little bit
        Vector3 targetDir = (target.GetComponent<EnemyController>().enemyInfo.shootPos.position - this.transform.position);

        float targetSpeed = target.GetComponent<EnemyController>().enemyInfo.speed;
        if (target.GetComponent<EnemyMovement>().atNexus) targetSpeed = 0;
        float timeToTarget = targetDir.magnitude / (projectile.speed - targetSpeed);
        Vector3 predictedPos = target.transform.position + (target.transform.forward * targetSpeed * timeToTarget);
        Vector3 fireDir = (predictedPos - this.transform.position).normalized;

        towerProjectile.Shoot(projectile, fireDir, target.GetComponent<EnemyController>(), accuracy);
    }

    public void SetSelectUnit(bool active)
    {
        if (!isLeader)
        {
            Debug.LogWarning("AIUnitController - Attempted to set selected of a non-leader unit");
            return;
        }
        if (selectedObj == null)
        {
            return;
        }
        selectedObj.SetActive(active);
        foreach (AIUnitController unit in units)
        {
            unit.SetSelectedActive(active);
        }
    }

    public void SetSelectedActive(bool active)
    {
        selectedObj.SetActive(active);
    }

    public void TakeDamage(int damage)
    {

        health -= damage;
        healthBar.UpdateHealthBar(health, maxHealth);
        Debug.Log("Unit took dmg. At health: " + health);
        if (health <= 0)
        {
            KillUnit();
        }
    }

    private void KillUnit()
    {
        tower.UnitHasDied();
        aiManager.KillUnit(this);
    }

    public void SetLeader(FlockGroup flock, List<AIUnitController> units, Building_UnitTower tower)
    {
        isLeader = true;
        this.flock = flock;
        this.units = units;
        this.tower = tower;
    }
    public void SetLeader(AIUnitController leader)
    {
        if (leader == null)
        {
            Debug.LogError("AIUnitController - Tried to pass through a null value for leader");
        }
        isLeader = false;
        this.leader = leader;
    }
}
