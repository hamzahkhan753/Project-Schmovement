using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControls : MonoBehaviour
{
    [Header("External Asset References")]
    public GameObject PlayerToLookAt;
    public InputActionReference LookReference;

    [Header("Camera Position Values")]
    public float distance = 6f;
    public float height = 4f;

    [Header("Camera Control Values")]
    public CursorLockMode CursorLock;
    public float sensitivity = 5f;
    public float camMin = -90;
    public float camMax = 90;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLock;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            switch (CursorLock)
            {
                case CursorLockMode.Locked:
                    Cursor.lockState = CursorLockMode.None;
                    break;
                case CursorLockMode.None:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
            }
            CursorLock = Cursor.lockState;
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLock;
        }

            transform.position = PlayerToLookAt.transform.position - (transform.forward * distance) + (transform.up * height);
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            transform.RotateAround(PlayerToLookAt.transform.position, Vector3.up, LookReference.action.ReadValue<Vector2>().x * sensitivity);
            transform.RotateAround(PlayerToLookAt.transform.position, transform.right, -LookReference.action.ReadValue<Vector2>().y * sensitivity);
                
            transform.eulerAngles = new Vector3(ClampAngle(transform.eulerAngles.x, camMin, camMax), 
                                                transform.eulerAngles.y, 0);
        }
    }

    private float ClampAngle(float angle, float from, float to)
    { 
        // accepts e.g. -80, 80, but don't use +/-90, as it kinda loops back around, preventing the code from clamping correctly
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }
}
