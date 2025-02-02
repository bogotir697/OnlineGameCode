using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    public bool playerSpawned;
    public GameObject PointPrefab; 
    public LineRenderer LineRenderer; 
    public List<GameObject> Points = new List<GameObject>();
    public const float PlayerWidth = 0.5f;
    [SerializeField] GameObject HpBarFill;
    [SerializeField] GameObject HpBorder;
    [SerializeField] Text hpUIText;
    [SerializeField] public int hpMax = 5;
    [SerializeField] GameObject head;
    [SerializeField] Text coordinatesText;
    [SerializeField] Text speedText;
    [SerializeField] float respawnTime;
    [SerializeField] Text ammoText;
    [SerializeField] Image circle;
    [SerializeField] GameObject BulletPrefab;
    [SerializeField] GameObject PortalSpherePrefab;
    [SerializeField] GameObject PortalPrefab;
    [SerializeField] GameObject GrenadePrefab;
    [SerializeField] GameObject GaussRayPrefab;
    [SerializeField] GameObject TurretPrefab;
    
    public int previousWeaponSwitchIndex = 0;
    Vector3 headRotation;
    
    [SyncVar(hook = nameof(SyncHealth))]
    int _SyncHealth;
    [SyncVar(hook = nameof(SyncHeadRotation))]
    Vector3 _SyncHeadRotation;

    public float lastHitTime;
    [SyncVar(hook = nameof(SyncLastHitTime))]
    public float _SyncLastHitTime;
    public float timeBeforeRegeneration = 5;
    public float lastRegenerationTime;
    public float regeneraionTick = 0.5f;
    public int regenerationPerTick = 1;
    
    [SerializeField] public Weapon rifle;
    [SerializeField] public Weapon shotgun;
    [SerializeField] public Portalgun portalgun;
    [SerializeField] public Grenade grenade;
    [SerializeField] public TurretSpawnBox turretSpawnBox;
    [SerializeField] public Weapon gaussGun;
    [SerializeField] GameObject weaponHolder;
    public int weaponSwitchIndex = -1;
    [SerializeField] GameObject itemHolder;
    public int itemSwitchIndex = -1;
    [SyncVar(hook = nameof(SyncRifleActive))]
    bool _SyncRifleActive;
    public bool rifleIsActive;
    [SyncVar(hook = nameof(SyncShotgunActive))]
    bool _SyncShotgunActive;
    public bool shotgunIsActive;
    [SyncVar(hook = nameof(SyncPortalgunActive))]
    bool _SyncPortalgunActive;
    public bool portalgunIsActive;
    [SyncVar(hook = nameof(SyncGrenadeActive))]
    bool _SyncGrenadeIsActive;
    public bool grenadeIsActive;
    [SyncVar(hook = nameof(SyncTurretSBActive))]
    bool _SyncTurretSBIsActive;
    public bool turretSBIsActive;
    [SyncVar(hook = nameof(SyncGaussGunActive))]
    bool _SyncGaussGunActive;
    public bool gaussGunIsActive;
    public Color fullAmmoColor;
    public Color ammoColor;
    public Color fewAmmoColor;
    List<bool> SyncWeapons = new List<bool>();
    List<bool> SyncItems = new List<bool>();

    [SyncVar(hook = nameof(SyncNickname))]
    string _SyncNickname;
    string nickname;
    public Text nicknameText;
    public Text teamNameText;
    Color teamColor;

    [SyncVar(hook = nameof(SyncPlayerIsDead))]
    bool _SyncPlayerIsDead = false;
    public bool playerIsDead = false;
    [SerializeField] Text playerRespawnText;
    public Canvas mainCanvas;
    public Canvas playerCanvas;
    public Canvas ammoCanvas;
    [SyncVar(hook = nameof(SyncPlayerIsDeadHasChanged))]
    bool _SyncPlayerIsDeadHasChanged;
    [SyncVar(hook = nameof(SyncPlayerVelocity))]
    Vector3 _SyncPlayerVelocity;
    Vector3 playerVelocity;
    public bool canPlayerMove = true;

    public int hp;

    int pointsCount;
    [Header("Team")]
    GameTeam team;

    [Server]
    public void SpawnPlayer(GameTeam team)
    {
        this.team = team;
        gameObject.transform.position = Vector3.zero;
        playerSpawned = true;
    }
    [Command]
    public void CmdSpawnPlayer(GameTeam team)
    {
        SpawnPlayer(team);
    }

    void SyncNickname(string oldValue, string newValue)
    {
        nickname = newValue;
    }
    [Server]
    public void ChangeNickname(string nickname)
    {
        _SyncNickname = nickname;
    }
    [Command]
    public void CmdChangeNickname(string nickname)
    {
        ChangeNickname(nickname);
    }

    public void Start()
    {
        ammoText.transform.GetChild(0).gameObject.SetActive(false);
        ammoText.transform.GetChild(1).gameObject.SetActive(false);
        ammoText.transform.GetChild(2).gameObject.SetActive(false);
        circle.gameObject.SetActive(false);
        hp = hpMax;
        lastRegenerationTime = Time.time;
        lastHitTime = Time.time;
        if (!hasAuthority)
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        mainCanvas.worldCamera = Camera.main;
        for (int i = 0; i < weaponHolder.transform.childCount; i++)
        {
            SyncWeapons.Add(false);
        }
        for (int i = 0; i < itemHolder.transform.childCount; i++)
        {
            SyncItems.Add(false);
        }
    }
    [Server]
    public void SetPlayerIsDead(bool playerIsDead)
    {
        _SyncPlayerIsDead = playerIsDead;
    }
    [Command]
    public void CmdSetPlayerIsDead(bool newValue)
    {
        SetPlayerIsDead(newValue);
    }
    float lastChangeTime;
    bool playerIsDeadHasChanged;
    [Server]
    public void SetPlayerSpawnedWithDelay()
    {
        _SyncPlayerIsDeadHasChanged = true;
    }
    [Command]
    public void CmdSetPlayerSpawnedWithDelay()
    {
        SetPlayerSpawnedWithDelay();
    }
    void SyncPlayerIsDeadHasChanged(bool oldValue, bool newValue) //обязательно делаем два значения - старое и новое. 
    {
        playerIsDeadHasChanged = newValue;
        if (hasAuthority) gameObject.GetComponent<Rigidbody>().isKinematic = newValue;
        if (newValue)
        {
            gameObject.layer = 8;
            lastChangeTime = Time.time + respawnTime;
        }
        else gameObject.layer = 10;
    }
    void SyncPlayerVelocity(Vector3 oldValue, Vector3 newValue)
    {
        playerVelocity = newValue;
    }
    [Server]
    public void ChangePlayerVelocity(float h, float v, float speed)
    {
        _SyncPlayerVelocity = new Vector3(h * speed, v * speed, 0);
    }
    [Command] 
    public void CmdChangePlayerVelocity(float h, float v, float speed)
    {
        ChangePlayerVelocity(h, v, speed);
    }
    public void AmmoText(int UsingAmmo, int MaxUsingAmmo, int Ammo, int MaxAmmo)
    {
        const int percentOfUsingAmmo = 10;
        if (UsingAmmo == 0 && MaxUsingAmmo == 0 || !hasAuthority)
        {
            ammoText.text = "";
            ammoText.transform.GetChild(0).gameObject.SetActive(false);
            ammoText.transform.GetChild(1).gameObject.SetActive(false);
            ammoText.transform.GetChild(2).gameObject.SetActive(false);
            ammoText.transform.GetChild(3).gameObject.SetActive(false);
        }
        else
        {
            ammoText.transform.GetChild(0).gameObject.SetActive(true);
            ammoText.transform.GetChild(2).gameObject.SetActive(true);
            ammoText.transform.GetChild(3).gameObject.SetActive(true);
            ammoText.text = $"{UsingAmmo}";
            if (UsingAmmo == MaxUsingAmmo)
                ammoText.color = fullAmmoColor;
            else if (UsingAmmo > (percentOfUsingAmmo - MaxUsingAmmo%percentOfUsingAmmo + MaxUsingAmmo)/percentOfUsingAmmo)
                ammoText.color = ammoColor;
            else
                ammoText.color = fewAmmoColor;

            int sum = Ammo - MaxUsingAmmo*((int)System.Math.Ceiling(Ammo * 1f/ MaxUsingAmmo) - 1);
            if (sum == 0) sum += MaxUsingAmmo;

            if (sum == MaxUsingAmmo)
                ammoText.transform.GetChild(2).gameObject.GetComponent<Image>().color = fullAmmoColor;
            else if (sum > (percentOfUsingAmmo - MaxUsingAmmo % percentOfUsingAmmo + MaxUsingAmmo) / percentOfUsingAmmo)
                ammoText.transform.GetChild(2).gameObject.GetComponent<Image>().color = ammoColor;
            else
                ammoText.transform.GetChild(2).gameObject.GetComponent<Image>().color = fewAmmoColor;

            ammoText.transform.GetChild(2).gameObject.transform.localScale = new Vector3(1, sum * 1f/MaxUsingAmmo, 1);
            ammoText.transform.GetChild(3).gameObject.GetComponent<Text>().text = $"{(int)System.Math.Ceiling(Ammo * 1f / MaxUsingAmmo) - 1}";
            if ((int)System.Math.Ceiling(Ammo * 1f / MaxUsingAmmo) - 1 <= 0)
            {
                ammoText.transform.GetChild(3).gameObject.GetComponent<Text>().text = "";
                ammoText.transform.GetChild(3).transform.GetChild(0).gameObject.SetActive(false);
                if (Ammo == 0)
                    ammoText.transform.GetChild(2).gameObject.transform.localScale = new Vector3();
            }
            else
                ammoText.transform.GetChild(3).transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    public void Reload(float reloading)
    {
        ammoText.gameObject.transform.GetChild(1).gameObject.SetActive(true);
        ammoText.transform.GetChild(1).gameObject.transform.localScale = new Vector3(reloading, 1, 1);
    }
    public void FillCircle(float fulling)
    {
        circle.gameObject.SetActive(true);
        circle.fillAmount = fulling;
    }
    private void FixedUpdate()
    {
        if (!playerSpawned)
            return;
        if (playerIsDead)
        {
            return;
        }
        if (!canPlayerMove)
            return;
        if (hasAuthority)
        {
            float h = 0;
            float v = 0;
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
            float speed = 250f * Time.deltaTime;
            if (System.Math.Abs(h) > 0 && System.Math.Abs(v) > 0)
            {
                speed /= 1.414f;
            }
            if (isServer)
                ChangePlayerVelocity(h, v, speed);
            else 
                CmdChangePlayerVelocity(h, v, speed);
            gameObject.GetComponent<Rigidbody>().velocity = playerVelocity;
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(head.transform.position.z - Camera.main.transform.position.z);
            Vector3 objectPos = Camera.main.WorldToScreenPoint(head.transform.position);
            mousePos.x -= objectPos.x;
            mousePos.y -= objectPos.y;
            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            if (isServer)
                ChangeHeadRotation(new Vector3(0, 0, 180 + angle));
            else
                CmdChangeHeadRotation(new Vector3(0, 0, 180 + angle));
        }
        head.transform.localEulerAngles = headRotation;
    }
    void Update()
    {
        if (!playerSpawned)
            return;
        if (playerIsDeadHasChanged)
        {
            if (Time.time > lastChangeTime)
            {
                _SyncPlayerIsDeadHasChanged = false;
                if (isServer)
                {
                    SetPlayerIsDead(!playerIsDead);
                }
                else
                {
                    CmdSetPlayerIsDead(!playerIsDead);
                }
            }
            else
            {
                if (hasAuthority)
                {
                    playerRespawnText.text = $"Player can respawn in {lastChangeTime - Time.time : 0.00} seconds...";
                }
                
            }
        }

        teamColor = team.color;
        teamNameText.text = team.name;
        teamNameText.color = teamColor;
        nicknameText.color = teamColor;
        rifle.gameObject.SetActive(rifleIsActive);
        shotgun.gameObject.SetActive(shotgunIsActive);
        portalgun.gameObject.SetActive(portalgunIsActive);
        gaussGun.gameObject.SetActive(gaussGunIsActive);
        grenade.gameObject.SetActive(grenadeIsActive);
        turretSpawnBox.gameObject.SetActive(turretSBIsActive);

        if (playerIsDead)
        {
            return;
        }
        
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.H)) //отнимаем у себя жизнь по нажатию клавиши H
            {
                ChangeHealth(hp - 10);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (isServer)
                    ChangeVector3Vars(transform.position);
                else
                    CmdChangeVector3Vars(transform.position);
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                previousWeaponSwitchIndex = weaponSwitchIndex;
                if (weaponSwitchIndex >= weaponHolder.transform.childCount - 1) weaponSwitchIndex = 0;
                else weaponSwitchIndex++;
                itemSwitchIndex = -1;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                previousWeaponSwitchIndex = weaponSwitchIndex;
                if (weaponSwitchIndex <= 0) weaponSwitchIndex = weaponHolder.transform.childCount - 1;
                else weaponSwitchIndex--;
                itemSwitchIndex = -1;
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                previousWeaponSwitchIndex = weaponSwitchIndex;
                weaponSwitchIndex = -1;
                itemSwitchIndex = -1;
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                int n = weaponSwitchIndex;
                weaponSwitchIndex = previousWeaponSwitchIndex;
                previousWeaponSwitchIndex = n;
                itemSwitchIndex = -1;
            }
            foreach (Transform weapon in weaponHolder.transform.GetComponentInChildren<Transform>())
            {
                KeyCode key = (KeyCode)weapon.name[0];
                if (Input.GetKeyDown(key))
                {
                    previousWeaponSwitchIndex = weaponSwitchIndex;
                    weaponSwitchIndex = int.Parse(weapon.name[1].ToString());
                    itemSwitchIndex = -1;
                }
            }
            foreach (Transform item in itemHolder.transform.GetComponentInChildren<Transform>())
            {
                KeyCode key = (KeyCode)item.name[0];
                if (Input.GetKeyDown(key))
                {
                    itemSwitchIndex = int.Parse(item.name[1].ToString());
                    previousWeaponSwitchIndex = weaponSwitchIndex;
                    weaponSwitchIndex = -1;
                }
            }

            if (isServer)
            {
                ChangeItem(itemSwitchIndex);
                ChangeWeapon(weaponSwitchIndex);
            }                
            else
            {
                CmdChangeItem(itemSwitchIndex);
                CmdChangeWeapon(weaponSwitchIndex);
            }
            coordinatesText.text = $"x: {transform.position.x : 0.00} y: {transform.position.y : 0.00}";
            speedText.text = $"speed: {gameObject.GetComponent<Rigidbody>().velocity.magnitude : 0.00} mps";
            if (hp > 0 && hp < hpMax && lastHitTime + timeBeforeRegeneration <= Time.time)
            {
                if (lastRegenerationTime + regeneraionTick <= Time.time)
                {
                    if (isServer)
                        ChangeHealth(hp + regenerationPerTick);
                    else
                        CmdChangeHealth(hp + regenerationPerTick);
                    lastRegenerationTime = Time.time;
                }
            }
            ammoText.transform.localEulerAngles = -headRotation;
            circle.gameObject.transform.localEulerAngles = -headRotation;
            gameObject.GetComponent<Rigidbody>().velocity = playerVelocity;
        }
        rifle.gameObject.SetActive(rifleIsActive);
        shotgun.gameObject.SetActive(shotgunIsActive);
        portalgun.gameObject.SetActive(portalgunIsActive);
        gaussGun.gameObject.SetActive(gaussGunIsActive);
        grenade.gameObject.SetActive(grenadeIsActive);
        turretSpawnBox.gameObject.SetActive(turretSBIsActive);
        if (weaponSwitchIndex == -1)
            AmmoText(0, 0, 0, 0);
        nicknameText.text = nickname;
        hpUIText.text = $"{hp}/{hpMax}";
        HpBarFill.transform.localScale = new Vector3((float)hp / hpMax, 1, 1);
        for (int i = pointsCount; i < Vector3Vars.Count; i++)
        {
            GameObject point = Instantiate(PointPrefab, Vector3Vars[i], Quaternion.identity);
            Points.Add(point);
            pointsCount++;

            LineRenderer.positionCount = Vector3Vars.Count;
            LineRenderer.SetPositions(Vector3Vars.ToArray());
        }
    }
    
    private void OnDestroy()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Destroy(Points[i]);
        }
        Points.Clear();
    }
    public override void OnStopClient()
    {
        CmdClearVector3Vars();
        base.OnStopClient();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        ClearVector3Vars();
    }

    [Server]
    public void ChangeWeapon(int newValue)
    {
        int i = 0;

        foreach (Transform weapon in weaponHolder.transform)
        {
            if (i == newValue)
            {
                SyncWeapons[int.Parse(weapon.gameObject.name[1].ToString())] = true;
            }
            else
            {
                SyncWeapons[int.Parse(weapon.gameObject.name[1].ToString())] = false;
            }
            i++;
            _SyncRifleActive = SyncWeapons[int.Parse(rifle.name[1].ToString())];
            _SyncShotgunActive = SyncWeapons[int.Parse(shotgun.name[1].ToString())];
            _SyncGaussGunActive = SyncWeapons[int.Parse(gaussGun.name[1].ToString())];
        }
        
    }
    [Command]
    public void CmdChangeWeapon(int newValue)
    {
        ChangeWeapon(newValue);
    }

    [Server]
    public void ChangeItem(int newValue)
    {
        int i = 0;

        foreach (Transform weapon in itemHolder.transform)
        {
            if (i == newValue)
            {
                SyncItems[int.Parse(weapon.gameObject.name[1].ToString())] = true;
            }
            else
            {
                SyncItems[int.Parse(weapon.gameObject.name[1].ToString())] = false;
            }
            i++;
            _SyncGrenadeIsActive = SyncItems[int.Parse(grenade.name[1].ToString())];
            _SyncTurretSBIsActive = SyncItems[int.Parse(turretSpawnBox.name[1].ToString())];
            _SyncPortalgunActive = SyncItems[int.Parse(portalgun.name[1].ToString())];
        }

    }
    [Command]
    public void CmdChangeItem(int newValue)
    {
        ChangeItem(newValue);
    }

    void SyncPlayerIsDead(bool oldValue, bool newValue)
    {
        playerIsDead = newValue;
        if (newValue)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            playerCanvas.enabled = false;
            mainCanvas.enabled = false;
            ammoCanvas.enabled = false;
            weaponSwitchIndex = -1;
            itemSwitchIndex = -1;
            rifle.Ammo = rifle.MaxAmmo;
            rifle.UsingAmmo = rifle.MaxUsingAmmo;
            shotgun.Ammo = shotgun.MaxAmmo;
            shotgun.UsingAmmo = shotgun.MaxUsingAmmo;
            gaussGun.Ammo = gaussGun.MaxAmmo;
            gaussGun.UsingAmmo = gaussGun.MaxUsingAmmo;
            grenade.countOfGrenades = grenade.maxCountOfGrenades;
            if (isServer) { ChangeWeapon(weaponSwitchIndex); ChangeItem(itemSwitchIndex); }
            else { CmdChangeWeapon(weaponSwitchIndex); CmdChangeItem(itemSwitchIndex); }
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            playerCanvas.enabled = true;
            mainCanvas.enabled = true;
            ammoCanvas.enabled = true;
            hp = hpMax;
            playerRespawnText.text = "";
        }
    }
    void SyncHealth(int oldValue, int newValue)
    {
        hp = newValue;
    }
    void SyncHeadRotation(Vector3 oldValue, Vector3 newValue)
    {
        headRotation = newValue;
    }

    void SyncLastHitTime(float oldValue, float newValue)
    {
        lastHitTime = newValue;
    }
    void SyncRifleActive(bool oldValue, bool newValue)
    {
        rifleIsActive = newValue;
    }
    void SyncShotgunActive(bool oldValue, bool newValue)
    {
        shotgunIsActive = newValue;
    }
    void SyncPortalgunActive(bool oldValue, bool newValue)
    {
        portalgunIsActive = newValue;
    }
    void SyncGrenadeActive(bool oldValue, bool newValue)
    {
        grenadeIsActive = newValue;
    }
    void SyncTurretSBActive(bool oldValue, bool newValue)
    {
        turretSBIsActive = newValue;
    }
    void SyncGaussGunActive(bool oldValue, bool newValue)
    {
        gaussGunIsActive = newValue;
    }

    [Server]
    public void ChangeHeadRotation(Vector3 newValue)
    {
        _SyncHeadRotation = newValue;
    }

    [Command]
    public void CmdChangeHeadRotation(Vector3 newValue)
    {
        ChangeHeadRotation(newValue);
    }

    [Server]
    public void ChangeHealthValue(int newValue)
    {
        if (hp > newValue)
            _SyncLastHitTime = Time.time;
        _SyncHealth = newValue;
        if (_SyncHealth <= 0)
        {
            _SyncPlayerIsDead = true;
        }
    }
    [Command]
    public void CmdChangeHealth(int newValue)
    {
        ChangeHealthValue(newValue); //переходим к непосредственному изменению переменной
    }
    public void ChangeHealth(int newValue)
    {
        if (hp > newValue) 
            _SyncLastHitTime = Time.time;
        if (newValue <= 0)
        {
            if (isServer)
            {
                SetPlayerSpawnedWithDelay();
            }
            else
            {
                CmdSetPlayerSpawnedWithDelay();
            }
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
    SyncList<Vector3> _SyncVector3Vars = new SyncList<Vector3>();

    [Server]
    void ChangeVector3Vars(Vector3 newValue)
    {
        _SyncVector3Vars.Add(newValue);
    }
    [Command]
    public void CmdChangeVector3Vars(Vector3 newValue)
    {
        ChangeVector3Vars(newValue);
    }
    [Server]
    void ClearVector3Vars()
    {
        _SyncVector3Vars.Clear();
    }
    [Command]
    public void CmdClearVector3Vars()
    {
        ClearVector3Vars();
    }
    public List<Vector3> Vector3Vars;

    void SyncVector3Vars(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem)
    {
        switch (op)
        {
            case SyncList<Vector3>.Operation.OP_ADD:
                {
                    Vector3Vars.Add(newItem);
                    break;
                }
            case SyncList<Vector3>.Operation.OP_CLEAR:
                {
                    Vector3Vars.Clear();
                    break;
                }
            case SyncList<Vector3>.Operation.OP_INSERT:
                {

                    break;
                }
            case SyncList<Vector3>.Operation.OP_REMOVEAT:
                {

                    break;
                }
            case SyncList<Vector3>.Operation.OP_SET:
                {

                    break;
                }
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        _SyncVector3Vars.Callback += SyncVector3Vars; //вместо hook, для SyncList используем подписку на Callback

        Vector3Vars = new List<Vector3>(_SyncVector3Vars.Count); //так как Callback действует только на изменение массива,  
        for (int i = 0; i < _SyncVector3Vars.Count; i++) //а у нас на момент подключения уже могут быть какие-то данные в массиве, нам нужно эти данные внести в локальный массив
        {
            Vector3Vars.Add(_SyncVector3Vars[i]);
        }
    }
    [Server]
    public void SpawnBullet(uint owner, int damage, Vector3 weaponPos, Quaternion weaponRotation)
    {
        GameObject bulletGo = Instantiate(BulletPrefab, weaponPos, weaponRotation);
        NetworkServer.Spawn(bulletGo);
        bulletGo.GetComponent<Bullet>().damage = damage;
        bulletGo.GetComponent<Bullet>().Init(owner);
    }
    [Command]
    public void CmdSpawnBullet(uint owner, int damage, Vector3 weaponPos, Quaternion weaponRotation)
    {
        SpawnBullet(owner, damage, weaponPos, weaponRotation);
    }
    [Server]
    public void SpawnPortalSphere(Player owner, Vector3 weaponPos, Quaternion weaponRotation, Vector3 target, string matName)
    {
        GameObject bulletGo = Instantiate(PortalSpherePrefab, weaponPos, weaponRotation);
        NetworkServer.Spawn(bulletGo);
        bulletGo.GetComponent<PortalgunSphere>().Init(owner, target, matName);
    }
    [Command]
    public void CmdSpawnPortalSphere(Player owner, Vector3 weaponPos, Quaternion weaponRotation, Vector3 target, string matName)
    {
        SpawnPortalSphere(owner, weaponPos, weaponRotation, target, matName);
    }
    [Server]
    public void SpawnPortal(string portalId, string data, Vector3 pos, Quaternion rot)
    {
        GameObject portal = Instantiate(PortalPrefab, pos, rot);
        NetworkServer.Spawn(portal);
        portal.GetComponent<Portal>().Init(portalId, data);
    }
    [Command]
    public void CmdSpawnPortal(string portalId, string data, Vector3 pos, Quaternion rot)
    {
        SpawnPortal(portalId, data, pos, rot);
    }
    [Server]
    public void SpawnGrenade(uint owner, Vector3 weaponPos, Vector3 target)
    {
        GameObject bulletGo = Instantiate(GrenadePrefab, weaponPos, new Quaternion());
        NetworkServer.Spawn(bulletGo);
        bulletGo.GetComponent<GrenadeProjectile>().Init(owner, target);
    }
    [Command]
    public void CmdSpawnGrenade(uint owner, Vector3 weaponPos, Vector3 target)
    {
        SpawnGrenade(owner, weaponPos, target);
    }
    [Server]
    public void SpawnGaussRay(uint owner, Vector3 weaponPos, Quaternion rot, float length)
    {
        GameObject bulletGo = Instantiate(GaussRayPrefab, weaponPos, rot);
        bulletGo.transform.localScale = new Vector3(0.15f, 0.1f, length);
        bulletGo.transform.Translate(new Vector3(0, 0, length / 2));
        NetworkServer.Spawn(bulletGo);
        bulletGo.GetComponent<GaussRay>().Init(owner);
    }
    [Command]
    public void CmdSpawnGaussRay(uint owner, Vector3 weaponPos, Quaternion rot, float length)
    {
        SpawnGaussRay(owner, weaponPos, rot, length);
    }
    [Server]
    public void SpawnTurret(Player owner, Vector3 pos, Quaternion rot)
    {
        GameObject bulletGo = Instantiate(TurretPrefab, pos, rot);
        NetworkServer.Spawn(bulletGo);
        bulletGo.GetComponent<Turret>().Init(owner);
    }
    [Command]
    public void CmdSpawnTurret(Player owner, Vector3 pos, Quaternion rot)
    {
        SpawnTurret(owner, pos, rot);
    }
}