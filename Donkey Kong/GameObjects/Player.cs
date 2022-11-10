using Donkey_Kong.Enums;
using Donkey_Kong.Managers;
using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Donkey_Kong.GameObjects
{
    public class Player
    {
        // Vector2
        Vector2 WalkVel;
        Vector2 ClimbVel;
        Vector2 Gravity;
        Vector2 JumpStartVel;
        Vector2 JumpEndVel;

        // Tile
        public Tile CurrentTile;
        Tile BellowTile;
        Tile[,] TileMap;

        // Rectangle
        public Rectangle DestinationRec;

        // Bool
        bool OnGround;
        bool Moving;
        bool Jumping;
        bool GotHurt;

        // Int
        int[] CurrentTileInArray;
        int[] DestinationTile;
        int? MoveDirection;
        int PlayerLives;

        // TileType
        List<TileType> GroundTiles = new() { TileType.Beam, TileType.LadderConnector };
        List<TileType> ClimbableTiles = new() { TileType.Ladder, TileType.LadderConnector };
        List<TileType> NonObstructiveTiles = new() { TileType.Empty, TileType.GoalTile, TileType.OneUp };

        // Other
        Texture2D Tex;
        public PlayerState CurrentState;
        SpriteEffects CurrentEffect;
        float DrawLayer;
        Timer GraceTimer;
        PlayerAnimationManager AnimationManager;

        public Player(Texture2D tex, Rectangle destinationRec, float drawLayer, Vector2[] vectors, int[] currentTile, int playerLives, bool isPauline)
        {
            Tex = tex;
            DestinationRec = destinationRec;
            DrawLayer = drawLayer;

            CurrentEffect = SpriteEffects.None;
            CurrentState = PlayerState.Idle;
            OnGround = true;
            Moving = false;
            GotHurt = false;

            Gravity = vectors[0];
            WalkVel = vectors[1];
            ClimbVel = vectors[2];

            int spriteSheetRow = 1;

            if (isPauline)
                spriteSheetRow = 18;

            Rectangle[] spriteFrames = new Rectangle[10];
            CreateSpriteFrames(spriteFrames, spriteSheetRow);

            AnimationManager = new(spriteFrames, .075f);

            CurrentTileInArray = currentTile;
            PlayerLives = playerLives;

            GraceTimer = new Timer();
            PlayerLives = playerLives;
        }

        public void Update(float deltatime, Tile[,] tileMap, out int currentPlayerLives, Action<bool> endGame, Action despawnEnemies, int? inputIndex)
        {
            GraceTimer.Update(deltatime);

            currentPlayerLives = PlayerLives;

            if (GraceTimer.IsDone())
                GotHurt = false;

            if (PlayerLives <= 0 && !DeadCheck())
            {
                Die();
                despawnEnemies.Invoke();
            }

            PlayerState? newState;
            AnimationManager.Update(deltatime, CurrentState, GotHurt, out newState);

            if (newState != null)
                CurrentState = (PlayerState)newState;

            TileMap = tileMap;

            CurrentTile = TileMap[CurrentTileInArray[0], CurrentTileInArray[1]];
            BellowTile = TileMap[CurrentTileInArray[0] + 1, CurrentTileInArray[1]];

            if (CurrentTile.Type == TileType.GoalTile)
                endGame.Invoke(true);
            else if (CurrentState == PlayerState.Dead)
                endGame.Invoke(false);

            if (CurrentTile.Type == TileType.OneUp && CurrentTile.ItemNotUsed)
            {
                PlayerLives++;
                CurrentTile.ItemNotUsed = false;
            }

            OnGround = OnGroundCheck();

            if (DeadCheck() && Moving)
                StopMoving();

            else
            {
                if (!Moving)
                {
                    if (OnGround)
                        CurrentState = PlayerState.Idle;
                    else if (CurrentState == PlayerState.Climbing)
                        CurrentState = PlayerState.ClimbingIdle;
                    else if (!OnGround && CurrentState != PlayerState.ClimbingIdle)
                        CurrentState = PlayerState.Falling;
                }

                if (CurrentState == PlayerState.Falling && !Moving)
                    MoveTo(1);
                else if (Moving)
                    MoveTo(MoveDirection);
                else if (inputIndex != null && inputIndex != MoveDirection)
                    switch (inputIndex)
                    {
                        case 0:
                            if (LadderCheck(0) && CanClimbCheck())
                            {
                                CurrentState = PlayerState.Climbing;
                                MoveTo(0);
                            }
                            break;
                        case 1:
                            if (LadderCheck(1) && CanClimbCheck())
                            {
                                CurrentState = PlayerState.Climbing;
                                MoveTo(1);
                            }
                            break;
                        case 2:
                            if (OnGround)
                            {
                                CurrentEffect = SpriteEffects.FlipHorizontally;
                                CurrentState = PlayerState.Walking;
                                MoveTo(2);
                            }
                            break;
                        case 3:
                            if (OnGround)
                            {
                                CurrentEffect = SpriteEffects.None;
                                CurrentState = PlayerState.Walking;
                                MoveTo(3);
                            }
                            break;
                        case 4:
                            if (OnGround && !Jumping)
                            {
                                if (CurrentState != PlayerState.jumping)
                                {
                                    Vector2 currentTilePos = CurrentTile.DestinationRec.Location.ToVector2();
                                    CurrentState = PlayerState.jumping;
                                    if (CurrentEffect == SpriteEffects.None)
                                    {

                                    }
                                    else if (CurrentEffect == SpriteEffects.FlipHorizontally)
                                    {

                                    }
                                }
                            }

                            break;
                    }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            SpriteEffects? tempEffect;
            Rectangle currentFrame = AnimationManager.GetCurrentFrame(out tempEffect);

            if (tempEffect != null)
                spriteBatch.Draw(Tex, DestinationRec, currentFrame, Color.White, 0f, new Vector2(), (SpriteEffects)tempEffect, DrawLayer);
            else
                spriteBatch.Draw(Tex, DestinationRec, currentFrame, Color.White, 0f, new Vector2(), CurrentEffect, DrawLayer);
        }

        /// <summary>
        /// Checks if Player is on any surface
        /// </summary>
        /// <returns>True if on surface, else false</returns>
        public bool OnGroundCheck()
        {
            return GroundTiles.Any(t => t == BellowTile.Type);
        }

        /// <summary>
        /// Checks if the current tile or tile bellow is a ladder type
        /// </summary>
        /// <returns>True if it is a ladder type, else false</returns>
        public bool LadderCheck(int climbDirection)
        {
            switch (climbDirection)
            {
                case 0:
                    return (ClimbableTiles.Any(t => t == CurrentTile.Type) || BellowTile.Type == TileType.LadderConnector) && !NonObstructiveTiles.Any(t => t == CurrentTile.Type);
                case 1:
                    return (ClimbableTiles.Any(t => t == CurrentTile.Type) || BellowTile.Type == TileType.LadderConnector) && BellowTile.Type != TileType.Beam;
                default:
                    return false;

            }
        }

        public bool CanClimbCheck()
        {
            return CurrentState == PlayerState.Idle || CurrentState == PlayerState.ClimbingIdle;
        }

        /// <summary>
        /// Checks if the player is dead
        /// </summary>
        /// <returns>true if player is dead/dying, else false</returns>
        public bool DeadCheck()
        {
            return CurrentState == PlayerState.Dying || CurrentState == PlayerState.Dead;
        }

        /// <summary>
        /// Move to A Certain tile
        /// </summary>
        public void MoveTo(int? moveDirection)
        {
            if (DestinationTile != null && MoveDirection != moveDirection)
            {
                Vector2 destinationTileDirection = Vector2.Normalize(DestinationRec.Location.ToVector2() - TileMap[DestinationTile[0], DestinationTile[1]].DestinationRec.Location.ToVector2());
                Vector2 destinationTilePos = TileMap[DestinationTile[0], DestinationTile[1]].DestinationRec.Location.ToVector2();

                switch (CurrentState)
                {
                    case PlayerState.Walking:
                        if (destinationTileDirection.X == 1)
                        {
                            DestinationRec.X -= (int)WalkVel.X;

                            if (DestinationRec.X <= destinationTilePos.X)
                            {
                                DestinationRec.X = (int)destinationTilePos.X;
                                StopMoving();
                            }
                        }
                        else if (destinationTileDirection.X == -1)
                        {
                            DestinationRec.X += (int)WalkVel.X;

                            if (DestinationRec.X >= destinationTilePos.X)
                            {
                                DestinationRec.X = (int)destinationTilePos.X;
                                StopMoving();
                            }
                        }
                        break;
                    case PlayerState.Climbing:
                        if (destinationTileDirection.Y == -1)
                        {
                            DestinationRec.Y += (int)ClimbVel.Y;

                            if (DestinationRec.Y >= destinationTilePos.Y)
                            {
                                DestinationRec.Y = (int)destinationTilePos.Y;
                                StopMoving();
                            }
                        }
                        else if (destinationTileDirection.Y == 1)
                        {
                            DestinationRec.Y -= (int)ClimbVel.Y;

                            if (DestinationRec.Y <= destinationTilePos.Y)
                            {
                                DestinationRec.Y = (int)destinationTilePos.Y;
                                StopMoving();
                            }
                        }
                        break;
                    case PlayerState.Falling:
                        DestinationRec.Y += (int)Gravity.Y;

                        if (DestinationRec.Y >= destinationTilePos.Y)
                        {
                            DestinationRec.Y = (int)destinationTilePos.Y;
                            StopMoving();
                        }
                        break;
                }
            }
            //else if (MoveDirection != moveDirection && moveDirection != null && MoveDirection != null)
            //{
            //    int[] temp = DestinationTile;
            //    DestinationTile = CurrentTileInArray;
            //    CurrentTileInArray = temp;

            //    switch (moveDirection)
            //    {
            //        case 0:
            //            moveDirection = 1;
            //            break;
            //        case 1:
            //            moveDirection = 0;
            //            break;
            //        case 2:
            //            moveDirection = 3;
            //            break;
            //        case 3:
            //            moveDirection = 4;
            //            break;
            //    }
            //}
            else
            {
                MoveDirection = moveDirection;
                Moving = true;
                switch (moveDirection)
                {
                    case 0:
                        DestinationTile = new int[] { CurrentTileInArray[0] - 1, CurrentTileInArray[1] };
                        break;
                    case 1:
                        DestinationTile = new int[] { CurrentTileInArray[0] + 1, CurrentTileInArray[1] };
                        break;
                    case 2:
                        DestinationTile = new int[] { CurrentTileInArray[0], CurrentTileInArray[1] - 1 };
                        break;
                    case 3:
                        DestinationTile = new int[] { CurrentTileInArray[0], CurrentTileInArray[1] + 1 };
                        break;
                }

                MoveTo(null);
            }
        }

        public void StopMoving()
        {
            CurrentTileInArray = new int[] { DestinationTile[0], DestinationTile[1] };
            DestinationTile = null;
            MoveDirection = null;
            Moving = false;
        }

        /// <summary>
        /// Makes Player jump from one tile to another, stopping by
        /// </summary>
        public void JumpTo()
        {

        }

        /// <summary>
        /// Creates all the spriteframes used for player animation
        /// </summary>
        /// <param name="spriteFrames">Array to be filled with the spiteframes</param>
        /// <param name="spriteSheetRow">row of the spritesheet</param>
        public void CreateSpriteFrames(Rectangle[] spriteFrames, int spriteSheetRow)
        {
            spriteFrames[0] = new(18, spriteSheetRow, 16, 16);
            spriteFrames[1] = new(35, spriteSheetRow, 16, 16);
            spriteFrames[2] = new(1, spriteSheetRow, 16, 16);
            spriteFrames[3] = new(154, spriteSheetRow, 16, 16);
            spriteFrames[4] = new(171, spriteSheetRow, 16, 16);
            spriteFrames[5] = new(256, spriteSheetRow, 16, 16);
            spriteFrames[6] = new(273, spriteSheetRow, 16, 16);
            spriteFrames[7] = new(290, spriteSheetRow, 16, 16);
            spriteFrames[8] = new(306, spriteSheetRow, 16, 16);
            spriteFrames[9] = new(324, spriteSheetRow, 16, 16);
        }

        public void Hurt()
        {
            if (GraceTimer.IsDone() && !DeadCheck())
            {
                PlayerLives--;
                GotHurt = true;

                

                GraceTimer.StartTimer(1);
            }
        }

        public void Die()
        {
            CurrentState = PlayerState.Dying;
        }
    }
}
