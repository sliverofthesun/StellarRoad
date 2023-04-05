using UnityEngine;

public class PulsatingCircle : MonoBehaviour
{
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float pulseSpeed = 1f;

    private Vector3 initialScale;
    private float time;

    private void Start()
    {
        initialScale = transform.localScale;
    }

    private void Update()
    {
        time += Time.deltaTime * pulseSpeed;
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(time) + 1) / 2);
        transform.localScale = initialScale * scale;
    }
}
