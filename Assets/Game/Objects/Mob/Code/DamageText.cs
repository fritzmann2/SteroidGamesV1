using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private const float DISAPPEAR_TIMER_MAX = 1f;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount, bool isCriticalHit)
    {
        Debug.Log("Text set to " + damageAmount.ToString());
        textMesh.SetText(damageAmount.ToString());
        transform.position = new Vector3(transform.position.x, transform.position.y+ 1f, transform.position.z);
        if (isCriticalHit)
        {
            textMesh.fontSize = 3;
            textMesh.color = Color.red;
        }
        else
        {
            textMesh.fontSize = 2; 
            textMesh.color = Color.yellow;
        }

        disappearTimer = DISAPPEAR_TIMER_MAX;
        
        textColor = textMesh.color;
        
        moveVector = new Vector3(0.5f, 1f) *1f; 
    }

    private void Update()
    {
        transform.position += moveVector * Time.deltaTime;
        
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        {
            float increaseScaleAmount = 0.1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            float decreaseScaleAmount = 0.1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}