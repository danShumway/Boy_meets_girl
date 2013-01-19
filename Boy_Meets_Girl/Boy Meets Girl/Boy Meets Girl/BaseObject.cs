using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Boy_Meets_Girl
{
    abstract class BaseObject
    {
        public int color;

        public Vector2 position;

        public String graphic;

        public BaseObject(int x, int y, int color, String graphic)
        {
            this.position = new Vector2(x, y);
            this.color = color;
            this.graphic = graphic;
        }

        /// <summary>
        /// Get all of the info about the current object that's being moused over.
        /// </summary>
        /// <returns>Each string in the array is a new line.  Nothing else needs to be parsed.</returns>
        public abstract String[] getInfo();
    }
}
