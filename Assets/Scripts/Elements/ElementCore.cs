using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementCore : MonoBehaviour
{
    public Transform meshParent;

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
        Instantiate(SymbolToObject.instance.GetObjFromSymbol(symbol), meshParent);
    }

}
