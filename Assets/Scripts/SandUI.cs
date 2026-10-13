using UnityEngine;

// Simple bridge script to hook up UI buttons to the RakePainter.
public class SandUI : MonoBehaviour
{
    public RakePainter painter;

    [Tooltip("Value to clear heightmap to (0..1), default 0 = flat")]
    [Range(0f, 1f)]
    public float clearValue = 0f;

    public void ClearSand()
    {
        if (painter == null)
        {
            Debug.LogWarning("SandUI: painter not assigned");
            return;
        }
        painter.Clear(clearValue);
    }
}
