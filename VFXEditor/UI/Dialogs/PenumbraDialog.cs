using System;
using System.IO;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;
using VFXEditor.External;

namespace VFXEditor.UI {
    public class PenumbraDialog : GenericDialog {
        public PenumbraDialog( Plugin plugin ) : base(plugin, "Penumbra") {
            Size = new Vector2( 400, 200 );
        }

        public string Name = "";
        public string Author = "";
        public string Version = "1.0.0";
        public bool ExportAll = false;
        public bool ExportTex = true;

        public override void OnDraw() {
            var id = "##Penumbra";
            float footerHeight = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();

            ImGui.BeginChild( id + "/Child", new Vector2( 0, -footerHeight ), true );
            ImGui.InputText( "Mod Name" + id, ref Name, 255 );
            ImGui.InputText( "Author" + id, ref Author, 255 );
            ImGui.InputText( "Version" + id, ref Version, 255 );
            ImGui.Checkbox( "Export Textures", ref ExportTex );
            ImGui.SameLine();
            ImGui.Checkbox( "Export All Documents", ref ExportAll );
            if( !Plugin.DocManager.HasReplacePath( ExportAll ) ) {
                ImGui.TextColored( new Vector4( 0.8f, 0.1f, 0.1f, 1.0f ), "Missing Replace Path" );
            }
            ImGui.EndChild();

            if( ImGui.Button( "Export" + id ) ) {
                SaveDialog();
            }
        }

        public void SaveDialog() { // idk why the folderselectdialog doesn't work, so this will do for now
            Plugin.SaveFolderDialog( "AVFX File (*.avfx)|*.avfx*|All files (*.*)|*.*", "Select a File Location.",
                ( string path ) => {
                    try {
                        Penumbra.Export( Plugin, Name, Author, Version, Path.GetDirectoryName( path ), ExportAll, ExportTex );
                        Visible = false;
                    }
                    catch( Exception ex ) {
                        PluginLog.LogError( ex, "Could not select a mod location" );
                    }
                }
            );
        }
    }
}
