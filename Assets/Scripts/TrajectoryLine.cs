using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [SerializeField] private int _stepCount = 10;
    [SerializeField] private LineRenderer _lineRenderer;

    public void DrawStraightTrajectory(Vector3 force, Vector3 initialPositon, Transform shootPoint)
    {
        _lineRenderer.SetPosition(0, initialPositon);
        _lineRenderer.SetPosition(1, initialPositon + shootPoint.forward * force.x);
    }

    public void DrawCurvedTrajectory(Vector3 force, Vector3 initialPosition)
    {
        float projectileMass = GetComponent<WeaponBehaviour>().ProjectilePrefab.GetComponent<Rigidbody>().mass;

        Vector3 velocity = (force / projectileMass) * Time.fixedDeltaTime;
        float flightDuration = (2 * velocity.y) / -Physics.gravity.y;
        float stepTime = flightDuration / (float) _stepCount;
        
        _lineRenderer.positionCount = _stepCount;
        
        for (int i = 0; i < _stepCount; i++)
        {
            float timePassed = stepTime * i;
            float height = velocity.y * timePassed - (0.5f * Physics.gravity.y * timePassed * timePassed);
            Vector3 curvePoint = initialPosition + new Vector3(velocity.x * timePassed, height, velocity.z * timePassed);
            _lineRenderer.SetPosition(i, curvePoint);
        }
    }
}
