using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Boy_Meets_Girl
{

    /// <summary>
    /// A lot of the variables are pretty messed up.  The motivation system were never really fleshed out, but that's fine for now.
    /// </summary>
    class Person : BaseObject
    {
        //Type for action pair.
        public delegate void action(params dynamic[] parameters);

        //It's, um...
        #region variables

        #region base

        /// <summary>
        /// Your current inventory.
        /// Int because color is the only thing we care about with flower.
        /// </summary>
        public List<int> inventory;

        /// <summary>
        /// This may need to be changed to a dictionary.
        /// </summary>
        public List<Action> knownCausality;

        /// <summary>
        /// A way to keep track of who's worth watching.
        /// </summary>
        public Dictionary<Person, int> peopleOfInterest;

        /// <summary>
        /// AI's know where they are.
        /// </summary>
        private World world;

        /// <summary>
        /// This is in pixels per second in order to simplify the math.
        /// </summary>
        public float movementSpeed = Main.random.Next(40, 80);

        /// <summary>
        /// Use 1 (right), 0, and -1 (left) to indicate a movement direction.  Everything else is handled in-house.
        /// </summary>
        public int movementX;

        /// <summary>
        /// Use 1 (down), 0, and -1 (up) to indicate a movement direction.  Everything else is handled in-house.
        /// </summary>
        public int movementY;

        /// <summary>
        /// Whether or not this person is under the control of the player.
        /// </summary>
        public bool controlled;

        /// <summary>
        /// Used for learning about the world. Timer hook back up to the world.
        /// </summary>
        public int currentInterest;

        /// <summary>
        /// What did you see?  Hold on to it for a while and see what you can figure out with this.
        /// </summary>
        public Action shortTermMemory;

        #endregion

        #region personality

        /// <summary>
        /// "People go into AI and the first thing they say is, I want to make my AI cry!" - David Schwartz
        /// </summary>
        public int happiness;

        /// <summary>
        /// What color do you like?
        /// </summary>
        public int favoriteColor;

        /// <summary>
        /// Just how interested are they in collecting flowers?
        /// </summary>
        public int flowerInterest;

        /// <summary>
        /// Implement this later - will they just collect everything?
        /// </summary>
        public bool greedy;

        /// <summary>
        /// How many steps forward the person thinks.
        /// </summary>
        public int intelligence;

        /// <summary>
        /// If your plans are interrupted, how will it affect you?
        /// </summary>
        public int dissapointment;

        #endregion

        //
        //public string name?
        //The interface is the next interesting thing to look at.
        //

        #endregion

        //You know what this is.  Stop it.
        #region constructor

        /// <summary>
        /// Fun with constructors, eh?
        /// </summary>
        /// <param name="x">xposition of person</param>
        /// <param name="y">yposition of person</param>
        /// <param name="world">Hook back to the current world.</param>
        public Person(int x, int y, World world, bool controlled)
            : base(x, y, Main.colors.Length - 1, "P")
        {
            inventory = new List<int>();

            //Everybody starts with one flower of each color.
            inventory.Add(1); inventory.Add(2); inventory.Add(0);

            knownCausality = new List<Action>();
            peopleOfInterest = new Dictionary<Person, int>();
            this.world = world;
            this.controlled = controlled;

            //Set up the basic knowledge - picking flowers of a certain color will make you happy.
            favoriteColor = Main.random.Next(0, Main.colors.Length - 1);
            flowerInterest = Main.random.Next(0, 10);
            intelligence = 9;
            dissapointment = Main.random.Next(0, 10);

            //Start off knowing how to pick up a flower.
            knownCausality.Add(new Action(this, pickFlower, null, new dynamic[] { this.favoriteColor}, 100, flowerInterest));
        }

        #endregion

        //AI updates and collision detection.
        #region update Code

        /// <summary>
        /// Handles all of the updates for the current person.  Called at a rate of once every 1/60th of a second.
        /// Also has collisions, and when it finishes, it returns a list of objects it would like removed from the map.
        /// </summary>
        public List<BaseObject> update()
        {
            List<BaseObject> toRemove = world.runCollision(this); //Check for collisions.
            if (!controlled) //Run the AI (movement, etc) if a player isn't in charge of this dude.
                runAI();

            //Update position.
            this.position.X += this.movementX * this.movementSpeed/60;
            this.position.Y += this.movementY * this.movementSpeed/60;

            return toRemove;
        }

        /// <summary>
        /// Allows the person to traverse their tree.
        /// </summary>
        private void runAI()
        {
            //What actions are available to you?
            Action toCheck = null;
            int toDo = -1;
            int happyindex = 0;

            //Start out standing still
            movementX = 0;
            movementY = 0;

            //Check if there's a flower nearby.
            foreach (BaseObject b in world.objects)
            {
                //If b is a flower
                if (b is Flower)
                {
                    //Run the decision process to see if it's worth picking up.
                    toCheck = new Action(this, this.pickFlower, null, new dynamic[] { (b as Flower).color }, 0, 0);

                    //Check the action you just got to see how good it is for you.
                    int index = knownCausality.IndexOf(toCheck);
                    int happycheck; //How happy the action will make you.
                    if (index != -1) //If the action is one you know will make something happen.
                    {
                        if ((happycheck = traverseTree(knownCausality[index], this.intelligence)) > happyindex) //Check to see if it ends up being a net positive action and is better than your previous plan.
                        {
                            happyindex = happycheck; //This is your new priority.
                            toDo = index;
                        }
                    }
                }
                if (b is Person)
                {
                    if (this.peopleOfInterest.ContainsKey(b as Person)) //If that's a person you care about.
                    {
                        for (int i = 0; i < Main.colors.Length - 1; i++)
                        {
                            if(inventory.Contains(i)) //Do you have that flower?
                                //What would happen if I gave them a flower?
                                toCheck = new Action(this, giveFlower, null, new dynamic[] { (b as Person), i }, 100, 1);

                            //Check the action you just got to see how good it is for you.
                            int index = knownCausality.IndexOf(toCheck);
                            int happycheck; //How happy the action will make you.
                            if (index != -1) //If the action is one you know will make something happen.
                            {
                                if ((happycheck = traverseTree(knownCausality[index], this.intelligence)) > happyindex) //Check to see if it ends up being a net positive action and is better than your previous plan.
                                {
                                    happyindex = happycheck; //This is your new priority.
                                    toDo = index;
                                }
                            }
                        }
                    }
                }
            }


            //If you've reached a decision and you like it.
            if (happyindex > 0)
                knownCausality[toDo].performAction(); //Do the action.


            //By the way, are you paying attention to anyone or anything?  Your attention starts to waver.
            if (currentInterest > 0)
                currentInterest--;
            else //Wipe everything.
            {
                if (shortTermMemory != null)
                {
                    shortTermMemory = null;
                }
            }   
        }

        /// <summary>
        /// Traverses a simple graph of causality and determins if an action is worth doing.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns>The net positive or negative of the action.</returns>
        private int traverseTree(Action currentNode, int intelligence)
        {
            //Very simple linear traversal.  We'll see if there are other ways to build decision making into this.
            if (intelligence != 0 && currentNode.reaction != null)
                return currentNode.netPositive + traverseTree(currentNode.reaction, intelligence-1);
            else
                return currentNode.netPositive;
        }

        public bool doCollision(BaseObject b)
        {
            if (b is Flower)
            {
                //Add it to your inventory.
                inventory.Add((b as Flower).color);

                if (this.controlled == true)
                {
                    int j = 3;
                }

                //Tell the world what you did.
                world.broadcastAction(new Action(this, this.getFlower, null, new dynamic[] { (b as Flower).color }, 100, 5));

                if(b.color == favoriteColor)
                    increaseOwnHappiness(flowerInterest);

                return true; //Collision is finished.  Object is to be removed.
            }

            return false; //Collision is finished.  Do not remove object.
        }

        public void observeAction(Action toObserve)
        {
            //If you aren't already looking around to figure something out.
            if (currentInterest == 0)
            {
                //Do you care about this guy?  You can't observe yourself since you don't care about yourself.  I think.
                if (peopleOfInterest.ContainsKey(toObserve.subject) && toObserve.subject != this)
                {
                    //increase awareness.
                    currentInterest = peopleOfInterest[toObserve.subject];
                    shortTermMemory = toObserve;

                }
            }
            else //If so, this is important.  It might be causation.
            {
                //Make the action reaction pair.
                int justSeen = knownCausality.IndexOf(toObserve);
                if (justSeen != -1)
                    shortTermMemory.reaction = knownCausality[justSeen]; //Connection with previously observed events
                else
                {
                    shortTermMemory.reaction = toObserve;
                    toObserve.reactionCertainty = 0; //You just saw this, and you're not particularly interested in whether or not it has its own causality.
                    knownCausality.Add(toObserve); //Also add it to the list so you'll know it if you run into it again.
                }

                justSeen = knownCausality.IndexOf(shortTermMemory);
                if (justSeen != -1) //You've seen this before.  Does it conform to your expectations?
                    knownCausality[justSeen].combineAction(shortTermMemory);
                else //You just saw this.  Add it now.
                    knownCausality.Add(shortTermMemory);

                //Is the action you just saw involve someone you weren't paying attention to before?  Start paying attention to them now.
                if(shortTermMemory.reaction.subject != this) //As long as it isn't you.
                    if(shortTermMemory.reaction.subject != shortTermMemory.subject) //Or the person you were watching before.
                        if (!peopleOfInterest.ContainsKey(shortTermMemory.reaction.subject))
                        {
                            peopleOfInterest.Add(shortTermMemory.reaction.subject, 1); //This guy is interesting.
                            //I'm going to set up some basic logic for this person.

                            for (int i = 0; i < Main.colors.Length - 1; i++) //For each flower color.
                            {
                                Action get = new Action(shortTermMemory.reaction.subject, shortTermMemory.reaction.subject.getFlower, null, new dynamic[] { i }, 100, 0);
                                //If it's your favorite color, you feel a loss by giving it.
                                knownCausality.Add(new Action(this, giveFlower, get, new dynamic[] { shortTermMemory.reaction.subject, i }, 100, ((color == favoriteColor) ? 50 : 50)));
                                knownCausality.Add(get);
                            }
                        }

                //Now set yourself to pay attention to what you just observed and see what happens.
                shortTermMemory = null;
                currentInterest = 0;
                observeAction((justSeen == -1)? toObserve : knownCausality[justSeen]);
            }
        }

        #endregion

        //These are the actions that are linked to delegates in the action reaction pair.
        #region actions

        /// <summary>
        /// Pick the closest flower that's your favorite color.
        /// </summary>
        public void pickFlower(params dynamic[] parameters)
        {
            //Get a color.
            int color = parameters[0];
            //Get closest flower.
            Flower closestFlower = null;
            foreach (BaseObject b in world.objects)
            {
                //If b is a flower and it's a flower that you want.
                if (b is Flower && b.color == this.favoriteColor)
                {
                    //If it's closer than what you're currently targeting.
                    if (closestFlower == null || (
                        Math.Abs(this.position.X - closestFlower.position.X) > Math.Abs(this.position.X - b.position.X)
                        && Math.Abs(this.position.Y - closestFlower.position.Y) > Math.Abs(this.position.Y - b.position.Y)))
                    {
                        closestFlower = b as Flower;
                    }
                }
            }

            if (closestFlower == null)
            {
                //If you've gotten here it means you went for a flower and someone else took it.
                //Depending on a variable, this might make you sadder.
                //Or maybe it's just time to re-update.  At the very least you should stop moving.
                happiness -= dissapointment;
                movementX = 0;
                movementY = 0;
            }
            else
            {
                //Start moving towards the flower.
                this.movementX = ((closestFlower.position.X - this.position.X) > 0)? 1 : -1;
                this.movementY = ((closestFlower.position.Y - this.position.Y) > 0)? 1 : -1;
            }
        }

        /// <summary>
        /// Gives a flower to a person of your choice.
        /// This should be a net negative.  Extra negative if it's a color you like.
        /// Add this action when someone else broadcasts that they've picked up a flower.
        /// </summary>
        /// <param name="parameters">Two parameters - the person, and the color flower.  We'll handle the rest of the inventory management and distance and stuff.</param>
        public void giveFlower(params dynamic[] parameters)
        {
            //If you have a flower of that color.  This should be an uneccessary check, but it's for safety's sake.
            if (inventory.Contains(parameters[1]))
            {
                if (parameters[0] is Person) //Don't know why I'm so suddenly attached to the idea of checking for this...
                {
                    //Simple rectangular collision detection.  You have to be a certain distance from someone to give them an item.  It's hardcoded for now.
                    //It's also broken and you need to fix it.
                    if (world.player == this || /* if you're the player, you can ignore distance */
                        ((parameters[0] as Person).position.X > this.position.X - 12 && (parameters[0] as Person).position.X < this.position.X + 12
                    && (parameters[0] as Person).position.Y > this.position.Y - 12 && (parameters[0] as Person).position.Y < this.position.Y + 12))
                    {
                        //Give the item.
                        this.inventory.Remove(parameters[1]);
                        //Feel the loss if it's your favorite.
                        /*if (parameters[1] == favoriteColor)
                            happiness -= 5; //This should be an event in the future, but for now, causality doesn't use n-trees, so it can't be without messing everything else up.
                        */

                        //You just gave someone something.
                        world.broadcastAction(new Action(this, giveFlower, null, new dynamic[] { parameters[0], parameters[1] }, 100, 0));
                        //The other person now gets the item.  Update them accordingly.
                        (parameters[0] as Person).getFlowerReal(parameters[1], this);

                    }
                    else
                    {
                        //Move towards the person.
                        this.movementX = (((parameters[0] as Person).position.X - this.position.X) > 0) ? 1 : -1;
                        this.movementY = (((parameters[0] as Person).position.Y - this.position.Y) > 0) ? 1 : -1;
                    }
                }
            }
        }

        /// <summary>
        /// The end goal.  Certain actions will just trigger this.  This probably shouldn't be in this section.
        /// </summary>
        public void increaseOwnHappiness(params dynamic[] parameters)
        {
            //Also tell the world you got happier.
            this.happiness += parameters[0];
            world.broadcastAction(new Action(this, increaseOwnHappiness, null, null, 100, 200)); //People want other people to be happy.
        }

        /// <summary>
        /// Get a flower from another person.
        /// </summary>
        /// <param name="parameters">the color of the flower (int) - the person giving the flower (Person)</param>
        public void getFlowerReal(int color, Person person)
        {
            inventory.Add(color);

            if (color == favoriteColor)
            {

                world.broadcastAction(new Action(this, this.getFlower, null, new dynamic[] { color }, 100, 50));
                increaseOwnHappiness(flowerInterest); //Get happy.
                if (!peopleOfInterest.ContainsKey(person)) //Pay attention to this guy.
                {
                    peopleOfInterest.Add(person, flowerInterest);
                    //I'm going to set up some basic logic for this person.

                    for (int i = 0; i < Main.colors.Length - 1; i++) //For each flower color.
                    {
                        Action get = new Action(person, person.getFlower, null, new dynamic[] { i }, 0, 0);
                        //If it's your favorite color, you feel a loss by giving it.
                        knownCausality.Add(new Action(this, giveFlower, get, new dynamic[] { person, i }, 100, ((color == favoriteColor)? -5 : 0)));
                        knownCausality.Add(get);
                    }

                    //Also increase your empathy, which doesn't exist yet.

                }
                else
                    peopleOfInterest[person] += 1; //I'm even more interested in you.  Also fixes the problem with people that aren't interested in anything.
                    //This is where the empathy stat would come in.
            }
        }


        /// <summary>
        /// Get a flower from another person.
        /// </summary>
        /// <param name="parameters">the color of the flower (int)</param>
        public void getFlower(params dynamic[] parameters)
        {
            //An empty hook that can be used for the AI.  The actual code will call the method above.
        }

        #endregion

        //These methods allow the person to share information with the main game engine.
        #region reflection

        public override string[] getInfo()
        {
            return new string[] { "Happiness: " + happiness, "Favorite Color: " + favoriteColor, "Flower Interest: " + flowerInterest, "Number of people interested in: " + peopleOfInterest.Count, "Interest towards you: " + ((peopleOfInterest.ContainsKey(world.player)) ? peopleOfInterest[world.player] : 0 ) };
        }

        #endregion

    }
}
