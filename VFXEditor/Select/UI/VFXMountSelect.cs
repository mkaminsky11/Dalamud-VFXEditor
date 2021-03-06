using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Plugin;
using ImGuiNET;
using VFXSelect.Data.Rows;

namespace VFXSelect.UI
{
    public class VFXMountSelect : VFXSelectTab<XivMount, XivMountSelected> {
        public VFXMountSelect( string parentId, string tabId, SheetManager sheet, VFXSelectDialog dialog ) : 
            base(parentId, tabId, sheet.Mounts, sheet.PluginInterface, dialog) {
        }

        ImGuiScene.TextureWrap Icon;
        public override void OnSelect() {
            LoadIcon( Selected.Icon, ref Icon );
        }

        public override bool CheckMatch( XivMount item, string searchInput ) {
            return VFXSelectDialog.Matches( item.Name, searchInput );
        }

        public override void DrawSelected( XivMountSelected loadedItem ) {
            if( loadedItem == null ) { return; }
            ImGui.Text( loadedItem.Mount.Name );
            ImGui.SetCursorPosY( ImGui.GetCursorPosY() + 5 );

            if( Icon != null ) {
                ImGui.Image( Icon.ImGuiHandle, new Vector2( Icon.Width, Icon.Height ) );
            }

            ImGui.Text( "Variant: " + loadedItem.Mount.Variant );
            ImGui.Text( "IMC Count: " + loadedItem.Count );
            ImGui.Text( "VFX Id: " + loadedItem.VfxId );

            ImGui.Text( "IMC Path: " );
            ImGui.SameLine();
            Dialog.DisplayPath( loadedItem.ImcPath );

            ImGui.Text( "VFX Path: " );
            ImGui.SameLine();
            Dialog.DisplayPath( loadedItem.GetVFXPath() );
            if( loadedItem.VfxExists ) {
                if( ImGui.Button( "SELECT" + Id ) ) {
                    Dialog.Invoke( new VFXSelectResult( VFXSelectType.GameNpc, "[NPC] " + loadedItem.Mount.Name, loadedItem.GetVFXPath() ) );
                }
                ImGui.SameLine();
                Dialog.Copy( loadedItem.GetVFXPath(), id: Id + "Copy" );
                Dialog.Spawn( loadedItem.GetVFXPath(), id: Id + "Spawn" );
            }
        }

        public override string UniqueRowTitle( XivMount item ) {
            return item.Name + Id;
        }
    }
}