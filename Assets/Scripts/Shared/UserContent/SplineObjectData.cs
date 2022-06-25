using System;
using System.Collections.Generic;
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class SplineObjectData : BaseObjectData
    {
        public string primaryTerrainAddress;
        public string secondaryTerrainAddress;
        public List<Vector4> points = new List<Vector4>();
    }
}