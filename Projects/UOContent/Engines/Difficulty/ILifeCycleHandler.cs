using Server.Mobiles;

namespace Server.Engines.Difficulty
{
    public interface ILifeCycleHandler
    {
        bool DeductLife(PlayerMobile to, out int livesRemaining);
        bool AddLife(PlayerMobile to, out int livesRemaining);
        bool Retire(PlayerMobile to);
    }
}
