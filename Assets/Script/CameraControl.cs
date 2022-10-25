using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float maxX = 60f;

    public float minX = -60f;

    public float sensitivity;

    public Camera cam;
    PlayerControl move;

    float rotaX = 0f;

    float rotaY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        move = GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        Rotation();
    }

    public void Rotation()
    {
        rotaX += Input.GetAxis("Mouse Y") * sensitivity;
        rotaY += Input.GetAxis("Mouse X") * sensitivity;

        rotaX = Mathf.Clamp(rotaX, minX, maxX);

        transform.localEulerAngles = new Vector3(0, rotaY, 0);
        cam.transform.localEulerAngles = new Vector3(-rotaX, 0, move.tilt);
    }
}
