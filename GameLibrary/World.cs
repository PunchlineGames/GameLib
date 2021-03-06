﻿using GameLibrary.Dependencies.Entities;
using GameLibrary.Entities.Systems;
using GameLibrary.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceHordes.Entities.Systems;
using System;
using GameLibrary.Dependencies.Physics.Dynamics;
using GameLibrary.Entities.Components;

namespace GameLibrary
{
    /// <summary>
    /// The world class for all world/games
    /// </summary>
    public abstract class World : EntityWorld
    {
        #region Constructors

        /// <summary>
        /// Constructs a world with a specified gravity.
        /// </summary>
        /// <param name="gravity">The gravity of a world.</param>
        /// <param name="camera"> The camera which views the world.</param>
        /// <param name="spriteBatch">The spritebatch to which the world may render.</param>
        public World(Game Game, Vector2 gravity)
            : base(gravity)
        {
            this.Camera = new Camera(Game.GraphicsDevice);
            this.SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            this.BuildEvents(Game);

            Speed = 1;
        }

        /// <summary>
        /// Constructs a world with no gravity.
        /// </summary>
        public World(Game Game)
            : this(Game, Vector2.Zero)
        {
        }

        #endregion Constructors

        #region Initialization

        /// <summary>
        /// Initializes the world.
        /// </summary>
        public virtual void Initialize()
        {
            this.BuildSystems();
            SystemManager.InitializeAll();
        }

        /// <summary>
        /// Loads the world's content.
        /// </summary>
        /// <param name="Content"></param>
        public virtual void LoadContent(ContentManager Content, params object[] args)
        {
#if DEBUG && WINDOWS//Debug render system
            _DebugSystem.LoadContent(SpriteBatch.GraphicsDevice, Content);
#endif

            //Builds templates in world.

            this.BuildTemplates(Content, args);
            this.BuildEntities(Content, args);
        }

        #region Building

        /// <summary>
        /// Builds all of the systems.
        /// </summary>
        protected virtual void BuildSystems()
        {
            //Default Systems
            _MovementSystem = this.SystemManager.SetSystem(new ParticleMovementSystem(), ExecutionType.Update, 0);

            //Render System
            _RenderSystem = this.SystemManager.SetSystem(new RenderSystem(SpriteBatch, this.Camera), ExecutionType.Draw, 0);
            _AnimationSystem = this.SystemManager.SetSystem(new AnimationSystem(), ExecutionType.Update, 0);

#if DEBUG && WINDOWS
            _DebugSystem = this.SystemManager.SetSystem(new DebugSystem(this), ExecutionType.Draw, 1);
#endif
        }

        /// <summary>
        /// Builds all of the templates in the world.
        /// </summary>
        protected virtual void BuildTemplates(ContentManager Content, params object[] args)
        {
        }

        /// <summary>
        /// Builds all of the entities in the world (initially)
        /// </summary>
        protected virtual void BuildEntities(ContentManager Content, params object[] args)
        {
        }

        protected virtual void BuildEvents(Game Game)
        {
            Game.Exiting += this.OnExit;
        }

        #endregion Building

        #endregion Initialization

        #region Functioning Loop

        /// <summary>
        /// Updates the world
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            this.LoopStart(); //Start the functioning loop
            {
                this.Delta = (int)(gameTime.ElapsedGameTime.TotalMilliseconds / Speed); //set change in time.
                this.SystemManager.UpdateSynchronous(ExecutionType.Update); //Update the entity world.
                this.Step((float)gameTime.ElapsedGameTime.TotalSeconds / Speed); //Update physical world.
                Camera.Update(gameTime); //Finally update the camera.

                for (int i = 0; i < BodyList.Count; i++)
                {
                    if (BodyList[i].UserData == null || (BodyList[i].UserData != null && EntityManager.GetEntity((BodyList[i].UserData as Entity).Id) != (BodyList[i].UserData as Entity)))
                    {
#if DEBUG
                        Console.WriteLine("We have a crisis; a nullification crisis. ");
#endif
                        RemoveBody(BodyList[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Dtaws the world with some gameTime and spriteBatch
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime)
        {
            //Render all of the systems (entities)
            this.SystemManager.UpdateSynchronous(ExecutionType.Draw);
        }

        #endregion Functioning Loop

        #region Properties

        public virtual string Name
        {
            get { return "GameLibrary"; }
        }

        private float _Speed;
        public float Speed
        {
            set
            {
                _Speed = value;
                SoundManager.Pitch = (1 / _Speed) - 1;
            }
            get
            {
                return _Speed;
            }
        }

        #endregion Properties

        #region Fields

        public Camera Camera;
        protected SpriteBatch SpriteBatch;

        //Systems
        protected RenderSystem _RenderSystem;

        protected AnimationSystem _AnimationSystem;
        protected ParticleMovementSystem _MovementSystem;
#if WINDOWS
        protected DebugSystem _DebugSystem;
#endif

        #endregion Fields

        #region Events

        /// <summary>
        /// Called whe the world is exited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void OnExit(object sender, EventArgs args)
        {
#if DEBUG && WINDOWS
            _DebugSystem.Dispose();
#endif
        }

        #endregion Events

        #region Helpers

        #endregion Helpers
    }
}