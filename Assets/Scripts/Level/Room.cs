﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public enum RoomType
{
    Default,
    Room,
    Corridor,
    Special,
    Boss,
    Treasure
}

[Serializable]
[RequireComponent(typeof(BoxCollider))]
public class Room : MonoBehaviour
{
    public GameObject[] exits;
    public GameObject[] spawnZones;
    public bool isActive = false;
    public bool isCleared = false;
    private bool isPlayerInside = false;
    private List<GameObject> enemies = new List<GameObject>();
    public RoomType roomType;
    [HideInInspector] public int depth;
    [HideInInspector] public int minDepth;

    // spawning
    public EnemyPack[] availablePacks;
    public float difficultyTarget;
    private bool spawnedEnemies;

    // Loot
    public float lootQualityModifier = 1f;
    public bool spawnTreasure = true;

    public GameObject minimapPrefab;
    private Collider roomCollider;

    private void Start()
    {
        roomCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == GameManager.Instance._player)
        {
            var bounds = other.bounds;
            if (roomCollider.bounds.Contains(bounds.min) && roomCollider.bounds.Contains(bounds.max))
                ActivateRoom();
        }
    }

    private void ActivateRoom()
    {
        var currActiveRoom = GameManager.Instance.activeRoom;
        if (currActiveRoom && !currActiveRoom.isCleared) return;
        GameManager.Instance.activeRoom = this;
        isActive = true;
        isPlayerInside = true;

        if (!spawnedEnemies)
        {
            spawnedEnemies = true;
            SpawnEnemies();
        }

        CheckCleared();

        if (!isCleared)
        {
            CloseAllDoors();
        }
    }

    private void SpawnEnemies()
    {
        if (availablePacks.Length != 0)
        {
            float currentDifficulty = 0;
            List<EnemyPack> toSpawn = new List<EnemyPack>();
            
            // Select packs until we meet a difficulty target
            while (currentDifficulty < difficultyTarget)
            {
                var newPack = Util.RandomItem(availablePacks);
                toSpawn.Add(newPack);
                currentDifficulty += newPack.difficultyRating;
            }

            var spawnPosition = spawnZones.Length > 0 ? spawnZones[0].transform.position : transform.position;

            toSpawn.ForEach(pack =>
                pack.SpawnEnemies(spawnPosition).ForEach(AddEnemy)
            );
        }
        else
        {
            // old implementation
            foreach (var o in spawnZones)
            {
                o.GetComponent<SpawnZone>().SetActive();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("Player"))
        // {
        //     ActivateRoom();
        // }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    private void OnDrawGizmos()
    {
        var bounds = GetComponent<Collider>().bounds;
        var color = isCleared ? Color.cyan : (isActive ? Color.green : Color.red);
        color.a = 0.2f;
        Gizmos.color = color;
        Gizmos.DrawCube(bounds.center, bounds.size);
    }

    private void Update()
    {
        if (isActive && isCleared && !isPlayerInside)
            isActive = false;
        if (isActive)
            CheckCleared();
    }

    private void CheckCleared()
    {
        if (isCleared || !isActive || !spawnedEnemies) return;

        var spawnersDone = spawnZones.Length == 0 || spawnZones.All((o =>
        {
            var spawnZone = o.GetComponent<SpawnZone>();
            return !(spawnZone is null) && spawnZone.IsDone();
        }));

        var enemiesDead = enemies.Count(o => o != null) == 0;
        isCleared = spawnersDone && enemiesDead;

        if (isCleared)
            OnClear();
    }

    private void OnClear()
    {
        OpenAllDoors();
        if (spawnTreasure)
        {
            SpawnTreasure();
        }
    }

    private void SpawnTreasure()
    {
        GameManager.SpawnTreasureChest(transform.position, lootQualityModifier);
    }

    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }

    public void OpenAllDoors()
    {
        var hasOpened = false;
        foreach (var o in exits)
        {
            var exit = o.GetComponent<RoomExit>();
            if (exit == null || !exit.isConnected)
            {
                // print("null exit or not connected");
            }
            else
            {
                // print("opening exit");
                hasOpened = exit.Open() || hasOpened;
            }
        }

        if (hasOpened) AudioManager.PlaySound("doorOpen");
    }

    public void CloseAllDoors()
    {
        foreach (var o in exits)
        {
            var exit = o.GetComponent<RoomExit>();
            if (exit == null || !exit.isConnected)
            {
                // print("null exit or not connected");
            }
            else
            {
                // print("opening exit");
                exit.Close();
            }
        }
    }

    public void GenerateMinimapSprite()
    {
        var minimaps = FindObjectsOfType<SpriteRenderer>();

        var bounds = GetComponent<Collider>().bounds;
        print(bounds);
        var minimapIcon = Instantiate(minimapPrefab, transform);
        var spriteRenderer = minimapIcon.GetComponent<SpriteRenderer>();
        var spriteWidth = spriteRenderer.bounds.size.x;
        var spriteHeight = spriteRenderer.bounds.size.z;

        var roomWidth = bounds.size.x;
        var roomHeight = bounds.size.z;

        minimapIcon.transform.localScale = new Vector3(roomWidth / spriteWidth, roomHeight / spriteHeight, 1);
        minimapIcon.transform.position = bounds.center + Vector3.up;
    }
}