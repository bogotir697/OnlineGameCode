using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Weapon
{
    public bool isAutomaicMode = true;
    public float timeBetweenSniperShots = 1.25f;
    public int sniperDamage = 5;
    public float autoModeCameraSize = 5;
    public float sniperModeCameraSize = 8;
    public int sniperModeMaxUsingAmmo;
    public float sniperModeTimeBeforeReload;
    public int autoModeMaxUsingAmmo;
    public float autoModeTimeBeforeReload;

    public float timeBetweenModeChanging = 0.5f;
    float lastTimeModeChanged = 0;
    private void Update()
    {
        base.Update();
        if (owner.hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (timeBetweenModeChanging + lastTimeModeChanged <= Time.time)
                {
                    isAutomaicMode = !isAutomaicMode;
                    if (isAutomaicMode)
                    {
                        MaxUsingAmmo = autoModeMaxUsingAmmo;
                        timeBeforeReload = autoModeTimeBeforeReload;
                    }
                    else
                    {
                        MaxUsingAmmo = sniperModeMaxUsingAmmo;
                        timeBeforeReload = sniperModeTimeBeforeReload;
                    }
                    Ammo -= MaxUsingAmmo - UsingAmmo;
                    UsingAmmo = MaxUsingAmmo;
                    lastTimeModeChanged = Time.time;
                }
            }
            if (isAutomaicMode)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                    Shot(timeBetweenShots, damage);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                    Shot(timeBetweenSniperShots, sniperDamage);
            }
            
        }
    }
}
