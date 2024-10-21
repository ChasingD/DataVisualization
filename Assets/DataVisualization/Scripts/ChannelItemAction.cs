using UnityEngine;

namespace DataVisualization.Scripts
{
    public class ChannelItemAction : MonoBehaviour
    {
        public void DeleteThis(GameObject target)
        {
            Destroy(target);
        }
    }
}