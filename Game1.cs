using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;


namespace idleGame;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    Texture2D playerTexture, coinTexture;
    Vector2 playerPosition;
    Vector2 playerVelocity;
    List<Vector2> coinPositions;
    bool isJumping = false;
    float gravity = 0.5f;
    float jumpForce;

    SpriteFont scorefont;
    int Gold = 0;

    //Coin generation
    Random rng = new Random();
    float spawnTimer = 0f;
    float spawnInterval = 10.0f;

    //Sword-Run Sprite
    List<Texture2D> runFrames = new();
    Animation runAnimation;

    // Sword-Jump Sprite
    List<Texture2D> jumpFrames = new();
    Animation jumpAnimation;

    // Sword-Combo Sprite
    List<Texture2D> comboFrames = new();
    Animation comboAnimation;
    // Sword-Air-Attack Sprite
    List<Texture2D> airAttackFrames = new();
    Animation airAttackAnimation;



    // Player
    float scale;
    Rectangle playerRect;
    Animation currentAnimation;



    //debug hit box pixel
    //Texture2D whitePixel;


    //Boden
    Texture2D groundTexture;
    List<Vector2> groundPositions = new();
    float ScrollSpeed = 2f;
    float groundScale;
    float groundY;
    Texture2D grassTexture;


    //Hintergrund
    Texture2D backgroundTexture;
    float scrollOffset = 0f;


    //Enemies
    float enemyScale;
    List<Enemy> enemies = new();

    //Orcs
    List<Texture2D> orcFrames = new();
    Animation orcIdleAnimation;
    Animation orcDeathAnimation;
    List<Vector2> orcPositions = new();





    






    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        //Fenster Größe
        Window.IsBorderless = true;
        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        graphics.ApplyChanges();

        playerPosition = new Vector2(100, 300);
            coinPositions = new List<Vector2>
            {
                new Vector2(400, 250),
                new Vector2(600, 200),
                new Vector2(800, 300)
            };
            base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        playerTexture = Content.Load<Texture2D>("Stickman");
        coinTexture = Content.Load<Texture2D>("Coin");
        scorefont = Content.Load<SpriteFont>("ScoreFont");

        //Load Sword-run Sprite
        runFrames = new();
        for (int i = 17; i <= 24; i++) // 8 Lauf-Frames
        {
            string name = $"Sprites/Run/sword_run_00{i}";
            //Console.WriteLine("Loading: " + name); // debug
            runFrames.Add(Content.Load<Texture2D>(name));
        }
        runAnimation = new Animation(runFrames);

        //Load Sword-Jump Sprite
        for (int i = 43; i <= 46; i++) // z. B. sword_jump_0043 bis sword_jump_0046
        {
            string name = $"Sprites/Jump/sword_jump_00{i}";
            jumpFrames.Add(Content.Load<Texture2D>(name));
        }
        jumpAnimation = new Animation(jumpFrames, frameTime: 0.12f); // etwas langsamer

        //Load Sword-Combo Sprite
        for (int i = 65; i <= 75; i++)
        {
            string name = $"Sprites/Combo/sword_combo_00{i}";
            comboFrames.Add(Content.Load<Texture2D>(name));
        }
        comboAnimation = new Animation(comboFrames, 0.08f); // TODO evtl. Framezeit anpassen
        //Load Sword-Air-Attack Sprite
        for (int i = 62; i <= 64; i++)
        {
            string name = $"Sprites/Air_Attack/sword_air_attack_00{i}";
            airAttackFrames.Add(Content.Load<Texture2D>(name));
        }
        airAttackAnimation = new Animation(airAttackFrames, 0.08f); // Timing evtl. anpassen




        int screenHeight = GraphicsDevice.Viewport.Height;
        int spriteOriginalHeight = runFrames[0].Height;
        scale = (screenHeight * 0.4f) / runFrames[0].Height;





        //debug hit box frame
        //whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        //whitePixel.SetData(new[] { Color.White });

        //Boden
        int screenWidth = GraphicsDevice.Viewport.Width;
        groundTexture = Content.Load<Texture2D>("groundBig");
        groundScale = (screenHeight * 0.15f) / groundTexture.Height;
        float scaledWidth = groundTexture.Width * groundScale;
        int tilesNeeded = (int)Math.Ceiling(screenWidth / scaledWidth) + 2;
        groundY = screenHeight - groundTexture.Height * groundScale;
        for (int i = 0; i < tilesNeeded; i++)
        {
            float x = (float)Math.Round(i * scaledWidth);
            groundPositions.Add(new Vector2(x, groundY));
        }
        grassTexture = Content.Load<Texture2D>("grass_foreground");

        Console.WriteLine($"Ground Width: {groundTexture.Width}");
        Console.WriteLine($"Grass Width: {grassTexture.Width}");

        //Player on Ground
        float scaledPlayerHeight = runFrames[0].Height * scale;
        playerPosition = new Vector2(100, groundY - scaledPlayerHeight);

        jumpForce = -(screenHeight * 0.0175f);

        //Hintergrund
        backgroundTexture = Content.Load<Texture2D>("background1");

        //Player animation
        currentAnimation = runAnimation;


        //Enemy sprites
        enemyScale = scale * 0.75f;
        //Orc-Idle
        orcFrames = new();
        for (int i = 0; i <= 5; i++)
        {
            string name = $"Sprites/Orc_Sprites/orc_0_{i}";
            orcFrames.Add(Content.Load<Texture2D>(name));
        }
        orcIdleAnimation = new Animation(orcFrames, 0.15f);

        //Orc-Death
        List<Texture2D> orcDeathFrames = new();
        for (int i = 0; i <= 3; i++)
        {
            string name = $"Sprites/Orc_Sprites/orc_5_{i}";
            orcDeathFrames.Add(Content.Load<Texture2D>(name));
        }
        orcDeathAnimation = new Animation(orcDeathFrames, 0.1f)
        {
            Loop = false
        };




    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Jump
        if (Keyboard.GetState().IsKeyDown(Keys.Space) && !isJumping)
        {
            playerVelocity.Y = jumpForce;
            isJumping = true;
        }

        //Generate Coins
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        spawnTimer += dt;


        // Coins außerhalb des Bildschirms löschen
        for (int i = coinPositions.Count - 1; i >= 0; i--)
            if (coinPositions[i].X < -32)
                        coinPositions.RemoveAt(i);

        // Neue Gruppe spawnen
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

           
            float baseX = GraphicsDevice.Viewport.Width;
            //float baseY = groundY-200f;
            float playerJumpHeight = jumpForce * jumpForce / (2 * gravity); // klassisch: v² / 2g
            float maxY = groundY - (90 * scale + playerJumpHeight * 0.7f);
            float baseY = (float)(maxY + rng.NextDouble() * 100); // z. B. 30 px Varianz

            bool spawnEnemy = rng.NextDouble() < 0.25;//25% chance for enemy instead of coins
            if (spawnEnemy)
            {
                var enemy = new Enemy(orcIdleAnimation.Clone(),orcDeathAnimation.Clone(), new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, groundY-110), ScrollSpeed, enemyScale);
                enemies.Add(enemy);
            }
            else
            {
                int groupType = rng.Next(3); // 0 = Linie, 1 = Treppe, 2 = Block
                switch (groupType)
                {
                    case 0: // Horizontale Linie
                        for (int i = 0; i < 5; i++)
                            coinPositions.Add(new Vector2(baseX + i * 40, baseY));
                        break;

                    case 1: // Treppenform
                        for (int i = 0; i < 5; i++)
                            coinPositions.Add(new Vector2(baseX + i * 40, baseY - i * 20));
                        break;

                    case 2: // 3x3 Block
                        for (int row = 0; row < 3; row++)
                            for (int col = 0; col < 3; col++)
                                coinPositions.Add(new Vector2(baseX + col * 32, baseY + row * 32));
                        break;
                }
            }

            
        }


        //Coin-Player Collision
        int frameWidth = runFrames[0].Width;
        int frameHeight = runFrames[0].Height;
        playerRect = new Rectangle(
                (int)(playerPosition.X + 200 * scale), // X-Verschiebung ins Zentrum
                (int)(playerPosition.Y + 200 * scale),  // Y-Verschiebung für Kopf/Beine
                (int)(140 * scale),
                (int)(180 * scale)
            );
        for (int i = coinPositions.Count - 1; i >= 0; i--)
        {
            Rectangle coinRect = new Rectangle((int)coinPositions[i].X, (int)coinPositions[i].Y, 32, 32);
            if (coinRect.Intersects(playerRect))
            {
                coinPositions.RemoveAt(i);
                Gold++;
            }
        }
        foreach (var enemy in enemies)
        {
            Rectangle enemyRect = new Rectangle((int)enemy.Position.X + 130, (int)enemy.Position.Y, 40, 80); // TODO Größe ggf. anpassen

            if (enemy.IsAlive && playerRect.Intersects(enemyRect))
            {
                enemy.IsAlive = false;
            }
        }


        // Gravity + jumping
        playerVelocity.Y += gravity;
        playerPosition += playerVelocity;
        if (playerPosition.Y >= groundY-270*scale)
        {
            playerPosition.Y = groundY-270*scale;
            playerVelocity.Y = 0;
            isJumping = false;
        }

        //Track if enemy nearby
        bool nearEnemy = false;
        foreach (var enemy in enemies)
        {
            float distance = Vector2.Distance(playerPosition, enemy.Position);
            if (distance < 200f) // Abstand zu den gegnern ab dem angegriffen wird
            {
                nearEnemy = true;
                break;
            }
        }

        if (nearEnemy)
        {
            if (isJumping)
            {
                currentAnimation = airAttackAnimation;
            }
            else
            {
                currentAnimation = comboAnimation;
            }
        }
        else
        {
            if (isJumping)
            {
                currentAnimation = jumpAnimation;
            }
            else
            {
                currentAnimation = runAnimation;
            }
        }
        currentAnimation.Update(gameTime);


        // Move coins
        for (int i = 0; i < coinPositions.Count; i++)
        {
            coinPositions[i] = new Vector2(coinPositions[i].X - ScrollSpeed, coinPositions[i].Y);
        }

        // Move Boden
        for (int i = 0; i < groundPositions.Count; i++)
        {
            groundPositions[i] = new Vector2((float)Math.Round(groundPositions[i].X - ScrollSpeed), groundPositions[i].Y);
        }

        // Wiederverwendung (Recycling) am rechten Rand
        if (groundPositions.Count > 0 && groundPositions[0].X + groundTexture.Width * groundScale < 0)
        {
            Vector2 last = groundPositions[groundPositions.Count - 1];
            groundPositions.RemoveAt(0);
            groundPositions.Add(new Vector2(last.X + groundTexture.Width * groundScale, last.Y));
        }


        //Hintergrund Loop
        scrollOffset += ScrollSpeed;
        float bgWidth = backgroundTexture.Width;


        //Enemy update
        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            enemies[i].Update(gameTime);
            if (enemies[i].ReadyToBeRemoved(dt))
                enemies.RemoveAt(i);
        }



        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(30, 30, 30) ); // Hintergrund
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        //Hintergrund
        float parallaxFactor = 0.5f;
        float bgScroll = scrollOffset * parallaxFactor;

        int bgWidth = backgroundTexture.Width;
        float bgX = -(scrollOffset * parallaxFactor) % bgWidth;
        if (bgX > 0) bgX -= bgWidth; // Verhindert sichtbare Linie bei Übergang

        for (float x = bgX; x < graphics.PreferredBackBufferWidth; x += bgWidth)
        {
            spriteBatch.Draw(backgroundTexture, new Vector2((int)Math.Round(x), 0), Color.White);
        }
        
        //Boden
        foreach (var pos in groundPositions)
        {
            int roundedX = (int)Math.Round(pos.X);
            int roundedY = (int)Math.Round(pos.Y);

            spriteBatch.Draw(groundTexture,
                new Vector2(roundedX, roundedY),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                groundScale,
                SpriteEffects.None,
                0f
            );
            spriteBatch.Draw(grassTexture,
                new Vector2(roundedX, roundedY + 90),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                groundScale,
                SpriteEffects.None,
                0f
            );
        }

        // Coins
        foreach (var coin in coinPositions)
        {
            Rectangle targetRect = new Rectangle((int)coin.X, (int)coin.Y, 32, 32);
            spriteBatch.Draw(coinTexture, targetRect, Color.White);
        }

        //Enemies
        foreach (var enemy in enemies)
            enemy.Draw(spriteBatch);



        // Spieler
        if (runFrames.Count > 0)
            currentAnimation.Draw(spriteBatch, playerPosition, scale);
        else
            Console.WriteLine("No frames to draw!");




        // Score
        spriteBatch.DrawString(scorefont, $"Gold: {Gold}", new Vector2(10, 10), Color.Black);



        spriteBatch.End();
        base.Draw(gameTime);
    }
}
