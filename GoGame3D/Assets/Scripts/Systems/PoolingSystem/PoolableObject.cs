using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    public PoolingSystem PoolingSystem { get; set; }
    [HideInInspector] public bool destroyOnDespawn = false;

    public virtual void OnSpawn()
    {
        gameObject.SetActive(true);
    }
    
    public virtual void OnDespawn()
    {
        if (!destroyOnDespawn)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Despawn()
    {
        if (!destroyOnDespawn && PoolingSystem != null)
        {
            PoolingSystem.Despawn(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}