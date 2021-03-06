using AVFXLib.AVFX;
using AVFXLib.Models;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFXEditor.Data.DirectX;

namespace VFXEditor.UI.VFX {
    public class UIModelView : UINodeSplitView<UIModel> {
        public UIMain Main;

        public UIModelView(UIMain main, AVFXBase avfx) : base(avfx, "##MDL") {
            Main = main;
            Group = main.Models;
            Group.Items = AVFX.Models.Select( item => new UIModel( Main, item ) ).ToList();
        }

        public override void DrawNewButton( string parentId ) {
            if( ImGui.SmallButton( "+ New" + Id ) ) {
                Group.Add( OnNew() );
            }
            ImGui.SameLine();
            if( ImGui.SmallButton( "Import" + Id ) ) {
                Main.ImportDialog();
            }
        }

        public override void OnSelect( UIModel item ) {
            DirectXManager.Manager.ModelView.LoadModel( item.Model );
        }

        public override void OnDelete( UIModel item ) {
            AVFX.RemoveModel( item.Model );
        }

        public override UIModel OnNew() {
            return new UIModel( Main, AVFX.AddModel() );
        }

        public override UIModel OnImport( AVFXNode node ) {
            AVFXModel mdl = new AVFXModel();
            mdl.Read( node );
            AVFX.AddModel( mdl );
            return new UIModel( Main, mdl );
        }
    }
}
