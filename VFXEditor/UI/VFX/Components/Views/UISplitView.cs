using AVFXLib.Models;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFXEditor.UI.VFX
{
    public class UISplitView : UIBase
    {
        public List<UIBase> Items;
        public bool NewButton;
        public UIBase SelectedItem = null;

        public UISplitView(List<UIBase> items, bool newButton = false)
        {
            Items = items;
            NewButton = newButton;
        }

        public override void Draw( string parentId )
        {
            ImGui.Columns( 2, parentId + "/Cols", true );
            // ===== C1 =========
            if( NewButton ) {
                DrawNewButton( parentId );
            }
            ImGui.BeginChild( parentId + "/Tree" );
            int idx = 0;
            foreach( var item in Items )
            {
                if( item.Assigned )
                {
                    item.Idx = idx;
                    item.DrawSelect( parentId, ref SelectedItem );
                }
                idx++;
            }
            foreach( var item in Items )
            {
                if( !item.Assigned )
                    item.DrawSelect( parentId, ref SelectedItem );
            }
            ImGui.EndChild();
            ImGui.SetColumnWidth( 0, 200 );
            // ===== C2 ============
            ImGui.NextColumn();
            ImGui.BeginChild( parentId + "/Split" );
            if( SelectedItem != null && SelectedItem.Assigned == true )
            {
                SelectedItem.DrawBody( parentId );
            }
            else
            {

            }
            ImGui.EndChild();
            ImGui.Columns( 1 );
        }

        public virtual void DrawNewButton(string parentId ) { }
    }
}