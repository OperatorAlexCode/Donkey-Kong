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
    public class DonkeyKongAnimationManager
    {
        // Timer
        Timer PoundChestDelay;
        Timer ChestPoundInterval;

        // float
        float PoundChestDelayTime = 10;
        float ChestPoundIntervalTime = 1f;

        // int
        int CurrentFrame;
        int ChestPoundAmountmax = 4;
        int ChestPoundCurrentAmount = 0;

        // Other
        Rectangle[] SpriteFrames;
        bool PoundChest;

        public DonkeyKongAnimationManager(Rectangle[] spriteFrames)
        {
            SpriteFrames = spriteFrames;
            CurrentFrame = 0;

            PoundChest = false;

            PoundChestDelay = new();
            ChestPoundInterval = new();
            PoundChestDelay.StartTimer(PoundChestDelayTime);
        }

        public void Update(float deltaTime)
        {
            PoundChestDelay.Update(deltaTime);
            ChestPoundInterval.Update(deltaTime);

            if (PoundChestDelay.IsDone())
            {
                PoundChest = true;
                PoundChestDelay.StartTimer(PoundChestDelayTime);
            }
            if (PoundChest)
            {
                if (ChestPoundCurrentAmount == 0 && CurrentFrame != 0)
                {
                    CurrentFrame++;
                    ChestPoundInterval.StartTimer(ChestPoundIntervalTime);
                }
                else if (ChestPoundInterval.IsDone())
                {
                    if (ChestPoundCurrentAmount < ChestPoundAmountmax)
                    {
                        if (CurrentFrame == 2)
                            CurrentFrame = 1;
                        else
                            CurrentFrame++;

                        ChestPoundInterval.StartTimer(ChestPoundIntervalTime);
                        ChestPoundCurrentAmount++;
                    }
                    else
                    {
                        ChestPoundCurrentAmount = 0;
                        CurrentFrame = 0;
                        PoundChest = false;
                    }
                } 
                
            }
        }

        public Rectangle GetCurrentFrame()
        {
            return SpriteFrames[CurrentFrame];
        }
    }
}
