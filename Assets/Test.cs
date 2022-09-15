using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] public GameObject user;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootForce = 100;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int numTrajectoryPoints = 25;
    [SerializeField] private List<Vector3> linePoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        lineRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = ((shootForce * transform.forward) / projectilePrefab.GetComponent<Rigidbody>().mass) *
                           Time.fixedDeltaTime;
        float flightDuration = (2 * velocity.y) / Physics.gravity.y;
        float stepTime = flightDuration / numTrajectoryPoints;
        Debug.Log(stepTime);

        linePoints.Clear();

        for (int i = 0; i < numTrajectoryPoints; i++)
        {
            float stepTimePassed = stepTime * i;
            //Debug.Log(stepTimePassed);

            Vector3 MovementVector = new Vector3(
                velocity.x * stepTimePassed,
                velocity.y * stepTimePassed - 0.5f * Physics.gravity.y * stepTimePassed * stepTimePassed,
                velocity.z * stepTimePassed);

            linePoints.Add(-MovementVector + shootPoint.position);
        }

        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());

        //lineRenderer.transform.rotation = transform.localRotation;
    }
}
