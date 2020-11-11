﻿using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    public float distanceCutoff;
    public float initTime = 8f;
    private void OnTriggerEnter(Collider other)
    {
        var activeRoom = GameManager.Instance.activeRoom.gameObject.transform.position;
        if (Vector3.Distance(activeRoom, other.ClosestPoint(activeRoom)) > distanceCutoff)
        {
            Debug.LogWarning("KillZone kills : " + other.name, other);
            Destroy(other);
        }
        else
        {
            other.transform.position = activeRoom + 2 * (Random.insideUnitSphere + Vector3.up);
        }
    }

    public void Start()
    {
        
        StartCoroutine(SwitchOffKill(distanceCutoff));
    }

    private IEnumerator SwitchOffKill(float dis)
    {
        distanceCutoff = 0;
        yield return new WaitForSeconds(initTime);
        distanceCutoff = dis;
    }
}
