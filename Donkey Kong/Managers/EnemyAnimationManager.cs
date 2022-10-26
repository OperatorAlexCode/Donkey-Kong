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
    public class EnemyAnimationManager
    {
        Rectangle[] SpriteFrames;
        int CurrentFrame;
        float RefreshRate;
        Timer NextFrame;

        public EnemyAnimationManager(Rectangle[] spriteFrames, float refreshRate)
        {
            SpriteFrames = spriteFrames;
            RefreshRate = refreshRate;
            CurrentFrame = 0;

            NextFrame = new Timer();
            NextFrame.StartTimer(RefreshRate);
        }

        public void Update(float deltaTime)
        {
            NextFrame.Update(deltaTime);

            if (NextFrame.IsDone())
            {
                if (CurrentFrame == SpriteFrames.Length-1)
                    CurrentFrame = 0;
                else
                    CurrentFrame++;

                NextFrame.StartTimer(RefreshRate);
            }
        }

        public Rectangle GetCurrentFrame()
        {
            return SpriteFrames[CurrentFrame];
        }

        public void ChangeRefreshRate(float newRefreshRate)
        {
            RefreshRate = newRefreshRate;
        }
    }
}
