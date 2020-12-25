using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolToObject : MonoBehaviour
{
    public static SymbolToObject instance;
    public List<string> packList;
    public string selectedPack = "SimpleGeometryPack";
    
    public Dictionary<char, GameObject> vocabulary = new Dictionary<char, GameObject>();

    void Awake()
    {
        instance = this;
    }

    public void SetSelectedPack(string pack)
    {
        if(packList.Contains(pack))
        { 
            selectedPack = pack; 
        }
        else
        { 
            Debug.LogError("Error: There is no pack \"" + pack + "\" in packlist of the " + gameObject.name + " object."); 
        }
    }

    public void AssociateLanguageWithMeshes(List<char> language)
    {
        foreach (char symbol in language)
        {
            AddSymbol(symbol);
        }
    }

    public void AddSymbol(char symbol)
    {
        GameObject targetMesh = Resources.Load<GameObject>("Graphics/3DModels/ElementModels/" + selectedPack + '/' + symbol);
        if (targetMesh != null)
        {
            vocabulary.Add(symbol, targetMesh);
        }
        else
        {
            Debug.LogError($"SymbolToMesh wasn't been able to find a mesh named {symbol} in a {selectedPack} pack!");
        }
    }

    public GameObject GetObjFromSymbol(char symbol)
    {
        if(vocabulary.ContainsKey(symbol))
        {
            return vocabulary[symbol];
        }
        else
        {
            Debug.LogError("Error: There is no symbol \"" + symbol + "\" in vocabulary of the " + gameObject.name + " object.");
            return null;         
        }
    }
}
