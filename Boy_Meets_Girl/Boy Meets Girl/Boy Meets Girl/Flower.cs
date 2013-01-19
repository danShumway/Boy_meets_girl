using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Boy_Meets_Girl
{
    class Flower : BaseObject
    {

        public Flower(int x, int y)
            : base(x, y, Main.random.Next(0, Main.colors.Length - 1 /*Leave room for white (Person color)*/ ), "f") { }

        public override string[]  getInfo()
        {
            return new String[] {"A beautiful flower."};
        }
    }
}
