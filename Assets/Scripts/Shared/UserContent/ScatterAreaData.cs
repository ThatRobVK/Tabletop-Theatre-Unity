using System;
using System.Collections.Generic;
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class ScatterAreaData : BaseObjectData
    {
        public List<Vector4Data> points = new List<Vector4Data>();
        public List<ScatterObjectData> scatterInstances = new List<ScatterObjectData>();
    }
}