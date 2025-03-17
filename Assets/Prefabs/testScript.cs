using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter[] filters = this.GetComponentsInChildren<MeshFilter>();

        Mesh finalMesh = new Mesh();

        CombineInstance[] combiner = new CombineInstance[filters.Length];

        for (int i = 0; i < filters.Length; i++)
        {
            combiner[i].subMeshIndex = 0;
            combiner[i].mesh = filters[i].sharedMesh;
            combiner[i].transform = filters[i].transform.localToWorldMatrix;
        }


        finalMesh.CombineMeshes(combiner);
        this.GetComponent<MeshFilter>().sharedMesh = finalMesh;

        for (int i = 0; i < this.transform.childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }
        this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

}
