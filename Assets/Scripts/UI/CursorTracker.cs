using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CursorTracker : MonoBehaviour
    {
        private static CursorTracker instance;
        public Image cursorImage;

        private void Awake()
        {
            instance = this;
        }

        public static void SetCursor(Sprite cursorSprite)
        {
            instance.cursorImage.enabled = true;
            instance.cursorImage.sprite = cursorSprite;
        }
        public static void ClearCursor()
        {
            instance.cursorImage.enabled = false;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var mousePos = Input.mousePosition;
            transform.position = mousePos;
        }
    }
}