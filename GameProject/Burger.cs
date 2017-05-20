using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameProject
{
    /// <summary>
    /// A burger
    /// </summary>
    public class Burger
    {
        #region Fields

        // graphic and drawing info
        Texture2D sprite;
        Rectangle drawRectangle;

        // burger stats
        int health = 100;

        // shooting support
        bool canShoot = true;
        int elapsedCooldownMilliseconds = 0;

        // sound effect
        SoundEffect shootSound;

        //ButtonState previousLeftButtonState = ButtonState.Released;
        #endregion

        #region Constructors

        /// <summary>
        ///  Constructs a burger
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="spriteName">the sprite name</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        /// <param name="shootSound">the sound the burger plays when shooting</param>
        public Burger(ContentManager contentManager, string spriteName, int x, int y,
            SoundEffect shootSound)
        {
            LoadContent(contentManager, spriteName, x, y);
            this.shootSound = shootSound;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collision rectangle for the burger
        /// </summary>
        public Rectangle CollisionRectangle
        {
            get { return drawRectangle; }
        }

        public int Health
        {
            get { return health; }
            set {
                if (value < 0)
                    health = 0;
                else
                    health = value;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates the burger's location based on mouse. Also fires 
        /// french fries as appropriate
        /// </summary>
        /// <param name="gameTime">game time</param>
        /// <param name="mouse">the current state of the mouse</param>
        //public void Update(GameTime gameTime, MouseState mouse)
        //{
        //    // burger should only respond to input if it still has health
        //    if (health > 0)
        //    {
        //        // move burger using mouse
        //        drawRectangle.X = mouse.X - sprite.Width/2;
        //        drawRectangle.Y = mouse.Y - sprite.Height/2;

        //        // clamp burger in window
        //        if (drawRectangle.X < 0)
        //        {
        //            drawRectangle.X = 0;
        //        }else if (drawRectangle.X > GameConstants.WindowWidth)
        //        {
        //            drawRectangle.X = GameConstants.WindowWidth - sprite.Width;
        //        }

        //        if (drawRectangle.Y < 0)
        //        {
        //            drawRectangle.Y = 0;
        //        }else if (drawRectangle.Y > GameConstants.WindowHeight)
        //        {
        //            drawRectangle.Y = GameConstants.WindowHeight - sprite.Height;
        //        }

        //        if (mouse.LeftButton == ButtonState.Pressed && canShoot)
        //        {
        //            // control fire rate
        //            canShoot = false;

        //            // create french fries projectile
        //            Texture2D projectileSprite = Game1.GetProjectileSprite(ProjectileType.FrenchFries);
        //            int xLocation = drawRectangle.Center.X;
        //            int yLocation = drawRectangle.Top - GameConstants.FrenchFriesProjectileOffset;
        //            float projectileSpeed = GameConstants.FrenchFriesProjectileSpeed;
        //            Projectile fryProjectile = new Projectile(ProjectileType.FrenchFries, projectileSprite, 
        //                xLocation, yLocation, projectileSpeed);

        //            // add projectile to game
        //            Game1.AddProjectile(fryProjectile);
        //        }
        //    }

        //    // capture previous state for left button
        //    //previousLeftButtonState = mouse.LeftButton;


        //    // update shooting allowed
        //    // timer concept (for animations) introduced in Chapter 7
        //    // shoot if appropriate 
        //    if (canShoot == false)
        //    {
        //        elapsedCooldownMilliseconds += gameTime.ElapsedGameTime.Milliseconds;

        //        // allow to fire again if cooldown is satisfied or left mouse button is released
        //        if (elapsedCooldownMilliseconds > GameConstants.BurgerTotalCooldownMilliseconds || mouse.LeftButton == ButtonState.Released)
        //        {
        //            canShoot = true;
        //            elapsedCooldownMilliseconds = 0;
        //        }
        //    }
        //}
        /// <summary>
        /// Updates the burger's location based on keys pressed. Also fires 
        /// french fries as appropriate using space bar
        /// </summary>
        /// <param name="gameTime">game time</param>
        /// <param name="keyboard">the current state of the keyboard</param>
        public void Update (GameTime gameTime, KeyboardState keyboard)
        {
            if (health > 0)
            {
                // move burger using keyboard
                if (keyboard.IsKeyDown(Keys.D))
                {
                    drawRectangle.X += GameConstants.BurgerMovementAmount;
                }else if (keyboard.IsKeyDown(Keys.A))
                {
                    drawRectangle.X -= GameConstants.BurgerMovementAmount;
                }else if (keyboard.IsKeyDown(Keys.S))
                {
                    drawRectangle.Y += GameConstants.BurgerMovementAmount;
                }else if (keyboard.IsKeyDown(Keys.W))
                {
                    drawRectangle.Y -= GameConstants.BurgerMovementAmount;
                }

                // clamp burger in the window
                if (drawRectangle.Left < 0)
                {
                    drawRectangle.X = 0;
                }else if (drawRectangle.Right > GameConstants.WindowWidth)
                {
                    drawRectangle.X = GameConstants.WindowWidth - sprite.Width;
                }
                if (drawRectangle.Top < 0)
                {
                    drawRectangle.Y = 0;
                }else if (drawRectangle.Bottom > GameConstants.WindowHeight)
                {
                    drawRectangle.Y = GameConstants.WindowHeight - sprite.Height;
                }

                // shoot projectiles using keyboard
                if (keyboard.IsKeyDown(Keys.Space) && canShoot)
                {
                    // control rate of fire
                    canShoot = false;

                    // create projectile to be fired
                    Texture2D frenchFrySprite = Game1.GetProjectileSprite(ProjectileType.FrenchFries);
                    int xLocation = drawRectangle.Center.X;
                    int yLocation = drawRectangle.Top - GameConstants.FrenchFriesProjectileOffset;
                    Projectile projectile = new Projectile(ProjectileType.FrenchFries, 
                        frenchFrySprite,
                        xLocation, yLocation, 
                        GameConstants.FrenchFriesProjectileSpeed);

                    // add projectile to game
                    Game1.AddProjectile(projectile);
                    shootSound.Play();
                }

                // update shooting allowed
                // timer concept (for animations) introduced in Chapter 7
                // shoot if appropriate 
                if (canShoot == false)
                {
                    elapsedCooldownMilliseconds += gameTime.ElapsedGameTime.Milliseconds;

                    // allow to fire again if cool down is satisfied or spacebar is up
                    if (elapsedCooldownMilliseconds > GameConstants.BurgerTotalCooldownMilliseconds
                        || keyboard.IsKeyUp(Keys.Space))
                    {
                        elapsedCooldownMilliseconds = 0;
                        canShoot = true;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the burger
        /// </summary>
        /// <param name="spriteBatch">the sprite batch to use</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, drawRectangle, Color.White);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads the content for the burger
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        /// <param name="spriteName">the name of the sprite for the burger</param>
        /// <param name="x">the x location of the center of the burger</param>
        /// <param name="y">the y location of the center of the burger</param>
        private void LoadContent(ContentManager contentManager, string spriteName,
            int x, int y)
        {
            // load content and set remainder of draw rectangle
            sprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle = new Rectangle(x - sprite.Width / 2,
                y - sprite.Height / 2, sprite.Width,
                sprite.Height);
        }

        #endregion
    }
}
