/// MIT License, Copyright Burak Kara, burak@burak.io, https://en.wikipedia.org/wiki/MIT_License

using System;
using System.IO;

namespace BUtility
{
    class BNode
    {
        public bool bHasParent = false;
        public int GLTFNodeIndex = -1;
        public BMetadata[] Metadata = null;
        public BNode[] Children = null;

        public void Serialize(Stream _Dest)
        {
            _Dest.Write(new byte[1] { bHasParent ? (byte)1 : (byte)0 }, 0, 1);

            {
                var BArray = BitConverter.GetBytes(GLTFNodeIndex);
                _Dest.Write(BArray, 0, BArray.Length);
            }

            {
                var BArray = BitConverter.GetBytes(Metadata == null ? 0 : Metadata.Length);
                _Dest.Write(BArray, 0, BArray.Length);
            }
            if (Metadata != null)
            {
                foreach (var Current in Metadata)
                {
                    Current.Serialize(_Dest);
                }
            }
            
            {
                var BArray = BitConverter.GetBytes(Children == null ? 0 : Children.Length);
                _Dest.Write(BArray, 0, BArray.Length);
            }
            if (Children != null)
            {
                foreach (var Child in Children)
                {
                    Child.Serialize(_Dest);
                }
            }
        }

        public static BNode Deserialize(Stream _Src)
        {
            var NewNode = new BNode();

            {
                var BArray = new byte[1];
                _Src.Read(BArray, 0, 1);
                NewNode.bHasParent = BArray[0] == 1 ? true : false;
            }

            {
                var BArray = new byte[4];
                _Src.Read(BArray, 0, 4);
                NewNode.GLTFNodeIndex = BitConverter.ToInt32(BArray, 0);
            }

            int ElementCount;

            {
                var BArray = new byte[4];
                _Src.Read(BArray, 0, 4);
                ElementCount = BitConverter.ToInt32(BArray, 0);
            }
            if (ElementCount > 0)
            {
                NewNode.Metadata = new BMetadata[ElementCount];
                for (int i = 0; i < ElementCount; i++)
                {
                    NewNode.Metadata[i] = BMetadata.Deserialize(_Src);
                }
            }

            {
                var BArray = new byte[4];
                _Src.Read(BArray, 0, 4);
                ElementCount = BitConverter.ToInt32(BArray, 0);
            }
            if (ElementCount > 0)
            {
                NewNode.Children = new BNode[ElementCount];
                for (int i = 0; i < ElementCount; i++)
                {
                    NewNode.Children[i] = Deserialize(_Src);
                }
            }

            return NewNode;
        }
    }
}