using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Variables

    public Gun[] loadOut;
    public Transform weaponParent;
    public GameObject bulletHolePrefab;
    public LayerMask canBeShot;

    private float currentCooldown;
    private int currentIndex;
    private GameObject currentWeapon;

    #endregion

    #region MonoBehavior Callbacks

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Equip(0); }

        // If has a gun on hand
        if (currentWeapon != null) 
        { 
            Aim(Input.GetMouseButton(1));

            if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
            {
                Shoot();
            }

            // Weapon position elasticity
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
        }
    }

    private void FixedUpdate()
    {
        // Cooldown
        if (currentCooldown > 0) { currentCooldown -= Time.deltaTime; }
    }

    #endregion

    #region Private Methods

    void Equip(int p_ind)
    {
        if (currentWeapon != null) { Destroy(currentWeapon); }

        currentIndex = p_ind;

        GameObject newWeapon = Instantiate(loadOut[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localEulerAngles = Vector3.zero;

        currentWeapon = newWeapon;
    }

    void Aim(bool isAiming)
    {
        Transform anchor = currentWeapon.transform.Find("Anchor");
        Transform states_ADS = currentWeapon.transform.Find("States/ADS");
        Transform states_Hip = currentWeapon.transform.Find("States/Hip");

        if (isAiming)
        {
            // ADS
            anchor.position = Vector3.Lerp(anchor.position, states_ADS.position, Time.deltaTime * loadOut[currentIndex].aimSpeed);
        }
        else
        {
            // Hip
            anchor.position = Vector3.Lerp(anchor.position, states_Hip.position, Time.deltaTime * loadOut[currentIndex].aimSpeed);
        }
    }

    void Shoot()
    {
        Transform spaw = transform.Find("Cameras/Normal Camera");

        // Bloom
        Vector3 bloom = spaw.position + spaw.forward * 1000f;
        bloom += Random.Range(-loadOut[currentIndex].bloom, loadOut[currentIndex].bloom) * spaw.up;
        bloom += Random.Range(-loadOut[currentIndex].bloom, loadOut[currentIndex].bloom) * spaw.right;
        bloom -= spaw.position;
        bloom.Normalize();

        // Raycast
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(spaw.position, bloom, out hit, 1000f, canBeShot))
        {
            GameObject newHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
            newHole.transform.LookAt(hit.point + hit.normal);
            Destroy(newHole, 5f);
        }

        // Gun fx
        currentWeapon.transform.Rotate(-loadOut[currentIndex].recoil, 0, 0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadOut[currentIndex].kickback;

        // Cooldown
        currentCooldown = 60f / loadOut[currentIndex].fireRate;
    }

    #endregion
}
