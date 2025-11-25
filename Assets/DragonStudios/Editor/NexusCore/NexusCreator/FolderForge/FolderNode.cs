using System.Collections.Generic;
using UnityEngine;

namespace DraconisNexus
{
    [System.Serializable]
    public class FolderNode
    {
        public string name;
        public Vector2 position;
        public List<FolderNode> children = new();

        public FolderNode(string name, Vector2 position)
        {
            this.name = name;
            this.position = position;
        }
    }
}