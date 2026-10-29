using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class LookAtIKMixerBehaviour : PlayableBehaviour
	{
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			LookAtIKComponent lookAtIKComponent = playerData as LookAtIKComponent;
			Vector3 zero = Vector3.zero;
			float num = 0f;
			if (!lookAtIKComponent)
			{
				return;
			}
			int inputCount = playable.GetInputCount();
			playable.GetInputWeight(0);
			LookAtIKBehaviour behaviour = ((ScriptPlayable<LookAtIKBehaviour>)playable.GetInput(0)).GetBehaviour();
			zero = behaviour.target.position;
			num = playable.GetInputWeight(0) * behaviour.IKWeight;
			if (inputCount > 1)
			{
				for (int i = 1; i < inputCount; i++)
				{
					playable.GetInputWeight(i);
					LookAtIKBehaviour behaviour2 = ((ScriptPlayable<LookAtIKBehaviour>)playable.GetInput(i)).GetBehaviour();
					zero = Vector3.Lerp(zero, behaviour2.target.position, playable.GetInputWeight(i) * behaviour2.IKWeight);
					num += playable.GetInputWeight(i) * behaviour2.IKWeight;
				}
			}
			lookAtIKComponent.targetPosition = zero;
			lookAtIKComponent.weight = num;
		}
	}
}
