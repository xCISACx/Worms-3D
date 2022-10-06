using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBehaviour : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Vector3[] vertices;
    [SerializeField] private Vector3[] newVertices;
    [SerializeField] private AnimationCurve curve;
    
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        newVertices = mesh.vertices;
    }

    // Update is called once per frame
    void Update()
    {
        /*Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] += normals[i] * Mathf.Sin(Time.time / 5f);
        }

        mesh.vertices = vertices;*/
    }

    public void DestroyTerrain(Vector3 origin, float radius)
    {
        for (int i = 0; i < newVertices.Length; i++)
        {
            origin = new Vector3(origin.x, newVertices[i].y, origin.z);
            float hitDistance = Vector3.Distance(newVertices[i], origin);

            //var fallOff = curve.Evaluate(hitDistance1);
            

            if (hitDistance < radius)
            {
                //Debug.Log("less than radius");
                newVertices[i] += Vector3.down * 1f;
            }
        }

        RegenerateMesh();
    }

    void RegenerateMesh()
    {
        mesh.vertices = newVertices;
        //mesh.UploadMeshData(true);
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateNormals();
    }
}
