using Microsoft.MixedReality.OpenXR;
using UnityEngine;

namespace DataVisualization.Scripts
{
    public class UGMarkerController : MonoBehaviour
    {
        private ARMarker arMarker;

        public string markerContent;
        private bool following;

        private Transform root;

        private void Start()
        {
            root = RootCollection.Instance.transform;
            arMarker = GetComponent<ARMarker>();
            if (arMarker.GetDecodedString() != markerContent)
            {
                print($"识别到{arMarker.GetDecodedString()}，期望{markerContent}，即将销毁该物体");
                Destroy(gameObject);
            }
            else
            {
                print($"识别到{arMarker.GetDecodedString()}，期望{markerContent}");
                // SceneRoot.Instance.followTarget = transform;
                root.position = transform.position;
                root.localEulerAngles = new Vector3(root.localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, root.localEulerAngles.z);
            }
        }

        // private void Update()
        // {
        //     if (following)
        //     {
        //         root.position = transform.position;
        //         root.localEulerAngles = new Vector3(root.localEulerAngles.x, transform.localEulerAngles.y + 180, root.localEulerAngles.z);
        //     }
        // }
    }
}