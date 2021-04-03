using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Difficulty
{
    public class DifficultyApplier : IDifficultyApplier
    {
        public bool ApplyDifficulty(PlayerMobile to, DifficultyMode mode)
        {
            if (to.DifficultyMode == DifficultyMode.Hard || to.DifficultyMode == DifficultyMode.Nightmare)
            {
                to.SendMessage($"Unable to apply {mode}. Your current mode is {to.DifficultyMode}.");
                return false;
            }

            var livesToApply = ServerConfiguration.GetSetting($"difficulty.{mode}", 50);
            to.LivesRemaining = livesToApply;
            to.SendMessage($"The following difficulty has been applied to your character: {mode}");

            return true;
        }
    }
}
