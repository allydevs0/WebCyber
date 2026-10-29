using UnityEngine;

namespace Reptile
{
	public class JunkStageHandler : MonoBehaviour
	{
		private JunkBehaviour junkBehaviour;

		private void Awake()
		{
			Junk[] componentsInChildren = GetComponentsInChildren<Junk>();
			junkBehaviour = new JunkBehaviour();
			junkBehaviour.Init(componentsInChildren);
			Core.OnUpdate += OnCoreUpdate;
		}

		private void OnCoreUpdate()
		{
			JunkBehaviour.JunkUpdate(junkBehaviour, Core.dt);
		}

		private void Start()
		{
			JunkBehaviour.RestoreAll(junkBehaviour);
		}

		private void OnDestroy()
		{
			Core.OnUpdate -= OnCoreUpdate;
		}
	}
}
