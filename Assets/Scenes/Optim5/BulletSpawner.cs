using UnityEngine;

public class BulletSpawner : MonoBehaviour 
{
    [SerializeField, Range(0.05f, 1f)] float fireRate = 0.1f;
    float nextFire;
    
    void Update() 
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFire) 
        {
            nextFire = Time.time + fireRate;
            SpawnBullet();
        }
    }
    
    void SpawnBullet() 
    {
        GameObject bullet = PoolManager.Instance.Spawn("Bullet", 
            transform.position,          
            transform.rotation);         
    }
}
