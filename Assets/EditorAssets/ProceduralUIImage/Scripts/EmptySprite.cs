using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
public static class EmptySprite
{
    static Sprite instance;

    ///<summary>
    /// Returns the instance of a (1 x 1) white Spprite
    /// </summary>	
    public static Sprite Get()
    {
        if (instance == null)
        {
            #if UNITY_EDITOR
            var spriteAsset = AssetDatabase.FindAssets("procedural_ui_image_default_sprite t:sprite").FirstOrDefault();
            if (spriteAsset != null)
            {
                instance = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(spriteAsset));
            }
            #endif
        }
        return instance;
    }

    public static bool IsEmptySprite(Sprite s)
    {
        if (Get() == s)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
