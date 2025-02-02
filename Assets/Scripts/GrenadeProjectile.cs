using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GrenadeProjectile : NetworkBehaviour
{
    uint owner;
    bool inited;
    public int damage = 30;
    [SyncVar(hook = nameof(SyncTarget))]
    public Vector3 _SyncTarget;
    Vector3 target;
    float creationTime;
    [SerializeField] float radius;
    [SerializeField] float explosionTime = 5f;
    public float explosionForce;
    [SerializeField] float speed;
    public bool dealDamageToOwner;
    public int health = 1;
    void SyncTarget(Vector3 oldValue, Vector3 newValue)
    {
        target = newValue;
    }
    [Server]
    public void Init(uint owner, Vector3 target)
    {
        this.owner = owner;
        _SyncTarget = target;
        this.target = target;
        inited = true;
        creationTime = Time.time;
    }
    private void Start()
    {
        gameObject.GetComponent<Rigidbody>().AddForce((target - transform.position).normalized * speed);
    }
    private void Update()
    {
        if (isServer && inited)
        {
            gameObject.GetComponent<Rigidbody>().AddForce(-gameObject.GetComponent<Rigidbody>().velocity * 30f * Time.deltaTime);
            if (Time.time >= creationTime + explosionTime || health <= 0)
            {
                foreach (var item in Physics.OverlapSphere(transform.position, radius))
                {
                    Player player = item.GetComponent<Player>();
                    GrenadeProjectile grenade = item.GetComponent<GrenadeProjectile>();
                    Turret turret = item.GetComponent<Turret>();
                    Vector3 rayDirection = (item.transform.position - transform.position).normalized;
                    float distanceToItem = Vector3.Distance(transform.position, item.transform.position);
                    Ray ray = new Ray(transform.position, rayDirection);
                    Physics.Raycast(ray, out RaycastHit hitData, distanceToItem, 1);
                    if (hitData.collider != null) continue;
                    if (player)
                    {
                        float pathToPlayer = Vector3.Distance(transform.position, player.transform.position);
                        int grenadeDamage = (int)(damage * ((radius - pathToPlayer)/ radius));
                        if (dealDamageToOwner || player.netId != owner)
                        {
                            player.gameObject.GetComponent<Rigidbody>().AddExplosionForce(1000 * explosionForce, transform.position, radius);
                            player.ChangeHealth(player.hp - grenadeDamage);
                        }
                    }
                    if (grenade)
                    {
                        grenade.gameObject.GetComponent<Rigidbody>().AddExplosionForce(1000 * explosionForce, transform.position, radius);
                        grenade.health -= damage;
                    }
                    if (turret)
                    {
                        turret.ChangeHealth(turret.hp - damage);
                        NetworkServer.Destroy(gameObject);
                    }
                }
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}
