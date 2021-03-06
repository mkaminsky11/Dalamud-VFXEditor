using Dalamud.Game.ClientState;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VFXEditor.Structs.Vfx;

namespace VFXEditor.Data.Vfx {
    public unsafe class VfxTracker {
        public bool Enabled = false;

        public struct TrackerData {
            public string path;
            public bool isChecked;
            public int actorId;
            public VfxStruct* Vfx;
        }

        public Plugin Plugin;
        public ConcurrentDictionary<IntPtr, TrackerData> ActorVfxs;
        public ConcurrentDictionary<IntPtr, TrackerData> StaticVfxs;

        public static ClosenessComp CloseComp = new ClosenessComp();

        public VfxTracker(Plugin plugin ) {
            Plugin = plugin;
            Reset();
        }

        public void AddActor(IntPtr actor, VfxStruct* vfx, string path ) {
            if( !Enabled ) return;
            var data = new TrackerData() {
                path = path,
                isChecked = false,
                actorId = -1,
                Vfx = vfx
            };
            ActorVfxs.TryAdd( new IntPtr(vfx), data );
        }

        public void RemoveActor( VfxStruct* vfx ) {
            if( !Enabled ) return;

            if( ActorVfxs.ContainsKey( new IntPtr( vfx ) ) ) {
                ActorVfxs.TryRemove( new IntPtr( vfx ), out var value );
            }
        }

        public void AddStatic( VfxStruct* vfx, string path ) {
            if( !Enabled ) return;
            var data = new TrackerData() {
                path = path,
                isChecked = false,
                actorId = -1,
                Vfx = vfx
            };
            StaticVfxs.TryAdd( new IntPtr( vfx ), data );
        }

        public void RemoveStatic( VfxStruct* vfx ) {
            if( !Enabled ) return;
            if( StaticVfxs.ContainsKey( new IntPtr( vfx ) ) ) {
                StaticVfxs.TryRemove( new IntPtr( vfx ), out var value );
            }
        }

        public bool WatchingCutscene() {
            return ( Plugin.PluginInterface.ClientState != null && Plugin.PluginInterface.ClientState.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Plugin.PluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene78] );
        }

        // ===== SCUFFED STUFF TO GROUP STATIC VFXS =========
        public struct StaticVfxGroup {
            public string path;
            public SharpDX.Vector3 position;
        }

        public class ClosenessComp : IEqualityComparer<SharpDX.Vector3> {
            public bool Equals( SharpDX.Vector3 x, SharpDX.Vector3 y ) {
                return ( x - y ).Length() < 2;
            }
            public int GetHashCode( SharpDX.Vector3 obj ) {
                return 0;
            }
        }

        public static int ChooseId(int caster, int target ) {
            return target > 0 ? target : ( caster > 0 ? caster : -1 );
        }

        public void Draw() {
            if( !Enabled ) return;
            var playPos = Plugin.PluginInterface.ClientState?.LocalPlayer?.Position;
            if( !playPos.HasValue ) return;

            var windowPos = ImGuiHelpers.MainViewport.Pos;

            // ======= IF IN A CUTSCENE, GIVE UP WITH POSITIONING ======
            if( Plugin.PluginInterface.ClientState.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Plugin.PluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene] || Plugin.PluginInterface.ClientState.Condition[ConditionFlag.WatchingCutscene78] ) {
                HashSet<string> paths = new HashSet<string>();
                foreach(var item in ActorVfxs ) {
                    paths.Add( item.Value.path );
                }
                foreach(var item in StaticVfxs ) {
                    paths.Add( item.Value.path );
                }

                Vector2 pos = windowPos + new Vector2( 15, 15 );
                DrawOverlayItems( pos, paths, 0 );
                return;
            }

            // ======== SET UP MATRIX, ONLY ONCE :) =====
            var matrixSingleton = Plugin.ResourceLoader.GetMatrixSingleton();
            var viewProjectionMatrix = new SharpDX.Matrix();
            float width, height;
            unsafe {
                var rawMatrix = ( float* )( matrixSingleton + 0x1b4 + ( 0x13c * 0 ) ).ToPointer(); // 0 = projection idx
                for( var i = 0; i < 16; i++, rawMatrix++ )
                    viewProjectionMatrix[i] = *rawMatrix;
                width = *rawMatrix;
                height = *( rawMatrix + 1 );
            }

            List<StaticVfxGroup> Groups = new List<StaticVfxGroup>(); // static vfxs without an actor
            Dictionary<int, HashSet<string>> ActorToVfxs = new Dictionary<int, HashSet<string>>(); // either one with an actor

            // ====== STATIC =======
            foreach( var item in StaticVfxs ) {
                if( item.Key == IntPtr.Zero ) continue;

                var vfx = item.Value;
                if( !vfx.isChecked ) {
                    var casterId = vfx.Vfx->StaticCaster;
                    var targetId = vfx.Vfx->StaticTarget;
                    var actorId = ChooseId( casterId, targetId );
                    vfx = new TrackerData
                    {
                        path = vfx.path,
                        isChecked = true,
                        actorId = actorId,
                        Vfx = vfx.Vfx
                    };
                    StaticVfxs[item.Key] = vfx;
                }

                if( vfx.actorId > 0 ) { // add to actor to vfxs
                    if( !ActorToVfxs.ContainsKey( vfx.actorId ) ) {
                        ActorToVfxs[vfx.actorId] = new HashSet<string>();
                    }
                    ActorToVfxs[vfx.actorId].Add( vfx.path );
                }
                else { // add to groups
                    Groups.Add( new StaticVfxGroup {
                        path = vfx.path,
                        position = vfx.Vfx->Position
                    } );
                }
            }

