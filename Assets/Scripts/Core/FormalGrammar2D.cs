using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Schema;
using UnityEngine;

// Element is a single letter of the word. It has 4 'neighboors', references to the other Elemnts in the framnets.
public class Element 
{
    public Element right, left, up, down;
    public int x, y;
    public ElementCore realObject = null;
    public FormalGrammar2D grammar;
    public char letter;

    public Element(FormalGrammar2D _grammar, int _x, int _y, char _letter = '_', Element _right = null, Element _left = null, Element _up = null, Element _down = null)
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

    public bool isNeighboor(Element element)
    {
        if(right == element || left == element || up == element || down == element) 
        { 
            return true; 
        }
        return false;
    }
}

public class FormalGrammar2D : MonoBehaviour 
{
    public Element[,] grid;
    public Element startingElement;
    public int gridSize;
    public GameObject elementPrefab;
    public Element latestCreatedElement;
    private int idNumber;

    public FormalGrammar2D(int _gridSize, GameObject _elementPrefab)
    {
        gridSize = _gridSize;
        // Increasing gridSize by 1 (making it even) in case if its cleanly divisible by 2 (so that there would be place for center element)
        if (gridSize % 2 == 0) {gridSize++;}
        grid = new Element[gridSize,gridSize];
        GrammarDebug.instance.gridDebugText.fontSize = 495 / gridSize;
        for (int i = 0; i < gridSize; i++)
        {
            for (int k = 0; k < gridSize; k++)
            {
                grid[i, k] = null;
            }
        }
        elementPrefab = _elementPrefab;
    }

    public bool isDirection(char symbol)
    {
        return (symbol == '<' || symbol == '>' || symbol == '^' || symbol == '|' || symbol == '*');
    }

    public Element AddLogicalElement(int x, int y, char direction, char _letter = '_',  Element _right = null, Element _left = null, Element _up = null, Element _down = null)
    {
        Debug.Log($"Got command to create element with letter {_letter} on pos [{x},{y}]");
        Element newElement = new Element(this, x, y, _letter, _right, _left, _up, _down);
        InsertIntoGrid(x, y, newElement, direction, direction);
        return newElement;
    }

    public void InsertIntoGrid(int x, int y, Element element, char directionFromPrev, char globalDirection)
    {
        if (x == gridSize || x < 0 || y == gridSize || y < 0)
        {
            Debug.LogError($"Out of grid bounds: trying to add {element.letter} at x:{x} y:{y}");
            return;
        }
        if (grid[x, y] == null)
        {
            Debug.Log($"There was nothing in the grid[{x},{y}], so element {element.letter} was placed there.");
            grid[x, y] = element;
            element.x = x;
            element.y = y;
            Update3DWordPart(new Vector3(x, y, 0), element);
        }
        else
        {
            MoveInTheGrid(x, y, element, directionFromPrev, globalDirection);
        }
        GrammarDebug.instance.UpdateGridDebug(grid, gridSize);
    }

