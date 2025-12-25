using GroupBehavior.Samples.Runtime;
using UnityEngine;

namespace GroupBehavior.Runtime.Example
{
	public class PlayerEnemyFormationManager : FormationsManager<PlayerFormationTarget, EnemyFormationUser>
	{
		public PlayerFormationTarget PlayerTarget;

		public override void Update()
		{
			base.Update();
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				var group = FormationGroups[PlayerTarget];
				group.Leader.OnDeath();
			}

			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				var group = FormationGroups[PlayerTarget];
				if (group.Users.Count > 0)
				{
					group.Users.GetRandom().OnDeath();
				}
			}
		}
	}
}