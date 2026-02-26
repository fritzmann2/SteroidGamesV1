using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothTime = 0.1f; 
    
    [Header("Look Ahead Settings")]
    public float lookAheadDistance = 2f; 
    public float lookAheadSpeed = 2f;  
    
    private Vector3 velocity = Vector3.zero;
    private float currentLookAheadX;

    void LateUpdate()
    {
        if (target == null) return;

        float facingDirection = Mathf.Sign(target.localScale.x);

        float targetLookAheadX = lookAheadDistance * facingDirection;

        currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, lookAheadSpeed * Time.deltaTime);

        Vector3 desiredPosition = target.position + offset + new Vector3(currentLookAheadX, 0, 0);
        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}