using UnityEngine;
using System.Collections.Generic;

public class PoolingSystem : MonoBehaviour
{
    [SerializeField] private GameObject _object;
    [SerializeField] private int _startSpawnCount = 5;
    [SerializeField] private int _spawnNewCount = 3;

    private Queue<PoolableObject> _inactiveObjects;
    private HashSet<PoolableObject> _activeObjects;
    private GameObject _poolableObjectsSocket;

    public void Awake()
    {
        _inactiveObjects = new Queue<PoolableObject>();
        _activeObjects = new HashSet<PoolableObject>();

        _poolableObjectsSocket = GameObject.Find("POOLABLE_OBJECTS_SOCKET");
        if (!_poolableObjectsSocket)
        {
            _poolableObjectsSocket = new GameObject("POOLABLE_OBJECTS_SOCKET");
        }

        SpawnNew(_startSpawnCount);
    }

    private void SpawnNew(int count)
    {
        for (int i = 0; i < count; i++)
        {
            PoolableObject newObject = Instantiate(_object, transform.position, transform.rotation)
                .GetComponent<PoolableObject>();

            newObject.PoolingSystem = this;
            newObject.transform.parent = _poolableObjectsSocket.transform;

            newObject.OnDespawn();
            _inactiveObjects.Enqueue(newObject);
        }
    }
    
    public PoolableObject Spawn()
    {
        if (_inactiveObjects.Count == 0)
        {
            SpawnNew(_spawnNewCount);
        }

        PoolableObject selectedObject = _inactiveObjects.Dequeue();
        _activeObjects.Add(selectedObject);

        selectedObject.transform.parent = null;
        selectedObject.OnSpawn();

        return selectedObject;
    }
    public void Despawn(PoolableObject objectToDespawn)
    {
        if (!_activeObjects.Contains(objectToDespawn)) return;

        _activeObjects.Remove(objectToDespawn);
        objectToDespawn.transform.parent = _poolableObjectsSocket.transform;
        objectToDespawn.OnDespawn();

        _inactiveObjects.Enqueue(objectToDespawn);
    }

    private void OnDestroy()
    {
        foreach (var inactiveObject in _inactiveObjects)
        {
            if (inactiveObject != null)
            {
                Destroy(inactiveObject.gameObject);
            }
        }

        foreach (var activeObject in _activeObjects)
        {
            if (activeObject != null)
            {
                activeObject.destroyOnDespawn = true;
            }
        }
    }
}