    public void MoveInTheGrid(int x, int y, Element element, char directionFromPrev, char globalDirection)
    {
        if (grid[x, y] != null && !element.isNeighboor(grid[x, y])) { Debug.LogError($"Error when applying rule: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed letter was '{element.letter}'"); return; }
        if (grid[x, y] == null)
        {
            if (grid[element.x, element.y] == element)
            {
                RemoveFromGrid(element.x, element.y);
            }
        }
        // Rabbit hole of recursive moving of the whole grid
        int dir_x = 0;
        int dir_y = 0;
        switch (globalDirection)
        {
            case '<': dir_x = -1; dir_y = 0; break;
            case '>': dir_x = 1; dir_y = 0; break;
            case '^': dir_x = 0; dir_y = 1; break;
            case '|': dir_x = 0; dir_y = -1; break;
            default:
                Debug.LogError("Global direction can't be something other that the default 4 directions.");
                return;
        }

        // Assuring that all elements placed in the target direction are moved
        //Debug.Log($"Recursion: {grid[x, y].realObject.transform.name} on pos [{element.x},{element.y}] was replaced by element {(idNumber + 1).ToString() + "_" +element.letter } on pos [{x},{y}]");     
        grid[x, y] = element;
        element.x = x;
        element.y = y;
        // Debug.Log($"Recursion placed {element.realObject.transform.name} at [{x},{y}]");
        // Assuring that all connected elements of each moved element are moved (as on first iteration directionFromPrev and globalDirection will be the same, update won't go into the wrong (opposite from the target direction) direction.
        if (element.left != null && directionFromPrev != '>')  { MoveInTheGrid(element.left.x + dir_x, element.left.y + dir_y, element.left,  '<', globalDirection); }
        if (element.right != null && directionFromPrev != '<') { MoveInTheGrid(element.right.x + dir_x, element.right.y + dir_y, element.right, '>', globalDirection); }
        if (element.up != null && directionFromPrev != '|')    { MoveInTheGrid(element.up.x + dir_x, element.up.y + dir_y, element.up,    '^', globalDirection); }
        if (element.down != null && directionFromPrev != '^')  { MoveInTheGrid(element.down.x + dir_x, element.down.y + dir_y, element.down,  '|', globalDirection); }              
        Update3DWordPart(new Vector3(x, y, 0), element);
    }

    public void RemoveFromGrid(int x, int y)
    {
        grid[x, y] = null;
    }

