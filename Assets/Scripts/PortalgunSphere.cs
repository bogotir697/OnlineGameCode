using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PortalgunSphere : NetworkBehaviour
{
    public Vector3 PortalPos;
    [SyncVar(hook = nameof(SyncOwner))]
    Player _SyncOwner;
    Player owner;
    public Vector3 target;
    string SphereMaterial;
    bool inited;
    public int damage = 1;
    float creationTime;
    [SerializeField] float DespawnTime = 2f;
    [SerializeField] float speed = 0.4f;

    string portalMaterialName;
    Material[] mats;
    [SyncVar(hook = nameof(SyncSphereMaterial))]
    string _SyncSphereMaterial;
    public void SyncSphereMaterial(string oldValue, string newValue)
    {
        SphereMaterial = newValue;
        portalMaterialName = newValue;
    }
    void SyncOwner(Player oldValue, Player newValue)
    {
        owner = newValue;
    }

    [Server]
    public void Init(Player owner, Vector3 target, string mat)
    {
        _SyncOwner = owner;
        this.target = target;
        _SyncSphereMaterial = mat;
        SphereMaterial = mat;
        portalMaterialName = mat;
        inited = true;
        creationTime = Time.time;
    }
    private void Start()
    {
        MaterialHolder matHold = GameObject.FindGameObjectWithTag("mh").GetComponent<MaterialHolder>();
        mats = matHold.portalMaterials;
        foreach (Material mat in mats)
        {
            if (mat.name.Contains(portalMaterialName))
            {
                gameObject.GetComponent<MeshRenderer>().material = mat;
            }
        }
        transform.localEulerAngles = new Vector3(0, 0, 0);
        gameObject.GetComponent<Rigidbody>().AddForceAtPosition((target - transform.position).normalized * speed, target);
    }
    void Update()
    {
        if (inited && isServer)
        {
            if ((target - transform.position).magnitude <= 0.6f)
            {
                if (owner.isServer) 
                    owner.SpawnPortal(owner.netId.ToString(), SphereMaterial, transform.position, transform.rotation);
                else 
                    owner.CmdSpawnPortal(owner.netId.ToString(), SphereMaterial, transform.position, transform.rotation);
                NetworkServer.Destroy(gameObject);
            }

            foreach (var item in Physics.OverlapSphere(transform.position, Player.PlayerWidth))
            {
                Player player = item.GetComponent<Player>();
                if (player)
                {
                    if (player.netId != owner.netId)
                    {
                        GameObject[] Portals = GameObject.FindGameObjectsWithTag("Portal");
                        if (Portals.Length != 0)
                        {
                            foreach (GameObject portal in Portals)
                            {
                                if (portal.GetComponent<Portal>().portalId == owner.ToString() && !(portal.GetComponent<Portal>().portalIndex == int.Parse(SphereMaterial[1].ToString())))
                                {
                                    player.transform.position = portal.transform.position;
                                }
                            }
                        }
                        NetworkServer.Destroy(gameObject);
                    }
                }
            }
            
            if (Time.time >= creationTime + DespawnTime) 
            {
                NetworkServer.Destroy(gameObject); 
            }
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Bullet")
        {
            if (owner.isServer) 
                owner.SpawnPortal(owner.netId.ToString(), SphereMaterial, transform.position, transform.rotation);
            else 
                owner.CmdSpawnPortal(owner.netId.ToString(), SphereMaterial, transform.position, transform.rotation);
            NetworkServer.Destroy(gameObject);
        }

    }

}
