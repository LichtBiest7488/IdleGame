using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace idleGame;

public class ItemDropManager
{
    private readonly Random rng = new();
    private readonly float dropChance;
    private readonly Dictionary<int, List<string>> tierItemPools = new();
    private readonly Dictionary<string, Texture2D> armorTextures;
    private readonly Dictionary<string, Texture2D> weaponTextures;
    private readonly string[] tierPrefixes = new[]
    {
        "Wood",      // 0  -> for weapons only
        "Copper",    // 1  -> from here on both weapon and armor
        "Bronze",    // 2
        "Iron",      // 3
        "Emerald",   // 4
        "Runic",     // 5
        "Infernal",  // 6 
        "Celestial"  // 7
    };
    private static readonly int[] TierUnlockLevels = new int[]
    {
        0,   // Tier 0 unlocked at level 0
        10,  // Tier 1 unlocked at level 10
        25,  // Tier 2 unlocked at level 20
        50,  // Tier 3 unlocked at level 30
        100,  // Tier 4 unlocked at level 40
        200,  // Tier 5 unlocked at level 50
        500,  // Tier 6 unlocked at level 60
        1000   // Tier 7 unlocked at level 70
    };

    private static readonly string Tier0ArmorPrefix = "Leather";


    public ItemDropManager(
        float dropChance,
        Dictionary<string, Texture2D> armorTextures,
        Dictionary<string, Texture2D> weaponTextures)
    {
        this.dropChance = dropChance;
        this.armorTextures = armorTextures;
        this.weaponTextures = weaponTextures;

        CategorizeItemsByTier();
    }

    private void CategorizeItemsByTier()
    {
        foreach (var key in armorTextures.Keys)
        {
            if (TryExtractTier(key, out int tier))
            {
                if (!tierItemPools.ContainsKey(tier))
                    tierItemPools[tier] = new();
                tierItemPools[tier].Add(key);
            }
        }

        foreach (var key in weaponTextures.Keys)
        {
            if (TryExtractTier(key, out int tier))
            {
                if (!tierItemPools.ContainsKey(tier))
                    tierItemPools[tier] = new();
                tierItemPools[tier].Add(key);
            }
        }
    }

    private bool TryExtractTier(string key, out int tier)
    {
        // key format: armor_X_Y or item_X_Y
        tier = 0;
        var parts = key.Split('_');
        if (parts.Length < 3)
            return false;
        return int.TryParse(parts[1], out tier);
    }
    private int GetTierForLevel(int level)
    {
        for (int i = TierUnlockLevels.Length - 1; i >= 0; i--)
        {
            if (level >= TierUnlockLevels[i])
                return i;
        }
        return 0;
    }


    public InventoryItem? TryDrop(int level)
    {
        if (rng.NextDouble() > dropChance)
            return null;

        int tier = GetTierForLevel(level);
        if (!tierItemPools.ContainsKey(tier) || tierItemPools[tier].Count == 0)
            return null;

        var keyList = tierItemPools[tier];
        string key = keyList[rng.Next(keyList.Count)];
        Texture2D tex = null;
        ItemType type = ItemType.Unknown;
        string name = "";

        if (armorTextures.TryGetValue(key, out tex))
        {
            string slot = key.Split('_')[2]; // z. B. armor_1_0 → 0 = Helmet
            type = slot switch
            {
                "0" => ItemType.Helmet,
                "1" => ItemType.Chest,
                "2" => ItemType.Pants,
                "3" => ItemType.Boots,
                "4" => ItemType.Gloves,
                "5" => ItemType.Ring,
                "6" => ItemType.Amulet,
                _ => ItemType.Unknown
            };

            string prefix = tier == 0 ? "Leather" : tierPrefixes[tier];
            name = $"{prefix} {type}";

        }
        else if (weaponTextures.TryGetValue(key, out tex))
        {
            type = ItemType.Weapon;

            string index = key.Split('_')[2]; // item_0_3 → "3"
            string baseName = index switch
            {
                "0" => "Sword",
                "1" => "Axe",
                "2" => "Shield",
                "3" => "Wand",
                "4" => "Bow",
                "5" => "Hammer",
                _ => "Weapon"
            };
            string prefix = tier < tierPrefixes.Length ? tierPrefixes[tier] : $"Tier{tier}";
            name = $"{prefix} {baseName}";
        }


        if (tex == null || type == ItemType.Unknown)
            return null;

        return new InventoryItem
        {
            Id = key,
            Icon = tex,
            Name = name,
            Type = type
        };
    }
}
