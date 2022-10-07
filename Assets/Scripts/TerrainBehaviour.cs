using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBehaviour : MonoBehaviour
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Vector3[] _vertices;
    [SerializeField] private Vector3[] _newVertices;
    [SerializeField] private AnimationCurve _curve;
    
    // Start is called before the first frame update
    void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _vertices = _mesh.vertices;
        _newVertices = _mesh.vertices;
    }

    public void DestroyTerrain(Vector3 origin, float radius)
    {
        for (int i = 0; i < _newVertices.Length; i++)
        {
            origin = new Vector3(origin.x, _newVertices[i].y, origin.z);
            float hitDistance = Vector3.Distance(_newVertices[i], origin);

            //var fallOff = curve.Evaluate(hitDistance1);
            

            if (hitDistance < radius)
            {
                //Debug.Log("less than radius");
                _newVertices[i] += Vector3.down * 1f;
            }
        }

        RegenerateMesh();
    }

    void RegenerateMesh()
    {
        _mesh.vertices = _newVertices;
        //mesh.UploadMeshData(true);
        GetComponent<MeshCollider>().sharedMesh = _mesh;
        _mesh.RecalculateNormals();
    }
}
