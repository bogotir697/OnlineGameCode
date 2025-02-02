using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Grenade : NetworkBehaviour
{
    public Player owner;
    public float timeBetweenThrows;
    public int countOfGrenades;
    public int maxCountOfGrenades;
    float lastThrowTime;

    void Start()
    {
        lastThrowTime = -timeBetweenThrows;
        countOfGrenades = maxCountOfGrenades;
    }
    private void Update()
    {
        owner.AmmoText(countOfGrenades, maxCountOfGrenades, 0, 0);
        if (owner.hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                Throw(timeBetweenThrows);
        }
    }
    public void Throw(float timeBetweenThrows)
    {
        if (Time.time - lastThrowTime >= timeBetweenThrows && countOfGrenades >= 1)
        {
            Vector3 pos = Input.mousePosition;
            pos.z = 10f;
            pos = Camera.main.ScreenToWorldPoint(pos);
            if (owner.isServer)
                owner.SpawnGrenade(owner.netId, transform.position, pos);
            else
                owner.CmdSpawnGrenade(owner.netId, transform.position, pos);
            countOfGrenades--;
            owner.AmmoText(countOfGrenades, maxCountOfGrenades, 0, 0);
            lastThrowTime = Time.time;
        }
    }
}
