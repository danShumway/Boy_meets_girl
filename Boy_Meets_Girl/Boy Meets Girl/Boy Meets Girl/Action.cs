using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boy_Meets_Girl
{
    /// <summary>
    /// Modeling causality.
    /// </summary>
    class Action
    {
        /// <summary>
        /// Who's doing the action.
        /// </summary>
        public Person subject;

        /// <summary>
        /// What the action is.
        /// </summary>
        public Person.action action;

        /// <summary>
        /// What the action causes to happen.
        /// </summary>
        public Action reaction;

        /// <summary>
        /// How certain you are that the action will happen.
        /// </summary>
        public int reactionCertainty;

        /// <summary>
        /// How much the action will affect your happiness.
        /// </summary>
        public int netPositive;

        /// <summary>
        /// What parameters to pass to whatever delegate you're linking.
        /// </summary>
        public dynamic[] parameters;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="subject">Who's performing the action.</param>
        /// <param name="action">What the action is.</param>
        /// <param name="reaction">The next link</param>
        /// <param name="parameters">The parameters for the function.</param>
        /// <param name="reactionCertainty">How certain you are that this action is actually legit.</param>
        /// <param name="netPositive">How much it will make you happy</param>
        public Action(Person subject, Person.action action, Action reaction, dynamic[] parameters, int reactionCertainty, int netPositive)
        {
            this.subject = subject;
            this.action = action;
            this.reaction = reaction;
            this.parameters = parameters;
            this.reactionCertainty = reactionCertainty;
            this.netPositive = netPositive;
        }

        /// <summary>
        /// Do it!
        /// </summary>
        public void performAction()
        {
            this.action(parameters);
        }

        /// <summary>
        /// Modifies an action by increasing or decreasing the certainty that it will lead to another reaction.
        /// </summary>
        /// <param name="newInfo">A new action-reaction that's been observed and needs to be banked against memory.</param>
        public void combineAction(Action newInfo)
        {
            if (reaction == null)
            {
                reaction = newInfo.reaction;
            }

            if (newInfo.reaction != this.reaction && newInfo.reaction != null) //If it's a new reaction and (for the purposes of this, if it's not null)
            {
                /*this.reactionCertainty -= newInfo.reactionCertainty;
                if (this.reactionCertainty <= 0)
                {
                    reactionCertainty = newInfo.reactionCertainty;
                    reaction = newInfo.reaction;
                }*/
            }
            else
            {
                this.reactionCertainty += newInfo.reactionCertainty;
            }

            //Either way, your preference for an action is much more fickle.
            //this.netPositive = newInfo.netPositive;
        }

        /// <summary>
        /// Used to check if you already have a model for the current action.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj != null)
                    if (this.subject == (obj as Action).subject)
                    {
                        //Make sure the parameters are the same.
                        bool match = true;
                        if(this.parameters != null)
                        {
                            if((obj as Action).parameters != null)
                            {
                                if(this.parameters.Length == (obj as Action).parameters.Length)
                                {
                                    for (int i = 0; i < parameters.Length; i++)
                                    {
                                        if (parameters[i] != (obj as Action).parameters[i])
                                        {
                                            match = false;
                                            break; //Completely unnecessary, but saves some processing power, and that's worth the bad style for me.
                                        }
                                    }

                                    return match; //If you got here, you can make a decision.
                                }
                                else
                                    match = false;
                            }
                            else
                                match = false;
                        }
                        else
                            if((obj as Action).parameters == null)
                                match = true;
                            else
                                match = false;

                        return match; //Also done.
                    }

            return false; //Also done.
        }
    }
}
