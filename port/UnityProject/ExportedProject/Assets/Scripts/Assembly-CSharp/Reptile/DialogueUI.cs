using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;

namespace Reptile
{
	public class DialogueUI : MonoBehaviour
	{
		private const float BLACK_BARS_ANIMATION_DURATION = 0.25f;

		private const string CHARACTER_NAME_FORMAT = "{0}:";

		[SerializeField]
		private RectTransform dialogueScreen;

		[SerializeField]
		private RectTransform textBox;

		[SerializeField]
		private TextMeshProUGUI characterNameText;

		[SerializeField]
		private GameObject characterNameTextContainer;

		[SerializeField]
		private TextMeshProUGUI textLabel;

		[SerializeField]
		private YesNoPanel yesNoScreen;

		[SerializeField]
		private RectTransform continueButton;

		private DialogueBehaviour currentDialogue;

		internal DialogueBehaviour nextDialogue;

		private Coroutine typingTextCoroutine;

		private bool allTextVisible;

		private bool showPrompt;

		private DialogueBehaviour.DialogueType action;

		private bool showContinueButton;

		private bool fastForwardTypewriter;

		private IGameTextLocalizer gameTextLocalizer;

		private UIManager uiManager;

		private EffectsUI effectsUI;

		private GameInput gameInput;

		private string currentLocalizationLine = string.Empty;

		public bool CanBeSkipped
		{
			get
			{
				return showContinueButton;
			}
			set
			{
				showContinueButton = value;
			}
		}

		public bool ReadyToResume => allTextVisible;

		public bool isYesNoPromptEnabled => yesNoScreen.IsEnabled;

		public void Init(IGameTextLocalizer gameTextLocalizer, UIManager uiManager, GameInput gameInput)
		{
			this.gameTextLocalizer = gameTextLocalizer;
			this.uiManager = uiManager;
			this.gameInput = gameInput;
			effectsUI = uiManager.effects;
			base.gameObject.SetActive(value: true);
			dialogueScreen.gameObject.SetActive(value: false);
			continueButton.gameObject.SetActive(value: false);
			yesNoScreen.GetRoot().gameObject.SetActive(value: false);
			characterNameText.enabled = false;
			gameInput.ListenToLastActiveControllerChanged(OnLastActiveControllerChanged);
			gameTextLocalizer.OnLanguageChanged += OnLanguageChanged;
		}

		private void OnDestroy()
		{
			gameInput.StopListenToLastActiveControllerChanged(OnLastActiveControllerChanged);
			gameTextLocalizer.OnLanguageChanged -= OnLanguageChanged;
		}

		public bool IsDialogueUIActive()
		{
			return dialogueScreen.gameObject.activeInHierarchy;
		}

		public void ToggleDialogueUI(bool state)
		{
			if (dialogueScreen.gameObject.activeInHierarchy != state)
			{
				dialogueScreen.gameObject.SetActive(state);
			}
		}

		public void RequestYesNoPrompt(DialogueBehaviour.DialogueType setAction)
		{
			showPrompt = true;
			action = setAction;
		}

		public void EnableYesNoPrompt()
		{
			if (!yesNoScreen.IsEnabled)
			{
				showPrompt = false;
				yesNoScreen.SetCallbacks(() =>
				{
					OnButtonPressed(action, yesClicked: true);
				}, () =>
				{
					OnButtonPressed(action, yesClicked: false);
				});
				Core.Instance.UIManager.ShowDialogueOptions(yesNoScreen);
			}
		}

		public void DisableYesNoPrompt()
		{
			showPrompt = false;
			if (yesNoScreen.IsEnabled)
			{
				yesNoScreen.ResetCallbacks();
				Core.Instance.UIManager.HideDialogueOptions();
			}
		}

		public void FastForwardTypewriter()
		{
			fastForwardTypewriter = true;
		}

		private void OnButtonPressed(DialogueBehaviour.DialogueType action, bool yesClicked)
		{
			if (IsShowingDialogue())
			{
				EndDialogue();
			}
			switch (action)
			{
			case DialogueBehaviour.DialogueType.YES_NO_FOR_INVOKE_EVENT:
				SequenceHandler.instance.AnswerYesNoInvokeEvent(yesClicked);
				break;
			case DialogueBehaviour.DialogueType.YES_NO_INVOKE_OR_RESTART_ENCOUNTER:
				SequenceHandler.instance.AnswerYesNoInvokeOrRestartEncounter(yesClicked);
				break;
			default:
				SequenceHandler.instance.AnswerYesNoGeneric(yesClicked);
				break;
			}
		}

		public void SetLine(string line)
		{
			currentLocalizationLine = line;
			textLabel.maxVisibleCharacters = 0;
			allTextVisible = false;
			fastForwardTypewriter = false;
			continueButton.gameObject.SetActive(value: false);
			UpdateTextLabel(currentLocalizationLine);
			UpdateDialogueContinueInterface();
			StopTypingText();
			typingTextCoroutine = StartCoroutine(TypeText());
		}

		public bool IsShowingDialogue()
		{
			return currentDialogue != null;
		}

