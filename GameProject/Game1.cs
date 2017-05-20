using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Vector2 centerBottomLocation = new Vector2(
            GameConstants.WindowWidth / 2,
            GameConstants.WindowHeight * 0.875f);


        // game objects. Using inheritance would make this
        // easier, but inheritance isn't a GDD 1200 topic
        Burger burger;
        List<TeddyBear> bears = new List<TeddyBear>();
        static List<Projectile> projectiles = new List<Projectile>();
        List<Explosion> explosions = new List<Explosion>();

        // projectile and explosion sprites. Saved so they don't have to
        // be loaded every time projectiles or explosions are created
        static Texture2D frenchFriesSprite;
        static Texture2D teddyBearProjectileSprite;
        static Texture2D explosionSpriteStrip;

        // scoring support
        int score = 0;
        string scoreString = GameConstants.ScorePrefix + 0;

        // health support
        string healthString = GameConstants.HealthPrefix +
            GameConstants.BurgerInitialHealth;
        bool burgerDead = false;

        // text display support
        SpriteFont font;

        // sound effects
        SoundEffect burgerDamage;
        SoundEffect burgerDeath;
        SoundEffect burgerShot;
        SoundEffect explosionSound;
        SoundEffect teddyBounce;
        SoundEffect teddyShot;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution
            graphics.PreferredBackBufferWidth = GameConstants.WindowWidth;
            graphics.PreferredBackBufferHeight = GameConstants.WindowHeight;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            RandomNumberGenerator.Initialize();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // load audio content
            burgerDamage = Content.Load<SoundEffect>(@"audio\BurgerDamage");
            burgerDeath = Content.Load<SoundEffect>(@"audio\BurgerDeath");
            burgerShot = Content.Load<SoundEffect>(@"audio\BurgerShot");
            explosionSound = Content.Load<SoundEffect>(@"audio\Explosion");
            teddyBounce = Content.Load<SoundEffect>(@"audio\TeddyBounce");
            teddyShot = Content.Load<SoundEffect>(@"audio\TeddyShot");

            // load sprite font
            font = Content.Load<SpriteFont>(@"fonts\Arial20");

            // load projectile and explosion sprites
            frenchFriesSprite = Content.Load<Texture2D>(@"graphics\frenchfries");
            teddyBearProjectileSprite = Content.Load<Texture2D>(@"graphics\teddybearprojectile");
            explosionSpriteStrip = Content.Load<Texture2D>(@"graphics\explosion");

            // add initial game objects
            burger = new Burger(Content, @"graphics\burger", (int)centerBottomLocation.X, (int)centerBottomLocation.Y, burgerShot);
            for (int i = 0; i < GameConstants.MaxBears; i++) 
                SpawnBear();

            // set initial health and score strings
            healthString = GameConstants.HealthPrefix + burger.Health;
            scoreString = GameConstants.ScorePrefix + score;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // get current mouse state and update burger
            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            //burger.Update(gameTime, mouse);
            burger.Update(gameTime, keyboard);

            // update other game objects
            foreach (TeddyBear bear in bears)
            {
                bear.Update(gameTime);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Update(gameTime);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Update(gameTime);
            }

            // check and resolve collisions between teddy bears
            for (int i = 0; i < bears.Count; i++)
            {
                for (int j = i + 1; j < bears.Count; j++)
                {
                    // bear1 and bear2 active, resolve collisions
                    if (bears[i].Active && bears[j].Active)
                    {
                        CollisionResolutionInfo collisionInfo = CollisionUtils.CheckCollision(gameTime.ElapsedGameTime.Milliseconds,
                            GameConstants.WindowWidth, GameConstants.WindowHeight, bears[i].Velocity, bears[i].CollisionRectangle, bears[j].Velocity, bears[j].CollisionRectangle);
                        if (collisionInfo != null)
                        {
                            if (collisionInfo.FirstOutOfBounds)
                            {
                                bears[i].Active = false;
                            }
                            else
                            {
                                bears[i].DrawRectangle = collisionInfo.FirstDrawRectangle;
                                bears[i].Velocity = collisionInfo.FirstVelocity;
                            }

                            if (collisionInfo.SecondOutOfBounds)
                            {
                                bears[j].Active = false;
                            }
                            else
                            {
                                bears[j].DrawRectangle = collisionInfo.SecondDrawRectangle;
                                bears[j].Velocity = collisionInfo.SecondVelocity;
                            }

                            teddyBounce.Play();
                        }
                    }
                }
            }

            // check and resolve collisions between burger and teddy bears
            foreach(TeddyBear bear in bears)
            {
                if(bear.Active && bear.CollisionRectangle.Intersects(burger.CollisionRectangle))
                {
                    // process burger damaged by teddy bear
                    burger.Health -= GameConstants.BearDamage;
                    bear.Active = false;
                    Explosion explosion = new Explosion(explosionSpriteStrip, bear.Location.X, bear.Location.Y, explosionSound);
                    explosions.Add(explosion);
                    healthString = GameConstants.HealthPrefix + burger.Health;
                    burgerDamage.Play();

                    // check burger health
                    CheckBurgerKill();
                }
            }

            // check and resolve collisions between burger and projectiles
            foreach(Projectile projectile in projectiles)
            {
                if(projectile.Type == ProjectileType.TeddyBear && projectile.Active
                    && projectile.CollisionRectangle.Intersects(burger.CollisionRectangle))
                {
                    // process burger damaged by projectile
                    projectile.Active = false;
                    burger.Health -= GameConstants.TeddyBearProjectileDamage;
                    healthString = GameConstants.HealthPrefix + burger.Health;
                    burgerDamage.Play();

                    // check burger health
                    CheckBurgerKill();
                }
            }

            // check and resolve collisions between teddy bears and projectiles
            foreach (TeddyBear bear in bears)
            {
                foreach(Projectile projectile in projectiles)
                {
                    if (bear.Active && projectile.Active && projectile.Type == ProjectileType.FrenchFries 
                        && bear.CollisionRectangle.Intersects(projectile.CollisionRectangle))
                    {
                        // colliding, then disable both bear and projectile
                        bear.Active = false;
                        projectile.Active = false;
                        int explosionX = bear.CollisionRectangle.Center.X;
                        int explosionY = bear.CollisionRectangle.Center.Y;

                        // Blow up teddy bear
                        Explosion explosion = new Explosion(explosionSpriteStrip, explosionX, explosionY, explosionSound);
                        explosions.Add(explosion);

                        // score points
                        score += GameConstants.BearPoints;
                        scoreString = GameConstants.ScorePrefix + score;
                    }
                }
            }
            // clean out inactive teddy bears and add new ones as necessary
            for (int i = bears.Count - 1; i >= 0; i--)
            {
                if (!bears[i].Active)
                    bears.RemoveAt(i);
            }
            // spawn enough teddy bears to fill the ranks
            while (bears.Count <= GameConstants.MaxBears)
            {
                SpawnBear();
            }

            // clean out inactive projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (!projectiles[i].Active)
                    projectiles.RemoveAt(i);
            }

            // clean out finished explosions
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                if (explosions[i].Finished)
                    explosions.RemoveAt(i);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // draw game objects
            burger.Draw(spriteBatch);
            foreach (TeddyBear bear in bears)
            {
                bear.Draw(spriteBatch);
            }
            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }
            foreach (Explosion explosion in explosions)
            {
                explosion.Draw(spriteBatch);
            }

            // draw score and health
            spriteBatch.DrawString(font, healthString, GameConstants.HealthLocation, Color.White);
            spriteBatch.DrawString(font, scoreString, GameConstants.ScoreLocation, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Public methods

        /// <summary>
        /// Gets the projectile sprite for the given projectile type
        /// </summary>
        /// <param name="type">the projectile type</param>
        /// <returns>the projectile sprite for the type</returns>
        public static Texture2D GetProjectileSprite(ProjectileType type)
        {
            // replace with code to return correct projectile sprite based on projectile type
            if (type == ProjectileType.FrenchFries)
                return frenchFriesSprite;
            else if (type == ProjectileType.TeddyBear)
                return teddyBearProjectileSprite;
            else
                return null;
        }

        /// <summary>
        /// Adds the given projectile to the game
        /// </summary>
        /// <param name="projectile">the projectile to add</param>
        public static void AddProjectile(Projectile projectile)
        {
            projectiles.Add(projectile);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Spawns a new teddy bear at a random location
        /// </summary>
        private void SpawnBear()
        {
            // generate random location
            int xLocation = GetRandomLocation(0 + GameConstants.SpawnBorderSize, 
                GameConstants.WindowWidth - GameConstants.SpawnBorderSize);
            int yLocation = GetRandomLocation(0 + GameConstants.SpawnBorderSize, 
                GameConstants.WindowHeight - GameConstants.SpawnBorderSize);

            // generate random speed
            float speed = GameConstants.MinBearSpeed + RandomNumberGenerator.NextFloat(GameConstants.BearSpeedRange);

            // generate random angle
            float angle = RandomNumberGenerator.NextFloat((float)Math.PI);

            // generate random velocity
            // sine angle * speed = y velocity 
            // cos angle * speed = x velocity
            float xVelocity = (float)Math.Sin(angle) * speed;
            float yVelocity = (float)Math.Cos(angle) * speed;
            Vector2 velocity = new Vector2(xVelocity, yVelocity);

            // create new bear
            // make sure we don't spawn into a collision
            TeddyBear newBear = new TeddyBear(Content, @"graphics\teddybear", xLocation, yLocation, velocity, teddyBounce,teddyShot);

            // ensure teddy bears don't spawn into collisions
            List<Rectangle> allCollisions = GetCollisionRectangles();
            while(!CollisionUtils.IsCollisionFree(newBear.CollisionRectangle, allCollisions))
            {
                int newXLocation = GetRandomLocation(0 + GameConstants.SpawnBorderSize, GameConstants.WindowWidth -
                    GameConstants.SpawnBorderSize);
                int newYLocation = GetRandomLocation(0 + GameConstants.SpawnBorderSize, GameConstants.WindowHeight -
                    GameConstants.SpawnBorderSize);
                newBear.X = newXLocation;
                newBear.Y = newYLocation;
            }

            // add new bear to list
            bears.Add(newBear);
        }

        /// <summary>
        /// Gets a random location using the given min and range
        /// </summary>
        /// <param name="min">the minimum</param>
        /// <param name="range">the range</param>
        /// <returns>the random location</returns>
        private int GetRandomLocation(int min, int range)
        {
            return min + RandomNumberGenerator.Next(range);
        }

        /// <summary>
        /// Gets a list of collision rectangles for all the objects in the game world
        /// </summary>
        /// <returns>the list of collision rectangles</returns>
        private List<Rectangle> GetCollisionRectangles()
        {
            List<Rectangle> collisionRectangles = new List<Rectangle>();
            collisionRectangles.Add(burger.CollisionRectangle);
            foreach (TeddyBear bear in bears)
            {
                collisionRectangles.Add(bear.CollisionRectangle);
            }
            foreach (Projectile projectile in projectiles)
            {
                collisionRectangles.Add(projectile.CollisionRectangle);
            }
            foreach (Explosion explosion in explosions)
            {
                collisionRectangles.Add(explosion.CollisionRectangle);
            }
            return collisionRectangles;
        }

        /// <summary>
        /// Checks to see if the burger has just been killed
        /// </summary>
        private void CheckBurgerKill()
        {
            if(burger.Health == 0 && !burgerDead)
            {
                burgerDead = true;
                burgerDeath.Play();
            }
        }

        #endregion
    }
}
