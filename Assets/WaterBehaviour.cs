using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBehaviour : MonoBehaviour
{
    [SerializeField] float timeElapsed;
    [SerializeField] float lerpDuration = 3f;
    [SerializeField] private float _offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator RaiseWaterLevel()
    {
        var currentPosition = transform.position;
        
        var newPosition = new Vector3(currentPosition.x, currentPosition.y + _offset, currentPosition.z);
        
        timeElapsed = 0;
        
        while (timeElapsed < lerpDuration)
        {
            transform.position = Vector3.Lerp(currentPosition, newPosition, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = newPosition;
    }
}
