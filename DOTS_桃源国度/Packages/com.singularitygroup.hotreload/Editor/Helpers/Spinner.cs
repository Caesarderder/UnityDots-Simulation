using System;
using UnityEngine;

namespace SingularityGroup.HotReload.Editor {
    internal class Spinner {
        internal static string SpinnerIconPath => HotReloadWindowStyles.IsDarkMode ? "icon_loading_star_dark_mode_96": "icon_loading_star_light_mode_96";
        internal static Texture2D _spinnerTextureLight;
        internal static Texture2D _spinnerTextureDark;
        internal static Texture2D spinnerTextureLight => _spinnerTextureLight ? _spinnerTextureLight : _spinnerTextureLight = Resources.Load<Texture2D>("icon_loading_star_light_mode_96");
        internal static Texture2D spinnerTextureDark => _spinnerTextureDark ? _spinnerTextureDark : _spinnerTextureDark = Resources.Load<Texture2D>("icon_loading_star_dark_mode_96");
        internal static Texture2D spinnerTexture => HotReloadWindowStyles.IsDarkMode ? spinnerTextureDark : spinnerTextureLight;
        private Texture2D _rotatedTextureLight;
        private Texture2D _rotatedTextureDark;
        private Texture2D rotatedTextureLight => _rotatedTextureLight ? _rotatedTextureLight : _rotatedTextureLight = GetCopy(spinnerTextureLight);
        private Texture2D rotatedTextureDark => _rotatedTextureDark ? _rotatedTextureDark : _rotatedTextureDark = GetCopy(spinnerTextureDark);
        internal Texture2D rotatedTexture => HotReloadWindowStyles.IsDarkMode ? rotatedTextureDark : rotatedTextureLight;
        
        private float _rotationAngle;
        private DateTime _lastRotation;
        private int _rotationPeriod;
        
        internal Spinner(int rotationPeriodInMilliseconds) {
            _rotationPeriod = rotationPeriodInMilliseconds;
        }
        
        internal Texture2D GetIcon() {
            if (DateTime.UtcNow - _lastRotation > TimeSpan.FromMilliseconds(_rotationPeriod)) {
                _lastRotation = DateTime.UtcNow;
                _rotationAngle += 45;
                if (_rotationAngle >= 360f) 
                    _rotationAngle -= 360f;
                return RotateImage(spinnerTexture, _rotationAngle);
            }
            return rotatedTexture;
        }
        
        private Texture2D RotateImage(Texture2D originalTexture, float angle) {
            int w = originalTexture.width;
            int h = originalTexture.height;
            
            int x, y;
            float centerX = w / 2f;
            float centerY = h / 2f;

            for (x = 0; x < w; x++) {
                for (y = 0; y < h; y++) {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float oldAngle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    float newAngle = oldAngle + angle;

                    float newX = centerX + distance * Mathf.Cos(newAngle * Mathf.Deg2Rad);
                    float newY = centerY + distance * Mathf.Sin(newAngle * Mathf.Deg2Rad);

                    if (newX >= 0 && newX < w && newY >= 0 && newY < h) {
                        rotatedTexture.SetPixel(x, y, originalTexture.GetPixel((int)newX, (int)newY));
                    } else {
                        rotatedTexture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            rotatedTexture.Apply();
            return rotatedTexture;
        }

        public static Texture2D GetCopy(Texture2D tex, TextureFormat format = TextureFormat.RGBA32, bool mipChain = false) {
            var tmp = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(tex, tmp);

            RenderTexture.active = tmp;
            try {
                var copy = new Texture2D(tex.width, tex.height, format, mipChain: mipChain);
                copy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                copy.Apply();
                return copy;
            } finally {
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(tmp);
            }
        }
    }
}