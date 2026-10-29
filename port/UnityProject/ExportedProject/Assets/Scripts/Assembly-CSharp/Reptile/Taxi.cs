using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(NPC))]
	public class Taxi : MonoBehaviour
	{
		public void OnEnable()
		{
			GetComponent<NPC>().dialogueLevel = 0;
		}
	}
}
