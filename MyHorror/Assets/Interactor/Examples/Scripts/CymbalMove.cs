using UnityEngine;

public class CymbalMove : MonoBehaviour
{
    [Tooltip("Duration of the full damping motion (seconds)")]
    public float duration = 0.5f;

    [Tooltip("Maximum initial rotation angle (degrees)")]
    public float intensity = 30f;

    [Tooltip("Oscillation frequency")]
    public float frequency = 6f;

    private Quaternion restRotation;
    private float timer = 0f;
    private bool animating = false;

    private void Start()
    {
        restRotation = transform.localRotation;
    }

    private void Update()
    {
        if (animating)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (t >= 1f)
            {
                animating = false;
                transform.localRotation = restRotation;
            }
            else
            {
                float decay = Mathf.Exp(-3f * t);
                float angle = intensity * decay * Mathf.Sin(2f * Mathf.PI * frequency * t);
                transform.localRotation = restRotation * Quaternion.Euler(angle, 0f, 0f);
            }
        }
    }

    public void TriggerCymbal()
    {
        timer = 0f;
        animating = true;
    }
}
