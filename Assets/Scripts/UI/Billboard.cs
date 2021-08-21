using System.Linq;
using UnityEngine;

namespace UI
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