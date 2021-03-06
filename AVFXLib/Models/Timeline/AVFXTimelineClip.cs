using AVFXLib.AVFX;
using AVFXLib.Main;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVFXLib.Models
{
    public class AVFXTimelineClip : Base
    {
        public const string NAME = "Clip";

        /*
         * unique id : string [4] (reverse)
         * 4 int[4]
         * 4 float[4]
         * 4 string[32]
         * 
         * total = 4 + 4*4 + 4*4 + 4*32 = 164
         */

        public string UniqueId;
        public int[] UnknownInts;
        public float[] UnknownFloats;

        public AVFXTimelineClip() : base(NAME)
        {
            UnknownInts = new int[4];
            UnknownFloats = new float[4];
        }

        public override void Read(AVFXNode node)
        {
            Assigned = true;

            AVFXLeaf leaf = (AVFXLeaf)node;
            byte[] contents = leaf.Contents;
            int offset = 0;
            byte[] stringBytes = new byte[4];
            Buffer.BlockCopy(contents, offset, stringBytes, 0, 4);
            UniqueId = Util.BytesToString(stringBytes);
            offset += 4;

            for(int idx = 0; idx < 4; idx++)
            {
                byte[] intBytes = new byte[4];
                Buffer.BlockCopy(contents, offset, intBytes, 0, 4);
                UnknownInts[idx] = Util.Bytes4ToInt(intBytes);
                offset += 4;
            }
            for (int idx = 0; idx < 4; idx++)
            {
                byte[] floatBytes = new byte[4];
                Buffer.BlockCopy(contents, offset, floatBytes, 0, 4);
                UnknownFloats[idx] = Util.Bytes4ToFloat(floatBytes);
                offset += 4;
            }
        }

        public override void ToDefault()
        {
            UniqueId = "LLIK";
            UnknownInts = new int[] { 0, 0, 0, 0 };
            UnknownFloats = new float[] { -1, 0, 0, 0 };
        }

        public override AVFXNode ToAVFX()
        {
            byte[] contents = new byte[164];
            int offset = 0;

            byte[] stringBytes = Util.StringToBytes(UniqueId);
            Buffer.BlockCopy(stringBytes, 0, contents, offset, 4);
            offset += 4;

            for (int idx = 0; idx < 4; idx++)
            {
                byte[] intBytes = Util.IntTo4Bytes(UnknownInts[idx]);
                Buffer.BlockCopy(intBytes, 0, contents, offset, 4);
                offset += 4;
            }
            for (int idx = 0; idx < 4; idx++)
            {
                byte[] floatBytes = Util.FloatTo4Bytes(UnknownFloats[idx]);
                Buffer.BlockCopy(floatBytes, 0, contents, offset, 4);
                offset += 4;
            }

            return new AVFXLeaf(NAME, 164, contents);
        }
    }
}
