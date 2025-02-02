using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Weapon
{

    private void Update()
    {
        base.Update();
        if (owner.hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shot(timeBetweenShots, damage);
            }
        }
    }
}
