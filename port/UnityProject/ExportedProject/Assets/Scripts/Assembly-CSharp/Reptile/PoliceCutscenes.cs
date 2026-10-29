using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class PoliceCutscenes : MonoBehaviour
	{
		[SerializeField]
		private PlayableDirector cutsceneFor0Stars;

		[SerializeField]
		private PlayableDirector cutsceneFor1Stars;

		[SerializeField]
		private PlayableDirector cutsceneFor2Stars;

		[SerializeField]
		private PlayableDirector cutsceneFor3Stars;

		[SerializeField]
		private PlayableDirector cutsceneFor4Stars;

		[SerializeField]
		private PlayableDirector cutsceneFor5Stars;

		[SerializeField]
		private PlayableDirector cutsceneFor6Stars;

		private SequenceHandler sequenceHandlerInstance;

		public void PlaySequenceForStars(int stars)
		{
			if (sequenceHandlerInstance == null)
			{
				sequenceHandlerInstance = SequenceHandler.instance;
			}
			switch (stars)
			{
			case 2:
				sequenceHandlerInstance.StartEnteringSequence(cutsceneFor2Stars, null, setHidePlayer: true, setInterruptPlayer: false, instantly: false, setPausePlayer: true);
				break;
			case 3:
				sequenceHandlerInstance.StartEnteringSequence(cutsceneFor3Stars, null, setHidePlayer: true, setInterruptPlayer: false, instantly: false, setPausePlayer: true);
				break;
			case 4:
				sequenceHandlerInstance.StartEnteringSequence(cutsceneFor4Stars, null, setHidePlayer: true, setInterruptPlayer: false, instantly: false, setPausePlayer: true);
				break;
			case 5:
				sequenceHandlerInstance.StartEnteringSequence(cutsceneFor5Stars, null, setHidePlayer: true, setInterruptPlayer: false, instantly: false, setPausePlayer: true);
				break;
			case 6:
				sequenceHandlerInstance.StartEnteringSequence(cutsceneFor6Stars, null, setHidePlayer: true, setInterruptPlayer: false, instantly: false, setPausePlayer: true);
				break;
			}
		}
	}
}
