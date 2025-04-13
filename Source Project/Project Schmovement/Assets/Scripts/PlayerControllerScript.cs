using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerScript : MonoBehaviour
{
    public Camera MainCam;

    [SerializeField]
    private InputActionReference schmove;

    void Start()
    {
        if (MainCam == null)
        {
            MainCam = Camera.main;
        }
    }

    void Update()
    {
        Vector2 schmoveInput = schmove.action.ReadValue<Vector2>();
        transform.position += new Vector3(schmoveInput.x * MainCam.transform.right.x, 0, schmoveInput.y);
    }
}
