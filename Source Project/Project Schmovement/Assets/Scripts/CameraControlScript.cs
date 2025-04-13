using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlScript : MonoBehaviour
{

    [SerializeField]
    private InputActionReference look;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;

            Vector2 lookInput = look.action.ReadValue<Vector2>();

            transform.RotateAround(transform.parent.position, Vector3.up, lookInput.x);
            transform.RotateAround(transform.parent.position, transform.right, -lookInput.y);
        }
    }
}
