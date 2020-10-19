﻿using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

class SplitSpellModifier : SpellModifier
{
    private int iterations = 2;
    private float damageReduction = 0.8f;
    private float randomMax = 0.2f;
    public override SpellBase ModifyBehaviour(SpellBase action)
    {
        //important to make sure it doesnt cast a recursive method
        Action oldBehavior = action.behaviour;
        Action temp = () =>
        {
            Vector3 originalPosDiff = action._direction;
            action._direction += new Vector3(Random.Range(-randomMax, randomMax), 0,
                Random.Range(-randomMax, randomMax));
            action._offset += new Vector3(Random.Range(-randomMax, randomMax), 0,
                Random.Range(-randomMax, randomMax));
            action._direction.Normalize();
            action._damage *= damageReduction;
            oldBehavior.Invoke();
            action._direction = originalPosDiff; //reset
        };
            
        Action spell = () =>
        {
            for (int i = 0; i < iterations; i++)
            {
                GameManager.Instance.StartCoroutine(DelayInvoke(temp, i / 10f));
            }
        };
        action.behaviour = spell;
        return action;
    }

    public override void UseQuality()
    {
        iterations = Mathf.RoundToInt(iterations * quality);
        damageReduction = quality / (iterations/2);
    }

    IEnumerator DelayInvoke(Action invoke, float delay)
    {
        yield return new WaitForSeconds(delay);
        invoke.Invoke();
    }
    public override Tooltip GetTooltip()
    {
        return new Tooltip("Echo <Modifier>", $"Repeats the spell effects {n} times, but each spell effect is {0}% weaker");
    }
}
