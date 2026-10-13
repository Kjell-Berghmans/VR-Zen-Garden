using UnityEngine;

// Attach to the rake tip transform. This will raycast toward the sand and
// call the RakePainter to stamp the heightmap at the hit UV location.
public class RakeTip : MonoBehaviour
{
    public LayerMask sandLayer;
    public float maxDistance = 0.5f; // Increased for VR leeway
    public RakePainter painter;

    [Header("Audio (Only enable on ONE RakeTip)")]
    public bool playRakeSound = false;
    public AudioClip rakeLoopSound;
    private AudioSource audioSource;

    // Minimum interval between stamps (to control performance)
    public float stampInterval = 0.02f;
    private float lastStampTime = 0f;

    private Vector2 lastUV;
    private bool wasTouching = false;

    void Start()
    {
        if (playRakeSound && rakeLoopSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = rakeLoopSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound so it comes from the rake!
        }
    }

    void Update()
    {
        if (painter == null)
        {
            Debug.LogError("RAKE ERROR: Painter is missing! Drag the RakePainter object into the Painter slot on the RakeTip.");
            return;
        }

        if (Time.time - lastStampTime < stampInterval) return;

        // Draw a debug line so you can see where the ray is pointing in the Scene view
        Debug.DrawRay(transform.position, -transform.up * maxDistance, Color.red);

        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, sandLayer))
        {
            Debug.DrawRay(transform.position, -transform.up * hit.distance, Color.green);

            if (hit.collider is not MeshCollider)
            {
                Debug.LogError("RAKE ERROR: Hit the sand, but it does not have a MeshCollider!");
            }

            Vector2 uv = hit.textureCoord;
            if (Time.frameCount % 30 == 0) Debug.Log($"RAKE SUCCESS: Hitting sand at UV {uv}! If you don't see tracks, your Material is missing the RenderTexture.");

            // MATH MAGIC: Interpolate to draw smooth continuous lines instead of dots!
            if (wasTouching)
            {
                float dist = Vector2.Distance(lastUV, uv);
                int steps = Mathf.Clamp(Mathf.CeilToInt(dist / 0.002f), 1, 30);
                for (int i = 1; i <= steps; i++)
                {
                    painter.StampAt(Vector2.Lerp(lastUV, uv, (float)i / steps));
                }
            }
            else
            {
                painter.StampAt(uv);
            }

            lastUV = uv;
            wasTouching = true;
            lastStampTime = Time.time;

            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            wasTouching = false; // We lifted the rake!

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }
}
