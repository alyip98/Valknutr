﻿using System;
using UnityEngine;

public class TreasureChest : HealthScript
{
    public int numTreasure = 3;
    [SerializeField]
    private float offsetDistance;
    
    public override void OnDeath()
    {
        base.OnDeath();
        DamageTextManager.SpawnTempWord("Choose one . . .", transform.position + Vector3.up, Color.yellow);
        var direction =  transform.position - GameManager.Instance._player.transform.position;
        direction.y = 0;
        direction = direction.normalized;
        var offset = offsetDistance * new Vector3(-direction.z, 0, direction.x);
        ItemDrop[] itemDrops = new ItemDrop[numTreasure];
        
        for (int i = 0; i < numTreasure; i++)
        {
            var itemDrop = GameManager.Instance.SpawnItem(transform.position + (i - (numTreasure - 1) / 2) * offset + direction);
            itemDrops[i]  = itemDrop;
        }
        for (int i = 0; i < numTreasure; i++)
        {
            for (int j = 0; j < numTreasure; j++)
            {
                itemDrops[i].OnPickup  += itemDrops[j].PickupHandler;
            }
        }
    }
}