    // Creates a word, with this class's startingElement as word's start point
    public void GenerateWord(string instructions, char startingLetter)    
    {
        if(isDirection(instructions[instructions.Length-1]))
        {
            Debug.LogError($"Error when generation word: Instructions should be ending with a letter! The passed instruction was '{instructions}'"); return;
        }
        // Initializing cooridnates which will be used to place elements into the correct grid slots
        int x = gridSize / 2;
        int y = gridSize / 2;
        startingElement = AddLogicalElement( x, y, '*', _letter: startingLetter);
        // Iterator for the loop below
        int i = 0;
        Element currentElement = startingElement;
        foreach (var letter in instructions)
        {
            switch (letter) 
            {
                case '<':
                    if (currentElement.left == null)
                    {
                        if (grid[x - 1, y] != null) { Debug.LogError($"Error when generation word: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; } 
                        { currentElement.left = AddLogicalElement(x - 1, y, letter, _letter: instructions[i + 1], _right: currentElement); currentElement.left.right = currentElement; }
                    }
                    currentElement = currentElement.left;
                    --x;
                break;
                case '>':
                    if (currentElement.right == null)
                    {
                        if (grid[x + 1, y] != null) { Debug.LogError($"Error when generation word: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; }
                        { currentElement.right = AddLogicalElement(x + 1, y, letter, _letter: instructions[i + 1], _left: currentElement); currentElement.right.left = currentElement; }
                    }
                        currentElement = currentElement.right;
                    ++x;
                break;
                case '^':
                    if (currentElement.up == null)
                    {
                        if (grid[x, y + 1] != null) { Debug.LogError($"Error when generation word: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; }
                        { currentElement.up = AddLogicalElement(x, y + 1, letter, _letter: instructions[i + 1], _down: currentElement); currentElement.up.down = currentElement; }
                    }
                    currentElement = currentElement.up;
                    ++y;
                break;
                case '|':
                    if (currentElement.down == null)
                    {
                        if (grid[x, y - 1] != null) { Debug.LogError($"Error when generation word: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; }
                        { currentElement.down = AddLogicalElement(x, y - 1, letter, _letter: instructions[i + 1], _up: currentElement); currentElement.down.up = currentElement; }
                    }
                    currentElement = currentElement.down;
                    --y;
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
            i++;
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

    public void ApplyRule(string instructions, Element targetElement)
    {
        Debug.Log($"Apply rule {instructions} on {targetElement.realObject}");
        int x = targetElement.x;
        int y = targetElement.y;
        // Some exception check
        if (isDirection(instructions[0]))
        { Debug.LogError($"Error! Instruction should be starting with a letter! The passed instruction was '{instructions}' , for element {targetElement.letter}"); return; }
        if (instructions.Length < 2)
        { Debug.LogError($"Error while parsing instruction {instructions}: instructions should be >= 2 in length."); }

        // First character of the string must be a symbol instruction. 
        RemoveFromGrid(x, y);
        Element currentElement = AddLogicalElement(x, y, '*', _letter: instructions[0]);
        startingElement = currentElement;
        // Destroy (untangle/disconnect) target non-terminal from which the transformation will happen
        ReplaceElement( targetElement, currentElement);

        char prevDirection = '*';
        char prevCommand = instructions[0];
        char command = instructions[1];
        // Starting from the second command, as the first one was already processed
        for (int i = 1; i < instructions.Length; i++)
        {
            command = instructions[i];
            bool insertWordCommand = !(command == '<' || command == '>' || command == '|' || command == '*' || command == '^');
            bool insertWordCommandPrev = !(prevCommand == '<' || prevCommand == '>' || prevCommand == '|' || prevCommand == '*' || prevCommand == '^');
            if(insertWordCommand && insertWordCommandPrev) { Debug.LogError($"Instruction {instructions} are incorrect: you cant have two symbol declaration in a row. Two commands are {prevCommand} and {command}. Index is {i}."); return; }

            switch (prevCommand)
            {
                case '<':
                    if (currentElement.left == null)//(grid[x--, y] == null)
                    {
                        if (grid[x - 1, y] != null) { Debug.LogError($"Error when applying rule: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; }
                        if (insertWordCommand)
                        { currentElement.left = AddLogicalElement(x - 1, y, prevCommand, _letter: command, _right: currentElement); prevDirection = '*'; }
                        else
                        { Debug.LogError($"Error applying rule {instructions} on {currentElement.realObject} - You can't move on without specifying the element's letter."); }
                    }
                    else
                    { prevDirection = '<'; }
                    currentElement = currentElement.left;
                    --x;
                    break;
                case '>':
                    if (currentElement.right == null)//(grid[x++, y] == null)
                    {
                        if (grid[x + 1, y] != null) { Debug.LogError($"Error when applying rule: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; }
                        if (insertWordCommand)
                        { currentElement.right = AddLogicalElement(x + 1, y, prevCommand, _letter: command, _left: currentElement); prevDirection = '*'; }
                        else
                        { Debug.LogError($"Error applying rule {instructions} on {currentElement.realObject} - You can't move on without specifying the element's letter."); }
                    }
                    else
                    { prevDirection = '>'; }
                    currentElement = currentElement.right;
                    ++x;
                    break;
                case '^':
                    if (currentElement.up == null)//(grid[x, y++] == null)
                    {
                        if (grid[x, y + 1] != null) { Debug.LogError($"Error when applying rule: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; }
                        if (insertWordCommand)
                        { currentElement.up = AddLogicalElement(x, y + 1, prevCommand, _letter: command, _down: currentElement); prevDirection = '*'; }
                        else
                        { Debug.LogError($"Error applying rule {instructions} on {currentElement.realObject} - You can't move on without specifying the element's letter."); }
                    }
                    else
                    { prevDirection = '^'; }
                    currentElement = currentElement.up;
                    ++y;
                    break;
                case '|':
                    if (currentElement.down == null)//(grid[x, y--] == null)
                    {
                        if (grid[x, y - 1] != null) { Debug.LogError($"Error when applying rule: Instructions shouldnt try to create a cycled connection (word connecting into each other)! The passed instruction was '{instructions}'"); return; }
                        if (insertWordCommand)
                        { currentElement.down = AddLogicalElement(x, y - 1, prevCommand, _letter: command, _up: currentElement); prevDirection = '*'; }
                        else
                        { Debug.LogError($"Error applying rule {instructions} on {currentElement.realObject} - You can't move on without specifying the element's letter."); }
                    }
                    else
                    { prevDirection = '|'; }
                    currentElement = currentElement.down;
                    --y;
                    break;
                case '*':
                    currentElement = startingElement;
                    x = currentElement.x;
                    y = currentElement.y;
                    break;
            }
            // In case it's neither of the directions symbols, it's a command to set word to current element.
            if (insertWordCommand)
            {
                switch (prevDirection)
                {
                    // In case if element already had other element connected on that place, create new one and re-structure their relations.
                    case '<':
                        currentElement.right.left = AddLogicalElement(x, y, prevDirection, _letter: command, _right: currentElement.right, _left: currentElement); currentElement.right = currentElement.right.left; ;
                        currentElement = currentElement.right;
                        break;
                    case '>':
                        currentElement.left.right = AddLogicalElement(x, y, prevDirection, _letter: command, _left: currentElement.left, _right: currentElement); currentElement.left = currentElement.left.right;
                        currentElement = currentElement.left;
                        break;
                    case '^':
                        currentElement.down.up = AddLogicalElement(x, y, prevDirection, _letter: command, _down: currentElement.down, _up: currentElement); currentElement.down = currentElement.down.up;
                        currentElement = currentElement.down;
                        break;
                    case '|':
                        currentElement.up.down = AddLogicalElement(x, y, prevDirection, _letter: command, _up: currentElement.up, _down: currentElement); currentElement.up = currentElement.up.down;
                        currentElement = currentElement.up;
                        break;
                    // If element was null, the direction will be *, thus nothing will be done in this step.
                    default:
                        break;
                }
            }
            prevCommand = command;
        }

    }

    public void ReplaceElement(Element targetElement, Element newElement)
    {
        // Rewiring Relationships TO
        Debug.Log("The Disconnected element is " + targetElement.letter);
        if (targetElement.down != null)
        { targetElement.down.up = newElement; }
        if (targetElement.up != null)
        { targetElement.up.down = newElement; }
        if (targetElement.left != null)
        { targetElement.left.right = newElement; }
        if (targetElement.right != null)
        { targetElement.right.left = newElement; }
        // Rewiring Relationships FROM
        newElement.left = targetElement.left;
        newElement.right = targetElement.right;
        newElement.up = targetElement.up;
        newElement.down = targetElement.down;
        // Destroing 3d representation of the old one
        targetElement.realObject.targetScale = Vector3.zero;
        Destroy(targetElement.realObject.gameObject, 3);
        targetElement.realObject = null;
    }

    public void Update3DWordPart(Vector3 pos, Element element)
    {
        Debug.Log($"The update3dpart command was called for {pos} , element {element.letter}");
        pos -= new Vector3(gridSize/2, gridSize/2, 0);
        // Create the object if it doesn't exist yet
        if (element.realObject == null)
        {
            ElementCore newWordPart = Instantiate(elementPrefab, pos, Quaternion.identity).GetComponent<ElementCore>();
            newWordPart.targetPos = pos;
            newWordPart.gameObject.AddComponent<BoxCollider>();
            newWordPart.transform.name = $"{++idNumber}_{element.letter.ToString()}";
            Debug.Log($"Created real object {newWordPart.transform.name} on pos {pos}");
            newWordPart.Initialize(element);
            element.realObject = newWordPart;
            latestCreatedElement = element;
        }
        if (element.realObject != null && element.realObject.targetPos != pos)
        {
            Debug.Log($"Move object {element.realObject.transform.name} to the position {pos}");
            // Command object to move to the new position
            element.realObject.targetPos = pos;
        }
    }
}

/*
    public void UpdateVisualization()
    {
        DrawWordWithObject(latestCreatedElement, null, latestCreatedElement.realObject.transform.position);
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
    }*/
