using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class TurretSpawnBox : NetworkBehaviour
{
    public float timeToSpawnTurret;
    public Player owner;
    public GameObject turret;
    bool canPlaceTurret;
    [SyncVar(hook = nameof(SyncCanPlaceTurret))]
    bool _SyncCanPlaceTurret;
    float turretSpawnTime;
    float spawningTurretTime;
    Material[] mats;

    void SyncCanPlaceTurret(bool oldValue, bool newValue)
    {
        canPlaceTurret = newValue;
        if (canPlaceTurret)
            turret.transform.GetChild(0).GetComponent<MeshRenderer>().material = mats[0];
        else
            turret.transform.GetChild(0).GetComponent<MeshRenderer>().material = mats[1];
    }
    private void Start()
    {
        MaterialHolder matHold = GameObject.FindGameObjectWithTag("mh").GetComponent<MaterialHolder>();
        mats = matHold.turretMaterials;
    }
    private void Update()
    {
        if (owner.hasAuthority)
        {
            turret.SetActive(true);
            owner.FillCircle(spawningTurretTime);
            CanPlaceTurret();
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (canPlaceTurret)
                {
                    turretSpawnTime = timeToSpawnTurret + Time.time;
                    if (owner.isServer)
                        owner.ChangePlayerVelocity(0, 0, 0);
                    else
                        owner.CmdChangePlayerVelocity(0, 0, 0);
                    owner.canPlayerMove = false;
                }
            }
            if (Input.GetKey(KeyCode.Mouse0) && turretSpawnTime != 0)
            {
                if (canPlaceTurret)
                    spawningTurretTime = (timeToSpawnTurret - turretSpawnTime + Time.time) / timeToSpawnTurret;
                else
                    spawningTurretTime = 0;
            }
            if (Input.GetKeyUp(KeyCode.Mouse0) && turretSpawnTime != 0)
            {
                turretSpawnTime = 0;
                spawningTurretTime = 0;
                owner.FillCircle(0);
                owner.canPlayerMove = true;
            }
            if (turretSpawnTime <= Time.time && turretSpawnTime != 0)
            {
                turretSpawnTime = 0;
                spawningTurretTime = 0;
                owner.canPlayerMove = true;
                owner.FillCircle(0);
                PlaceTurret();
            }
            owner.FillCircle(spawningTurretTime);
            if (canPlaceTurret)
                turret.transform.GetChild(0).GetComponent<MeshRenderer>().material = mats[0];
            else
                turret.transform.GetChild(0).GetComponent<MeshRenderer>().material = mats[1];
        }
        else
            turret.SetActive(false);
    }

    void PlaceTurret()
    {
        spawningTurretTime = 0;
        owner.FillCircle(0);
        if (owner.isServer)
            owner.SpawnTurret(owner, turret.transform.position, turret.transform.rotation);
        else
            owner.CmdSpawnTurret(owner, turret.transform.position, turret.transform.rotation);
        owner.itemSwitchIndex = -1;
    }
    void CanPlaceTurret()
    {
        Vector3 turretPos = turret.transform.position;
        Vector3 rayDirection = (owner.gameObject.transform.position - turretPos).normalized;
        Ray ray = new Ray(turretPos, rayDirection);
        float distanceToPlayer = Vector3.Distance(turretPos, owner.gameObject.transform.position);
        Physics.Raycast(ray, out RaycastHit hitData, distanceToPlayer, 1);
        if (hitData.collider != null)
        {
            _SyncCanPlaceTurret = false;
            canPlaceTurret = false;
            return;
        }
        foreach (var item in Physics.OverlapSphere(turretPos, 0.1f))
        {
            if (item.gameObject.layer == 0)
            {
                _SyncCanPlaceTurret = false;
                canPlaceTurret = false;
                return;
            }
        }
        _SyncCanPlaceTurret = true;
        canPlaceTurret = true;
    }
}
