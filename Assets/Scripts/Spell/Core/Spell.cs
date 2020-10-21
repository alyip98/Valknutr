﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class Spell : SpellItem
{
    #region fields
    
    [SerializeField] public SpellBase spellBase;
    [SerializeField] public List<SpellModifier> _spellModifiers = new List<SpellModifier>();
    [HideInInspector] public CastAnimation castAnimation;
    
    public float cooldownMax = 1;
    [HideInInspector]
    public float cooldownRemaining = 1;
    private readonly int hashCode = DateTime.Now.GetHashCode();
    private string _tooltipMessage;
    #endregion

    /// <summary>
    /// Use this method to create the sprite based on Base type and modifiers.
    /// Requires read/write enabled texture
    /// Requires RGBA 32bit color
    /// </summary>
    public Sprite CreateProceduralSprite(Sprite baseType, List<Sprite> modifiers)
    {
        int SpriteHeight = 128;
        var newTexture = Texture2D.Instantiate(baseType.texture);
        Vector2Int offset = new Vector2Int(80,80); // use this to place the modifiers
        
        foreach (var modiSprite in modifiers)
        {
            var modiTex = modiSprite.texture;
            for (int x = offset.x; x < modiTex.width; x++) {
                for (int y = offset.y; y < modiTex.height; y++) {
                    newTexture.SetPixel(x, y, modiTex.GetPixel(x, y));
                }
            }

            offset.y += 10;
        }
        newTexture.Apply();
        return Sprite.Create(newTexture, baseType.rect, Vector2.zero);
    }
    
    /// <summary>
    /// Use this method to create the tooltip
    /// </summary>
    public void CreateTooltip(string behav, List<string> modifiers)
    {
        string tooltip = behav;
        
        foreach (var modiTooltip in modifiers)
        {
            tooltip += " " + modiTooltip;
        }
        _tooltipMessage = tooltip;
    }

    public override Tooltip GetTooltip(SpellContext ctx)
    {
        string bodyMessage = spellBase.GetTooltip(ctx).Body +
                             _spellModifiers.Aggregate("", (s, modifier) => s + " " + modifier.GetTooltip(ctx).Body);
        return new Tooltip(spellBase.GetTooltip(ctx).Title, bodyMessage);
        
        /*
         * Damaging Multi-Projectile of Speed
         * CD: 0.6
         * Dmg: 2
         * Count: 3
         *
         * Fires a projectile. Affected spells will be faster. Repeats spell 3 times with 50% effect. Spell deals 100% increased damage.
         *
         * Flavor text
         */
    }

    public SpellContext GetFinalSpellContext()
    {
        return ApplyModifiers(spellBase.GetContext());
    }

    private SpellContext ApplyModifiers(SpellContext ctx)
    {
        if (_spellModifiers != null && _spellModifiers.Count != 0)
        {
            foreach (var modifier in _spellModifiers)
            {
                ctx = modifier.Modify(ctx);
            }
        }

        return ctx;
    }
    
    public void CastSpell(SpellCastData data)
    {
        if (!IsReady()) return;

        var ctx = spellBase.GetContext();
        ctx.direction = data.castDirection;
        ctx = ApplyModifiers(ctx);

        // float totalCooldown = spellBase.cooldown;
        // spellBase.InitializeValues();
        // spellBase.direction = data.castDirection;
        // spellBase.behaviour = () => spellBase.SpellBehaviour(this);

        

        ctx.action(ctx);
        cooldownMax = cooldownRemaining = ctx.cooldown;
    }

    public override int GetHashCode()
    {
        return hashCode;
    }

    public float GetAnimSpeed()
    {
        var ctx = GetFinalSpellContext();

        castAnimation = spellBase.animationType;

        // float oriSpeed = spellBase.speed;
        // if (_spellModifiers != null && _spellModifiers.Count != 0)
        // {
        //     foreach (var modifier in _spellModifiers)
        //     {
        //         modifier.ModifySpell(spellBase);
        //     }
        // }

        return ctx.speed;
        // return spellBase.speed / oriSpeed;
    }

    public void AddBaseType(SpellBase baseType)
    {
        SpellBase copy = Instantiate(baseType);
        spellBase = copy;
        castAnimation = copy.animationType;
    }

    public void AddModifier(SpellModifier modifier)
    {
        SpellModifier copy = Instantiate(modifier);
        _spellModifiers.Add(copy);
    }

    public bool IsReady()
    {
        return cooldownRemaining <= 0;
    }

    public void Tick(float t)
    {
        cooldownRemaining = Mathf.Clamp(cooldownRemaining - t, 0, cooldownMax);
    }

    /// <summary>
    /// Returns fraction of cooldown left, i.e. 0 = off cd
    /// </summary>
    /// <returns></returns>
    public float GetCooldownRemainingPercentage()
    {
        if (cooldownMax <= 0) return 0;
        return cooldownRemaining / cooldownMax;
    }
    
}