using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class DialogueBehaviour : PlayableBehaviour
	{
		public enum DialogueType
		{
			NORMAL_LINE = 0,
			YES_NO_GENERIC = 1,
			YES_NO_FOR_INVOKE_EVENT = 2,
			YES_NO_INVOKE_OR_RESTART_ENCOUNTER = 3
		}

		public enum ShowBarsType
		{
			NoBars = 0,
			OnStartOnly = 1,
			Always = 2
		}

		[SerializeField]
		[HideInInspector]
		private string localizationKey = string.Empty;

		[Header("General Settings")]
		[Tooltip("Set to true if you do not want the timeline to pause once the dialogue line is shown")]
		public bool doNotPause;

		[Tooltip("shows ui bars when dialogue starts. always means the bars stay after dialogue line ends")]
		public ShowBarsType showBars = ShowBarsType.OnStartOnly;

		[CharacterNamePicker]
		public int speakingCharacterName;

		[Tooltip("Show yes no prompt to user")]
		public DialogueType dialogueType;

		[Header("Yesno Settings")]
		public bool onlyShowOnYesAnswer;

		public bool onlyShowOnNoAnswer;

		[Header("Audio Settings")]
		[SerializeField]
		public AudioSourceID channel = AudioSourceID.Voices;

		[SerializeField]
		internal SfxClip sfxClip;

		[SerializeField]
		[Range(-1f, 24f)]
		public int indexOfAudioClip = -1;

		[SerializeField]
		public bool randomAudioClip;

		[HideInInspector]
		internal AudioSource audioSource;

		private DialogueUI dialogueUI;

		private bool clipPlaying;

		internal PlayableDirector director;

		~DialogueBehaviour()
		{
			dialogueUI = null;
			director = null;
		}

		public override void OnPlayableCreate(Playable playable)
		{
			director = playable.GetGraph().GetResolver() as PlayableDirector;
			dialogueUI = Core.Instance.UIManager.dialogueUI;
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			bool answeredYes = SequenceHandler.instance.answeredYes;
			if ((answeredYes && onlyShowOnNoAnswer) || (!answeredYes && onlyShowOnYesAnswer))
			{
				return;
			}
			clipPlaying = true;
			if (dialogueUI.IsShowingDialogue())
			{
				dialogueUI.nextDialogue = this;
				return;
			}
			dialogueUI.StartDialogue(this);
			if (!doNotPause)
			{
				BaseMenuTimelineBehaviour.LoopAnimationTracks(director, info);
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (!clipPlaying || dialogueUI == null)
			{
				return;
			}
			clipPlaying = false;
			if (!doNotPause && dialogueUI.IsShowingDialogue() && dialogueType == DialogueType.NORMAL_LINE)
			{
				if (director.playableGraph.IsValid())
				{
					SequenceHandler.PauseGraph(director);
				}
			}
			else
			{
				dialogueUI.EndDialogue();
			}
		}

		internal string GetLineToSet()
		{
			string lineToSet = "";
			if (!localizationKey.Equals(string.Empty))
			{
				lineToSet = localizationKey;
			}
			return GetLineToSetDebug(lineToSet);
		}

		private string GetLineToSetDebug(string lineToSet)
		{
			return lineToSet;
		}
	}
}
