using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolToMesh : MonoBehaviour
{
    public static SymbolToMesh instance;
    public List<string> meshList;
    public List<string> packList;
    public string selectedPack = "SimpleGeometryPack";
    
    public Dictionary<char, Mesh> vocabulary;

    void Start()
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
        int i = 0;
        foreach (char symbol in language)
        {
            if (i < meshList.Count)
            {
                AddSymbol(symbol, meshList[i++]);
            }
            else
            {
                Debug.LogError("Code tried to go out of scope of the meshList!. i = " + i + " , meshList count is " + meshList.Count);
            }
        }
    }

    public void AddSymbol(char symbol, string meshName)
    {
        vocabulary.Add(symbol, Resources.Load<Mesh>("Graphics/3DModels/ElementModels/" + selectedPack + '/' + meshName));
    }

    public Mesh GetMeshFromSymbol(char symbol)
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
