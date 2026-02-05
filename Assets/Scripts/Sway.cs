using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sway : MonoBehaviour
{
    #region Variables

    public float intensity;
    public float smoothness;
    public bool isMine;

    private Quaternion initRotation;

    #endregion

    #region MonoBehavior Callbacks

    private void Start()
    {
        initRotation = transform.localRotation;
}

    private void Update()
    {
        UpdateSway();
    }

    #endregion

    #region Private Methods

    private void UpdateSway()
    {
        // Controls
        float xMove = Input.GetAxis("Mouse X");
        float yMove = Input.GetAxis("Mouse Y");

        if (!isMine)
        {
            xMove = 0f;
            yMove = 0f;
        }

        // Calculate target rotation
        Quaternion xAdjustment = Quaternion.AngleAxis(intensity * (-1) * xMove, Vector3.up);
        Quaternion yAdjustment = Quaternion.AngleAxis(intensity * yMove, Vector3.right);
        Quaternion targetedRotation = initRotation * xAdjustment * yAdjustment;

        // Rotate towards target ratation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetedRotation, Time.deltaTime * smoothness);
    }

    #endregion
}
