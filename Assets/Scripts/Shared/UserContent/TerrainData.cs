using System;
using System.Collections.Generic;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class TerrainData
    {
        public List<string> terrainLayers = new List<string>();
        public int splatWidth;
        public int splatHeight;
        public List<Vector4Data> splatMaps = new List<Vector4Data>();
    }
}