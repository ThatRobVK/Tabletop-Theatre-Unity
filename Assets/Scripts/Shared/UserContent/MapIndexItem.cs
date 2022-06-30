using System;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class MapIndexItem
    {
        public string id;
        public string name;
        public string description;
        public long dateSaved;
        public long dateCreated;
        public string authorUsername;
    }
}