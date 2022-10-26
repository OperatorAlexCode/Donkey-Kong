using Donkey_Kong.Enums;
using Donkey_Kong.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Donkey_Kong.GameObjects
{
    public class Enemy
    {
        // Rectange
        public Rectangle DestinationRectangle;
        Rectangle[] SpriteFrames;

        // Bool
        bool Moving;
        bool Deactivate;

        // Other
        Texture2D Tex;
        float DrawLayer;
        Vector2 Vel;
        EnemyAnimationManager AnimationManager;
        SpriteEffects CurrentEffect;
        int[] CurrentTile;
        Tile[,] TileMap;
        List<TileType> NonObstructingTiles = new() { TileType.Empty, TileType.OneUp };

        public Enemy(Texture2D tex, Rectangle destinationRec, float drawLayer, int[] currentTile)
        {
            Tex = tex;
            DestinationRectangle = destinationRec;
            DrawLayer = drawLayer;
            Deactivate = false;
            Moving = false;

            Vel = new Vector2(MathF.Round((float)new Random().NextDouble(-2, 2)), 0);

            while (MathF.Round(Vel.X) == 0)
                Vel.X = MathF.Round((float)new Random().NextDouble(-2, 2));

            SpriteFrames = new Rectangle[3];
            SpriteFrames[0] = new Rectangle(8,88,16,16);
            SpriteFrames[1] = new Rectangle(26, 88, 16, 16);
            SpriteFrames[2] = new Rectangle(44, 88, 16, 16);

            CurrentTile = currentTile;
            AnimationManager = new(SpriteFrames, .2f);
        }

        public void Update(float deltatime, Tile[,] tileMap, Player player)
        {
            TileMap = tileMap;
            AnimationManager.Update(deltatime);

            if (Deactivate)
                Vel = Vector2.Zero;
            else
            {
                if (Moving == true)
                    MoveTo();
                else
                {
                    if (Vel.X > 0)
                    {
                        if (CanWalkCheck(TileMap[CurrentTile[0], CurrentTile[1] + 1], TileMap[CurrentTile[0] + 1, CurrentTile[1] + 1]))
                            Vel.X *= -1;

                        else
                        {
                            CurrentEffect = SpriteEffects.FlipHorizontally;
                            Moving = true;
                        }

                    }
                    else
                    {
                        if (CanWalkCheck(TileMap[CurrentTile[0], CurrentTile[1] - 1], TileMap[CurrentTile[0] + 1, CurrentTile[1] - 1]))
                            Vel.X *= -1;

                        else
                        {
                            CurrentEffect = SpriteEffects.None;
                            Moving = true;
                        }

                    }
                }

                if (DestinationRectangle.Intersects(player.DestinationRec))
                    player.Hurt();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Tex, DestinationRectangle, AnimationManager.GetCurrentFrame(), Color.White, 0f, new Vector2(), CurrentEffect, DrawLayer);
        }

        public void MoveTo()
        {
            DestinationRectangle.X += (int)Vel.X;

            if (Vel.X > 0)
            {
                if (DestinationRectangle.X >= TileMap[CurrentTile[0], CurrentTile[1]+1].DestinationRec.X)
                {
                    DestinationRectangle.X = TileMap[CurrentTile[0], CurrentTile[1] + 1].DestinationRec.X;
                    CurrentTile = new int[] { CurrentTile[0], CurrentTile[1] + 1 };
                    Moving = false;
                }
            }
            else
            {
                if (DestinationRectangle.X <= TileMap[CurrentTile[0], CurrentTile[1]-1].DestinationRec.X)
                {
                    DestinationRectangle.X = TileMap[CurrentTile[0], CurrentTile[1] - 1].DestinationRec.X;
                    CurrentTile = new int[] { CurrentTile[0], CurrentTile[1] - 1 };
                    Moving = false;
                }
            }
        }

        public void DeactivateEnemy()
        {
            Deactivate = true;
        }

        public bool CanWalkCheck(Tile forwardTile, Tile forwardBellowTile)
        {
            return NonObstructingTiles.Any(t => forwardTile.Type == t) && NonObstructingTiles.Any(t => forwardBellowTile.Type == t);
        }
    }
}
