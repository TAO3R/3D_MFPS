using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

public class PlayerWeapon : MonoBehaviourPunCallbacks
{
    #region Variables

    public Gun[] loadOut;
    public Transform weaponParent;
    public GameObject bulletHolePrefab;
    public LayerMask canBeShot;

    private float currentCooldown;
    private int currentIndex;
    private GameObject currentWeapon;
    private bool isReloading;

    #endregion

    #region MonoBehavior Callbacks

    private void Start()
    {
        foreach (Gun a in loadOut) { a.Initialize(); }
        Equip(0);
    }

    void Update()
    {
        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) { photonView.RPC("Equip", RpcTarget.All, 0); }

        // If has a gun on hand
        if (currentWeapon != null) 
        { 
            if (photonView.IsMine)
            {
                Aim(Input.GetMouseButton(1));

                if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
                {
                    if (loadOut[currentIndex].FireBullet()) { photonView.RPC("Shoot", RpcTarget.All); }
                    else { StartCoroutine(Reload(loadOut[currentIndex].reload)); }
                }

                if (Input.GetKeyDown(KeyCode.R)) { StartCoroutine(Reload(loadOut[currentIndex].reload)); }

                // Cooldown
                if (currentCooldown > 0) { currentCooldown -= Time.deltaTime; }
            }

            // Weapon position elasticity
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
        }
    }

    #endregion

    #region Private Methods

    IEnumerator Reload(float p_wait)
    {
        isReloading = true;
        currentWeapon.SetActive(false);

        yield return new WaitForSeconds(p_wait);

        loadOut[currentIndex].Reload();
        currentWeapon.SetActive(true);
        isReloading = false;
    }

    [PunRPC]
    void Equip(int p_ind)
    {   
        if (currentWeapon != null) 
        {
            if (isReloading) { StopCoroutine("Reload"); }
            Destroy(currentWeapon); 
        }

        currentIndex = p_ind;

        GameObject newWeapon = Instantiate(loadOut[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localEulerAngles = Vector3.zero;
        newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

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

    [PunRPC]
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

            if (photonView.IsMine)
            {
                // Shooting other players on network
                if (hit.collider.gameObject.layer == 11)
                {
                    hit.collider.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadOut[currentIndex].damage);
                }
            }
        }

        // Gun fx
        currentWeapon.transform.Rotate(-loadOut[currentIndex].recoil, 0, 0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadOut[currentIndex].kickback;

        // Cooldown
        currentCooldown = 60f / loadOut[currentIndex].fireRate;
    }

    [PunRPC]
    private void TakeDamage(int p_damage)
    {
        GetComponent<Player>().TakeDamage(p_damage);
    }

    #endregion

    #region Public Methods

    public void RefreshAmmo(Text p_text)
    {
        int t_clip = loadOut[currentIndex].GetClip();
        int t_stash = loadOut[currentIndex].GetStash();

        p_text.text = t_clip.ToString("D2") + " / " + t_stash.ToString("D2"); 
    }

    #endregion
}
