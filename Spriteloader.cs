using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace idleGame
{
    public static class SpriteLoader
    {
        // For Stickman: e.g., sword_jump_0043.png to sword_jump_0047.png
        public static List<Texture2D> LoadPlayerFrames(ContentManager content, string basePath, int start, int end)
        {
            List<Texture2D> frames = new();
            for (int i = start; i <= end; i++)
            {
                string path = $"{basePath}{i:D4}"; // e.g. 0043
                frames.Add(content.Load<Texture2D>(path));
            }
            return frames;
        }

        // For Orcs: e.g., orc_0_0.png to orc_0_5.png
        public static List<Texture2D> LoadEnemyFrames(ContentManager content, string basePath, int start, int end)
        {
            List<Texture2D> frames = new();
            for (int i = start; i <= end; i++)
            {
                string path = $"{basePath}{i}.png"; // e.g. orc_0_0.png
                frames.Add(content.Load<Texture2D>(path.Replace(".png", ""))); // remove .png for MGCB loading
            }
            return frames;
        }

        public static Dictionary<string, Texture2D> LoadArmorItems(ContentManager content, string baseFolder)
        {
            Dictionary<string, Texture2D> armorTextures = new();

            for (int category = 0; category <= 7; category++)
            {
                for (int variant = 0; variant <= 6; variant++)
                {
                    string key = $"armor_{category}_{variant}";
                    string path = $"{baseFolder}/{key}";

                    try
                    {
                        Texture2D texture = content.Load<Texture2D>(path);
                        armorTextures[key] = texture;
                    }
                    catch (ContentLoadException)
                    {
                        // Optional: skip missing files silently
                    }
                }
            }

            return armorTextures;
        }
    }
}
