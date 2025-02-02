using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Mirror;

public class Portalgun : Weapon
{
    public int type;
    public GameObject portal0;
    public GameObject portal1;
    string portal0MaterialName;
    string portal1MaterialName;
    Material[] mats;

    [SyncVar(hook = nameof(SyncP0MN))]
    string _SyncP0WN;
    [SyncVar(hook = nameof(SyncP1MN))]
    string _SyncP1WN;
    [SyncVar(hook = nameof(SyncType))]
    int _SyncType;

    private void Start()
    {
        MaterialHolder matHold = GameObject.FindGameObjectWithTag("mh").GetComponent<MaterialHolder>();
        mats = matHold.portalMaterials;
        Random rand = new Random((int)owner.netId);
        int tempRandom = rand.Next(0, 3);
        _SyncType = tempRandom;
        type = tempRandom;
        foreach (Material mat in mats)
        {
            if (mat.name.Contains($"{type}0")) { _SyncP0WN = mat.name; portal0MaterialName = mat.name; }
            if (mat.name.Contains($"{type}1")) { _SyncP1WN = mat.name; portal1MaterialName = mat.name; }
        }
        foreach (Material mat in mats)
        {
            if (mat.name == portal0MaterialName)
            {
                portal0.GetComponent<MeshRenderer>().material = mat;
            }
            if (mat.name == portal1MaterialName)
            {
                portal1.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }
    private void Update()
    {
        base.Update();
        if (owner.hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                PortalShot(timeBetweenShots, portal0MaterialName);
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                PortalShot(timeBetweenShots, portal1MaterialName);
            }
        }
    }
    public void SyncP0MN(string oldValue, string newValue)
    {
        portal0MaterialName = newValue;
        foreach (Material mat in mats)
        {
            if (mat.name == portal0MaterialName)
            {
                portal0.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }
    public void SyncP1MN(string oldValue, string newValue)
    {
        portal1MaterialName = newValue;
        foreach (Material mat in mats)
        {
            if (mat.name == portal1MaterialName)
            {
                portal1.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }
    public void SyncType(int oldValue, int newValue)
    {
        type = newValue;
        foreach (Material mat in mats)
        {
            if (mat.name.Contains($"{type}0")) _SyncP0WN = mat.name;
            if (mat.name.Contains($"{type}1")) _SyncP1WN = mat.name;
        }
    }
    public void PortalShot(float timeBetweenShots, string matName)
    {
        if (Time.time - lastShotTime >= timeBetweenShots)
        {
            Vector3 bulletSpawnPlace = transform.position;
            Vector3 rayDirection = (owner.gameObject.transform.position - bulletSpawnPlace).normalized;
            Ray ray = new Ray(bulletSpawnPlace, rayDirection);
            float distanceToPlayer = Vector3.Distance(bulletSpawnPlace, owner.gameObject.transform.position);
            Physics.Raycast(ray, out RaycastHit hitData, distanceToPlayer, 1);
            if (hitData.collider != null) return;
            Quaternion bulletDirection = transform.rotation;
            Vector3 pos = Input.mousePosition;
            pos.z = 10f;
            pos = Camera.main.ScreenToWorldPoint(pos);
            if (owner.isServer)
                owner.SpawnPortalSphere(owner, bulletSpawnPlace, bulletDirection, pos, matName);
            else
                owner.CmdSpawnPortalSphere(owner, bulletSpawnPlace, bulletDirection, pos, matName);
            lastShotTime = Time.time;
        }
    }
}
