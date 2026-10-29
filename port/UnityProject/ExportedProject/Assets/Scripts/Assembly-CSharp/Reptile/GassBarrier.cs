using UnityEngine;

namespace Reptile
{
	public class GassBarrier : MonoBehaviour
	{
		public void DoGas(Player player)
		{
			player.GetHit(1, player.tf.forward + player.tf.right * Random.Range(-0.2f, 0.2f), KnockbackAbility.KnockbackType.BIG);
		}
	}
}
