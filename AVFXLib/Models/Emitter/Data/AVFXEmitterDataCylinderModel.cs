using AVFXLib.AVFX;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVFXLib.Models
{
    public class AVFXEmitterDataCylinderModel : AVFXEmitterData
    {
        public LiteralEnum<RotationOrder> RotationOrderType = new LiteralEnum<RotationOrder>("ROT");
        public LiteralEnum<GenerateMethod> GenerateMethodType = new LiteralEnum<GenerateMethod>("GeMT");
        public LiteralInt DivideX = new LiteralInt("DivX");
        public LiteralInt DivideY = new LiteralInt("DivY");

        public AVFXCurve Length = new AVFXCurve("Len");
        public AVFXCurve Radius = new AVFXCurve("Rad");
        public AVFXCurve InjectionSpeed = new AVFXCurve("IjS");
        public AVFXCurve InjectionSpeedRandom = new AVFXCurve("IjSR");

        List<Base> Attributes;

        public AVFXEmitterDataCylinderModel() : base("Data")
        {
            Attributes = new List<Base>(new Base[] {
                RotationOrderType,
                GenerateMethodType,
                DivideX,
                DivideY,
                Length,
                Radius,
                InjectionSpeed,
                InjectionSpeedRandom
            });
        }

        public override void ToDefault()
        {
            Assigned = true;
            SetUnAssigned(Attributes);
            SetDefault(RotationOrderType);
            SetDefault(GenerateMethodType);
            DivideX.GiveValue(1);
            DivideY.GiveValue(1);
        }

        public override void Read(AVFXNode node)
        {
            Assigned = true;
            ReadAVFX(Attributes, node);
        }

        public override AVFXNode ToAVFX()
        {
            AVFXNode dataAvfx = new AVFXNode("Data");
            PutAVFX(dataAvfx, Attributes);
            return dataAvfx;
        }
    }
}
