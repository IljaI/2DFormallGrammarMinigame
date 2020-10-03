using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FormalGrammar2D grammar = new FormalGrammar2D();
        grammar.GenerateWord("<a<a|b*>b>a^a|>b", 'S');
        Debug.Log(grammar.PrintWord(grammar.startingElement, null));
    }

}
