using System;
using System.Collections.Generic;
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class ScatterAreaData : BaseObjectData
    {
        public List<Vector3> points = new List<Vector3>();
        public List<ScatterObjectData> scatterInstances = new List<ScatterObjectData>();
    }
}