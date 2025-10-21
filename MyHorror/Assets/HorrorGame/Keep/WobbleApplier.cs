using System.Collections;
using UnityEngine;

public class WobbleApplier : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Local transform that will be wobbled (usually the camera head transform).")]
    public Transform Target;

    [Header("Wobble Settings")]
    [Tooltip("Multiplier for final positional offsets")]
    public float Intensity = 1f;

    [Tooltip("How quickly wobble fades out after duration ends")]
    public float DecaySpeed = 2f;

    [Tooltip("Whether wobble affects localPosition (true) or world position (false)")]
    public bool UseLocalPosition = true;

    // runtime
    private float amplitude;
    private float frequency;
    private float remainingDuration;
    private bool isPlaying;
    private Vector3 originalPosition;
    private float weight = 1f; // future use if you want blending

    private void Awake()
    {
        if (Target == null) Target = transform;
        originalPosition = UseLocalPosition ? Target.localPosition : Target.position;
    }

    private void OnEnable()
    {
        if (Target == null) Target = transform;
        originalPosition = UseLocalPosition ? Target.localPosition : Target.position;
    }

    private void Update()
    {
        if (Target == null) return;

        if (isPlaying)
        {
            // apply noise while there is remaining duration
            if (remainingDuration > 0f)
            {
                remainingDuration -= Time.deltaTime;
                float t = Time.time * frequency;

                float x = (Mathf.PerlinNoise(t, 0.0f) - 0.5f) * 2f;
                float y = (Mathf.PerlinNoise(0.0f, t) - 0.5f) * 2f;
                float z = (Mathf.PerlinNoise(t * 1.37f, t * 0.73f) - 0.5f) * 2f;

                Vector3 offset = new Vector3(x, y, z) * amplitude * Intensity * weight;

                if (UseLocalPosition) Target.localPosition = originalPosition + offset;
                else Target.position = originalPosition + offset;
            }
            else
            {
                // decay back to original position smoothly
                if (UseLocalPosition)
                {
                    Target.localPosition = Vector3.Lerp(Target.localPosition, originalPosition, Time.deltaTime * DecaySpeed);
                    if (Vector3.SqrMagnitude(Target.localPosition - originalPosition) < 0.00001f) StopWobble();
                }
                else
                {
                    Target.position = Vector3.Lerp(Target.position, originalPosition, Time.deltaTime * DecaySpeed);
                    if (Vector3.SqrMagnitude(Target.position - originalPosition) < 0.00001f) StopWobble();
                }
            }
        }
    }

    private void StopWobble()
    {
        isPlaying = false;
        remainingDuration = 0f;
        // ensure exact reset
        if (Target != null)
        {
            if (UseLocalPosition) Target.localPosition = originalPosition;
            else Target.position = originalPosition;
        }
    }

    /// <summary>
    /// Start wobble on the assigned target.
    /// amplitude: how large offsets are
    /// frequency: speed of noise
    /// duration: how long the wobble plays (after which it decays)
    /// </summary>
    public void ApplyWobble(float amplitude, float frequency, float duration, float optionalWeight = 1f)
    {
        if (Target == null)
        {
            Debug.LogWarning("[WobbleApplier] No target assigned — wobble skipped.");
            return;
        }

        this.amplitude = Mathf.Max(0f, amplitude);
        this.frequency = Mathf.Max(0.0001f, frequency);
        this.remainingDuration = Mathf.Max(0f, duration);
        this.weight = Mathf.Clamp01(optionalWeight);
        this.isPlaying = true;
        // ensure original position cached (in case transform moved)
        originalPosition = UseLocalPosition ? Target.localPosition : Target.position;
    }
}
