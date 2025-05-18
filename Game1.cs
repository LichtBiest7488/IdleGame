using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using idleGame;
using MonoGame.Framework.Devices.Sensors;


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
    int Level;

    float screenHeight;
    float screenWidth;




    //Coin generation
    Texture2D playerTexture, coinTexture;
    List<Vector2> coinPositions;
    Random rng = new Random();
    float spawnTimer = 0f;
    float spawnInterval = 10.0f;
    float coinScale;
    float enemySpawnChance = 1f; //25%


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
    float ScrollSpeed = 4f;
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

    //UI Buttons
    SpriteFont buttonfont;
    UIButton shopButton, inventoryButton;
    bool showShop = false, showInventory = false;
    Texture2D buttonBackground;

    //Inventory
    Texture2D inventoryBackground;
    Vector2 inventoryPosition;

    Texture2D helmetSlot, chestSlot, pantSlot, bootSlot, glovesSlot;
    Texture2D ringSlot, amuletSlot, weaponSlot;

    Vector2 helmetPos, chestPos, pantsPos, bootsPos, glovesPos;
    Vector2 ringPos, amuletPos, weaponPos;

    Vector2 basePosition = new Vector2(100, 100); // top-left corner
    // ganz oben in Game1 (bei den anderen Feldern)
    const int InventoryColumns = 10;
    const int InventoryRows = 5;
    const int SlotWidth = 75;
    const int SlotHeight = 71;
    const int SlotSpacingX = 22;
    const int SlotSpacingY = 24;
    const int InventoryMarginX = 28;
    const int InventoryMarginY = 22;



    //Items 
    private ItemDropManager itemDropManager;
    Dictionary<int, List<string>> itemPools;
    Dictionary<string, Texture2D> armorItemTextures;
    Dictionary<string, Texture2D> weaponItemTextures;
    float itemDropChance = 1f; // 50% chance
    List<string> tier0ItemKeys = new();
    List<InventoryItem> inventoryItems = new();
    InventoryItem draggedItem = null;
    Point? hoveredSlot = null;
    Vector2 dragOffset = Vector2.Zero;
    InventoryItem equippedHelmet, equippedChest, equippedPants, equippedBoots;
    InventoryItem equippedGloves, equippedRing, equippedAmulet, equippedWeapon;
    private enum DragOrigin
    {
        Inventory,
        Equipment
    }

    private DragOrigin? dragOrigin = null;
    private Point? dragOldInventorySlot = null;
    private ItemType? dragOldEquipSlotType = null;
    private InventoryItem hoveredItem = null;







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
        this.screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        this.screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;


        attackDistance = 20f * scale;
        


        playerPosition = new Vector2(100, 300);
        coinPositions = new List<Vector2>();
        Gold = 0;
        goldPerCoin = 1;
        goldPerEnemy = 5;
        Level = 1;
       
        base.Initialize();
    }

    protected override void LoadContent()
    {
        this.screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        this.screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        coinTexture = Content.Load<Texture2D>("Coin");
        coinScale = this.screenHeight * 0.04f / coinTexture.Height; //4% von der bildschirm größe
        scorefont = Content.Load<SpriteFont>("ScoreFont");
        buttonfont = Content.Load<SpriteFont>("ButtonFont");

        //Load Sword-run Sprite
        playerFrames = SpriteLoader.LoadPlayerFrames(Content, "Sprites/Run/sword_run_", 17, 24);
        runAnimation = new Animation(playerFrames);
        //Load Sword-Jump Sprite
        playerFrames = SpriteLoader.LoadPlayerFrames(Content, "Sprites/Jump/sword_jump_", 43, 47);
        jumpAnimation = new Animation(playerFrames, frameTime: 0.12f) // etwas langsamer
        {
            Loop = false
        };
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

        //UI Buttons
        buttonBackground = new Texture2D(GraphicsDevice, 1, 1);
        buttonBackground.SetData(new[] { Color.White });

        shopButton = new UIButton(new Rectangle(20, 60, 120, 40), "Shop", buttonfont, buttonBackground);
        inventoryButton = new UIButton(new Rectangle(20, 110, 120, 40), "Inventory", buttonfont, buttonBackground);

        //Inventory
        inventoryBackground = Content.Load<Texture2D>("Inventory/inventory");

        helmetSlot = Content.Load<Texture2D>("Inventory/helmet_slot");
        chestSlot = Content.Load<Texture2D>("Inventory/chest_slot");
        pantSlot = Content.Load<Texture2D>("Inventory/pant_slot");
        bootSlot = Content.Load<Texture2D>("Inventory/boot_slot");
        ringSlot = Content.Load<Texture2D>("Inventory/ring_slot");
        amuletSlot = Content.Load<Texture2D>("Inventory/amulet_slot");
        weaponSlot = Content.Load<Texture2D>("Inventory/weapon_slot");
        glovesSlot = Content.Load<Texture2D>("Inventory/gloves_slot");


        float inventoryX = (screenWidth - inventoryBackground.Width) / 2f;
        float inventoryY = (screenHeight - inventoryBackground.Height) / 2f;
        inventoryPosition = new Vector2(inventoryX, inventoryY);

        float slotWidth = helmetSlot.Width;
        float slotHeight = helmetSlot.Height;
        float slotScale = 1f; // Falls du skalierst
        float offsetX = (slotWidth * slotScale) * 0.5f; // halber Slot-Abstand

        Vector2 leftSlotStart = inventoryPosition - new Vector2(offsetX + slotWidth, 0); // links vom Inventar
        Vector2 topOffset = new Vector2(0, 0); // Offset falls nötig

        helmetPos = leftSlotStart + topOffset;
        chestPos  = helmetPos + new Vector2(0, 1) * (slotHeight + 10); // vertikaler Abstand 10px z.B.
        glovesPos = chestPos + new Vector2(0, 1) * (slotHeight + 10);
        pantsPos  = glovesPos  + new Vector2(0, 1) * (slotHeight + 10);
        bootsPos  = pantsPos  + new Vector2(0, 1) * (slotHeight + 10);

        float leftOffset = slotWidth + offsetX;
        ringPos   = chestPos  - new Vector2(leftOffset, 0);
        amuletPos = glovesPos  - new Vector2(leftOffset, 0);
        weaponPos = pantsPos - new Vector2(leftOffset, 0);

        //Items
        armorItemTextures = SpriteLoader.LoadArmorItems(Content, "Armor");
        // Collect all tier 0 armor keys
        foreach (var key in armorItemTextures.Keys)
        {
            if (key.StartsWith("armor_0_"))
                tier0ItemKeys.Add(key);
        }
        // Collect all tier 0 weapon keys
        weaponItemTextures = SpriteLoader.LoadWeaponItems(Content, "Weapons");
        Console.WriteLine("Geladene Waffen:");
        foreach (var key in weaponItemTextures.Keys)
            Console.WriteLine(" → " + key);

        foreach (var key in weaponItemTextures.Keys)
        {
            if (key.StartsWith("item_0_"))
                tier0ItemKeys.Add(key);
        }

        Console.WriteLine("Dropbare Items:");
        foreach (var key in tier0ItemKeys)
            Console.WriteLine($"→ {key}");

        //Enemies

        //Orc-Idle
        orcFrames = SpriteLoader.LoadEnemyFrames(Content, "Sprites/Orc_Sprites/orc_0_", 0, 5);
        orcIdleAnimation = new Animation(orcFrames, 0.15f);
        //Orc-Death
        orcFrames = SpriteLoader.LoadEnemyFrames(Content, "Sprites/Orc_Sprites/orc_5_", 0, 3);
        orcDeathAnimation = new Animation(orcFrames, 0.1f)
        {
            Loop = false
        };



        itemPools = new Dictionary<int, List<string>>()
        {
            { 0, new List<string>() },
            { 1, new List<string>() },
            { 2, new List<string>() },
            { 3, new List<string>() },
            { 4, new List<string>() },
            { 5, new List<string>() },
            { 6, new List<string>() },
            { 7, new List<string>() },
        };

        foreach (var key in armorItemTextures.Keys)
        {
            if (key.StartsWith("armor_0_")) itemPools[0].Add(key);
            if (key.StartsWith("armor_1_")) itemPools[1].Add(key);
            if (key.StartsWith("armor_2_")) itemPools[2].Add(key);
            if (key.StartsWith("armor_3_")) itemPools[3].Add(key);
            if (key.StartsWith("armor_4_")) itemPools[4].Add(key);
            if (key.StartsWith("armor_5_")) itemPools[5].Add(key);
            if (key.StartsWith("armor_6_")) itemPools[6].Add(key);
            if (key.StartsWith("armor_7_")) itemPools[7].Add(key);
        }
        foreach (var key in weaponItemTextures.Keys)
        {
            if (key.StartsWith("item_0_")) itemPools[0].Add(key);
            if (key.StartsWith("item_1_")) itemPools[1].Add(key);
            if (key.StartsWith("item_2_")) itemPools[2].Add(key);
            if (key.StartsWith("item_3_")) itemPools[3].Add(key);
            if (key.StartsWith("item_4_")) itemPools[4].Add(key);
            if (key.StartsWith("item_5_")) itemPools[5].Add(key);
            if (key.StartsWith("item_6_")) itemPools[6].Add(key);
            if (key.StartsWith("item_7_")) itemPools[7].Add(key);
        }



        //itemDropManager
        itemDropManager = new ItemDropManager(itemDropChance, armorItemTextures, weaponItemTextures);

        //start player in run animation
        currentAnimation = runAnimation;


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

            bool spawnEnemy = rng.NextDouble() < enemySpawnChance;//25% chance for enemy instead of coins
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


        //Entity-Player Collision
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

                var item = itemDropManager.TryDrop(Level);
                if (item != null)
                {
                    Point? freeSlot = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                    if (freeSlot.HasValue)
                    {
                        item.SlotPosition = freeSlot.Value;
                        inventoryItems.Add(item);
                    }
                    else
                    {
                        Console.WriteLine("Inventar voll!");
                    }
                }
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

        //Button Updates
        shopButton.Update();
        inventoryButton.Update();

        if (shopButton.WasClicked)
            showShop = !showShop;

        if (inventoryButton.WasClicked)
            showInventory = !showInventory;

        

        //Inventar Items bewegen
        if (showInventory)
        {
            MouseState mouse = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);

            //Item Hover
            hoveredSlot = GetInventorySlotAtPosition(mousePos, inventoryPosition + new Vector2(InventoryMarginX, InventoryMarginY), InventoryColumns, InventoryRows, SlotWidth, SlotHeight, SlotSpacingX, SlotSpacingY);
            var equipHover = GetHoveredEquipSlot(mousePos);
            hoveredItem = null;
            if (hoveredSlot != null)
            {
                hoveredItem = inventoryItems.Find(it => it.SlotPosition == hoveredSlot.Value);
            }
            else if (equipHover != null)
            {
                switch (equipHover.Value.type)
                {
                    case ItemType.Helmet: hoveredItem = equippedHelmet; break;
                    case ItemType.Chest: hoveredItem = equippedChest; break;
                    case ItemType.Pants: hoveredItem = equippedPants; break;
                    case ItemType.Boots: hoveredItem = equippedBoots; break;
                    case ItemType.Gloves: hoveredItem = equippedGloves; break;
                    case ItemType.Weapon: hoveredItem = equippedWeapon; break;
                    case ItemType.Ring: hoveredItem = equippedRing; break;
                    case ItemType.Amulet: hoveredItem = equippedAmulet; break;
                }
            }

            if (mouse.LeftButton == ButtonState.Pressed && draggedItem == null)
                {
                    // Inventar item aufnehmen
                    foreach (var item in inventoryItems)
                    {
                        if (item.SlotPosition == hoveredSlot)
                        {
                            draggedItem = item;
                            dragOffset = mousePos - (inventoryPosition + new Vector2(InventoryMarginX + hoveredSlot.Value.X * (SlotWidth + SlotSpacingX),
                                                                                     InventoryMarginY + hoveredSlot.Value.Y * (SlotHeight + SlotSpacingY)));
                            dragOrigin = DragOrigin.Inventory;
                            dragOldInventorySlot = item.SlotPosition;
                            return;
                        }
                    }
                    //Equip Slot item aufnehmen
                    if (equipHover != null)
                    {
                        switch (equipHover.Value.type)
                        {
                            case ItemType.Helmet:
                                if (equippedHelmet != null)
                                {
                                    draggedItem = equippedHelmet;
                                    equippedHelmet = null;
                                    dragOldEquipSlotType = ItemType.Helmet;
                                }
                                break;
                            case ItemType.Chest: if (equippedChest != null) { draggedItem = equippedChest; equippedChest = null; dragOldEquipSlotType = ItemType.Chest; } break;
                            case ItemType.Pants: if (equippedPants != null) { draggedItem = equippedPants; equippedPants = null; dragOldEquipSlotType = ItemType.Pants; } break;
                            case ItemType.Boots: if (equippedBoots != null) { draggedItem = equippedBoots; equippedBoots = null; dragOldEquipSlotType = ItemType.Boots; } break;
                            case ItemType.Gloves: if (equippedGloves != null) { draggedItem = equippedGloves; equippedGloves = null; dragOldEquipSlotType = ItemType.Gloves; } break;
                            case ItemType.Weapon: if (equippedWeapon != null) { draggedItem = equippedWeapon; equippedWeapon = null; dragOldEquipSlotType = ItemType.Weapon; } break;
                            case ItemType.Ring: if (equippedRing != null) { draggedItem = equippedRing; equippedRing = null; dragOldEquipSlotType = ItemType.Ring; } break;
                            case ItemType.Amulet: if (equippedAmulet != null) { draggedItem = equippedAmulet; equippedAmulet = null; dragOldEquipSlotType = ItemType.Amulet; } break;
                        }

                        dragOffset = new Vector2(SlotWidth / 2, SlotHeight / 2); // optisch zentriert greifen
                        dragOrigin = DragOrigin.Equipment;
                    }
                }
                //aufgehobenes item loslassen
                else if (mouse.LeftButton == ButtonState.Released && draggedItem != null)
                {
                    var equipTarget = GetHoveredEquipSlot(mousePos);
                    if (equipTarget != null && equipTarget.Value.type == draggedItem.Type)
                    {
                        // Ausrüsten
                        switch (draggedItem.Type)
                        {
                            case ItemType.Helmet:
                                if (equippedHelmet != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedHelmet.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedHelmet);
                                    }
                                }
                                equippedHelmet = draggedItem;
                                break;
                            case ItemType.Chest:
                                if (equippedChest != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedChest.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedChest);
                                    }
                                }
                                equippedChest = draggedItem;
                                break;
                            case ItemType.Pants:
                                if (equippedPants != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedPants.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedPants);
                                    }
                                }
                                equippedPants = draggedItem;
                                break;
                            case ItemType.Boots:
                                if (equippedBoots != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedBoots.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedBoots);
                                    }
                                }
                                equippedBoots = draggedItem;
                                break;
                            case ItemType.Gloves:
                                if (equippedGloves != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedGloves.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedGloves);
                                    }
                                }
                                equippedGloves = draggedItem;
                                break;
                            case ItemType.Ring:
                                if (equippedRing != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedRing.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedRing);
                                    }
                                }
                                equippedRing = draggedItem;
                                break;
                            case ItemType.Amulet:
                                if (equippedAmulet != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedAmulet.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedAmulet);
                                    }
                                }
                                equippedAmulet = draggedItem;
                                break;
                            case ItemType.Weapon:
                                if (equippedWeapon != null)
                                {
                                    Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                                    if (free.HasValue)
                                    {
                                        equippedWeapon.SlotPosition = free.Value;
                                        inventoryItems.Add(equippedWeapon);
                                    }
                                }
                                equippedWeapon = draggedItem;
                                break;
                        }

                        inventoryItems.Remove(draggedItem);
                    }
                    else if (hoveredSlot != null && !inventoryItems.Exists(it => it.SlotPosition == hoveredSlot.Value))
                    {
                        // Normales Verschieben im Raster
                        draggedItem.SlotPosition = hoveredSlot.Value;
                        if (!inventoryItems.Contains(draggedItem))
                            inventoryItems.Add(draggedItem);

                    }
                    else if (hoveredSlot != null && inventoryItems.Exists(it => it.SlotPosition == hoveredSlot.Value))
                    {
                        var targetItem = inventoryItems.Find(it => it.SlotPosition == hoveredSlot.Value);
                        // Swap logic
                        if (dragOrigin == DragOrigin.Inventory)
                        {
                            // Beide Items tauschen die Plätze
                            Point temp = draggedItem.SlotPosition;
                            draggedItem.SlotPosition = targetItem.SlotPosition;
                            targetItem.SlotPosition = temp;
                        }
                        else if (dragOrigin == DragOrigin.Equipment)
                        {
                            // Nur tauschen, wenn Typ gleich
                            if (draggedItem.Type == targetItem.Type)
                            {
                                // Ziel wird ins Equip gelegt
                                switch (draggedItem.Type)
                                {
                                    case ItemType.Helmet: equippedHelmet = targetItem; break;
                                    case ItemType.Chest: equippedChest = targetItem; break;
                                    case ItemType.Pants: equippedPants = targetItem; break;
                                    case ItemType.Boots: equippedBoots = targetItem; break;
                                    case ItemType.Gloves: equippedGloves = targetItem; break;
                                    case ItemType.Weapon: equippedWeapon = targetItem; break;
                                    case ItemType.Ring: equippedRing = targetItem; break;
                                    case ItemType.Amulet: equippedAmulet = targetItem; break;
                                }
                                // DraggedItem wird auf Inventory-Slot gesetzt
                                draggedItem.SlotPosition = hoveredSlot.Value;
                                // Replace draggedItem with target
                                inventoryItems.Remove(targetItem);
                                inventoryItems.Add(draggedItem);
                            }
                            else
                            {
                                // Kein Tausch möglich, Item zurück ins Equipment
                                switch (draggedItem.Type)
                                {
                                    case ItemType.Helmet: equippedHelmet = draggedItem; break;
                                    case ItemType.Chest: equippedChest = draggedItem; break;
                                    case ItemType.Pants: equippedPants = draggedItem; break;
                                    case ItemType.Boots: equippedBoots = draggedItem; break;
                                    case ItemType.Gloves: equippedGloves = draggedItem; break;
                                    case ItemType.Weapon: equippedWeapon = draggedItem; break;
                                    case ItemType.Ring: equippedRing = draggedItem; break;
                                    case ItemType.Amulet: equippedAmulet = draggedItem; break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Point? free = FindFirstFreeInventorySlot(InventoryColumns, InventoryRows);
                        if (free.HasValue)
                        {
                            draggedItem.SlotPosition = free.Value;
                            if (!inventoryItems.Contains(draggedItem))
                                inventoryItems.Add(draggedItem);
                        }
                        else
                        {
                            if (dragOrigin == DragOrigin.Inventory && dragOldInventorySlot.HasValue)
                            {
                                // zurück an alte Inventarstelle
                                draggedItem.SlotPosition = dragOldInventorySlot.Value;
                                if (!inventoryItems.Contains(draggedItem))
                                    inventoryItems.Add(draggedItem);
                            }
                            else if (dragOrigin == DragOrigin.Equipment && dragOldEquipSlotType.HasValue)
                            {
                                // zurück in Equip-Slot
                                switch (dragOldEquipSlotType.Value)
                                {
                                    case ItemType.Helmet: equippedHelmet = draggedItem; break;
                                    case ItemType.Chest: equippedChest = draggedItem; break;
                                    case ItemType.Pants: equippedPants = draggedItem; break;
                                    case ItemType.Boots: equippedBoots = draggedItem; break;
                                    case ItemType.Gloves: equippedGloves = draggedItem; break;
                                    case ItemType.Weapon: equippedWeapon = draggedItem; break;
                                    case ItemType.Ring: equippedRing = draggedItem; break;
                                    case ItemType.Amulet: equippedAmulet = draggedItem; break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Kein gültiger Zielort, Item verloren!");
                            }
                        }
                    }

                    draggedItem = null;
                    dragOrigin = null;
                }


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
    private Point? GetInventorySlotAtPosition(Vector2 mousePos, Vector2 inventoryStart, int columns, int rows, int slotW, int slotH, int spacingX, int spacingY)
    {
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                float x = inventoryStart.X + col * (slotW + spacingX);
                float y = inventoryStart.Y + row * (slotH + spacingY);
                Rectangle rect = new Rectangle((int)x, (int)y, slotW, slotH);

                if (rect.Contains(mousePos))
                    return new Point(col, row);
            }
        }
        return null;
    }

    private Point? FindFirstFreeInventorySlot(int columns, int rows)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Point slot = new Point(col, row);
                if (!inventoryItems.Exists(item => item.SlotPosition == slot))
                    return slot;
            }
        }
        return null; // kein freier Slot mehr
    }

    private (ItemType type, Vector2 pos)? GetHoveredEquipSlot(Vector2 mousePos)
    {
        Dictionary<ItemType, Vector2> equipSlots = new()
        {
            { ItemType.Helmet, helmetPos },
            { ItemType.Chest, chestPos },
            { ItemType.Pants, pantsPos },
            { ItemType.Boots, bootsPos },
            { ItemType.Gloves, glovesPos },
            { ItemType.Weapon, weaponPos },
            { ItemType.Ring, ringPos },
            { ItemType.Amulet, amuletPos }
        };

        foreach (var pair in equipSlots)
        {
            Rectangle rect = new Rectangle((int)pair.Value.X, (int)pair.Value.Y, SlotWidth, SlotHeight);
            if (rect.Contains(mousePos))
                return (pair.Key, pair.Value);
        }

        return null;
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
            //spriteBatch.Draw(whitePixel, hitbox, Color.Red * 0.4f);
        }
        //Debug player hitbox
        Rectangle playerHitbox = GetPlayerHitbox();
        //spriteBatch.Draw(whitePixel, playerHitbox, Color.Red * 0.4f); // halbtransparentes Rot

        shopButton.Draw(spriteBatch);
        inventoryButton.Draw(spriteBatch);

        if (showShop)
        {
            // draw shop panel
            spriteBatch.Draw(whitePixel, new Rectangle(200, 100, 400, 300), Color.White * 0.9f);
            spriteBatch.DrawString(scorefont, "Shop: Buy upgrades here", new Vector2(220, 120), Color.Black);
        }

        if (showInventory)
        {
            // draw inventory panel
            // Zielgröße anhand der Slotgröße und Slotanzahl (z.B. 8 Spalten × 4 Zeilen)
            int slotsWide = 8;
            int slotsHigh = 4;
            int slotSize = helmetSlot.Width; // oder Height, wenn quadratisch

            Vector2 inventorySize = new Vector2(slotsWide * slotSize, slotsHigh * slotSize);
            spriteBatch.Draw(inventoryBackground,
                destinationRectangle: new Rectangle((int)inventoryPosition.X, (int)inventoryPosition.Y, (int)inventorySize.X, (int)inventorySize.Y),
                color: Color.White
            );
            spriteBatch.Draw(inventoryBackground, new Vector2((int)inventoryPosition.X, (int)inventoryPosition.Y), Color.White);



            spriteBatch.Draw(helmetSlot, helmetPos, Color.White);
            spriteBatch.Draw(chestSlot, chestPos, Color.White);
            spriteBatch.Draw(glovesSlot, glovesPos, Color.White);
            spriteBatch.Draw(pantSlot, pantsPos, Color.White);
            spriteBatch.Draw(bootSlot, bootsPos, Color.White);
            spriteBatch.Draw(weaponSlot, weaponPos, Color.White);
            spriteBatch.Draw(ringSlot, ringPos, Color.White);
            spriteBatch.Draw(amuletSlot, amuletPos, Color.White);

            //Draw items in inventory

            int itemsize = (int)((float)slotSize * 2 / 3);
            Vector2 start = inventoryPosition + new Vector2(InventoryMarginX, InventoryMarginY);

            foreach (var item in inventoryItems)
            {
                if (item == draggedItem)
                    continue;

                int col = item.SlotPosition.X;
                int row = item.SlotPosition.Y;

                float x = start.X + col * (SlotWidth + SlotSpacingX);
                float y = start.Y + row * (SlotHeight + SlotSpacingY);

                Rectangle targetRect = new Rectangle((int)x, (int)y, SlotWidth, SlotHeight);
                spriteBatch.Draw(item.Icon, targetRect, Color.White);
            }

            //Draw equipped items
            if (equippedHelmet != null)
                spriteBatch.Draw(equippedHelmet.Icon, new Rectangle((int)helmetPos.X, (int)helmetPos.Y, SlotWidth, SlotHeight), Color.White);

            if (equippedChest != null)
                spriteBatch.Draw(equippedChest.Icon, new Rectangle((int)chestPos.X, (int)chestPos.Y, SlotWidth, SlotHeight), Color.White);

            if (equippedPants != null)
                spriteBatch.Draw(equippedPants.Icon, new Rectangle((int)pantsPos.X, (int)pantsPos.Y, SlotWidth, SlotHeight), Color.White);

            if (equippedBoots != null)
                spriteBatch.Draw(equippedBoots.Icon, new Rectangle((int)bootsPos.X, (int)bootsPos.Y, SlotWidth, SlotHeight), Color.White);

            if (equippedGloves != null)
                spriteBatch.Draw(equippedGloves.Icon, new Rectangle((int)glovesPos.X, (int)glovesPos.Y, SlotWidth, SlotHeight), Color.White);

            if (equippedRing != null)
                spriteBatch.Draw(equippedRing.Icon, new Rectangle((int)ringPos.X, (int)ringPos.Y, SlotWidth, SlotHeight), Color.White);

            if (equippedAmulet != null)
                spriteBatch.Draw(equippedAmulet.Icon, new Rectangle((int)amuletPos.X, (int)amuletPos.Y, SlotWidth, SlotHeight), Color.White);

            if (equippedWeapon != null)
                spriteBatch.Draw(equippedWeapon.Icon, new Rectangle((int)weaponPos.X, (int)weaponPos.Y, SlotWidth, SlotHeight), Color.White);



            // Gezogenes Item ganz oben zeichnen
            if (draggedItem != null)
            {
                MouseState mouse = Mouse.GetState();
                Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
                Rectangle dragRect = new Rectangle((int)(mousePos.X - dragOffset.X), (int)(mousePos.Y - dragOffset.Y), SlotWidth, SlotHeight);
                spriteBatch.Draw(draggedItem.Icon, dragRect, Color.White);
            }


        }

        // Spieler
        if (playerFrames.Count > 0)
            currentAnimation.Draw(spriteBatch, playerPosition, scale);
        else
            Console.WriteLine("No frames to draw!");

        // Score
        spriteBatch.DrawString(scorefont, $"Gold: {Gold}", new Vector2(10, 10), Color.Black);

        //Hovered Item
        if (hoveredItem != null)
        {
            Vector2 mouse = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            string text = hoveredItem.Name ?? hoveredItem.Id;

            Vector2 size = scorefont.MeasureString(text);
            Rectangle bg = new Rectangle((int)mouse.X + 10, (int)mouse.Y + 10, (int)size.X + 10, (int)size.Y + 6);

            spriteBatch.Draw(whitePixel, bg, Color.Black * 0.85f);
            spriteBatch.DrawString(scorefont, text, new Vector2(bg.X + 5, bg.Y + 3), Color.White);
        }


        spriteBatch.End();
        base.Draw(gameTime);
    }
}
