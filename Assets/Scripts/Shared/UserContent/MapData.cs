using System;
using System.Collections.Generic;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class MapData
    {
        public string id;
        public string name;
        public string description;
        public string author;
        public string modifiedBy;
        public long dateCreated;
        public long dateSaved;
        public string terrainTextureAddress;
        public float time = 12;
        public float wind = 0.1f;
        public float windDirection;
        public int lightingMode;
        public TerrainData terrain;
        public List<WorldObjectData> worldObjects;
        public List<SplineObjectData> splineObjects;
        public List<ScatterAreaData> scatterAreas;
    }
}