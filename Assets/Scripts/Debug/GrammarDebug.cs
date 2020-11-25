﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrammarDebug : MonoBehaviour
{
    public GameObject wordPart;
    public Text gridDebugText;
    public static GrammarDebug instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        SymbolToObject.instance.SetSelectedPack("SimpleGeometryPack");
        SymbolToObject.instance.AssociateLanguageWithMeshes(new List<char>{ 'a', 'b', 'c', 'd', 'S', '_'});
        FormalGrammar2D grammar = new FormalGrammar2D(15, wordPart);
        grammar.GenerateWord("", 'S');
        //Debug.Log(grammar.PrintWord(grammar.startingElement, null));
        //grammar.ApplyRule("<a<a|b*>b>a^a|>b", (grammar.gridSize / 2) + 1, (grammar.gridSize / 2) + 1);
        //grammar.DrawWordWithObject(wordPart);
    }

    public void UpdateGridDebug(Element[,] elementGrid, int size)
    {
        gridDebugText.text = "";
        for (int i = 0; i < size; i++)
        {
            for (int k = 0; k < size; k++)
            {
                if (elementGrid[k, i] != null)
                { 
                    gridDebugText.text += $"[{elementGrid[k, i].letter}]"; 
                }
                else
                {
                    gridDebugText.text += "[ ]";
                }
            }
            gridDebugText.text += '\n';
        }
    }
}
