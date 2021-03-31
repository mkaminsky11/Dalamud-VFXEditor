using AVFXLib.Models;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFXEditor
{
    public struct PenumbraMod {
        public string Name;
        public string Author;
        public string Description;
        public string? Version;
        public string? Website;
        public Dictionary<string, string> FileSwaps;
    }

    public class Penumbra
    {
        public Plugin _plugin;

        public Penumbra(Plugin plugin ) {
            _plugin = plugin;
            /*
             * {
             *  "Name":"Ultimate Manatrigger",
             *  "Author":"Gabster",
             *  "Description":"Mod imported from TexTools mod pack",
             *  "Version":null,
             *  "Website":null,
             *  "FileSwaps":{}
             *  }
             */
        }

        public void Export( string name, string author, string version, string path, string saveLocation, bool exportAll, bool exportTex ) {
            try {
                PenumbraMod mod = new PenumbraMod();
                mod.Name = name;
                mod.Author = author;
                mod.Description = "Exported from VFXEditor";
                mod.Version = version;
                mod.Website = null;
                mod.FileSwaps = new Dictionary<string, string>();

                string modFolder = Path.Combine( saveLocation, name );
                Directory.CreateDirectory( modFolder );
                string modConfig = Path.Combine( modFolder, "meta.json" );
                string configString = JsonConvert.SerializeObject( mod );
                File.WriteAllText( modConfig, configString );

                void AddMod( AVFXBase _avfx, string _path ) {
                    if( !string.IsNullOrEmpty( _path ) && _avfx != null ) {
                        var data = _avfx.toAVFX().toBytes();
                        string modFile = Path.Combine( modFolder, _path );
                        string modFileFolder = Path.GetDirectoryName( modFile );
                        Directory.CreateDirectory( modFileFolder );
                        File.WriteAllBytes( modFile, data );
                    }
                }

                void AddTex(string localPath, string _path ) {
                    if(!string.IsNullOrEmpty(localPath) && !string.IsNullOrEmpty( _path ) ) {
                        string modFile = Path.Combine( modFolder, _path );
                        string modFileFolder = Path.GetDirectoryName( modFile );
                        Directory.CreateDirectory( modFileFolder );
                        File.Copy( localPath, modFile, true );
                    }
                }

                if( exportAll ) {
                    foreach( var doc in _plugin.Doc.Docs ) {
                        AddMod( doc.AVFX, doc.Replace.Path );
                    }
                }
                else {
                    AddMod( _plugin.AVFX, path );
                }

                if( exportTex ) {
                    foreach(KeyValuePair<string, TexReplace> entry in _plugin.Manager.TexManager.GamePathReplace ) {
                        AddTex( entry.Value.localPath, entry.Key );
                    }
                }

                PluginLog.Log( "Exported To: " + saveLocation );
            }
            catch( Exception e )
            {
                PluginLog.LogError( e, "Could not export to Penumbra" );
            }
        }
    }
}
