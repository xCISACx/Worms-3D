using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Vector2 _mouseInput;
    [SerializeField] private float _sensitivity = .5f;
    public Transform FollowTarget;
    [SerializeField] private float _followDistance;
    [SerializeField] private float _yOffset;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        _mouseInput.x = Input.GetAxis("Mouse X") * _sensitivity;
        _mouseInput.y = Input.GetAxis("Mouse Y") * _sensitivity;
        
        transform.localRotation = Quaternion.Euler(-_mouseInput.y, _mouseInput.x, 0);
        _mouseInput.y = Mathf.Clamp(_mouseInput.y, -90, 90);
    }

    private void LateUpdate()
    {
        if (FollowTarget)
        {
            transform.position = new Vector3(FollowTarget.position.x, FollowTarget.position.y + _yOffset, FollowTarget.position.z - _followDistance);   
        }

    }
}
