using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Condition : MonoBehaviour
{
    public float curVal;
    public float startVal;
    public float maxVal;
    public float passiveVal;
    public Image Bar;

    void Start()
    {
        curVal = startVal;    
    }

    void Update()
    {
        Bar.fillAmount = GetPercentage();    
    }

    float GetPercentage()
    {
        return curVal / maxVal;
    }

    public void Add(float value)
    {
        curVal = Mathf.Min(curVal + value, maxVal);
    }

    public void Subtract(float value)
    {
        curVal = Mathf.Max(curVal - value, 0);
    }
}
