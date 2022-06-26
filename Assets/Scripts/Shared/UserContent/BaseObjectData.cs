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
        public Vector4Data position;
        public Vector4Data rotation;
        public Vector4Data scale;
        public string prefabAddress;
        public string gameLayer;
        public List<ObjectOptionData> options = new List<ObjectOptionData>();
        public bool starred;
        public float scaleMultiplier;
        public int objectType;
    }
}