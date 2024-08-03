using UnityEngine;
using System.Collections.Generic;

public class PoolingSystem : MonoBehaviour
{
    [SerializeField] private GameObject _object;
    [SerializeField] private int _startSpawnCount = 5;
    [SerializeField] private int _spawnNewCount = 3;

    private List<PoolableObject> _inactiveObjects;
    private List<PoolableObject> _activeObjects;
    private GameObject _poolableObjectsSocket;

    public void Awake()
    {
        _inactiveObjects = new List<PoolableObject>();
        _activeObjects = new List<PoolableObject>();
        _poolableObjectsSocket = GameObject.Find("POOLABLE_OBJECTS_SOCKET");
        if (!_poolableObjectsSocket)
        {
            _poolableObjectsSocket =  new GameObject("POOLABLE_OBJECTS_SOCKET");
        }
        SpawnNew(_startSpawnCount);
    }

    private PoolableObject SpawnNew(int count)
    {
        for (int i = 0; i < count; i++)
        {
            PoolableObject newObject = Instantiate(_object, transform.position, transform.rotation)
                .GetComponent<PoolableObject>();
            newObject.PoolingSystem = this;
            newObject.transform.parent = _poolableObjectsSocket.transform;
            _inactiveObjects.Add(newObject);
            newObject.OnDespawn();
        }

        return _inactiveObjects[0];
    }

    public PoolableObject Spawn()
    {
        PoolableObject selectedObject;
        if (_inactiveObjects.Count > 0)
        {
            selectedObject = _inactiveObjects[0];
        }
        else
        {
            selectedObject = SpawnNew(_spawnNewCount);
        }
        _inactiveObjects.Remove(selectedObject);
        _activeObjects.Add(selectedObject);
        selectedObject.transform.parent = null;
        selectedObject.OnSpawn();
        return selectedObject;
    }

    public void Despawn(PoolableObject objectToDespawn)
    {
        if (!_activeObjects.Contains(objectToDespawn)) return;
        objectToDespawn.transform.parent = _poolableObjectsSocket.transform;
        objectToDespawn.OnDespawn();
        _inactiveObjects.Add(objectToDespawn);
        _activeObjects.Remove(objectToDespawn);
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