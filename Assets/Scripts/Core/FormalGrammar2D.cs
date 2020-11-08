using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

// Element is a single letter of the word. It has 4 'neighboors', references to the other Elemnts in the framnets.
public class Element 
{
    public Element right, left, up, down;
    public int x, y;
    public ElementCore realObject = null;
    public FormalGrammar2D grammar;
    public char letter;

    public Element(FormalGrammar2D _grammar, int _x, int _y, char _letter = '?', Element _right = null, Element _left = null, Element _up = null, Element _down = null)
    {
        letter = _letter;
        right = _right;
        left = _left;
        up = _up;
        down = _down;
        grammar = _grammar;
        x = _x;
        y = _y;
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

public class FormalGrammar2D : MonoBehaviour 
{
    public Element[,] grid;
    public Element startingElement;
    public int gridSize;
    public GameObject elementPrefab;
    public Element latestCreatedElement;

    public FormalGrammar2D(int _gridSize, GameObject _elementPrefab)
    {
        gridSize = _gridSize;
        // Increasing gridSize by 1 (making it even) in case if its cleanly divisible by 2 (so that there would be place for center element)
        if (gridSize % 2 == 0) {gridSize++;}
        grid = new Element[gridSize,gridSize];
        elementPrefab = _elementPrefab;
    }

    public Element AddLogicalElement(int x, int y, char _letter = '?',  Element _right = null, Element _left = null, Element _up = null, Element _down = null)
    {
        Element newElement = new Element(this, x, y, _letter, _right, _left, _up, _down);
        if(x == gridSize || x < 0 || y == gridSize || y < 0)
        {
            Debug.LogError($"Out of grid bonds: trying to add {_letter} at x:{x} y:{y}");
        }
        grid[x, y] = newElement;
        return newElement;
    }

    // Creates a word, with this class's startingElement as word's start point
    public void GenerateWord(string instructions, char startingLetter)    
    {
        // Initializing cooridnates which will be used to place elements into the correct grid slots
        int x = gridSize / 2;
        int y = gridSize / 2;
        startingElement = AddLogicalElement( x, y, startingLetter);
        // Iterator for the loop below
        Element currentElement = startingElement;
        Update3DWordPart(Vector3.zero, currentElement); 
        foreach (var letter in instructions)
        {
            switch (letter) 
            {
                case '<':
                    if(currentElement.left == null) 
                        { currentElement.left = AddLogicalElement(--x, y, _right: currentElement); }
                    currentElement = currentElement.left;
                break;
                case '>':
                    if(currentElement.right == null) 
                        { currentElement.right = AddLogicalElement(++x, y, _left: currentElement); }
                    currentElement = currentElement.right;
                break;
                case '^':
                    if(currentElement.up == null) 
                        { currentElement.up = AddLogicalElement(x, ++y, _down: currentElement); }
                    currentElement = currentElement.up;
                break;
                case '|':
                    if(currentElement.down == null) 
                        { currentElement.down = AddLogicalElement(x,--y, _up: currentElement); }
                    currentElement = currentElement.down;
                break;
                case '*':
                    currentElement = startingElement;
                    x = gridSize / 2;
                    y = gridSize / 2;
                    break;
                default:
                    // In case it's neither of the directions symbols, it's a command to set word to current element.
                    currentElement.letter = letter;
                break;
            }
        }
        UpdateVisualization();
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

    public void ApplyRule(string instructions, Element targetElement)
    {
        int x = targetElement.x;
        int y = targetElement.y;
        // Some exception check
        if(instructions[0] == '<' || instructions[0] == '>' || instructions[0] == '^' || instructions[0] == '|' || instructions[0] == '*')
        { Debug.LogError($"Error! Instruction should be starting with a letter! The passed instruction was '{instructions}' , for element {targetElement.letter}"); return; }

        // First character of the string must be a symbol instruction. Take over all of the relationships of the target element
        Element currentElement = AddLogicalElement(x, y, instructions[0]);
        Update3DWordPart(targetElement.realObject.transform.position, currentElement);
        // Destroy (untangle/disconnect) target non-terminal from which the transformation will happen
        ReplaceElement(ref targetElement, currentElement);

        char prevDirection = '*';
        foreach (var letter in instructions)
        {
            switch (letter)
            {
                case '<':
                    if (grid[--x, y] == null)
                    // [new] <- (old)
                    { currentElement.left = new Element(this, _right: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '<'; }
                    currentElement = currentElement.left;
                    break;
                case '>':
                    if (grid[++x, y] == null)
                    // (old) -> [new]
                    { currentElement.right = new Element(this, _left: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '>'; }
                    currentElement = currentElement.right;
                    break;
                case '^':
                    if (grid[x, ++y] == null)
                        { currentElement.up = new Element(this, _down: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '^'; }
                    currentElement = currentElement.up;
                    break;
                case '|':
                    if (grid[x, --y] == null)
                        { currentElement.down = new Element(this, _up: currentElement); prevDirection = '*'; }
                    else
                    { prevDirection = '|'; }
                    currentElement = currentElement.down;
                    break;
                case '*':
                    currentElement = targetElement;
                    break;
                default:
                    // In case it's neither of the directions symbols, it's a command to set word to current element.              
                    switch (prevDirection)
                    {
                        case '<':
                            // (a) [c] (b)
                            currentElement.right.left = new Element(this, _right: currentElement.right); currentElement.right = currentElement.right.left; currentElement.right.left = currentElement;
                            currentElement = currentElement.right;
                            break;
                        case '>':
                            currentElement.left.right = new Element(this, _left: currentElement.left); currentElement.left = currentElement.left.right; currentElement.left.right = currentElement;
                            currentElement = currentElement.left;
                            break;
                        case '^':
                            currentElement.down.up = new Element(this, _down: currentElement.down); currentElement.down = currentElement.down.up; currentElement.down.up = currentElement;
                            currentElement = currentElement.down;
                            break;
                        case '|':
                            currentElement.up.down = new Element(this, _up: currentElement.up); currentElement.up = currentElement.up.down; currentElement.up.down = currentElement;
                            currentElement = currentElement.up;
                            break;
                        default:
                            break;
                    }
                    currentElement.letter = letter;
                    break;
            }
        }
        startingElement = targetElement;
        UpdateVisualization();
    }

    public void ReplaceElement(ref Element targetElement, Element newStartingElement)
    {
        // Rewiring Relationships TO
        Debug.Log("The Disconnected element is " + targetElement.letter);
        if (targetElement.down != null)
        { targetElement.down.up = newStartingElement; }
        if (targetElement.up != null)
        { targetElement.up.down = newStartingElement; }
        if (targetElement.left != null)
        { targetElement.left.right = newStartingElement; }
        if (targetElement.right != null)
        { targetElement.right.left = newStartingElement; }
        // Rewiring Relationships FROM
        newStartingElement.left = targetElement.left;
        newStartingElement.right = targetElement.right;
        newStartingElement.up = targetElement.up;
        newStartingElement.down = targetElement.down;
        // Destroing 3d representation of the old one
        targetElement.realObject.targetScale = Vector3.zero;
        Destroy(targetElement.realObject.gameObject, 3);
        targetElement.realObject = null;
        targetElement = newStartingElement;
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
        DrawWordWithObject(latestCreatedElement, null, latestCreatedElement.realObject.transform.position);
    }

    // DEBUG ONLY Prints the word based on the helper grid [DEPRECATED]
   /* public void DrawWordWithObject(GameObject wordObject)
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
    }*/

    // Recursively prints word as sequence of relations between elements.
    public string DrawWordWithObject(Element currentElement, Element prevElement, Vector3 prevPos, string directionFromPrev = "center")
    {
        if(currentElement != null)
        {
            string result = "";
            Vector2 direction = new Vector2();
            switch (directionFromPrev)
            {
                case "center": direction.x = 0; direction.y = 0; break;
                case "left": direction.x = 1f; direction.y = 0; break;
                case "right": direction.x = -1f; direction.y = 0; break;
                case "up": direction.x = 0; direction.y = 1f; break;
                case "down": direction.x = 0; direction.y = -1f; break;
            }

            // Creating new 3D object to represent this wordpart
            Vector3 newPos = new Vector3(prevPos.x + direction.x, prevPos.y + direction.y, 0);
            Update3DWordPart(newPos, currentElement);

            // Continuing recursion
            if(currentElement.left != null && directionFromPrev != "right") { DrawWordWithObject(currentElement.left, currentElement, newPos, "left"); }
            if(currentElement.right != null && directionFromPrev != "left") { DrawWordWithObject(currentElement.right, currentElement, newPos, "right"); }
            if(currentElement.up != null && directionFromPrev != "down") { DrawWordWithObject(currentElement.up, currentElement, newPos,"up" ); }
            if(currentElement.down != null && directionFromPrev != "up") { DrawWordWithObject(currentElement.down, currentElement, newPos,  "down"); }
            return result;
        }
        Debug.LogError("Can't print the word: Current element is null");
        return "Can't print the word: Current element is null";
    }

    public void Update3DWordPart(Vector3 pos, Element element)
    {
        // Create the object if it doesn't exist yet
        if (element.realObject == null)
        {
            ElementCore newWordPart = Instantiate(elementPrefab, pos, Quaternion.identity).GetComponent<ElementCore>();
            Debug.Log($"Created {element.letter} on pos ${pos}");
            newWordPart.targetPos = pos;
            newWordPart.gameObject.AddComponent<BoxCollider>();
            newWordPart.transform.name = element.letter.ToString();
            newWordPart.Initialize(element);
            element.realObject = newWordPart;
            latestCreatedElement = element;
        }
        if (element.realObject.targetPos != pos)
        {
            Debug.Log($"Move object {element.letter} to the position {pos}");
            // Command object to move to the new position
            element.realObject.targetPos = pos;
        }
    }
}
