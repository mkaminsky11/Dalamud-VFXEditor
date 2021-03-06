using AVFXLib.Main;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AVFXLib.AVFX
{
    public class AVFXNode
    {

        public string Name { get; set; }
        // calculate size on the fly

        public List<AVFXNode> Children { get; set; }

        public AVFXNode(string n) {
            Name = n;
            Children = new List<AVFXNode>();
        }

        public virtual byte[] ToBytes() {
            int totalSize = 0;
            byte[][] byteArrays = new byte[Children.Count][];
            for(int i = 0; i < Children.Count; i++)
            {
                byteArrays[i] = Children[i].ToBytes();
                totalSize += byteArrays[i].Length;
            }
            byte[] bytes = new byte[8 + Util.RoundUp(totalSize)];
            byte[] name = Util.NameTo4Bytes(Name);
            byte[] size = Util.IntTo4Bytes(totalSize);
            Buffer.BlockCopy(name, 0, bytes, 0, 4);
            Buffer.BlockCopy(size, 0, bytes, 4, 4);
            int bytesSoFar = 8;
            for(int i = 0; i < byteArrays.Length; i++)
            {
                Buffer.BlockCopy(byteArrays[i], 0, bytes, bytesSoFar, byteArrays[i].Length);
                bytesSoFar += byteArrays[i].Length;
            }
            return bytes;
        }

        // =====================
        public bool CheckEquals(AVFXNode node, out List<string> messages) {
            messages = new List<string>();
            return EqualsNode(node, messages);
        }

        public virtual bool EqualsNode(AVFXNode node, List<string> messages) {
            if((node is AVFXLeaf) || (node is AVFXBlank))
            {
                messages.Add(string.Format("Wrong Type {0} / {1}", Name, node.Name));
                return false;
            }
            if (Name != node.Name)
            {
                messages.Add(string.Format("Wrong Name {0} / {1}", Name, node.Name));
                return false;
            }

            List<AVFXNode> notBlank = Children.Where( x => !( x is AVFXBlank ) ).ToList();
            List<AVFXNode> notBlank2 = node.Children.Where( x => !( x is AVFXBlank ) ).ToList();

            if(notBlank.Count != notBlank2.Count)
            {
                messages.Add(string.Format("Wrong Node Size {0} : {1} / {2} : {3}", Name, notBlank.Count, node.Name, notBlank2.Count));

                return false;
            }
            for(int idx = 0; idx < notBlank.Count; idx++)
            {
                bool e = notBlank[idx].EqualsNode(notBlank2[idx], messages);
                if (!e)
                {
                    messages.Add(string.Format("Not Equal {0} index: {1}", Name, idx));
                    return false;
                }
            }
            return true;
        }

        public virtual string ExportString(int level) {
            string ret = string.Format("{0}+---  {1} ----\n", new string('\t', level), Name);
            foreach(var c in Children)
            {
                ret = ret + c.ExportString( level + 1);
            }
            return ret;
        }
    }
}
