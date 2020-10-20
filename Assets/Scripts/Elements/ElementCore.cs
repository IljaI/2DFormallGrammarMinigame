using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementCore : MonoBehaviour
{
    public MeshFilter mainMesh;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(char symbol)
    {
        mainMesh.sharedMesh = SymbolToMesh.instance.GetMeshFromSymbol(symbol);
    }

}
