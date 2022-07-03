using System;
using UnityEngine;
using UnityEngine.UI;

namespace TT.UI
{
    [RequireComponent(typeof(Image))]
    public class ImageSpinner : MonoBehaviour
    {
        [SerializeField][Tooltip("The number of degrees to rate each second.")] private float rotationSpeed = 360;

        private Image _image;
        private Vector3 _rotationDirection; 
        
        // Start is called before the first frame update
        void Start()
        {
            _image = GetComponent<Image>();
            _rotationDirection = new Vector3(0, 0, -1);
        }

        private void Update()
        {
            _image.transform.Rotate(_rotationDirection, rotationSpeed * Time.deltaTime);
        }
    }
}
