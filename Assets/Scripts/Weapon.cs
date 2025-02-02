using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Weapon : NetworkBehaviour
{
    public float lastShotTime = 0;
    public float timeBetweenShots = 0;
    public int damage = 1;
    public Player owner;
    public int countOfBullets;
    public float spread = 15;
    public int UsingAmmo = 0;
    public int MaxUsingAmmo = 10;
    public int Ammo = 300;
    public int MaxAmmo = 300;
    public float timeBeforeReload;
    public float reloadTime;

    private void Start()
    {
        lastShotTime = -timeBetweenShots;
        UsingAmmo = MaxUsingAmmo;
        Ammo = MaxAmmo;
        owner.AmmoText(UsingAmmo, MaxUsingAmmo, Ammo, MaxAmmo);
    }

    public void Shot(float timeBetweenShots, int damage)
    {
        if (Time.time - lastShotTime >= timeBetweenShots && UsingAmmo >= 1)
        {
            reloadTime = 0;
            Vector3 bulletSpawnPlace = transform.position;
            Vector3 rayDirection = (owner.gameObject.transform.position - bulletSpawnPlace).normalized;
            Ray ray = new Ray(bulletSpawnPlace, rayDirection);
            float distanceToPlayer = Vector3.Distance(bulletSpawnPlace, owner.gameObject.transform.position);
            Physics.Raycast(ray, out RaycastHit hitData, distanceToPlayer, 1);
            if (hitData.collider != null) return;
            UsingAmmo -= 1;
            owner.AmmoText(UsingAmmo, MaxUsingAmmo, Ammo, MaxAmmo);
            float iCount = (countOfBullets - 1) / 2;
            for (float i = -iCount; i <= iCount; i += 1)
            {
                Quaternion bulletDirection = transform.rotation * Quaternion.Euler(0, spread * i, 0);
                if (owner.isServer)
                    owner.SpawnBullet(owner.netId, damage, bulletSpawnPlace, bulletDirection);
                else
                    owner.CmdSpawnBullet(owner.netId, damage, bulletSpawnPlace, bulletDirection);
                lastShotTime = Time.time;
            }
        }
    }
    public void Update()
    {
        if (owner.hasAuthority)
        {
            owner.AmmoText(UsingAmmo, MaxUsingAmmo, Ammo, MaxAmmo);
            if (( Input.GetKeyDown(KeyCode.R) && Ammo != 0 && UsingAmmo != MaxUsingAmmo ) || ( Input.GetKeyDown(KeyCode.Mouse0) && UsingAmmo == 0 && Ammo != 0))
                reloadTime = Time.time + timeBeforeReload;
            if (reloadTime <= Time.time && reloadTime != 0)
            {
                Reload();
                reloadTime = 0;
            }
            else if (reloadTime != 0)
                owner.Reload((reloadTime - Time.time) / timeBeforeReload);
            else
                owner.Reload(0);
        }
    }
    public void Reload()
    {
        Ammo -= MaxUsingAmmo - UsingAmmo;
        UsingAmmo = MaxUsingAmmo;
        if (Ammo <= 0)
        {
            UsingAmmo += Ammo;
            Ammo = 0;
        }
        owner.AmmoText(UsingAmmo, MaxUsingAmmo, Ammo, MaxAmmo);
    }
}
