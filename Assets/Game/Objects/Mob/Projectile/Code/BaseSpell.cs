using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class BaseSpell : NetworkBehaviour
{
    protected float speed = 7f;
    protected float damage = 0;
    protected float despawnTime = 5f;
    protected Vector2 direction = Vector2.zero;
    protected Rigidbody2D rb;
    public void Init(Vector2 targetPosition, float _damage)
    {
        damage = _damage;
        direction = (targetPosition - (Vector2)transform.position).normalized;
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void FixedUpdate()
    {
        if (despawnTime > 0f)
        {
            despawnTime -= Time.fixedDeltaTime;
        }
        else
        {
            if (IsServer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<BaseEntety>().TakeDamage(damage, false);
            GetComponent<NetworkObject>().Despawn();
        }
        else if (other.CompareTag("Obstacle"))
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

}
