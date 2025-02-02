using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    uint owner;
    bool inited;
    public int damage = 1;
    float creationTime;
    [SerializeField] float DespawnTime = 2f;
    [SerializeField] float bulletSpeed = 0.4f;
    Vector3 dir;

    [Server]
    public void Init(uint owner)
    {
        this.owner = owner;
        inited = true;
        creationTime = Time.time;
    }
    private void Start()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * bulletSpeed);
    }

    void Update()
    {
        if (inited && isServer)
        {
            foreach (var item in Physics.OverlapSphere(transform.position, Player.PlayerWidth))
            {
                Player player = item.GetComponent<Player>();
                GrenadeProjectile grenade = item.GetComponent<GrenadeProjectile>();
                Turret turret = item.GetComponent<Turret>();
                if (player)
                {
                    if (player.netId != owner)
                    {
                        player.ChangeHealth(player.hp - damage); //отнимаем одну жизнь по аналогии с примером SyncVar
                        NetworkServer.Destroy(gameObject); //уничтожаем пулю
                    }
                }
                if (grenade)
                {
                    grenade.health -= damage;
                    NetworkServer.Destroy(gameObject);
                }
                if (turret)
                {
                    if (turret.owner.netId != owner)
                    {
                        turret.ChangeHealth(turret.hp - damage);
                        NetworkServer.Destroy(gameObject);
                    }
                }
            }

            if (Time.time >= creationTime + DespawnTime) //пуля достигла конечной точки
            {
                NetworkServer.Destroy(gameObject); //значит ее можно уничтожить
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Bullet")
        {
            NetworkServer.Destroy(gameObject);
        }
        
    }
}