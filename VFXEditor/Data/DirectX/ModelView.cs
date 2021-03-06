using System;
using Dalamud.Plugin;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Vec2 = System.Numerics.Vector2;

namespace VFXEditor.Data.DirectX {
    public abstract class ModelView {
        public DirectXManager Manager;
        public Device Device;
        public DeviceContext Ctx;

        public int Width = 300;
        public int Height = 300;
        public bool FirstModel = false;
        public bool IsWireframe = false;

        public RasterizerState RState;
        public Buffer RendersizeBuffer;
        public Buffer WorldBuffer;
        public Matrix ViewMatrix;
        public Matrix ProjMatrix;
        public Texture2D DepthTex;
        public DepthStencilView DepthView;
        public Texture2D RenderTex;
        public ShaderResourceView RenderShad;
        public RenderTargetView RenderView;

        public bool IsDragging = false;
        private Vector2 LastMousePos;
        private float Yaw;
        private float Pitch;
        private Vector3 Position = new Vector3( 0, 0, 0 );
        private float Distance = 5;

        public Matrix LocalMatrix = Matrix.Identity;

        public ModelView(DirectXManager manager) {
            Manager = manager;
            Device = Manager.Device;
            Ctx = Manager.Ctx;

            //  ====== CONSTANT BUFFERS =======
            WorldBuffer = new Buffer( Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0 );
            ViewMatrix = Matrix.LookAtLH( new Vector3(0, 0, -Distance), Position, Vector3.UnitY );

            RendersizeBuffer = new Buffer( Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0 );

            RefreshRasterizeState();
            ResizeResources();
        }

        public void RefreshRasterizeState() {
            RState?.Dispose();
            RState = new RasterizerState( Device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = IsWireframe ? FillMode.Wireframe : FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            } );
        }

        public void Resize( Vec2 size ) {
            var w_ = ( int )size.X;
            var h_ = ( int )size.Y;
            if( w_ != Width || h_ != Height ) {
                Width = w_;
                Height = h_;
                ResizeResources();
                if( FirstModel ) {
                    Draw();
                }
            }
        }

        public static int GetIdx( int faceIdx, int pointIdx, int span, int pointsPer ) {
            return span * ( faceIdx * pointsPer + pointIdx );
        }

        public void ResizeResources() {
            ProjMatrix = Matrix.PerspectiveFovLH( ( float )Math.PI / 4.0f, Width / ( float )Height, 0.1f, 100.0f );
            RenderTex?.Dispose();
            RenderTex = new Texture2D( Device, new Texture2DDescription()
            {
                Format = Format.B8G8R8A8_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = Width,
                Height = Height,
                SampleDescription = new SampleDescription( 1, 0 ),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            } );
            RenderShad?.Dispose();
            RenderShad = new ShaderResourceView( Device, RenderTex );
            RenderView?.Dispose();
            RenderView = new RenderTargetView( Device, RenderTex );

            DepthTex?.Dispose();
            DepthTex = new Texture2D( Device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = Width,
                Height = Height,
                SampleDescription = new SampleDescription( 1, 0 ),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            } );
            DepthView?.Dispose();
            DepthView = new DepthStencilView( Device, DepthTex );
        }

        private float Clamp( float value, float min, float max ) {
            return value > max ? max : value < min ? min : value;
        }

        public void Drag(Vec2 newPos, bool rotate ) {
            if( IsDragging ) {
                if( rotate ) {
                    Yaw += ( newPos.X - LastMousePos.X ) * 0.01f;
                    Pitch += -( newPos.Y - LastMousePos.Y ) * 0.01f;
                    Pitch = Clamp( Pitch, -1.55f, 1.55f );
                }
                else {
                    Position.Y += ( newPos.Y - LastMousePos.Y ) * 0.01f;
                }
                UpdateViewMatrix();
            }
            IsDragging = true;
            LastMousePos = new Vector2( newPos.X, newPos.Y );
        }

        public void Zoom(float mouseWheel ) {
            if(mouseWheel != 0 ) {
                Distance += mouseWheel * 0.2f;
                UpdateViewMatrix();
            }
        }

        public void UpdateViewMatrix() {
            var lookRotation = Quaternion.RotationYawPitchRoll( Yaw, Pitch, 0f );
            Vector3 lookDirection = Vector3.Transform( -Vector3.UnitZ, lookRotation );
            ViewMatrix = Matrix.LookAtLH( Position - Distance * lookDirection, Position, Vector3.UnitY );
            Draw();
        }

        public void UpdateDraw() {
            if( !FirstModel ) {
                FirstModel = true;
                UpdateViewMatrix();
            }
            else {
                Draw();
            }
        }

        public abstract void OnDraw();

        public void Draw() {
            Manager.BeforeDraw( out var oldState, out var oldRenderViews, out var oldDepthStencilView );

            var viewProj = Matrix.Multiply( ViewMatrix, ProjMatrix );
            var worldViewProj = LocalMatrix * viewProj;
            worldViewProj.Transpose();
            Ctx.UpdateSubresource( ref worldViewProj, WorldBuffer );

            var renderSize = new Vector4( Width, Height, 0, 0 );
            Ctx.UpdateSubresource( ref renderSize, RendersizeBuffer );

            Ctx.OutputMerger.SetTargets( DepthView, RenderView );
            Ctx.ClearDepthStencilView( DepthView, DepthStencilClearFlags.Depth, 1.0f, 0 );
            Ctx.ClearRenderTargetView( RenderView, new Color4( 0.3f, 0.3f, 0.3f, 1.0f ) );

            Ctx.Rasterizer.SetViewport( new Viewport( 0, 0, Width, Height, 0.0f, 1.0f ) );
            Ctx.Rasterizer.State = RState;

            OnDraw();

            Ctx.Flush();

            Manager.AfterDraw( oldState, oldRenderViews, oldDepthStencilView );
        }

        public abstract void OnDispose();
        public void Dispose() {
            RState?.Dispose();
            RenderTex?.Dispose();
            RenderShad?.Dispose();
            RenderView?.Dispose();
            DepthTex?.Dispose();
            DepthView?.Dispose();
            WorldBuffer?.Dispose();
            RendersizeBuffer?.Dispose();

            OnDispose();
        }
    }
}
