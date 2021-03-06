using AVFXLib.Models;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFXEditor.UI.VFX
{
    public class UIParticleUVSet : UIItem
    {
        public AVFXParticleUVSet UVSet;
        public UIParticle Particle;

        public UICurve2Axis Scale;
        public UICurve2Axis Scroll;
        public UICurve Rotation;
        List<UIItem> Curves = new List<UIItem>();

        public UIParticleUVSet(AVFXParticleUVSet uvSet, UIParticle particle)
        {
            UVSet = uvSet;
            Particle = particle;
            //=================
            Attributes.Add( new UICombo<TextureCalculateUV>( "Calculate UV", UVSet.CalculateUVType ) );

            Curves.Add( Scale = new UICurve2Axis( UVSet.Scale, "Scale" ) );
            Curves.Add( Scroll = new UICurve2Axis( UVSet.Scroll, "Scroll" ) );
            Curves.Add( Rotation = new UICurve( UVSet.Rot, "Rotation" ) );
            Curves.Add( new UICurve( UVSet.RotRandom, "Rotation Random" ) );
        }

        public override void DrawBody( string parentId ) {
            string id = parentId + "/UV";
            DrawAttrs( id );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );
            DrawListTabs( Curves, parentId );
        }

        public override string GetDefaultText() {
            return "UV " + Idx;
        }
    }
}
