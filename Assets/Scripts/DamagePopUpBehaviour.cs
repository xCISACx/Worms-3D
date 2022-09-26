using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DamagePopUpBehaviour : MonoBehaviour
{
    public CinemachineFreeLook mainCamera;
    public TMP_Text DamageText;
    public Vector3 randomDirection;
    public Vector3 initialPosition;

    private void Awake()
    {
        DamageText = GetComponentInChildren<TMP_Text>();
        randomDirection = new Vector3(Random.Range(2f, 5f), 8f, 0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(initialPosition, initialPosition + randomDirection, 2f * Time.deltaTime);
    }
}
