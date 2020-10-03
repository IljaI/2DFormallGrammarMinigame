using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Element is a single letter of the word. It has 4 'neighboors', references to the other Elemnts in the framnets.
public class Element {
    public Element right, left, up, down;
    public char letter;

    public Element(char _letter = '?', Element _right = null, Element _left = null, Element _up = null, Element _down = null)
    {
        letter = _letter;
        right = _right;
        left = _left;
        up = _up;
        down = _down;
    }

    // Returns true if this Element is connected to only one other Elemnt, thus being sort of a words/branch end.
    public int amountOfNeighbors()
    {
        int amount = 0;
        amount += (right != null)? 1 : 0;
        amount += (left != null)? 1 : 0;
        amount += (up != null)? 1 : 0;
        amount += (down)!= null? 1 : 0;
        return amount;
    }
}

public class FormalGrammar2D
{
    public Element[,] grid;
    public Element startingElement;

    // FormalGrammar2D(char startingChar, int gridSize)
    // {
    //     grid = new Element[gridSize,gridSize];
    // }

    // Creates a word, with this class's startingElement as word's start point
    public void GenerateWord(string instructions, char startingLetter)    
    {
        startingElement = new Element(startingLetter);
        // Iterator for the loop below
        Element currentElement = startingElement;
        foreach (var letter in instructions)
        {
            switch (letter) 
            {
                case '<':
                    if(currentElement.left == null) 
                        { currentElement.left = new Element(_right: currentElement); }
                    currentElement = currentElement.left;
                break;
                case '>':
                    if(currentElement.right == null) 
                        { currentElement.right = new Element(_left: currentElement); }
                    currentElement = currentElement.right;
                break;
                case '^':
                    if(currentElement.up == null) 
                        { currentElement.up = new Element(_down: currentElement); }
                    currentElement = currentElement.up;
                break;
                case '|':
                    if(currentElement.down == null) 
                        { currentElement.down = new Element(_up: currentElement); }
                    currentElement = currentElement.down;
                break;
                case '*':
                    currentElement = startingElement;
                break;
                default:
                    // In case it's neither of the directions symbols, it's a command to set word to current element.
                    currentElement.letter = letter;
                break;
            }
        }
    }

    
    // Recursively prints word as sequence of realtions between elements.
    public string PrintWord(Element currentElement, Element prevElement, string directionFromPrev = "center")
    {
        if(currentElement != null)
        {
            string result = "";
            result += directionFromPrev + " from " + ((prevElement == null)? '*' : prevElement.letter) + " is ";
            result += currentElement.letter.ToString() + " \n";
            //Debug.Log("Amount of neighbors for " + currentElement.letter + " is " + currentElement.amountOfNeighbors());
            if(currentElement.left != null && directionFromPrev != "right") { result += PrintWord(currentElement.left, currentElement, "left"); }
            if(currentElement.right != null && directionFromPrev != "left") { result += PrintWord(currentElement.right, currentElement, "right"); }
            if(currentElement.up != null && directionFromPrev != "down") { result += PrintWord(currentElement.up, currentElement, "up"); }
            if(currentElement.down != null && directionFromPrev != "up") { result += PrintWord(currentElement.down, currentElement, "down"); }
            return result;
        }
        return "Can't print the word: Current element is null";
    }
}
