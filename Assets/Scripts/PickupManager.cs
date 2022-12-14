using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager Instance;
    
    [SerializeField] private List<Pickup> _pickupList;

    [SerializeField] private GameObject _pickupPrefab;

    private void Awake()
    {
        if (!Instance)
        { 
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnRandomPickup()
    {
        //Debug.Log("spawning random pickup");
        
        var xPos = UnityEngine.Random.Range(-27f, 27f);
        var zPos = UnityEngine.Random.Range(-27f, 27f);
        var yPos = 40f;

        var spawnPoint = new Vector3(xPos, yPos, zPos);

        var randomPickup = _pickupList[Random.Range(0, _pickupList.Count)];

        var newPickup = Instantiate(_pickupPrefab, spawnPoint, Quaternion.identity);
        
        newPickup.GetComponent<PickupBehaviour>().Init(randomPickup);
    }
}
