using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Donkey_Kong
{
    public class Timer
    {
        private double CurrentTime = 0.0;

        public void StartTimer(double delay)
        {
            CurrentTime = delay;
        }
        public bool IsDone()
        {
            return CurrentTime <= 0.0;
        }

        public void Update(double deltaTime)
        {
            CurrentTime -= deltaTime;
        }
    }
}
