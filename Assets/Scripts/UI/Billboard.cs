using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class Billboard : MonoBehaviour
    {
        public string cameraName;

        private GameObject cam;

        void Start()
        {
            cam = GameObject.FindGameObjectsWithTag("MainCamera").Where(gameObject => gameObject.name == cameraName).First();
        }

        void LateUpdate()
        {
            transform.LookAt(transform.position + cam.transform.forward);
        }
    }
}