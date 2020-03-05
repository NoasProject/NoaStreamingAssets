### StreamingAssetsのロードを行う場合のHelperシステム

```csharp
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Noa;

namespace NoaSample
{
    public class SampleLoad : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            this.LoadText();
            this.LoadImage();
        }

        public Image image = null;

        private void LoadText()
        {
            string csvPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Csv/sample.csv");
            StreamingAssetsLoader.ReadAllBytes(csvPath, bytes =>
            {
                if (bytes == null)
                {
                    Debug.Log($"ファイルが存在しません Path:{csvPath}");
                    return;
                }
                string txt = Encoding.UTF8.GetString(bytes);

                Debug.Log(txt);
            });


        }

        private void LoadImage()
        {
            string imgPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Image/sample.png");
            StreamingAssetsLoader.ReadAllBytes(imgPath, bytes =>
            {
                if (bytes == null)
                {
                    Debug.Log($"ファイルが存在しません Path:{imgPath}");
                    return;
                }

                Texture2D texture = new Texture2D(1024, 1024);

                texture.LoadImage(bytes);

                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            });
        }
    }
}
```

## Package
[Package](Assets/Plugins/Noa/StreamingAssetsHelper/.Package/NoaStreamingAssets.unitypackage?raw=true)<br>