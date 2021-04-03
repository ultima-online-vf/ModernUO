using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Difficulty
{
    public class LifeCycleHandler : ILifeCycleHandler
    {
        public bool DeductLife(PlayerMobile to, out int livesRemaining)
        {
            if (to.DifficultyMode == DifficultyMode.Normal)
            {
                livesRemaining = 999;
                return true;
            }

            if (to.Alive)
            {
                livesRemaining = to.LivesRemaining;
                return false;
            }

            if (to.LivesRemaining <= 0)
            {
                Retire(to);
                livesRemaining = 0;
                return true;
            }

            to.LivesRemaining -= 1;
            to.SendMessage($"Your remaining lives: {to.LivesRemaining}.");
            livesRemaining = to.LivesRemaining;
            return true;
        }

        public bool AddLife(PlayerMobile to, out int livesRemaining)
        {
            if (to.DifficultyMode == DifficultyMode.Normal)
            {
                livesRemaining = 999;
                return true;
            }

            to.LivesRemaining += 1;
            to.SendMessage($"You gained an extra life!");
            livesRemaining = to.LivesRemaining;
            return true;
        }

        public bool Retire(PlayerMobile to)
        {
            to.SendMessage($"You're fucked bro.");
            to.NetState.Connection.Close();
            // Here we'll be burrying our character
            return true;
        }
    }
}
