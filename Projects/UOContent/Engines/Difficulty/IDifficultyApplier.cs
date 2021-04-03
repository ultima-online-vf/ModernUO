
using Server.Mobiles;

namespace Server.Engines.Difficulty
{
    public interface IDifficultyApplier
    {
        // Refactor this to OneOf
        bool ApplyDifficulty(PlayerMobile to, DifficultyMode mode);
    }
}
