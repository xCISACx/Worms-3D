using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Vector2 MouseInput;
    [SerializeField] private float sensitivity = .5f;
    public Transform followTarget;
    [SerializeField] private float followDistance;
    [SerializeField] private float yOffset;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(followTarget.position.x, followTarget.position.y + yOffset, followTarget.position.z - followDistance);
        MouseInput.x = Input.GetAxis("Mouse X") * sensitivity;
        MouseInput.y = Input.GetAxis("Mouse Y") * sensitivity;
        transform.localRotation = Quaternion.Euler(-MouseInput.y, MouseInput.x, 0);
        MouseInput.y = Mathf.Clamp(MouseInput.y, -90, 90);

    }
}
