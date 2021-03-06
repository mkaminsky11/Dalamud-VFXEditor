using AVFXLib.AVFX;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVFXLib.Models
{
    public class AVFXEmitterCreateParticle : Base
    {
        public const string NAME = "ItPr";

        public List<AVFXEmitterIterationItem> Items;

        public AVFXEmitterCreateParticle() : base(NAME)
        {
            Items = new List<AVFXEmitterIterationItem>();
        }

        public override void Read(AVFXNode node)
        {
            Assigned = true;
            // split every 26 leafs, make dummy elements and insert
            int numItems = (int)Math.Floor((double)node.Children.Count / 26);
            for (int idx = 0; idx < numItems; idx++)
            {
                List<AVFXNode> subItem = node.Children.GetRange(idx * 26, 26);
                AVFXNode dummyNode = new AVFXNode("ItPr_Item");
                dummyNode.Children = subItem;

                AVFXEmitterIterationItem Item = new AVFXEmitterIterationItem();
                Item.Read(dummyNode);
                Items.Add(Item);
            }
        }

        public override AVFXNode ToAVFX()
        {
            // make ItPr by concatting elements of dummy elements
            AVFXNode ItPr = new AVFXNode("ItPr");
            foreach (AVFXEmitterIterationItem Item in Items)
            {
                ItPr.Children.AddRange(Item.ToAVFX().Children); // flatten
            }
            return ItPr;
        }
    }
}
