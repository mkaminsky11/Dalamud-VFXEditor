using AVFXLib.AVFX;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVFXLib.Models
{
    public class AVFXEffectorDataDirectionalLight : AVFXEffectorData
    {
        public AVFXCurveColor Ambient = new AVFXCurveColor( "Amb" );
        public AVFXCurveColor Color = new AVFXCurveColor();
        public AVFXCurve Power = new AVFXCurve( "Pow" );
        public AVFXCurve3Axis Rotation = new AVFXCurve3Axis( "Rot" );

        List<Base> Attributes;

        public AVFXEffectorDataDirectionalLight() : base("Data")
        {
            Attributes = new List<Base>(new Base[]{
                Ambient,
                Color,
                Power,
                Rotation
            });
        }

        public override void Read(AVFXNode node)
        {
            Assigned = true;
            ReadAVFX(Attributes, node);
        }

        public override void ToDefault()
        {
            Assigned = true;
            SetDefault(Attributes);
        }

        public override AVFXNode ToAVFX()
        {
            AVFXNode dataAvfx = new AVFXNode("Data");
            PutAVFX(dataAvfx, Attributes);
            return dataAvfx;
        }
    }
}
