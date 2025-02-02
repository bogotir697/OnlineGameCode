using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Portal : NetworkBehaviour
{
    public string portalId = "";
    [SyncVar(hook = nameof(SyncPortalId))]
    string _SyncPortalId;
    public int portalIndex;
    [SyncVar(hook = nameof(SyncPortalIndex))]
    int _SyncPortalIndex;
    public string portalData;
    [SyncVar(hook = nameof(SyncPortalData))]
    string _SyncPortalData;
    public Portal otherPortal;
    public List<GameObject> PortalsToDelete = new List<GameObject> { };
    bool inited;
    public bool skipTeleport = false;
    [SyncVar(hook = nameof(SyncTeleport))]
    bool _SyncSkipTeleport;


    public void SyncPortalId(string oldValue, string newValue)
    {
        portalId = newValue;
    }
    public void SyncPortalData(string oldValue, string newValue)
    {
        portalData = newValue;
    }
    public void SyncPortalIndex(int oldValue, int newValue)
    {
        portalIndex = newValue;
    }

    [Server]
    public void Init(string portalId, string data)
    {
        _SyncPortalId = portalId;
        this.portalId = portalId;
        _SyncPortalIndex = int.Parse(data[1].ToString());
        portalIndex = int.Parse(data[1].ToString());
        _SyncPortalData = data;
        portalData = data;
        this.portalId = portalId;
        portalIndex = int.Parse(data[1].ToString());
        inited = true;
    }
    private void Start()
    {
        MaterialHolder matHold = GameObject.FindGameObjectWithTag("mh").GetComponent<MaterialHolder>();
        Material[] mats = matHold.portalMaterials;
        foreach (Material mat in mats)
        {
            if (mat.name.Contains(portalData)) gameObject.GetComponent<MeshRenderer>().material = mat;
        }
        GameObject[] Portals = GameObject.FindGameObjectsWithTag("Portal");
        foreach (GameObject portal in Portals)
        {
            bool sameId = portal.GetComponent<Portal>().portalId == portalId;
            bool sameIndex = portal.GetComponent<Portal>().portalIndex == portalIndex;
            if (sameId && sameIndex && portal != gameObject)
            {
                PortalsToDelete.Add(portal);
            }
            if (sameId && !sameIndex)
            {
                otherPortal = portal.GetComponent<Portal>();
                otherPortal.otherPortal = gameObject.GetComponent<Portal>();
            }
        }
        foreach (GameObject portal in PortalsToDelete)
        {
            NetworkServer.Destroy(portal);
        }
        
    }
    private void Update()
    {
        if (inited && isServer)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject owner = null;
            foreach (GameObject player in players)
            {
                if (player.GetComponent<Player>().netId.ToString() == portalId)
                {
                    owner = player;
                }
            }
            if (owner == null)
            {
                NetworkServer.Destroy(gameObject);
            }
            
        }
    }

    public void SyncTeleport(bool oldValue, bool newValue)
    {
        skipTeleport = newValue;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PortalgunSphere>(out PortalgunSphere T)) return;
        if (skipTeleport) { 
            _SyncSkipTeleport = false;
            skipTeleport = false;
            otherPortal._SyncSkipTeleport = false;
            otherPortal.skipTeleport = false;
            return;
        }
        otherPortal._SyncSkipTeleport = true;
        otherPortal.skipTeleport = true;
        _SyncSkipTeleport = true;
        skipTeleport = true;
        other.transform.position = otherPortal.transform.position;
    }
}
