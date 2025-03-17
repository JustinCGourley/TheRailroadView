using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockGroup : MonoBehaviour
{
    //0 - top left
    //1 - top center
    //2 - top right

    //3 - middle left
    //4 - middle center
    //5 - middle right

    //6 - bottom left
    //7 - bottom center
    //8 - bottom right


    [SerializeField] List<Transform> positions;

    private void OnDrawGizmos()
    {
        foreach (Transform t in positions)
        {
            Gizmos.color = new Color(100, 100, 100, 0.1f);
            Gizmos.DrawSphere(t.position, 0.04f);
        }
    }

    //all possible combos
    // key:
    // X - not a value
    // {0-8} represent an index for which unit will take shown position
    // ex, in chart you see 0 in the center, unit 1 will take the center position
    int[,] positionCombo = new int[,] 
    {
        // 1
        // X X X
        // X 0 X
        // X X X
        { 4, -1, -1, -1, -1, -1, -1, -1, -1 },
        // 2
        // X X X
        // X 0 1
        // X X X
        { 4, 5, -1, -1, -1, -1, -1, -1, -1 },
        // 3
        // X X X
        // 2 0 1
        // X X X
        { 4, 5, 3, -1, -1, -1, -1, -1, -1 },
        // 4
        // X 3 X
        // 2 0 1
        // X X X
        { 4, 5, 3, 1, -1, -1, -1, -1, -1 },
        // 5
        // 1 X 2
        // X 0 X
        // 3 X 4
        { 4, 0, 2, 6, 8, -1, -1, -1, -1 },
        // 6
        // 1 2 3
        // 4 0 5
        // X X X
        { 4, 0, 1, 2, 3, 5, -1, -1, -1 },
        // 7
        // 1 2 3
        // 4 0 5
        // X 6 X
        { 4, 0, 1, 2, 3, 5, 7, -1, -1 },
        // 8
        // 1 2 3
        // 4 0 5
        // 6 X 7
        { 4, 0, 1, 2, 3, 5, 6, 8, -1 },
        // 9
        // 1 2 3
        // 4 0 5
        // 6 7 8
        { 4, 0, 1, 2, 3, 5, 6, 7, 8 },
    };


    private int groupSize;
    
    public void SetupGroup(int groupSize)
    {
        this.groupSize = groupSize;
    }

    public Transform GetPosition(int position)
    {
        return positions[positionCombo[groupSize, position]];
    }
}
