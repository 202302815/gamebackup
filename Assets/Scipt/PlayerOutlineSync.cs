using UnityEngine;

[DefaultExecutionOrder(100)]
public class PlayerOutlineSync : MonoBehaviour
{
    public SpriteRenderer playerRenderer;
    public SpriteRenderer outlineRenderer;

    [Min(0.1f)]
    public float outlineWidth = 1f;

    private MaterialPropertyBlock properties;
    private Sprite previousSprite;
    private Vector4 spriteUVBounds;

    void Awake()
    {
        properties = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (playerRenderer == null || outlineRenderer == null)
            return;

        Sprite sprite = playerRenderer.sprite;

        outlineRenderer.enabled =
            playerRenderer.enabled && sprite != null;

        if (sprite == null)
            return;

        outlineRenderer.sprite = sprite;
        outlineRenderer.flipX = playerRenderer.flipX;
        outlineRenderer.flipY = playerRenderer.flipY;
        outlineRenderer.spriteSortPoint =
            playerRenderer.spriteSortPoint;

        // Recalculate bounds only when the animation frame changes.
        if (sprite != previousSprite)
        {
            Vector2[] uv = sprite.uv;

            Vector2 minimum = uv[0];
            Vector2 maximum = uv[0];

            foreach (Vector2 point in uv)
            {
                minimum = Vector2.Min(minimum, point);
                maximum = Vector2.Max(maximum, point);
            }

            spriteUVBounds = new Vector4(
                minimum.x, minimum.y,
                maximum.x, maximum.y);

            previousSprite = sprite;
        }

        outlineRenderer.GetPropertyBlock(properties);

        properties.SetVector("_UVRect", spriteUVBounds);

        // Match thickness across low-resolution boy sprites
        // and higher-resolution girl/scooter sprites.
        float pixels = Mathf.Max(
            1f, outlineWidth * sprite.pixelsPerUnit / 16f);

        properties.SetFloat("_OutlinePixels", pixels);

        outlineRenderer.SetPropertyBlock(properties);
    }

    void OnDisable()
    {
        if (outlineRenderer != null)
            outlineRenderer.enabled = false;
    }
}