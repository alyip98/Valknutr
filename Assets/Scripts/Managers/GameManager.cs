﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector]
    public GameObject _player;
    public ItemList _itemList;
    [HideInInspector]
    public GameObject _weapon;

    public Room activeRoom;
    public GameObject itemDropPrefab;
    public GameObject healthPickupObj;
    public GameObject treasurePrefab;
    public PlayerHealth _playerHealth;
    public float healthPickupValue = 2f;
    public float healthPickupDropChance = 0.3f;

    public QualityManager QualityManager;
    
    public delegate void PlayerDeathAction();
    public static event PlayerDeathAction OnPlayerDeath;
    
    public delegate void LevelCompleteAction();
    public static event LevelCompleteAction OnLevelComplete;
    public void Awake()
    {
        _player = GameObject.Find("Player");
        _playerHealth = _player.GetComponent<PlayerHealth>();
        if (_player == null)
        {
            Debug.LogError("No player in this scene for GameManager");
            return;
        }

        SpellBase._player = _player.transform;
        _playerHealth.OnPlayerDeath += () => OnPlayerDeath?.Invoke();

        //extension method (fluent)
        _weapon = _player.transform.FindDescendentTransform("Weapon").gameObject;
        if (_weapon == null)
        {
            Debug.LogError("No weapon in player in this scene for GameManager");
            return;
        }
    }

    public ItemDrop SpawnItem(Vector3 position, SpellItem _SpellItem = null, QualityManager.Quality quality = default, SpellElement notThis = null)
    {
        if (_SpellItem == null)
        {
            
            var itemListSpellItems = _itemList._SpellItems;
            for (int i = 0; i < 10; i++)
            {
                _SpellItem = Instantiate(itemListSpellItems[Random.Range(0, itemListSpellItems.Count)]);
                if (_SpellItem._spellElement != notThis)
                {
                    break;
                }
            }

            _SpellItem._spellElement = Instantiate(_SpellItem._spellElement); //copies
        }
        //Add quality to item
        QualityManager.RandomizeAndInitProperties(_SpellItem, QualityManager.GetQuality(DifficultyScalingSystem.Instance.difficultyLevel));
        
        var itemDrop = Instantiate(itemDropPrefab, position, Quaternion.identity).GetComponent<ItemDrop>();
        itemDrop._spellItem = _SpellItem;
        return itemDrop;
    }

    public ItemDrop SpawnHP(Vector3 position)
    {

        var hp = Instantiate(healthPickupObj, position, Quaternion.identity).GetComponent<ItemDrop>();
            
        return hp;
        
    }

    public static TreasureChest SpawnTreasureChest(Vector3 position, float quality)
    {
        var treasure = Instantiate(Instance.treasurePrefab, position, Quaternion.identity).GetComponent<TreasureChest>();
        // todo: set loot + quality
        return treasure;
    }

    public void SetGameWin()
    {
        //Display winning screen

        //Disable controls?

        OnLevelComplete?.Invoke();
        //Kill all enemies
        List<GameObject> enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        //This code is scary
        foreach(GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    public void HealthPickup()
    {
        _playerHealth.IncreaseCurrHealth(healthPickupValue);
        _playerHealth.IncreaseMaxHealth(healthPickupValue);
    }
    public void AffectPlayerCurrHealth(float value)
    {
        if(value > 0)
            _playerHealth.IncreaseCurrHealth(value);
        else
        {
            _playerHealth.ApplyDamage(-value);
        }
    }
}
