using Donkey_Kong.Enums;
using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Donkey_Kong.Managers
{
    public class PlayerAnimationManager
    {
        // float
        float RefreshRate;
        float DyingRefreshRate = .25f;

        // int
        int CurrentFrame;
        int DyingRollAmountMax = 2;
        int DyingRollAmount = 0;
        int DyingRollCurrentFrame = 0;
        int DyingLayingDownAmountMax = 6;
        int DyingLayingDownAmount = 0;
        int CurrentClimbingFrame = 0;
        int ClimbFrameMax = 2;

        // Timer
        Timer NextFrame;
        Timer DyingTimer;

        // other
        Rectangle[] SpriteFrames;

        PlayerState LastState;
        SpriteEffects? SpriteEffect;

        public PlayerAnimationManager(Rectangle[] spriteFrames, float refreshRate)
        {
            SpriteFrames = spriteFrames;
            RefreshRate = refreshRate;
            CurrentFrame = 0;
            NextFrame = new();
            DyingTimer = new();
            SpriteEffect = null;
        }

        public void Update(float deltaTime, PlayerState currentState, bool gotHurt, out PlayerState? newState)
        {
            NextFrame.Update(deltaTime);
            DyingTimer.Update(deltaTime);

            SpriteEffect = null;
            newState = null;

            if (gotHurt && (currentState != PlayerState.Dying || currentState != PlayerState.Dead))
                CurrentFrame = 5;

            else
            switch (currentState)
            {
                case PlayerState.Idle:
                    CurrentFrame = 0;
                    break;
                case PlayerState.Walking:
                    if (currentState != LastState)
                    {
                        CurrentFrame = 1;
                        NextFrame.StartTimer(RefreshRate);
                    }
                    else if (NextFrame.IsDone())
                    {
                        if (CurrentFrame == 1)
                            CurrentFrame = 2;
                        else if (CurrentFrame == 2)
                            CurrentFrame = 1;
                        NextFrame.StartTimer(RefreshRate);
                    }
                    break;
                case PlayerState.Climbing:
                    CurrentFrame = 3;
                    if (NextFrame.IsDone())
                    {
                        if (CurrentClimbingFrame == ClimbFrameMax)
                        {
                            if (SpriteEffect == null || SpriteEffect == SpriteEffects.None)
                                SpriteEffect = SpriteEffects.FlipHorizontally;
                            else if (SpriteEffect == SpriteEffects.FlipHorizontally)
                                SpriteEffect = SpriteEffects.None;
                            CurrentClimbingFrame = 0;
                        }
                        else
                            CurrentClimbingFrame++;

                        NextFrame.StartTimer(RefreshRate);
                    }
                    break;
                case PlayerState.ClimbingIdle:
                    CurrentFrame = 3;
                    break;
                case PlayerState.Falling:
                    CurrentFrame = 4;
                    break;
                case PlayerState.jumping:
                    CurrentFrame = 4;
                    break;
                case PlayerState.Dying:
                    if (LastState != PlayerState.Dying)
                    {
                        CurrentFrame = 5;
                        DyingTimer.StartTimer(DyingRefreshRate);
                        SpriteEffect = SpriteEffects.None;
                    }
                    else if (DyingTimer.IsDone())
                    {
                        if (DyingRollAmount < DyingRollAmountMax)
                        {
                            if (DyingRollCurrentFrame == 3)
                            {
                                DyingRollCurrentFrame = 0;
                                DyingRollAmount++;
                            }
                            else
                                DyingRollCurrentFrame++;

                            CurrentFrame = 5 + DyingRollCurrentFrame;

                            DyingTimer.StartTimer(DyingRefreshRate);
                        }

                        else if (DyingLayingDownAmount < DyingLayingDownAmountMax)
                        {
                            CurrentFrame = 9;
                            DyingLayingDownAmount++;
                            DyingTimer.StartTimer(DyingRefreshRate);
                        }

                        else
                            newState = PlayerState.Dead;

                    }
                    break;
                default:
                    CurrentFrame = 0;
                    break;
            }

            LastState = currentState;
        }

        public Rectangle GetCurrentFrame(out SpriteEffects? spriteEffect)
        {
            spriteEffect = SpriteEffect;
            return SpriteFrames[CurrentFrame];
        }

        public void ChangeRefreshRate(float newRefreshRate)
        {
            RefreshRate = newRefreshRate;
        }
    }
}