		public void StartDialogue(DialogueBehaviour dialogue)
		{
			currentDialogue = dialogue;
			if (currentDialogue == null)
			{
				return;
			}
			if (currentDialogue.showBars != DialogueBehaviour.ShowBarsType.NoBars)
			{
				effectsUI.ShowBars(0.25f, 150f);
			}
			if (currentDialogue.sfxClip.audioClipId != AudioClipID.NONE && currentDialogue.sfxClip.audioClipId != 0)
			{
				if (currentDialogue.audioSource != null)
				{
					Core.Instance.AudioManager.PlaySfx(currentDialogue.sfxClip, currentDialogue.randomAudioClip, currentDialogue.indexOfAudioClip, currentDialogue.audioSource, currentDialogue.channel);
				}
				else
				{
					Core.Instance.AudioManager.PlaySfx(currentDialogue.sfxClip, currentDialogue.randomAudioClip, currentDialogue.indexOfAudioClip, currentDialogue.channel);
				}
			}
			CanBeSkipped = !currentDialogue.doNotPause;
			ToggleDialogueUI(state: true);
			UpdateDialogueLocalization();
			characterNameText.enabled = currentDialogue.speakingCharacterName > 0;
			if (currentDialogue.dialogueType == DialogueBehaviour.DialogueType.YES_NO_INVOKE_OR_RESTART_ENCOUNTER || currentDialogue.dialogueType == DialogueBehaviour.DialogueType.YES_NO_FOR_INVOKE_EVENT || currentDialogue.dialogueType == DialogueBehaviour.DialogueType.YES_NO_GENERIC)
			{
				RequestYesNoPrompt(currentDialogue.dialogueType);
				SequenceHandler.PauseGraph(dialogue.director);
			}
		}

		public void AbortDialogue()
		{
			nextDialogue = null;
			EndDialogue();
		}

		public void EndDialogue()
		{
			if (currentDialogue != null && currentDialogue.showBars == DialogueBehaviour.ShowBarsType.OnStartOnly && effectsUI != null)
			{
				effectsUI.HideBars(0.25f);
			}
			StopTypingText();
			ToggleDialogueUI(state: false);
			DisableYesNoPrompt();
			StartDialogue(nextDialogue);
			nextDialogue = null;
		}

		public void StopTypingText()
		{
			if (typingTextCoroutine != null)
			{
				StopCoroutine(typingTextCoroutine);
			}
		}

		private void UpdateTextLabel(string line)
		{
			if (currentDialogue.speakingCharacterName > 0)
			{
				characterNameText.text = $"{gameTextLocalizer.GetCharacterName(currentDialogue.speakingCharacterName)}:";
				characterNameTextContainer.SetActive(value: true);
			}
			else
			{
				characterNameText.text = string.Empty;
				characterNameTextContainer.SetActive(value: false);
			}
			textLabel.text = line;
			uiManager.SetLocalizationValueOnDialogueText(textLabel, gameTextLocalizer);
		}

		private IEnumerator TypeText()
		{
			yield return null;
			if (textLabel.textInfo.lineCount > 1)
			{
				textLabel.verticalAlignment = VerticalAlignmentOptions.Top;
			}
			else
			{
				textLabel.verticalAlignment = VerticalAlignmentOptions.Middle;
			}
			int characterCount = textLabel.textInfo.characterCount;
			int typedCount = 0;
			Core coreInstance = Core.Instance;
			while (typedCount < characterCount)
			{
				if (coreInstance.IsCorePaused)
				{
					yield return new WaitUntil(() => !coreInstance.IsCorePaused);
				}
				typedCount++;
				textLabel.maxVisibleCharacters = typedCount;
				if (fastForwardTypewriter)
				{
					break;
				}
				yield return new WaitForSeconds(0.02f);
			}
			if (fastForwardTypewriter)
			{
				textLabel.maxVisibleCharacters = characterCount;
				yield return new WaitForSeconds(0.02f);
			}
			allTextVisible = true;
			UpdateDialogueContinueInterface();
		}

		private void UpdateDialogueContinueInterface()
		{
			if (allTextVisible)
			{
				DetermineContinueButtonVisibility();
				if (showPrompt)
				{
					EnableYesNoPrompt();
				}
			}
		}

		private void DetermineContinueButtonVisibility()
		{
			if (showPrompt || yesNoScreen.IsEnabled || !allTextVisible)
			{
				showContinueButton = false;
			}
			continueButton.gameObject.SetActive(showContinueButton);
		}

		private void OnLastActiveControllerChanged(int playerid, ControllerType controllerType)
		{
			if (dialogueScreen.gameObject.activeSelf)
			{
				UpdateTextLabel(currentLocalizationLine);
				if (allTextVisible)
				{
					UpdateDialogueContinueInterface();
				}
			}
		}

		private void OnLanguageChanged(IGameTextLocalizer localizer)
		{
			UpdateDialogueLocalization();
		}

		private void UpdateDialogueLocalization()
		{
			if (currentDialogue != null)
			{
				string lineToSet = currentDialogue.GetLineToSet();
				SetLine(lineToSet);
			}
		}
	}
}
