using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Turret : NetworkBehaviour
{
    public int viewAngle = 45;
    public float radius;
    public Player owner;
    public float lastShotTime = 0;
    public float timeBetweenShots = 0;
    public int UsingAmmo = 0;
    public int MaxUsingAmmo = 10;
    public float timeBeforeReload;
    public float reloadTime;
    Vector3 forwardVector;
    public int damage = 1;
    public int hp;
    public int maxHp;
    public float angle;

    [SerializeField] public Material myTeamMaterial;
    [SerializeField] public Material enemyTeamMaterial;

    public Canvas hpCanvas;
    public GameObject fillHealth;
    public Text hpText;
    public Canvas ammoCanvas;
    public Text ammoText;
    public Color fullAmmoColor;
    public Color ammoColor;
    public Color fewAmmoColor;

    [SyncVar(hook = nameof(SyncHeadRotation))]
    Vector3 _SyncHeadRotation;
    Vector3 headRotation;
    [SyncVar(hook = nameof(SyncHp))]
    public int _SyncHp;
    [SyncVar(hook = nameof(SyncOwner))]
    Player _SyncOwner;

    bool inited;
    [SerializeField] public GameObject turretHead;
    [SerializeField] public GameObject turretLegs;
    public GameObject sphere;
    public GameObject otherTurret;


    public void Reload(float reloading)
    {
        ammoText.gameObject.transform.GetChild(1).gameObject.SetActive(true);
        ammoText.transform.GetChild(1).gameObject.transform.localScale = new Vector3(reloading, 1, 1);
    }

    void SyncOwner(Player oldValue, Player newValue)
    {
        owner = newValue;
    }

    void SyncHp(int oldValue, int newValue)
    {
        hp = newValue;
    }
    [Server]
    public void ChangeHealthValue(int newValue)
    {
        _SyncHp = newValue;
    }
    [Command]
    public void CmdChangeHealth(int newValue)
    {
        ChangeHealthValue(newValue); //переходим к непосредственному изменению переменной
    }
    public void ChangeHealth(int newValue)
    {
        if (newValue <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
        if (isServer)
        {
            ChangeHealthValue(newValue); //переходим к непосредственному изменению переменной
        }
        else
        {
            CmdChangeHealth(newValue);
        }
    }

    [Server]
    public void Init(Player owner)
    {
        _SyncOwner = owner;
        Debug.Log(owner + " Init");
        inited = true;
    }
    private void Start()
    {
        GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");
        foreach (GameObject turret in turrets)
        {
            if (turret.GetComponent<Turret>().owner.netId == owner.netId && turret != gameObject)
            {
                otherTurret = turret;
            }
        }
        if (otherTurret != null)
            NetworkServer.Destroy(otherTurret);
        forwardVector = sphere.transform.forward;
        ChangeHealth(maxHp);
        MaterialHolder mat = GameObject.FindGameObjectWithTag("mh").GetComponent<MaterialHolder>();
        myTeamMaterial = mat.turretMaterials[2];
        enemyTeamMaterial = mat.turretMaterials[3];
        UsingAmmo = MaxUsingAmmo;
        reloadTime = 0;
    }
    private void Update()
    {
        if (UsingAmmo == 0 && reloadTime == 0)
            reloadTime = Time.time + timeBeforeReload;
        
        if (reloadTime <= Time.time && reloadTime != 0)
        {
            UsingAmmo = MaxUsingAmmo;
            reloadTime = 0;
        }
        hpCanvas.transform.rotation = Quaternion.Euler(0, 0, 0);
        hpText.text = $"{hp}/{maxHp}";
        fillHealth.transform.localScale = new Vector3(hp * 1f / maxHp, 1, 1);
        if (owner.hasAuthority)
        {
            const int percentOfUsingAmmo = 10;
            turretHead.transform.GetChild(0).GetComponent<MeshRenderer>().material = myTeamMaterial;
            turretLegs.GetComponent<MeshRenderer>().material = myTeamMaterial;
            ammoCanvas.gameObject.SetActive(true);
            ammoCanvas.transform.rotation = Quaternion.Euler(0, 0, 0);
            ammoText.text = $"{UsingAmmo}";
            if (UsingAmmo == MaxUsingAmmo)
                ammoText.color = fullAmmoColor;
            else if (UsingAmmo > (percentOfUsingAmmo - MaxUsingAmmo % percentOfUsingAmmo + MaxUsingAmmo) / percentOfUsingAmmo)
                ammoText.color = ammoColor;
            else
                ammoText.color = fewAmmoColor;
            if (reloadTime != 0)
                Reload((reloadTime - Time.time) / timeBeforeReload);
        }
        else
        {
            turretHead.transform.GetChild(0).GetComponent<MeshRenderer>().material = enemyTeamMaterial;
            turretLegs.GetComponent<MeshRenderer>().material = enemyTeamMaterial;
            ammoCanvas.gameObject.SetActive(false);
        }
        float minimalDistanceToTarget = radius;
        GameObject target = null;
        foreach(var item in Physics.OverlapSphere(sphere.transform.position, radius))
        {
            Player player = item.GetComponent<Player>();
            Turret turret = item.GetComponent<Turret>();
            if ((player && player.netId != owner.netId && !player.playerIsDead) || turret && turret.gameObject != gameObject)
            {
                Vector3 turretPos = sphere.transform.position;
                Vector3 rayDirection = (item.gameObject.transform.position - turretPos).normalized;
                Ray ray = new Ray(turretPos, rayDirection);
                float distanceToTarget = Vector3.Distance(turretPos, item.gameObject.transform.position);
                Physics.Raycast(ray, out RaycastHit hitData, distanceToTarget, 1);
                if (hitData.collider != null)
                    continue;
                var relativePos = item.transform.position - sphere.transform.position;
                var angle = Vector3.Angle(relativePos, forwardVector);
                if (angle > viewAngle)
                    continue;
                if (minimalDistanceToTarget > distanceToTarget)
                {
                    minimalDistanceToTarget = distanceToTarget;
                    target = item.gameObject;
                }
            }
        }
        if (target != null)
        {
            Debug.Log($"{target}");
            sphere.transform.LookAt(target.transform.position);
            sphere.transform.localEulerAngles = new Vector3(0, sphere.transform.localEulerAngles.y, 0);
            if (isServer)
                ChangeHeadRotation(sphere.transform.localEulerAngles);
            else
                CmdChangeHeadRotation(sphere.transform.localEulerAngles);
            sphere.transform.localEulerAngles = headRotation;
            Shot();
        }
        if (isServer)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject owner = null;
            foreach (GameObject player in players)
            {
                if (player.GetComponent<Player>().netId == this.owner.netId)
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
    void Shot()
    {
        if (Time.time - lastShotTime >= timeBetweenShots && UsingAmmo >= 1)
        {
            reloadTime = 0;
            UsingAmmo -= 1;
            Quaternion bulletDirection = sphere.transform.rotation;
            if (owner.isServer)
                owner.SpawnBullet(owner.netId, damage, turretHead.transform.position, bulletDirection);
            else
                owner.CmdSpawnBullet(owner.netId, damage, turretHead.transform.position, bulletDirection);
            lastShotTime = Time.time;
        }
    }

    void SyncHeadRotation(Vector3 oldValue, Vector3 newValue)
    {
        headRotation = newValue;
    }

    [Server]
    public void ChangeHeadRotation(Vector3 newValue)
    {
        _SyncHeadRotation = newValue;
        headRotation = newValue;
    }

    [Command]
    public void CmdChangeHeadRotation(Vector3 newValue)
    {
        ChangeHeadRotation(newValue);
    }

}
