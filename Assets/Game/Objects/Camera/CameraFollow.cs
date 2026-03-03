using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("Movement Settings")]
    public float smoothTime = 0.1f; 
    
    [Header("Look Ahead Settings")]
    public float lookAheadDistance = 1f; 
    public float lookAheadSpeed = 1f;  
    
    [Header("Zoom Settings")]
    public float zoomSmoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private float currentLookAheadX;
    
    private Camera cam;
    private float originalOrthoSize;
    private float targetOrthoSize;
    private float zoomVelocity = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        if (cam != null)
        {
            originalOrthoSize = cam.orthographicSize;
            targetOrthoSize = originalOrthoSize;
        }
    }

    public void SetZoom(float targetSize)
    {
        offset = new Vector3(0, 2, -10);
        targetOrthoSize = targetSize;
    }

    public void ResetZoom()
    {
        offset = new Vector3(0, 0, -10);
        targetOrthoSize = originalOrthoSize;
    }

    void LateUpdate()
    {
        if (cam != null)
        {
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetOrthoSize, ref zoomVelocity, zoomSmoothTime);
        }

        if (target == null) return;
        
        float facingDirection = Mathf.Sign(target.localScale.x);
        float targetLookAheadX = lookAheadDistance * facingDirection;
        
        currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, lookAheadSpeed * Time.deltaTime);
        
        Vector3 desiredPosition = target.position + offset + new Vector3(currentLookAheadX, 0, 0);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}