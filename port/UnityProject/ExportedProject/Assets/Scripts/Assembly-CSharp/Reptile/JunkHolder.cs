using UnityEngine;

namespace Reptile
{
	public class JunkHolder : MonoBehaviour
	{
		internal bool moved;

		private Junk[] junkChildren;

		public Rigidbody[] stuffToLaunchAway;

		public SfxClip audioClips = SfxCollectionID.EnvironmentSfx;

		private void Awake()
		{
			junkChildren = GetComponentsInChildren<Junk>();
		}

		private void OnDestroy()
		{
			junkChildren = null;
			stuffToLaunchAway = null;
		}

		public void FallApart(Vector3 hitPos, bool byBump = true)
		{
			if (moved)
			{
				return;
			}
			moved = true;
			Core.Instance.AudioManager.PlaySfxGameplay(audioClips, WorldHandler.instance.GetSpatialAudioSource(base.transform.position));
			int num = junkChildren.Length;
			for (int i = 0; i < num; i++)
			{
				Junk junk = junkChildren[i];
				junk.FallApart(byBump);
				if (!byBump || !(hitPos != Vector3.zero))
				{
					continue;
				}
				for (int j = 0; j < stuffToLaunchAway.Length; j++)
				{
					if (junk.transform == stuffToLaunchAway[j].transform)
					{
						stuffToLaunchAway[j].AddExplosionForce(300f, hitPos, 5f, 1.5f);
					}
				}
			}
		}

		public void Restore()
		{
			moved = false;
		}
	}
}
