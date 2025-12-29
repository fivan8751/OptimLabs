using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable 
{
    [SerializeField] float speed = 20f;
    [SerializeField] float lifetime = 3f;
    
    private float currentLifetime;

    public void OnSpawn() 
    {
        currentLifetime = lifetime;  
        Debug.Log("Bullet spawned");
    }

    public void OnDespawn() 
    {
    }

    void Update() 
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        currentLifetime -= Time.deltaTime;
        
        if (currentLifetime <= 0) 
        {
            PoolManager.Instance.Despawn(gameObject);
        }
    }
    
    
}
