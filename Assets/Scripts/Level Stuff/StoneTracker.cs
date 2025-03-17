using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneTracker : MonoBehaviour
{

    private int resourceAmount;

    // Start is called before the first frame update
    void Start()
    {
        resourceAmount = Constants.STONE_DEFAULT_MINE_AMOUNT;
    }

    public int MineStone(int amount)
    {
        Debug.Log($"Mining! cur amount: {resourceAmount}");
        if (amount > resourceAmount)
        {
            Debug.Log("we ran out");
            resourceAmount = 0;
            LevelManager.Instance.DestroyObjectOnTile(Utility.Instance.GetTileCoordsFromPosition(this.transform.position));
            return resourceAmount;
        }
        else
        {
            resourceAmount -= amount;
            return amount;
        }
    }
}
