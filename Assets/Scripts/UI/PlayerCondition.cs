using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCondition : MonoBehaviour
{
    public UICondition uiCondition;

    public Condition health { get { return uiCondition.health; } }
    public Condition hunger { get { return uiCondition.hunger; } }
    public Condition stamina { get { return uiCondition.stamina; } }

    public float noHungerHealthDecay;

    // Update is called once per frame
    void Update()
    {
        hunger.Subtract(hunger.passiveVal * Time.deltaTime);
        stamina.Add(hunger.passiveVal * Time.deltaTime);

        if(hunger.curVal <= 0f)
        {
            health.Subtract(noHungerHealthDecay * Time.deltaTime);
        }

        if(health.curVal <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {

    }

    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Eat(float amount)
    {
        hunger.Add(amount);
    }
}
