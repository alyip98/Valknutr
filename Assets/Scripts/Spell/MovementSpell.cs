﻿using UnityEngine;

class MovementSpell : SpellBaseType
{
    public float speed = 2500f;
    private Rigidbody _rb;
    public void Init()
    {
        _objectForSpell = GameManager.Instance._player;
        _rb = _objectForSpell.GetComponent<Rigidbody>();
    }
    public override void SpellBehaviour(Spell spell)
    {
        _rb.AddForce(_rb.transform.forward * speed);
    }
}