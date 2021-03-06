using AVFXLib.Models;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFXEditor.UI.VFX
{
    public class UITimelineItem : UIWorkspaceItem {
        public AVFXTimelineSubItem Item;
        public UITimeline Timeline;
        //===========================
        public bool ClipAssigned;
        public UIInt ClipNumber;

        public UINodeSelect<UIBinder> BinderSelect;
        public UINodeSelect<UIEmitter> EmitterSelect;
        public UINodeSelect<UIEffector> EffectorSelect;

        public UIInt StartTime;
        public UIInt EndTime;

        public UITimelineItem(AVFXTimelineSubItem item, UITimeline timeline) {
            Item = item;
            Timeline = timeline;

            BinderSelect = new UINodeSelect<UIBinder>( timeline, "Binder Select", Timeline.Main.Binders, Item.BinderIdx );
            EmitterSelect = new UINodeSelect<UIEmitter>( timeline, "Emitter Select", Timeline.Main.Emitters, Item.EmitterIdx );
            EffectorSelect = new UINodeSelect<UIEffector>( timeline, "Effector Select", Timeline.Main.Effectors, Item.EffectorIdx );

            ClipNumber = new UIInt( "ClipNumber", Item.ClipNumber );
            ClipAssigned = Item.ClipNumber.Assigned;
            //==================
            Attributes.Add( new UICheckbox( "Enabled", Item.Enabled ) );
            Attributes.Add( StartTime = new UIInt( "Start Time", Item.StartTime ) );
            Attributes.Add( EndTime = new UIInt( "End Time", Item.EndTime ) );
            Attributes.Add( new UIInt( "Platform", Item.Platform ) );
        }

        public override void DrawBody( string parentId ) {
            string id = parentId + "/Item";
            DrawRename( id );

            BinderSelect.Draw( id );
            EmitterSelect.Draw( id );
            EffectorSelect.Draw( id );
            DrawAttrs( id );

            if(ImGui.Checkbox("Clip Enabled" + id, ref ClipAssigned ) ) {
                Item.ClipNumber.Assigned = ClipAssigned;
            }
            ClipNumber.Draw( id );
        }

        public override string GetDefaultText() {
            return Idx + ": Emitter " + Item.EmitterIdx.Value;
        }

        public override string GetWorkspaceId() {
            return $"{Timeline.GetWorkspaceId()}/Item{Idx}";
        }
    }
}
