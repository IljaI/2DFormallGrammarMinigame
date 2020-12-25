using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonstrationManager : MonoBehaviour
{
    public GameObject wordPart;
    // Start is called before the first frame update
    void Start()
    {
        SymbolToObject.instance.SetSelectedPack("SimpleGeometryPack");
        SymbolToObject.instance.AssociateLanguageWithMeshes(new List<char> { 'a', 'b', 'c', 'd', 'S', '_' });
        FormalGrammar2D grammar = new FormalGrammar2D(35, wordPart, transform.position, transform);
        grammar.GenerateWord(">a>a^b", 'S');
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.RotateAround(transform.position, Vector3.up, 20 * Time.deltaTime);
    }
}
