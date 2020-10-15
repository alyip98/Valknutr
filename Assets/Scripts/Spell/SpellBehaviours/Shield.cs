﻿using System;
using UnityEngine;

public class Shield : NoTrigger
{
    private Transform parent;
    private float angularSpeed = 100f;
    private Quaternion rotation;
    private Vector3 orgPosition;
    private Transform newParent;
    private PlayerHealth playerHealth;
    private HealthScript _healthScript;
    protected override void Start()
    {
        base.Start();
        SpellManager.Instance.AddShield();
        parent = transform.parent;
        playerHealth = parent.GetComponent<PlayerHealth>();
        orgPosition = parent.position - transform.position;
        newParent = new GameObject("shield").transform;
        var o = newParent.gameObject;
        o.tag = "Player";
        o.layer = parent.gameObject.layer;
        newParent.transform.position = parent.position;
        transform.SetParent(newParent);
        _healthScript = gameObject.AddComponent<HealthScript>();
        _healthScript.damageColor = Color.yellow;
        _healthScript.maxHealth = playerHealth.maxHealth/10 + parent.lossyScale.x * 10f;
        _healthScript.hurtSound = "shieldHit";
        playerHealth = parent.GetComponent<PlayerHealth>();
        playerHealth.AddBuffer(this);
        AudioManager.PlaySoundAtPosition("shieldBuff", transform.position);
    }

    private void Update()
    {
        newParent.position = Vector3.Lerp(newParent.position, parent.position, 0.2f);   
        transform.RotateAround(newParent.position, Vector3.up, angularSpeed * Time.deltaTime);
    }
    
    public void SetSpeed(float speed)
    {
        angularSpeed = speed;
    }

    public bool Damage(float damage)
    {
        var dead = _healthScript.ApplyDamage(damage);
        return dead;
    }

    private void OnDestroy()
    {
        if(SpellManager.Instance)
            SpellManager.Instance.RemoveShield();
    }
}