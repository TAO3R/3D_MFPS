using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLook : MonoBehaviourPunCallbacks
{
    #region Variables

    public static bool cursorLocked = true;

    public Transform player;
    public Transform cams;
    public Transform weapon;

    public float xSensitivity;
    public float ySensitivity;
    public float maxAngle;

    Quaternion camCenter;

    #endregion

    #region MonoBehavior Callbacks

    void Start()
    {
        camCenter = cams.localRotation;
    }

    void Update()
    {
        if (!photonView.IsMine) { return; }

        SetY();
        SetX();

        UpdateCursorLock();
    }

    #endregion

    #region Private Methods

    // To look up and down
    void SetY()
    {
        float input = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
        Quaternion adjustment = Quaternion.AngleAxis(input, -Vector3.right);
        Quaternion delta = cams.localRotation * adjustment;

        if (Quaternion.Angle(camCenter, delta) < maxAngle)
        {
            cams.localRotation = delta;
        }

        weapon.rotation = cams.rotation;
    }

    // look around
    void SetX()
    {
        float input = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        Quaternion adjustment = Quaternion.AngleAxis(input, Vector3.up);
        Quaternion delta = player.localRotation * adjustment;
        player.localRotation = delta;
    }

    void UpdateCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cursorLocked)
            {
                cursorLocked = false;
                Debug.Log("Cursor unlocked!");
            }
            else
            {
                cursorLocked = true;
                Debug.Log("Cursor locked!");
            }
        }

        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #endregion
}
