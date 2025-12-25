using GroupBehavior.Runtime;

namespace GroupBehavior.Samples.Runtime
{
	public class PlayerFormationTarget : FormationTarget<PlayerFormationTarget, EnemyFormationUser>
	{
		void Start()
		{
			Subscribe(this);
		}
	}
}