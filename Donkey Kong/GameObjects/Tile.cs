using Donkey_Kong.Enums;
using Donkey_Kong.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Donkey_Kong.GameObjects
{
    public class Tile
    {
        // Rectangle
        public Rectangle DestinationRec;
        Rectangle? DonkeyKongRec;
        Rectangle? SourceRec;

        // Other
        public TileType Type;
        Texture2D[] Textures;
        float[] DrawLayers;
        DonkeyKongAnimationManager? AnimationManager;
        public bool ItemNotUsed;

        public Tile(TileType type,Texture2D[] textures, Rectangle destinationRec, float[] drawLayer, Rectangle? sourceRec = null, Rectangle? donkeyKongRec = null)
        {
            Type = type;
            Textures = textures;
            DestinationRec = destinationRec;
            DrawLayers = drawLayer;
            DonkeyKongRec = donkeyKongRec;
            SourceRec = sourceRec;

            if (DonkeyKongRec != null)
            {
                Rectangle[] spriteFrames = new Rectangle[3];
                spriteFrames[0] = new(5, 6, 38, 32);
                spriteFrames[1] = new(110, 6, 48, 34);
                spriteFrames[2] = new(164, 6, 48, 34);

                AnimationManager = new(spriteFrames);
            }
            else if (Type == TileType.OneUp)
            {
                ItemNotUsed = true;
            }
        }

        public void Update(float deltaTime)
        {
            if (DonkeyKongRec != null)
                AnimationManager.Update(deltaTime);

            if (!ItemNotUsed)
            {

            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < Textures.Length;x++)
            {
                if (DonkeyKongRec != null)
                    spriteBatch.Draw(Textures[x], (Rectangle)DonkeyKongRec, AnimationManager.GetCurrentFrame(), Color.White, 0f, new Vector2(), SpriteEffects.None, DrawLayers[x]);
                else if (SourceRec != null && Textures[x].Name == "mario-pauline")
                    spriteBatch.Draw(Textures[x], DestinationRec, (Rectangle)SourceRec, Color.White, 0f, new Vector2(), SpriteEffects.FlipHorizontally, DrawLayers[x]);
                else if (Type == TileType.OneUp && ItemNotUsed)
                    spriteBatch.Draw(Textures[x], DestinationRec, (Rectangle)SourceRec, Color.White, 0f, new Vector2(), SpriteEffects.None, DrawLayers[x]);
                else if (Type == TileType.OneUp && !ItemNotUsed)
                    continue;
                else
                    spriteBatch.Draw(Textures[x], DestinationRec, null, Color.White, 0f, new Vector2(), SpriteEffects.None, DrawLayers[x]);
            }
            
        }
    }
}
