using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public Transform orientation;

    public Transform firstPersonCameraPosition;
    public Transform thirdPersonCameraPosition;
    private bool isThirdPerson = false;

    public KeyCode camKey = KeyCode.F5;

    float xRotation;
    float yRotation;
    // Start is called before the first frame update
    void Start()
    {
        //locking cursor in the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

        //make sure you cant look at more than 90deg
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        if (Input.GetKeyDown(camKey)) {
            isThirdPerson = !isThirdPerson;
        }

        if (isThirdPerson) {
            OrbitCamera();
        } else {
            transform.SetPositionAndRotation(firstPersonCameraPosition.position, Quaternion.Euler(xRotation, yRotation, 0));
            //orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    private void OrbitCamera() {
        Vector3 direction = (thirdPersonCameraPosition.position - orientation.position).normalized;
        direction = Quaternion.Euler(xRotation, yRotation, 0) * Vector3.back;
        transform.position = orientation.position + direction * Vector3.Distance(orientation.position, thirdPersonCameraPosition.position);

        transform.LookAt(orientation);
    }
}
