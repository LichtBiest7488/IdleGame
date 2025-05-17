using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
public class Animation
{
    private List<Texture2D> frames;
    private float timer;
    private float frameTime;
    public int currentFrame;
    public Texture2D CurrentFrame => frames[currentFrame];


    public bool Loop { get; set; } = true;
    public bool Finished => !Loop && currentFrame >= frames.Count - 1;

    public Animation(List<Texture2D> frames, float frameTime = 0.1f)
    {
        this.frames = frames;
        this.frameTime = frameTime;
        this.timer = 0f;
        this.currentFrame = 0;
    }
    

    public void Update(GameTime gameTime)
{
    if (Finished) return;

    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

    if (timer >= frameTime)
    {
        timer -= frameTime;
        currentFrame++;

        if (currentFrame >= frames.Count)
        {
            if (Loop)
            {
                currentFrame = 0;
            }
            else
            {
                currentFrame = frames.Count - 1; // auf letztem Frame bleiben
            }
        }
    }
}


    public Animation Clone()
    {
        return new Animation(new List<Texture2D>(frames), frameTime)
        {
            Loop = this.Loop
        };
    }



    public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale = 1f, Color? color = null)
    {
        if (frames.Count == 0) return;
        spriteBatch.Draw(
            frames[currentFrame],
            position,
            null,
            color ?? Color.White,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f
        );
    }


    public void Reset()
    {
        timer = 0f;
        currentFrame = 0;
    }
}
