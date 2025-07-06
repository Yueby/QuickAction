using UnityEngine;

namespace Yueby.QuickActions
{
    public static class Util
    {
        public static Texture2D GrabScreenSwatch(Rect rect)
        {
            int width = (int)rect.width;
            int height = (int)rect.height;
            int x = (int)rect.x;
            int y = (int)rect.y;
            Vector2 position = new Vector2(x, y);

            Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position, width, height);

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            Color32[] pixels32 = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels32[i] = pixels[i];
            }
            texture.SetPixels32(pixels32);
            texture.Apply();
            texture.name = "QuickActionBackground";

            return texture;
        }
    }
}