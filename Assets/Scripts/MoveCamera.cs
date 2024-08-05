using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform firstPersonCameraPosition;
    public Transform thirdPersonCameraPosition;
    private bool isThirdPerson = false;

    public KeyCode camKey = KeyCode.F5;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = firstPersonCameraPosition.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(camKey))
        {
            isThirdPerson = !isThirdPerson;
        }

        if (isThirdPerson)
        {
            transform.position = thirdPersonCameraPosition.position;
        }
        else
        {
            transform.position = firstPersonCameraPosition.position;
        }
    }

    
}
