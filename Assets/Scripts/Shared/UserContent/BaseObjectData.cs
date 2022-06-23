using System;
using System.Collections.Generic;
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class BaseObjectData
    {
        public string objectId;
        public string name;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public string prefabAddress;
        public string gameLayer;
        public List<ObjectOptionData> options;
        public bool starred;
        public float scaleMultiplier;
        public int objectType;
    }
}