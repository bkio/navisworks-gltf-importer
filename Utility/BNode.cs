/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System.Collections.Generic;

namespace BUtility
{
    public class BNode
    {
        public int GLTFNodeIndex = -1;
        public List<BMetadata> Metadata = new List<BMetadata>();
        public List<BNode> Children = new List<BNode>();
    }
}