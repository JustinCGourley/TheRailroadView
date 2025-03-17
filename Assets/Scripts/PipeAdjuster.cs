using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeAdjuster : MonoBehaviour
{
    [SerializeField] GameObject topObj;
    [SerializeField] GameObject bottomObj;
    [SerializeField] GameObject leftObj;
    [SerializeField] GameObject rightObj;
    [SerializeField] GameObject centerObj;

    public int pipeLineNum;

    //Updates pipe using an integer to represent the position the pipe should be in
    //Key for that is as follows:
    // 0 -> top
    // 1 -> bottom
    // 2 -> left
    // 3 -> right
    // 4 -> bottom + top (vertical)
    // 5 -> left + right (horizontal)
    // 6 -> top + left (corner)
    // 7 -> top + bottom (corner)
    // 8 -> bottom + left (corner)
    // 9 -> bottom + right (corner)
    
    public void UpdatePipe(int position)
    {
        topObj.SetActive(false);
        bottomObj.SetActive(false);
        leftObj.SetActive(false);
        rightObj.SetActive(false);


        // do stuff here 
        switch (position)
        {
            case 0:
                topObj.SetActive(true);
                break;
            case 1:
                bottomObj.SetActive(true);
                break;
            case 2:
                leftObj.SetActive(true);
                break;
            case 3:
                rightObj.SetActive(true);
                break;
            case 4:
                bottomObj.SetActive(true);
                topObj.SetActive(true);
                break;
            case 5:
                leftObj.SetActive(true);
                rightObj.SetActive(true);
                break;
            case 6:
                topObj.SetActive(true);
                leftObj.SetActive(true);
                break;
            case 7:
                topObj.SetActive(true);
                rightObj.SetActive(true);
                break;
            case 8:
                bottomObj.SetActive(true);
                leftObj.SetActive(true);
                break;
            case 9:
                bottomObj.SetActive(true);
                rightObj.SetActive(true);
                break;
            default:
                break;
        }

    }
}
