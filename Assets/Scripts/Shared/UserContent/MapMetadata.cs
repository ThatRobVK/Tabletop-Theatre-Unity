using System;

namespace TT.Shared.UserContent
{
    [Serializable]
    public class MapMetadata
    {
        public string id;
        public string name;
        public string description;
        public string authorId;
        public string authorName;
        public string modifiedById;
        public string modifiedByName;
        public long dateCreated;
        public long dateSaved;
    }
}