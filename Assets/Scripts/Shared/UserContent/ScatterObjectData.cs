using System;
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class ScatterObjectData
    {
        public Vector3 position;
        public Quaternion rotation;
        public string prefabAddress;
    }
}