using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementCore : MonoBehaviour
{
    public Transform meshParent;
    public Vector3 targetScale;
    public Vector3 startScale = Vector3.zero;
    public Vector3 targetPos;
    public Element logicalElement;
    public bool logicallyDisabled = false; 
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = startScale;
        targetPos = transform.position;
        //InvokeRepeating("dummyFunc", 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if ( (targetScale.x - transform.localScale.x) > 0.0001f || (transform.localScale.x - targetScale.x) > 0.0001f )
        {
            UpdateScaling();
        }
        if ( Vector3.Distance(transform.position, targetPos) > 0.0001f )
        {
            UpdatePosition();
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

    public void UpdatePosition()
    {
        transform.position = Vector3.Slerp(transform.position, targetPos, 3f * Time.deltaTime);
    }

    public void ApplyRuleOnThis(string rule)
    {
        if (!logicallyDisabled)
        {
            Debug.Log($"Apply rule {rule} on {transform.name}");
            logicalElement.grammar.ApplyRule(rule, logicalElement);
            logicallyDisabled = true;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnMouseDown()
    {
        ApplyRuleOnThis("S>b^a");
    }

    void dummyFunc()
    {
        ApplyRuleOnThis("S^b<a|a*<a*>b^c*|c");
    }

}
