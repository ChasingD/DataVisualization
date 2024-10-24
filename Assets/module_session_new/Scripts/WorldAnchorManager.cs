using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Module.Session
{
    public class WorldAnchorManager : Microsoft.MixedReality.Toolkit.Experimental.Utilities.WorldAnchorManager
    {
        public static WorldAnchorManager Instance;
        private void Awake()
        {
            Instance = this;
        }
    }
}