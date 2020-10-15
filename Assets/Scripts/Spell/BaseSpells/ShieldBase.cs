﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

class ShieldBase : SpellBase
{
    private float offsetIncrement;
    
    protected override void SetValues()
    {
        offsetIncrement = 45f;
    }
    
    public override void SpellBehaviour(Spell spell)
    {
        if (SpellManager.Instance.ShieldFull())
        {
            return;
        }
        for (int i = 0; i < _iterations; i++)
        {
            var p = Instantiate(_objectForSpell, _player.position + 
                                                 _offset + _player.forward * _speed / 50f * Random.Range(0.6f, 1.4f) * _scale, _player.localRotation);
            float rotateBy = (float) Math.Ceiling(i / 2.0) * (i % 2 == 0 ? -1 : 1) * offsetIncrement;
            rotateBy += _direction.x * 90 + _direction.z * 90;
            p.transform.RotateAround(_player.position,Vector3.up, rotateBy);
            p.transform.SetParent(_player);
            p.AddComponent<Shield>().SetSpeed(_speed);
            _objectForSpell = p;
        }
    }
}