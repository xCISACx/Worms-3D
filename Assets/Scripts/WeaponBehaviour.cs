using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;
using Math = System.Math;

public class WeaponBehaviour : MonoBehaviour
{
    public GameObject user;
    public GameObject projectilePrefab;
    public GameObject weaponModel;
    public Transform weaponModelParent;
    [SerializeField] private Transform shootPoint;
    [SerializeField] public float shootForce;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int lineSegmentCount = 100;
    [SerializeField] private int linePointCount = 100;
    [SerializeField] private int showPercentage = 100;
    [SerializeField] private List<Vector3> linePoints = new List<Vector3>();
    [SerializeField] private int multiplier = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        lineRenderer.enabled = true;
        linePointCount = (int)(lineSegmentCount * (showPercentage / 100f));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var newProjectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Physics.IgnoreCollision(newProjectile.GetComponent<Collider>(), user.GetComponent<Collider>());
            newProjectile.GetComponent<Rigidbody>().AddForce(transform.forward * shootForce, ForceMode.Impulse);
            //Debug.Log("shoot");
            Destroy(newProjectile.gameObject, 10f);
        }
        
        //DrawProjection();
    }

    private void FixedUpdate()
    {
        
    }

    private void UpdateLineRenderer()
    {
        /*float timeBetweenPoints = 0.1f;
        
        lineRenderer.positionCount = (int) numTrajectoryPoints;
        List<Vector3> points = new List<Vector3>();
        Vector3 startingPosition = shootPoint.localPosition;
        Vector3 startingVelocity = ((shootPoint.forward * shootForce) / projectilePrefab.GetComponent<Rigidbody>().mass);
        
        for (float t = 0; t < numTrajectoryPoints; t += timeBetweenPoints)
        {
            Vector3 newPoint = startingPosition + t * startingVelocity;
            newPoint.y = startingPosition.y + startingVelocity.y * t + Physics.gravity.y / 2f * t * t;
            points.Add(newPoint);
            
            lineRenderer.positionCount = points.Count;
            
            /*if(Physics.OverlapSphere(newPoint, 2, CollidableLayers).Length > 0)
            {
                lineRenderer.positionCount = points.Count;
                break;
            }
        }

        lineRenderer.SetPositions(points.ToArray());*/
        
        //lineRenderer.transform.rotation = transform.localRotation;

        /*Vector3 velocity = (shootForce * transform.forward / projectilePrefab.GetComponent<Rigidbody>().mass) * Time.fixedDeltaTime;
        float flightDuration = (2 * velocity.y) / Physics.gravity.y;
        var angle = Vector3.Angle(transform.forward, user.transform.forward);

        Debug.Log(angle);
        float stepTime = flightDuration / lineSegmentCount;

        linePoints.Clear();
        linePoints.Add(shootPoint.localPosition);

        for (int i = 1; i < linePointCount; i++)
        {
            float stepTimePassed = stepTime * i;

            Vector3 MovementVector = new Vector3(
                velocity.x * stepTimePassed,
                velocity.y * stepTimePassed - 0.5f * Physics.gravity.y * stepTimePassed * stepTimePassed,
                velocity.z * stepTimePassed);

            Vector3 NewPointOnLine = -MovementVector + shootPoint.localPosition;
            
            linePoints.Add(NewPointOnLine);
        }

        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());*/
        
        //lineRenderer.transform.rotation = transform.localRotation;
        
        Vector3 startVelocity = shootForce * transform.forward / projectilePrefab.GetComponent<Rigidbody>().mass;
        float flightDuration = (2 * startVelocity.y) / Physics.gravity.y;
        float stepTime = flightDuration / linePointCount;
        lineRenderer.positionCount = Mathf.CeilToInt(linePointCount / stepTime) + 1;

        int i = 0;
        lineRenderer.SetPosition(i, shootPoint.localPosition);

        for (float time = 0; time < linePointCount; time += stepTime)
        {
            i++;
            Vector3 point = shootPoint.localPosition + time * startVelocity;
            point.y = shootPoint.localPosition.y + startVelocity.y * time * (Physics.gravity.y / 2f * time * time);
            lineRenderer.SetPosition(i, point);
        }

        lineRenderer.transform.rotation = transform.localRotation;
    }
    
    private void DrawProjection()
    {
        var TimeBetweenPoints = 0.1f;
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePointCount / TimeBetweenPoints) + 1;
        Vector3 startPosition = shootPoint.localPosition;
        Vector3 startVelocity = shootForce * shootPoint.transform.forward / projectilePrefab.GetComponent<Rigidbody>().mass;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < linePointCount; time += TimeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            lineRenderer.SetPosition(i, point);
        }
    }
}