            // ======= ACTORS =========
            foreach( var item in ActorVfxs ) {
                if( item.Key == IntPtr.Zero ) continue;

                var vfx = item.Value;
                if( !vfx.isChecked ) {
                    var casterId = vfx.Vfx->ActorCaster;
                    var targetId = vfx.Vfx->ActorTarget;
                    var actorId = ChooseId( casterId, targetId );
                    vfx = new TrackerData
                    {
                        path = vfx.path,
                        isChecked = true,
                        actorId = actorId,
                        Vfx = vfx.Vfx
                    };
                    ActorVfxs[item.Key] = vfx;
                }

                if( vfx.actorId > 0 ) { // add to actor to vfxs
                    if( !ActorToVfxs.ContainsKey( vfx.actorId ) ) {
                        ActorToVfxs[vfx.actorId] = new HashSet<string>();
                    }
                    ActorToVfxs[vfx.actorId].Add( vfx.path );
                }
            }
            // ====== DRAW GROUPS ======
            int idx = 0;
            foreach( var group in Groups.GroupBy( item => item.position, item => item.path, CloseComp ) ) {
                HashSet<string> paths = new HashSet<string>( group );

                // ==== CHECK WINDOW POSITION ======
                if(!WorldToScreen(height, width, ref viewProjectionMatrix, windowPos, group.Key, out var screenCoords ) ) continue;
                var d = Distance( playPos.Value, group.Key );
                if( d > 100f && Configuration.Config.OverlayLimit ) {
                    continue;
                }
                DrawOverlayItems( new Vector2( screenCoords.X, screenCoords.Y ), paths, idx );
                idx++;
            }

            // ====== DRAW ACTORS ======
            var actorTable = Plugin.PluginInterface.ClientState.Actors;
            if( actorTable == null ) {
                return;
            }
            foreach( var actor in actorTable ) {
                if( actor == null ) continue;
                if( Plugin.PluginInterface.ClientState.LocalPlayer == null ) continue;

                var result = ActorToVfxs.TryGetValue( actor.ActorId, out var paths );
                if( !result ) continue;

                var pos = new SharpDX.Vector3 {
                    X = actor.Position.X,
                    Y = actor.Position.Z + 2,
                    Z = actor.Position.Y
                };

                // ===== CHECK WINDOW POSITION =========
                if(!WorldToScreen( height, width, ref viewProjectionMatrix, windowPos, pos, out var screenCoords ) ) continue;
                var d = Distance( playPos.Value, pos );
                if( d > 100f && Configuration.Config.OverlayLimit ) {
                    continue;
                }
                DrawOverlayItems( new Vector2( screenCoords.X, screenCoords.Y ), paths, idx );
                idx++;
            }
        }

        public void DrawOverlayItems(Vector2 pos, HashSet<string> items, int idx ) {
            string longestString = "";
            foreach(var item in items ) {
                if(item.Length > longestString.Length ) {
                    longestString = item;
                }
            }
            var screenPos = ImGui.GetMainViewport().Pos;
            var screenSize = ImGui.GetMainViewport().Size;
            var windowSize = ImGui.CalcTextSize( longestString );
            var largestSize = windowSize;
            windowSize.X += ImGui.GetStyle().WindowPadding.X + 100; // account for "COPY" button
            windowSize.Y += ImGui.GetStyle().WindowPadding.Y + 10;
            if( pos.X + windowSize.X > screenPos.X + screenSize.X || pos.Y + windowSize.Y > screenPos.Y + screenSize.Y ) return;

            var maxDisplay = Math.Floor( (screenPos.Y + screenSize.Y - pos.Y) / windowSize.Y); // how many can we fit vertically?

            ImGui.SetNextWindowPos( new Vector2( pos.X, pos.Y ) );
            ImGui.SetNextWindowBgAlpha( 0.5f );
            ImGuiHelpers.ForceNextWindowMainViewport();

            if( ImGui.Begin( $"vfx-{idx}",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.AlwaysAutoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.NoNav ) ) {
                int i = 0;
                foreach( var path in items ) {
                    if( i >= maxDisplay ) break;

                    ImGui.Text( $"{path}" );
                    ImGui.SameLine( largestSize.X + 20 );
                    if( ImGui.Button( $"COPY##vfx-{idx}-{i}" ) ) {
                        ImGui.SetClipboardText( path );
                    }
                    i++;
                }

                ImGui.End();
            }
        }

        public void Reset() {
            ActorVfxs = new();
            StaticVfxs = new();
        }

        public static float Distance(Vector3 p1, SharpDX.Vector3 p2 ) {
            return (float) Math.Sqrt(Math.Pow( p1.X - p2.X, 2 ) + Math.Pow( p1.Y - p2.Z, 2 ) + Math.Pow( p1.Z - p2.Y, 2 ));
        }

        public bool WorldToScreen( float height, float width, ref SharpDX.Matrix viewProjectionMatrix, Vector2 windowPos, SharpDX.Vector3 worldPos, out SharpDX.Vector2 screenPos ) {
            SharpDX.Vector3.Transform( ref worldPos, ref viewProjectionMatrix, out SharpDX.Vector3 pCoords );
            screenPos = new SharpDX.Vector2( pCoords.X / pCoords.Z, pCoords.Y / pCoords.Z );
            screenPos.X = 0.5f * width * ( screenPos.X + 1f ) + windowPos.X;
            screenPos.Y = 0.5f * height * ( 1f - screenPos.Y ) + windowPos.Y;
            return pCoords.Z > 0 && screenPos.X > windowPos.X && screenPos.X < windowPos.X + width && screenPos.Y > windowPos.Y && screenPos.Y < windowPos.Y + height;
        }
    }
}
