using UnityEngine;

public class destroyobj : MonoBehaviour
{
    private Collider2D cd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cd = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckBreak(other.gameObject);
    }
    private void CheckBreak(GameObject obj)
    {
        // Wir fragen: Hat das Objekt, das uns berührt hat, den Tag "Player"?
        if (obj.CompareTag("Player"))
        {
            if (obj.GetComponent<movement>().dashtimer > obj.GetComponent<movement>().dashcooldown - 0.2f)
            {
                // Zerstöre dieses Objekt
                Destroy(gameObject);
            }
        }
    }
}
