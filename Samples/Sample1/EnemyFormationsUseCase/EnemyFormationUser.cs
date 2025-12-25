using System.Threading.Tasks;
using GroupBehavior.Runtime;
using GroupBehavior.Runtime.Example;
using TMPro;
using UnityEngine;

namespace GroupBehavior.Samples.Runtime
{
	public class EnemyFormationUser : FormationUser<PlayerFormationTarget, EnemyFormationUser>
	{
		public MeshRenderer meshRenderer;
		public Material leaderMaterial;
		public Material deadMaterial;
		public EnemyMovement enemyMovement;
		public bool acceptLeadership;

		public TextMeshProUGUI debugText;
		private float debugTime;

		void Update()
		{
			if (debugTime > 0)
			{
				debugTime -= Time.deltaTime;
				if (debugTime <= 0)
				{
					debugText.text = "";
				}
			}
		}

		public void SetTarget(PlayerFormationTarget target)
		{
			Subscribe(target, this);
		}

		public override Task PromotedToLeaderAsync()
		{
			meshRenderer.material = leaderMaterial;
			Log("Promoted To Leader", 2);
			return Task.CompletedTask;
		}

		public override void SetFormationPosition(Vector3 position)
		{
			enemyMovement.MoveToPosition(position);
		}

		public void OnDeath()
		{
			meshRenderer.material = deadMaterial;
			Log("Dead", 5);
			Unsubscribe(this);
		}

		public override bool WillAcceptLeadership()
		{
			return acceptLeadership;
		}

		public override Task VoteForLeaderAsync(VotingData<PlayerFormationTarget, EnemyFormationUser> votingData)
		{
			Log("Voting...");
			return base.VoteForLeaderAsync(votingData);
		}

		public override Task DeclinedLeadershipAsync()
		{
			Log("Declined Leadership");
			return base.DeclinedLeadershipAsync();
		}

		public override Task NewLeaderAppointedAsync(EnemyFormationUser newLeader)
		{
			Log("New Leader Appointed: " + newLeader.gameObject.name, 0.5f);
			return base.NewLeaderAppointedAsync(newLeader);
		}

		void Log(string message, float logTime = 1f)
		{
			debugText.text = message;
			debugTime = logTime;
		}
	}
}