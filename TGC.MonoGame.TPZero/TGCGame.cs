using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Models;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Clase principal del juego.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        private GraphicsDeviceManager Graphics { get; }
        private CityScene City { get; set; }
        private Model CarModel { get; set; }
        private Matrix CarWorld { get; set; }
        private FollowCamera FollowCamera { get; set; }


        private float Speed = 0f;

        private float Acceleration = 1f;

        private float HoldingTimeW = 0f;

        private float HoldingTimeS = 0f;
        private Vector3 CurrentDirection = Vector3.Forward;

        private Quaternion CarRotation = Quaternion.Identity;
        private const float RotationSpeed = (float)Math.PI / 2;

        private bool isJumping = true;
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Se encarga de la configuracion y administracion del Graphics Device.
            Graphics = new GraphicsDeviceManager(this);

            // Carpeta donde estan los recursos que vamos a usar.
            Content.RootDirectory = "Content";

            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        public Vector3 Position
        {
            get;
            set;
        }
        /// <summary>
        ///     Llamada una vez en la inicializacion de la aplicacion.
        ///     Escribir aca todo el codigo de inicializacion: Todo lo que debe estar precalculado para la aplicacion.
        /// </summary>
        protected override void Initialize()
        {
            // Enciendo Back-Face culling.
            // Configuro Blend State a Opaco.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            // Creo una camaar para seguir a nuestro auto.
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);

            // Configuro la matriz de mundo del auto.
            CarWorld = Matrix.Identity;



            base.Initialize();
        }

        /// <summary>
        ///     Llamada una sola vez durante la inicializacion de la aplicacion, luego de Initialize, y una vez que fue configurado GraphicsDevice.
        ///     Debe ser usada para cargar los recursos y otros elementos del contenido.
        /// </summary>
        protected override void LoadContent()
        {
            // Creo la escena de la ciudad.
            City = new CityScene(Content);

            // La carga de contenido debe ser realizada aca.
            CarModel = Content.Load<Model>(ContentFolder3D + "scene/car");

            base.LoadContent();
        }

        /// <summary>
        ///     Es llamada N veces por segundo. Generalmente 60 veces pero puede ser configurado.
        ///     La logica general debe ser escrita aca, junto al procesamiento de mouse/teclas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {


            // Capturo (?) el estado del teclado.
            var keyboardState = Keyboard.GetState();
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Salgo del juego.
                Exit();
            }



            
            if (keyboardState.IsKeyDown(Keys.A))
            {
                CarRotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, RotationSpeed * elapsedTime); 
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                CarRotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -RotationSpeed * elapsedTime);
            }

            CurrentDirection = Vector3.Transform(Vector3.Forward, CarRotation);
            
            // La logica debe ir aca.
            if (keyboardState.IsKeyDown(Keys.W))
            {
                HoldingTimeW += elapsedTime;
                HoldingTimeS = 0f;
                Speed += Acceleration * HoldingTimeW;
                
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                HoldingTimeS += elapsedTime;
                HoldingTimeW = 0f;
                Speed -= Acceleration * HoldingTimeS;
                
            }

            if (keyboardState.IsKeyDown(Keys.Space) && !isJumping)
            {
                Vector3 vec = new Vector3(0f,80f,0f);
                Position +=  vec;
                if(Position.Y>=80f){
                    Position = new Vector3(Position.X,80f,Position.Z);
                }
                isJumping=true;
            }
            else{
                
                Vector3 vec = new Vector3(0f,-3f,0f);
                Position +=  vec;
                if(Position.Y<=0f){
                    Position = new Vector3(Position.X,1f,Position.Z);
                    isJumping=false;
                }
            }

             if (keyboardState.IsKeyDown(Keys.LeftShift)){
                Acceleration+=elapsedTime;
             }
             if (keyboardState.IsKeyDown(Keys.LeftControl)){
                Acceleration-=elapsedTime;
                if(Acceleration<=1f){
                    Acceleration=1f;
                }
             }
            
            
            
            
            if (Speed < 0) Speed = 0;
            Position += CurrentDirection * Speed * elapsedTime;

            CarWorld = Matrix.CreateFromQuaternion(CarRotation) * Matrix.CreateTranslation(Position)  ;

            // Actualizo la camara, enviandole la matriz de mundo del auto.
            FollowCamera.Update(gameTime, CarWorld);

           // HoldingTimeW = 0f;

            //HoldingTimeS = 0f;

            base.Update(gameTime);
        }


        /// <summary>
        ///     Llamada para cada frame.
        ///     La logica de dibujo debe ir aca.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Limpio la pantalla.
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Dibujo la ciudad.
            City.Draw(gameTime, FollowCamera.View, FollowCamera.Projection);

            // El dibujo del auto debe ir aca.
            CarModel.Draw(CarWorld, FollowCamera.View, FollowCamera.Projection);

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos cargados.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos cargados dessde Content Manager.
            Content.Unload();

            base.UnloadContent();
        }
    }
}