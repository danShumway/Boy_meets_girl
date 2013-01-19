using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boy_Meets_Girl
{
    class World
    {
        //World dimensions
        int startX;
        int startY;
        int endX;
        int endY;

        //List of game objects.  Right now it's only flowers though.
        public List<BaseObject> objects;
        //The player controlled person.
        public Person player;

        //Timers.  The world can occasionally do things on a time based scale.
        //I know that const is wrong convention, but for a private project, this is just so much less ugly.
        const int respawnFlowerTimerReset = 60 /*updates in a second*/ * 3 /*seconds*/;
        int respawnFlowerTimer = 60;


        /// <summary>
        /// Constructor for world.
        /// </summary>
        /// <param name="startX">The starting x position.</param>
        /// <param name="startY">Starting y position.</param>
        /// <param name="width">Width of world.</param>
        /// <param name="height">Height of world.</param>
        /// <param name="numberOfFlowers">Number of starting flowers.</param>
        /// <param name="numberOfPeople">Number of starting people.</param>
        public World(int startX, int startY, int width, int height, int numberOfFlowers, int numberOfPeople)
        {
            objects = new List<BaseObject>();

            for (int i = 0; i < numberOfFlowers; i++)
            {
                //Add flowers.
                objects.Add(new Flower(Main.random.Next(startX, startX + width), Main.random.Next(startY, startY + height)));
            }

            for (int i = 0; i < numberOfPeople; i++)
            {
                //Add people.
                objects.Add(new Person(Main.random.Next(startX, startX + width), Main.random.Next(startY, startY + height), this, false));
            }

            //Add the player.  Also set the world dimensions.
            objects.Add(player = new Person(Main.random.Next(this.startX = startX, this.endX = startX + width),
                Main.random.Next(this.startY = startY, this.endY = startY + height), this, true));
        }


        /// <summary>
        /// Call during Main.update(); - once every 1/60th of a second.  Which is pretty good.
        /// </summary>
        public void update()
        {
            List<BaseObject> toRemove = new List<BaseObject>();

            //Decrement timers and update accordingly.
            --respawnFlowerTimer;
            if (respawnFlowerTimer == 0)
            {
                objects.Add(new Flower(Main.random.Next(startX, endX), Main.random.Next(startY, endY)));
                respawnFlowerTimer = respawnFlowerTimerReset;
            }

            //Update each gameObject.
            foreach (BaseObject o in objects)
            {
                if (o is Person)
                {
                    foreach (BaseObject remove in (o as Person).update())
                        toRemove.Add(remove);
                }
            }

            foreach (BaseObject o in toRemove)
                objects.Remove(o);
        }

        public List<BaseObject> runCollision(Person checkWith)
        {
            List<BaseObject> toRemove = new List<BaseObject>();
            //Run through all of the other objects and check a collision.
            foreach (BaseObject b in objects)
            {
                if (checkWith != b /* no self collisions */
                    && checkWith.position.X > b.position.X - Main.characterDimension.X && checkWith.position.X < b.position.X + Main.characterDimension.X
                    && checkWith.position.Y > b.position.Y - Main.characterDimension.Y && checkWith.position.Y < b.position.Y + Main.characterDimension.Y)
                {
                    //Do collision.
                    if (checkWith.doCollision(b))
                        toRemove.Add(b);
                }
            }

            return toRemove;
        }


        public void broadcastAction(Action toBroadcast)
        {
            foreach (BaseObject b in objects)
            {
                if (b is Person)
                {
                    (b as Person).observeAction(toBroadcast);
                }
            }

        }
    }
}
