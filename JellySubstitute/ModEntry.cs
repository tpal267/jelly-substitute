using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace SubstituteJelly
{
    public class ModEntry : Mod
    {
        public int sf_prev = 0; //cache for last known friendship value
        NPC spouse = null;

        public override void Entry(IModHelper helper) //Initialization boilerplate
        {
            helper.Events.GameLoop.DayStarted += this.FindSpouse;               //Game always starts at start of day, also ensures fuction immediately after the player gets married.
            helper.Events.GameLoop.OneSecondUpdateTicked += this.JellyFinder;   //One second seems to not affect performance much, though could be longer if needed.

            helper.ConsoleCommands.Add("player_getsf", "Displays exact friendship rating of the player's spouse.\n\nUsage: player_getsf", this.GetSF); //Init my console debug function
        }

        private void FindSpouse(object sender, EventArgs e)
        {
            //this.Monitor.Log("Entering Game", LogLevel.Info); //debug code, check that entry is done correctly
            spouse = Game1.player.getSpouse();
            if (spouse == null)
                return;

            sf_prev = Game1.player.getFriendshipLevelForNPC(spouse.getName());
            //this.Monitor.Log($"On game load spouse friendship is {sf_prev}",LogLevel.Warn); //debug code
        }

        private void JellyFinder(object sender, EventArgs e)
        {
            if (spouse == null)
                return; //Return if no spouse
            if (Game1.player.getFriendshipLevelForNPC(spouse.getName()) == sf_prev)
                return; //Return if no change in friendship in last second
            else if (Game1.player.getFriendshipLevelForNPC(spouse.getName()) > sf_prev)
                sf_prev = Game1.player.getFriendshipLevelForNPC(spouse.getName());
            //If spousal friendship has increased in last second, update counter.
            else if (Game1.player.getFriendshipLevelForNPC(spouse.getName()) == (sf_prev - 30))
            {
                int sf_tmp = Game1.player.getFriendshipLevelForNPC(spouse.getName());
                //this.Monitor.Log($"Spouse friendship changed to {sf_tmp}", LogLevel.Warn); //Debug code
                Game1.player.changeFriendship(30, spouse);
                //If spousal friendship has fallen exactly 30 points in last second, add 30 points.
                sf_prev = Game1.player.getFriendshipLevelForNPC(spouse.getName()); //Redundant

                //Only allow "jealousy" lines to be used if they are overwritten by my content patch.
                if (!this.Helper.ModRegistry.IsLoaded("WitlessJester.JellyLines"))
                    spouse.resetCurrentDialogue(); //Flush queued dialog

                this.Monitor.Log("Jealousy Event", LogLevel.Trace);
                // this.Monitor.Log($"Spouse friendship is now {sf_prev}", LogLevel.Warn); //Debug code
            }
            else sf_prev = Game1.player.getFriendshipLevelForNPC(spouse.getName());
            //If spousal friendship has fallen by any other amount, update counter. 
            return;
        }

        private void GetSF(string command, string[] args) //Command line tool to check spouse friendship
        {
            if (spouse == null)
            {
                this.Monitor.Log("The current farmer is not married.", LogLevel.Info);
                return;
            }

            int sf_tmp = Game1.player.getFriendshipLevelForNPC(spouse.getName());
            this.Monitor.Log($"Current Spouse Friendship: {sf_tmp}", LogLevel.Info);
        }
    }
}
