using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireFrameFilter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Quads, 0);
    }
}
