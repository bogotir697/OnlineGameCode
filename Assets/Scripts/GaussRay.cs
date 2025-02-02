using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GaussRay : NetworkBehaviour
{
    uint owner;
    bool inited;
    public float timeBeforeDespawn;
    public int damage;
    float creationTime;

    [Server]
    public void Init(uint owner)
    {
        this.owner = owner;
        inited = true;
        creationTime = Time.time;
    }
    private void Start()
    {
        if (isServer && inited)
        {
            foreach (var item in Physics.OverlapBox(transform.position, transform.localScale / 2, transform.rotation))
            {
                Player player = item.GetComponent<Player>();
                GrenadeProjectile grenade = item.GetComponent<GrenadeProjectile>();
                Turret turret = item.GetComponent<Turret>();
                if (player != null)
                {
                    if (player.netId != owner)
                    {
                        player.ChangeHealth(player.hp - damage);
                    }
                }
                if (grenade != null)
                {
                    grenade.health -= damage;
                }
                if (turret != null)
                {
                    if (turret.owner.netId != owner)
                    {
                        turret.ChangeHealth(turret.hp - damage);
                    }
                }
            }
        }
    }
    private void Update()
    {
        if (isServer && inited)
        {
            if (Time.time >= creationTime + timeBeforeDespawn)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}
