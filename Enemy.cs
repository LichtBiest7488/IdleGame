using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
public class Enemy
{
    public Vector2 Position;
    private Animation idleAnimation;
    private Animation deathAnimation;
    private Animation currentAnimation;
    public float Speed;
    public bool IsAlive = true;

    private float deathTimer = 0f;
    private float deathDuration = 1.5f; // wie lange sie nach der Animation noch liegen bleiben


    private float scale;

    public Enemy(Animation idleAnim, Animation deathAnim, Vector2 pos, float speed, float scale)
    {
        idleAnimation = idleAnim;
        deathAnimation = deathAnim;
        currentAnimation = idleAnimation;
        Position = pos;
        Speed = speed;
        this.scale = scale;
    }

    public Rectangle GetHitbox()
    {
        int fullWidth = (int)(currentAnimation.CurrentFrame.Width * scale);
        int fullHeight = (int)(currentAnimation.CurrentFrame.Height * scale);
    
        // z.B. 60 % Breite, 70 % Höhe
        int width = (int)(fullWidth * 0.6f);
        int height = (int)(fullHeight * 0.7f);
    
        // zentriert innerhalb des Bildes platzieren
        int offsetX = (int)((fullWidth - width) / 2f);
        int offsetY = (int)((fullHeight - height) / 2f);
    
        return new Rectangle(
            (int)Position.X + offsetX,
            (int)Position.Y + offsetY,
            width,
            height
        );
    }

    public void Update(GameTime gameTime)
    {
        // Animation nicht bei jedem Frame neu zuweisen
        if (!IsAlive && currentAnimation != deathAnimation)
            currentAnimation = deathAnimation;
        else if (IsAlive && currentAnimation != idleAnimation)
            currentAnimation = idleAnimation;

        currentAnimation.Update(gameTime);
        Position.X -= Speed;

        if (Position.X < -100) // offscreen cleanup
            IsAlive = false;
    }

    public bool ReadyToBeRemoved(float dt)
    {
        if (!IsAlive && currentAnimation.Finished)
        {
            deathTimer += dt;
            return deathTimer >= deathDuration;
        }
        return false;
    }


    public bool IsAnimationFinished()
    {
        return currentAnimation.Finished;
    }


    public void Draw(SpriteBatch spriteBatch)
    {
        currentAnimation.Draw(spriteBatch, Position, scale);
    }
}
