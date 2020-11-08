using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarDebug : MonoBehaviour
{
    public GameObject wordPart;
    // Start is called before the first frame update
    void Start()
    {
        SymbolToObject.instance.SetSelectedPack("SimpleGeometryPack");
        SymbolToObject.instance.AssociateLanguageWithMeshes(new List<char>{ 'a', 'b', 'c', 'd', 'S'});
        FormalGrammar2D grammar = new FormalGrammar2D(100, wordPart);
        grammar.GenerateWord("", 'S');
        //Debug.Log(grammar.PrintWord(grammar.startingElement, null));
        //grammar.ApplyRule("<a<a|b*>b>a^a|>b", (grammar.gridSize / 2) + 1, (grammar.gridSize / 2) + 1);
        //grammar.DrawWordWithObject(wordPart);
    }
}
