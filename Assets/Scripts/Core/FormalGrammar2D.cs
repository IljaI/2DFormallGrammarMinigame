using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Element is a single letter of the word. It has 4 'neighboors', references to the other Elemnts in the framnets.
public class Element 
{
    public Element right, left, up, down;
    public ElementCore realObject;
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

    ~Element()
    {
        
    }
}

public class FormalGrammar2D : MonoBehaviour 
{
    public Element[,] grid;
    public Element startingElement;
    public int gridSize;
    public GameObject elementPrefab;

    public FormalGrammar2D(int _gridSize)
    {
        gridSize = _gridSize;
        // Increasing gridSize by 1 (making it even) in case if its cleanly divisible by 2 (so that there would be place for center element)
        if (gridSize % 2 == 0) {gridSize++;}
        grid = new Element[gridSize,gridSize];
    }

    // Creates a word, with this class's startingElement as word's start point
    public void GenerateWord(string instructions, char startingLetter)    
    {
        startingElement = new Element(startingLetter);
        int i = 0;
        int k = 0;
        // Iterator for the loop below
        Element currentElement = startingElement;
        // Filling in first grid cell
        grid[i += ((gridSize/2) + 1), k += ((gridSize/2) + 1)] = startingElement;       
        foreach (var letter in instructions)
        {
            switch (letter) 
            {
                case '<':
                    if(currentElement.left == null) 
                        { currentElement.left = new Element(_right: currentElement); }
                    currentElement = currentElement.left;
                    if(i-1 < 0) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions);} else { grid[--i, k] = currentElement; }
                break;
                case '>':
                    if(currentElement.right == null) 
                        { currentElement.right = new Element(_left: currentElement); }
                    currentElement = currentElement.right;
                    if(i+1 > gridSize - 1) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions);} else { grid[++i, k] = currentElement; }
                break;
                case '^':
                    if(currentElement.up == null) 
                        { currentElement.up = new Element(_down: currentElement); }
                    currentElement = currentElement.up;
                    if(k+1 > gridSize - 1) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions);} else { grid[i, ++k] = currentElement; }
                break;
                case '|':
                    if(currentElement.down == null) 
                        { currentElement.down = new Element(_up: currentElement); }
                    currentElement = currentElement.down;
                    if(k-1 < 0) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions);} else { grid[i, --k] = currentElement; }
                break;
                case '*':
                    currentElement = startingElement;
                    i = ((gridSize/2) + 1);
                    k = ((gridSize/2) + 1);
                break;
                default:
                    // In case it's neither of the directions symbols, it's a command to set word to current element.
                    currentElement.letter = letter;
                break;
            }
        }
    }


    
    // Recursively prints word as sequence of relations between elements.
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

    public void ApplyRule(string instructions, int element_x, int element_y )
    {
        Element currentElement = grid[element_x, element_y];
        int i = element_x;
        int k = element_y;
        char prevDirection = '*';
        foreach (var letter in instructions)
        {
            switch (letter)
            {
                case '<':
                    if (currentElement.left == null)
                    { currentElement.left = new Element(_right: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '<'; }
                    currentElement = currentElement.left;
                    if (i - 1 < 0) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions); } else { grid[--i, k] = currentElement; }
                    break;
                case '>':
                    if (currentElement.right == null)
                    { currentElement.right = new Element(_left: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '>'; }
                    currentElement = currentElement.right;
                    if (i + 1 > gridSize - 1) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions); } else { grid[++i, k] = currentElement; }
                    break;
                case '^':
                    if (currentElement.up == null)
                        { currentElement.up = new Element(_down: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '^'; }
                    currentElement = currentElement.up;
                    if (k + 1 > gridSize - 1) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions); } else { grid[i, ++k] = currentElement; }
                    break;
                case '|':
                    if (currentElement.down == null)
                        { currentElement.down = new Element(_up: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '|'; }
                    currentElement = currentElement.down;
                    if (k - 1 < 0) { Debug.Log("Grammar tried to go out of bonds while generating the word " + instructions); } else { grid[i, --k] = currentElement; }
                    break;
                case '*':
                    currentElement = grid[element_x, element_y];
                    i = element_x;
                    k = element_y;
                    break;
                default:
                    // In case it's neither of the directions symbols, it's a command to set word to current element.
                    
                    switch (prevDirection)
                    {
                        case '<':
                            currentElement.right.left = new Element(_right: currentElement.right); currentElement.right = currentElement.right.left; currentElement.right.left = currentElement;
                            currentElement = currentElement.right;
                            break;
                        case '>':
                            currentElement.left.right = new Element(_left: currentElement.left); currentElement.left = currentElement.left.right; currentElement.left.right = currentElement;
                            currentElement = currentElement.left;
                            break;
                        case '^':
                            currentElement.down.up = new Element(_down: currentElement.down); currentElement.down = currentElement.down.up; currentElement.down.up = currentElement;
                            currentElement = currentElement.down;
                            break;
                        case '|':
                            currentElement.up.down = new Element(_up: currentElement.up); currentElement.up = currentElement.up.down; currentElement.up.down = currentElement;
                            currentElement = currentElement.up;
                            break;
                        default:
                            break;
                    }
                    currentElement.letter = letter;
                    break;
            }
        }
        // Destroy (untangle/disconnect) non-terminal from which the transformation happened
        currentElement = grid[element_x, element_y];
        DisconnectElement(currentElement);
    }

    public void DisconnectElement(Element targetElement)
    {
        Debug.Log("The Disconnected element is " + targetElement.letter);
        if (targetElement.down != null)
        { targetElement.down.up = targetElement.up; startingElement = targetElement.down; }
        if (targetElement.up != null)
        { targetElement.up.down = targetElement.down; startingElement = targetElement.up; }
        if (targetElement.left != null)
        { targetElement.left.right = targetElement.right; startingElement = targetElement.left; }
        if (targetElement.right != null)
        { targetElement.right.left = targetElement.left; startingElement = targetElement.right; }
    }

    public void InsertToGrid(int x, int y, Element element, char direction = '*')
    {
        if(x > gridSize - 1 || x < 0 || y >gridSize-1 || y < 0) { Debug.Log("Grammar tried to go out of bonds while inserting the word [" + element.letter + "into the " + x + ", " + y + "] position"); }
        else
        {
            Element currentElement = element;
            switch (direction)
            {
                case '<':
                    while (currentElement.amountOfNeighbors() > 1)
                    {
                        // I feel like I will need to use recursion again...
                    }
                    break;
            }
        }
    }

    public void UpdateVisualization()
    {
        DrawWordWithObject(startingElement, null, elementPrefab, Vector3.zero);
    }

    // DEBUG ONLY Prints the word based on the helper grid [DEPRECATED]
    public void DrawWordWithObject(GameObject wordObject)
    {
        for(int i = 0; i < gridSize; i++)
        {
            for(int k = 0; k < gridSize; k++)
            {
                if(grid[i,k] != null)
                {
                    Vector3 newPos = new Vector3(i, k, 0);
                    GameObject newWordPart = Instantiate(wordObject, newPos, Quaternion.identity);
                    newWordPart.transform.name = $"{grid[i,k].letter}";
                    newWordPart.transform.GetComponentInChildren<TextMesh>().text = grid[i,k].letter.ToString();
                }
            }
        }
    }

    // Recursively prints word as sequence of relations between elements.
    public string DrawWordWithObject(Element currentElement, Element prevElement, GameObject wordObject, Vector3 prevPos, string directionFromPrev = "center")
    {
        if(currentElement != null)
        {
            string result = "";
            Vector2 direction = new Vector2();
            switch (directionFromPrev)
            {
                case "center": direction.x = 0; direction.y = 0; break;
                case "left": direction.x = 1.05f; direction.y = 0; break;
                case "right": direction.x = -1.05f; direction.y = 0; break;
                case "up": direction.x = 0; direction.y = 1.05f; break;
                case "down": direction.x = 0; direction.y = -1.05f; break;
            }

            // Creating new 3D object to represent this wordpart
            Vector3 newPos = new Vector3(prevPos.x + direction.x, prevPos.y + direction.y, 0);
            Create3DWordPart(newPos, wordObject, currentElement.letter);

            // Continuing recursion
            if(currentElement.left != null && directionFromPrev != "right") { result += DrawWordWithObject(currentElement.left, currentElement, wordObject, newPos, "left"); }
            if(currentElement.right != null && directionFromPrev != "left") { result += DrawWordWithObject(currentElement.right, currentElement, wordObject,  newPos, "right"); }
            if(currentElement.up != null && directionFromPrev != "down") { result += DrawWordWithObject(currentElement.up, currentElement, wordObject, newPos,"up" ); }
            if(currentElement.down != null && directionFromPrev != "up") { result += DrawWordWithObject(currentElement.down, currentElement, wordObject, newPos,  "down"); }
            return result;
        }
        Debug.Log("Can't print the word: Current element is null");
        return "Can't print the word: Current element is null";
    }

    public void Create3DWordPart(Vector3 pos, GameObject wordObject, char letter)
    {
        ElementCore newWordPart = Instantiate(wordObject, pos, Quaternion.identity).GetComponent<ElementCore>();
        newWordPart.transform.name = letter.ToString();
        newWordPart.Initialize(letter);
    }
}
