using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy
{
    public GameObject obj;
    public EnemyController controller;
    public int enemyType;
    public Enemy(GameObject obj, EnemyController aIController)
    {
        this.obj = obj;
        this.controller = aIController;
    }
}