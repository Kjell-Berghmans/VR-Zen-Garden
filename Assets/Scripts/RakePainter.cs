using UnityEngine;

// Paints stamps into a RenderTexture heightmap using a stamp material.
public class RakePainter : MonoBehaviour
{
    [Tooltip("RenderTexture used as the sand heightmap (RGBA, e.g. RFloat or ARGB32)")]
    public RenderTexture heightmap;

    [Tooltip("Material using the Stamp shader. Should take _BrushTex and _Center (x,y,size,strength)")]
    public Material stampMaterial;

    [Tooltip("Brush texture (white circle recommended)")]
    public Texture2D brushTexture;

    [Range(0.001f, 0.5f)]
    public float brushSize = 0.05f;

    [Range(-1f, 1f)]
    public float strength = 0.2f;

    void Start()
    {
        if (heightmap == null)
            Debug.LogWarning("RakePainter: heightmap not assigned");
        if (stampMaterial == null)
            Debug.LogWarning("RakePainter: stampMaterial not assigned");

        // Automatically clear old rake marks when the game starts!
        Clear(0f);
    }

    // Fill the heightmap with a neutral value (0..1). Useful for Clear/reset.
    public void Clear(float value = 0f)
    {
        if (heightmap == null) return;

        RenderTexture active = RenderTexture.active;
        RenderTexture.active = heightmap;
        GL.Clear(false, true, new Color(value, value, value, 1f));
        RenderTexture.active = active;
    }

    // Call this with UV (0..1) coords on the sand mesh to paint
    public void StampAt(Vector2 uv)
    {
        if (heightmap == null || stampMaterial == null || brushTexture == null)
            return;

        int w = heightmap.width;
        int h = heightmap.height;

        var tmp = RenderTexture.GetTemporary(w, h, 0, heightmap.graphicsFormat);
        Graphics.Blit(heightmap, tmp);

        stampMaterial.SetTexture("_BrushTex", brushTexture);
        stampMaterial.SetVector("_Center", new Vector4(uv.x, uv.y, brushSize, strength));

        Graphics.Blit(tmp, heightmap, stampMaterial);

        RenderTexture.ReleaseTemporary(tmp);
    }
}
