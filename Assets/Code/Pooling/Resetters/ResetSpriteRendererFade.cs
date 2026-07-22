using UnityEngine;

public class ResetSpriteRendererFade : MonoBehaviour, IPoolReset
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Color resetColor;
    
    public void ResetForReuse()
    {
        spriteRenderer.color = resetColor;
        
    }
}
