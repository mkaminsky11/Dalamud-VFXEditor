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
    public class LiteralBool : LiteralBase
    {
        public bool? Value { get; set; }

        public LiteralBool(string avfxName, int size = 4) : base(avfxName, size)
        {
        }

        public override void Read(AVFXNode node)
        {
        }

        public override void read( AVFXLeaf leaf)
        {
            if(Size == 1)
            {
                Value = Util.Bytes1ToBool(leaf.Contents);
            }
            else if(Size == 4)
            {
                Value = Util.Bytes4ToBool(leaf.Contents);
            }

            Size = leaf.Size;
            Assigned = true;
        }

        public void GiveValue(bool? value)
        {
            Value = value;
            Assigned = true;
        }

        public override void ToDefault()
        {
            GiveValue(false);
        }

        public override AVFXNode ToAVFX()
        {
            byte[] bytes = new byte[0];
            if(Size == 4)
            {
                bytes = Util.BoolTo4Bytes(Value);
            }
            else if(Size == 1)
            {
                bytes = Util.BoolTo1Bytes(Value);
            }
            return new AVFXLeaf(AVFXName, Size, bytes);
        }

        public override string stringValue()
        {
            return Value.ToString();
        }
    }
}
