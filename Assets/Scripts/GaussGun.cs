using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GaussGun : Weapon
{
    public float explosionForce;
    private void Update()
    {
        base.Update();
        if (owner.hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shot(timeBetweenShots);
            }
        }
    }
    public void Shot(float timeBetweenShots)
    {
        if (Time.time - lastShotTime >= timeBetweenShots && UsingAmmo >= 1)
        {
            Vector3 bulletSpawnPlace = transform.position;
            Vector3 rayDirection = (owner.gameObject.transform.position - bulletSpawnPlace).normalized;
            Ray ray = new Ray(bulletSpawnPlace, rayDirection);
            float distanceToPlayer = Vector3.Distance(bulletSpawnPlace, owner.gameObject.transform.position);
            Physics.Raycast(ray, out RaycastHit hitData, distanceToPlayer, 1);
            if (hitData.collider != null) return;
            UsingAmmo--;
            owner.AmmoText(UsingAmmo, MaxUsingAmmo, Ammo, MaxAmmo);
            Vector3 pos = Input.mousePosition;
            pos.z = 10f;
            pos = Camera.main.ScreenToWorldPoint(pos);
            rayDirection = (pos - bulletSpawnPlace).normalized;
            Ray gaussRay = new Ray(bulletSpawnPlace, rayDirection);
            float distance = Vector3.Distance(pos, bulletSpawnPlace);
            Physics.Raycast(gaussRay, out RaycastHit hitDataGaussRay, distance, 1);
            float length;
            if (hitDataGaussRay.collider != null)
                length = hitDataGaussRay.distance;
            else
                length = Vector3.Distance(pos, bulletSpawnPlace);
            if (owner.isServer)
                owner.SpawnGaussRay(owner.netId, bulletSpawnPlace, transform.rotation, length);
            else
                owner.CmdSpawnGaussRay(owner.netId, bulletSpawnPlace, transform.rotation, length);
            owner.GetComponent<Rigidbody>().AddExplosionForce(1000 * explosionForce, bulletSpawnPlace, distanceToPlayer);
            lastShotTime = Time.time;
        }
    }
}
