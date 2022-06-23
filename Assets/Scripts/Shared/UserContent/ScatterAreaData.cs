using System;
using System.Collections.Generic;
using UnityEngine;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class ScatterAreaData : BaseObjectData
    {
        public List<Vector3> points;
        public List<ScatterObjectData> scatterInstances;
    }
}