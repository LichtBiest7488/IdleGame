using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace idleGame
{
    public enum ItemType
{
    Helmet, Chest, Pants, Boots, Gloves,
    Weapon, Ring, Amulet,
    Unknown
}

    public class InventoryItem
    {
        public Texture2D Icon;
        public string Id;
        public Point SlotPosition;
        public ItemType Type;
        public string Name;
}

}