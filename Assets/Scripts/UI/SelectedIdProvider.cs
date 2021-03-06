using Dman.LSystem.SystemRuntime.Turtle;
using System.Collections;
using UnityEngine;

namespace UI
{
    public class SelectedIdProvider : MonoBehaviour
    {
        public static SelectedIdProvider instance;

        public Camera mainCamera;
        public Camera idCamera;

        public uint HoveredId { get; private set; }


        public RenderTexture idTexture;
        private Texture2D idTextureTempSpace;

        private void Awake()
        {
            instance = this;

            this.SetupIdTexture();


            idTexture.Release();
            idTexture.antiAliasing = 1;
            idTexture.filterMode = FilterMode.Point;
            idTexture.autoGenerateMips = false;
            idTexture.depth = 24;
            idTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
            idTexture.format = RenderTextureFormat.ARGB32;
            idTexture.useMipMap = false;
            idTexture.Create();

            idTextureTempSpace = new Texture2D(1, 1)
            {
                filterMode = FilterMode.Point,
                minimumMipmapLevel = 0,
            };

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (idTexture.width != mainCamera.pixelWidth || idTexture.height != mainCamera.pixelHeight)
            {
                Debug.Log("Detected screen resultion change");
                Debug.Log($"resolutions: ({mainCamera.pixelWidth}, {mainCamera.pixelHeight}) ; ({idCamera.pixelWidth}, {idCamera.pixelHeight})");
                SetupIdTexture();
            }
            var mousePosition = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            var nextHoveredId = ReadColorAtPixel(mousePosition.x, mousePosition.y, idTexture)?.UIntValue ?? HoveredId;
            if (HoveredId != nextHoveredId)
            {
                HoveredId = nextHoveredId;
            }
        }

        private void SetupIdTexture()
        {
            if (idTexture == null)
            {
                idTexture = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24, RenderTextureFormat.ARGB32, 0)
                {
                    antiAliasing = 1,
                    filterMode = FilterMode.Point,
                    autoGenerateMips = false,
                    depth = 24,
                    graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm
                };
            }
            else
            {
                idTexture.Release();
                idTexture.width = mainCamera.pixelWidth;
                idTexture.height = mainCamera.pixelHeight;
            }
            idTexture.Create();
            idCamera.targetTexture = null; // have to reset, to make sure it adjusts for aspect ratio
            idCamera.targetTexture = idTexture;
        }

        private static UIntFloatColor32? ReadColorAtPixel(int x, int y, RenderTexture renderTexture)
        {
            if (x < 0 || x >= renderTexture.width || y < 0 || y >= renderTexture.height)
            {
                return null;
            }

            var lastActiveTexture = RenderTexture.active;

            RenderTexture.active = renderTexture;

            // Create a new Texture2D and read the RenderTexture image into it
            Texture2D tex = new Texture2D(1, 1);
            tex.ReadPixels(new Rect(x, renderTexture.height - y, 1, 1), 0, 0);

            RenderTexture.active = lastActiveTexture;

            var nativeData = tex.GetPixelData<uint>(0);

            var result = nativeData[0];

            return new UIntFloatColor32(result);
        }

        private void OnDestroy()
        {
            idTexture.Release();
        }
    }

}