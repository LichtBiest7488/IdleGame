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
    Vector2 playerPosition;
    Vector2 playerVelocity;

    bool isJumping = false;
    float gravity = 0.5f;
    float jumpForce;

    SpriteFont scorefont;
    int Gold;
    int goldPerCoin;
    int goldPerEnemy;

    float screenHeight;
    float screenWidth;



    //Coin generation
    Texture2D playerTexture, coinTexture;
    List<Vector2> coinPositions;
    Random rng = new Random();
    float spawnTimer = 0f;
    float spawnInterval = 10.0f;
    float coinScale;



    // Player
    float scale;
    Rectangle playerRect;
    Animation currentAnimation;
    float attackDistance;
    List<Texture2D> playerFrames = new();
    //Sword-Run Sprite
    Animation runAnimation;
    // Sword-Jump Sprite
    Animation jumpAnimation;
    // Sword-Combo Sprite
    Animation comboAnimation;
    // Sword-Air-Attack Sprite
    Animation airAttackAnimation;



    //debug hit box pixel
    Texture2D whitePixel;


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
        Window.IsBorderless = false;
        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        graphics.ApplyChanges();
        


        attackDistance = 20f * scale;
        


        playerPosition = new Vector2(100, 300);
        coinPositions = new List<Vector2>();
        Gold = 0;
        goldPerCoin = 1;
        goldPerEnemy = 5;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        this.screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        this.screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        spriteBatch = new SpriteBatch(GraphicsDevice);
        playerTexture = Content.Load<Texture2D>("Stickman");
        
        coinTexture = Content.Load<Texture2D>("Coin");
        coinScale = this.screenHeight * 0.04f / coinTexture.Height; //4% von der bildschirm größe
        scorefont = Content.Load<SpriteFont>("ScoreFont");

        //Load Sword-run Sprite
        playerFrames = SpriteLoader.LoadPlayerFrames(Content, "Sprites/Run/sword_run_", 17, 24);
        runAnimation = new Animation(playerFrames);
        //Load Sword-Jump Sprite
        playerFrames = SpriteLoader.LoadPlayerFrames(Content, "Sprites/Jump/sword_jump_", 43, 47);
        jumpAnimation = new Animation(playerFrames, frameTime: 0.12f); // etwas langsamer
        //Load Sword-Combo Sprite
        playerFrames = SpriteLoader.LoadPlayerFrames(Content, "Sprites/Combo/sword_combo_", 65, 75);
        comboAnimation = new Animation(playerFrames, 0.08f); // TODO evtl. Framezeit anpassen
        //Load Sword-Air-Attack Sprite
        playerFrames = SpriteLoader.LoadPlayerFrames(Content, "Sprites/Air_Attack/sword_air_attack_", 62, 64);
        airAttackAnimation = new Animation(playerFrames, 0.08f); // Timing evtl. anpassen

        scale = screenHeight * 0.4f / playerFrames[0].Height;
        enemyScale = scale * 0.75f;

        //debug hit box frame
        whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        whitePixel.SetData(new[] { Color.White });

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
        float scaledPlayerHeight = playerFrames[0].Height * scale;
        playerPosition = new Vector2(100, groundY - scaledPlayerHeight);

        jumpForce = -(screenHeight * 0.0175f);

        //Hintergrund
        backgroundTexture = Content.Load<Texture2D>("background1");

        //start in run animation
        currentAnimation = runAnimation;

        //Orc-Idle
        for (int i = 0; i <= 5; i++)
        {
            string name = $"Sprites/Orc_Sprites/orc_0_{i}";
            orcFrames.Add(Content.Load<Texture2D>(name));
        }
        orcFrames = SpriteLoader.LoadEnemyFrames(Content, "Sprites/Orc_Sprites/orc_0_", 0, 5);
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

        playerRect = GetPlayerHitbox();
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
                //Spawn Orc
                // temporary enemy for hitbox size calculation
                var tempEnemy = new Enemy(orcIdleAnimation.Clone(), orcDeathAnimation.Clone(), Vector2.Zero, ScrollSpeed, enemyScale);
                Rectangle enemyHitbox = tempEnemy.GetHitbox();
                float enemyHeight = enemyHitbox.Height;
                float enemyY = groundY - enemyHeight - (groundTexture.Height*groundScale);
                var enemy = new Enemy(orcIdleAnimation.Clone(),orcDeathAnimation.Clone(), new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, groundY-110), ScrollSpeed, enemyScale);
                enemies.Add(enemy);
            }
            else
            {
                int groupType = rng.Next(3); // 0 = Linie, 1 = Treppe, 2 = Block
                float coinSpacing = coinTexture.Width * coinScale * 1.1f; // 10% Abstand zusätzlich
                switch (groupType)
                {
                    case 0: // Horizontale Linie
                        for (int i = 0; i < 5; i++)
                            coinPositions.Add(new Vector2(baseX + i * coinSpacing, baseY));
                        break;

                    case 1: // Treppenform
                        for (int i = 0; i < 5; i++)
                            coinPositions.Add(new Vector2(baseX + i * coinSpacing, baseY - i * coinSpacing/2));
                        break;

                    case 2: // 3x3 Block
                        for (int row = 0; row < 3; row++)
                            for (int col = 0; col < 3; col++)
                                coinPositions.Add(new Vector2(baseX + col * coinSpacing, baseY + row * coinSpacing));
                        break;
                }
            }

            
        }


        //Coin-Player Collision
        int frameWidth = playerFrames[0].Width;
        int frameHeight = playerFrames[0].Height;
        for (int i = coinPositions.Count - 1; i >= 0; i--)
        {
            Rectangle coinRect = new Rectangle((int)coinPositions[i].X, (int)coinPositions[i].Y, (int)(coinTexture.Width * coinScale) , (int)(coinTexture.Height * coinScale));
            if (coinRect.Intersects(playerRect))
            {
                coinPositions.RemoveAt(i);
                Gold+=goldPerCoin;
            }
        }
        foreach (var enemy in enemies)
        {
            Rectangle enemyRect = new Rectangle((int)enemy.Position.X + 130, (int)enemy.Position.Y, 40, 80); // TODO Größe ggf. anpassen

            if (enemy.IsAlive && playerRect.Intersects(enemyRect))
            {
                enemy.IsAlive = false;
                Gold += goldPerEnemy;
            }
        }


        // Gravity + jumping
        playerVelocity.Y += gravity;
        playerPosition += playerVelocity;
        float playerHeight = playerFrames[0].Height * scale;
        if (playerPosition.Y >= groundY - playerRect.Height - groundTexture.Height * groundScale / 2f)
        {
            playerPosition.Y = groundY - playerRect.Height - groundTexture.Height * groundScale / 2f;
            playerVelocity.Y = 0;
            isJumping = false;
        }



        //Track if enemy nearby
        bool nearEnemy = false;

        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            Rectangle enemyHitbox = enemy.GetHitbox();

            // Abstand zwischen Hitbox-Rändern (horizontal)
            int playerRight = playerRect.Right;
            int enemyLeft = enemyHitbox.Left;

            float distance = enemyLeft - playerRight;
            if (distance < attackDistance) // Abstand in pixeln zwischen hitboxen
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
    
    private Rectangle GetPlayerHitbox()
    {
        return new Rectangle(
            (int)(playerPosition.X + 200 * scale),  // X-Offset
            (int)(playerPosition.Y + 200 * scale),  // Y-Offset
            (int)(140 * scale),                     // Breite
            (int)(180 * scale)                      // Höhe
        );
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(30, 30, 30)); // Hintergrund
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        //Hintergrund
        float parallaxFactor = 0.5f;
        float bgScroll = scrollOffset * parallaxFactor;

        int bgWidth = backgroundTexture.Width;
        float bgX = -(scrollOffset * parallaxFactor) % bgWidth;
        float backgroundOffset = groundTexture.Height * groundScale * 0.10f;
        float bgY = groundY - backgroundTexture.Height + backgroundOffset;
        if (bgX > 0) bgX -= bgWidth; // Verhindert sichtbare Linie bei Übergang

        for (float x = bgX; x < graphics.PreferredBackBufferWidth; x += bgWidth)
        {
            spriteBatch.Draw(backgroundTexture, new Vector2((int)Math.Round(x), bgY), Color.White);
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
            spriteBatch.Draw(
                coinTexture,
                coin,                   // Position als Vector2
                null,                   // gesamte Textur
                Color.White,            // Farbe
                0f,                     // Rotation
                Vector2.Zero,           // Ursprung
                coinScale,              // Skalierungsfaktor
                SpriteEffects.None,
                0f                      // Layer depth
            );
        }


        //Enemies
        foreach (var enemy in enemies)
            enemy.Draw(spriteBatch);

        //Debug enemy hitbox
        foreach (var enemy in enemies)
        {
            enemy.Draw(spriteBatch);

            // Optional: nur anzeigen, wenn Debug-Modus aktiv
            Rectangle hitbox = enemy.GetHitbox(); // Diese Methode musst du selbst hinzufügen!
            spriteBatch.Draw(whitePixel, hitbox, Color.Red * 0.4f);
        }
        Rectangle playerHitbox = GetPlayerHitbox();
        spriteBatch.Draw(whitePixel, playerHitbox, Color.Red * 0.4f); // halbtransparentes Rot


        // Spieler
        if (playerFrames.Count > 0)
            currentAnimation.Draw(spriteBatch, playerPosition, scale);
        else
            Console.WriteLine("No frames to draw!");

        // Score
        spriteBatch.DrawString(scorefont, $"Gold: {Gold}", new Vector2(10, 10), Color.Black);



        spriteBatch.End();
        base.Draw(gameTime);
    }
}
