﻿
using UnityEngine;

public class GroundStrikeBehaviour : SpellBehavior
{
    public float radius = 2F;
    public float power = 50.0F;
    private Damage damageScript;
    public Vector3 offset;
    public float scale = 1f;
    
    public override void Init()
    {
        _damage = 4f;
        _speed = 2f;
        _cooldown = .2f;
        scale = _objectForSpell.transform.localScale.x;
        _objectForSpell = GameManager.Instance._weapon;
        radius = scale * 1.5f;
        animationType = CastAnimation.Projectile;
        damageScript = _objectForSpell.GetComponent<Damage>();
        offset = Vector3.down;
    }

    public override void SpellBehaviour(Spell spell)
    {
        var position = _objectForSpell.transform.position + _posDiff * scale;
        position.y = Mathf.Max(position.y, 1.6f); //will not work with lower terrain
        ScreenShakeManager.Instance.ScreenShake(0.1f, 0.1f);
        AudioManager.PlaySoundAtPosition("groundStrike", position);
        EffectManager.PlayEffectAtPosition("groundStrike", position + offset, 
            new Vector3(scale,scale,scale));

        var cols = Physics.OverlapSphere(position, radius);
        
        foreach (var col in cols)
        {
            if (!col.CompareTag("Player") && col.attachedRigidbody != null)
            {
                col.attachedRigidbody.AddExplosionForce(power * _damage, position, radius, 0, ForceMode.Impulse);
            }

            if (!col.CompareTag("Player") && !col.CompareTag("Projectile"))
            {
                damageScript.SetDamage(_damage);
                damageScript.DealDamage(col);
            }
             
        }
        
    }
}
