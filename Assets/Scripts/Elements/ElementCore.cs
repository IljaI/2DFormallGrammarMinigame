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
    public bool newPosition = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ((targetScale.x - transform.localScale.x) > 0.0001f || (transform.localScale.x - targetScale.x) > 0.0001f)
        {
            UpdateScaling();
        }
        if (newPosition)
        {
            if (Vector3.Distance(transform.position, targetPos) > 0.0001f)
            {
                UpdatePosition();
            }
            else
            {
                newPosition = false;
            }
        }
    }

    public void Initialize(Element element)
    {
        Instantiate(SymbolToObject.instance.GetObjFromSymbol(element.letter), meshParent);
        targetScale = Vector3.one;
        logicalElement = element;
        transform.localScale = startScale;
        targetPos = transform.position;
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
            logicalElement.grammar.ApplyRule(rule, logicalElement);
            logicallyDisabled = true;
            GetComponent<Collider>().enabled = false;
        }
    }

    private void OnMouseDown()
    {
        ApplyRuleOnThis("S|b");
    }

}
