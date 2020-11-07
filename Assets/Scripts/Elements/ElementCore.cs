using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementCore : MonoBehaviour
{
    public Transform meshParent;
    public Vector3 targetScale;
    public Vector3 startScale = Vector3.zero;
    public Element logicalElement;
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = startScale;
    }

    // Update is called once per frame
    void Update()
    {
        if ( (targetScale.x - transform.localScale.x) > 0.0001f || (transform.localScale.x - targetScale.x) > 0.0001f )
        {
            UpdateScaling();
        }
    }

    public void Initialize(Element element)
    {
        Instantiate(SymbolToObject.instance.GetObjFromSymbol(element.letter), meshParent);
        targetScale = Vector3.one;
        logicalElement = element;
    }

    public void UpdateScaling()
    {
        transform.localScale = Vector3.Slerp(transform.localScale, targetScale, 2f * Time.deltaTime);
        //Debug.Log($"Updating Scaling [{System.DateTime.Now}]");
    }

    public void ApplyRuleOnThis(string rule)
    {
        logicalElement.grammar.ApplyRule(rule, logicalElement);
        Debug.Log($"Applied rule {rule} on {transform.name} [{System.DateTime.Now}]");
    }

    private void OnMouseDown()
    {
        ApplyRuleOnThis("S^S");
    }

}
