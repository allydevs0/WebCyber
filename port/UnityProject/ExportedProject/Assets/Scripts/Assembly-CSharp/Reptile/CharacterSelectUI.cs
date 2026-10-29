using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class CharacterSelectUI : AMenuController
	{
		private const string outfitCountFormat = "{0}/{1}";

		private const int NO_SELECTED_CHARACTER = -1;

		[SerializeField]
		private Sprite moveStyleSkateboardIcon;

		[SerializeField]
		private Sprite moveStyleSkateIcon;

		[SerializeField]
		private Sprite moveStyleBMXIcon;

		[SerializeField]
		private Sprite selectedCharacterIcon;

		[SerializeField]
		private Sprite deselectedCharacterIcon;

		[SerializeField]
		private Sprite unavailableCharacterIcon;

		[SerializeField]
		private Image[] selectedCharacterIcons;

		[SerializeField]
		private TextMeshProUGUI characterNameLabel;

		[SerializeField]
		private TextMeshProUGUI characterUnlockedOutfitCountLabel;

		[SerializeField]
		private Image characterMoveStyle;

		[SerializeField]
		private OutfitUnlockable[] outfitUnlockables;

		private SaveManager saveManager;

		private int currentSelectCharacterIndex = -1;

		private int availableCharacters;

		public void Init()
		{
			base.gameObject.SetActive(value: false);
			saveManager = Core.Instance.SaveManager;
		}

		public override GameObject GetFirstFocusGameObject()
		{
			return null;
		}

		public void SetAvailableCharacterCount(int availableCharacters)
		{
			this.availableCharacters = availableCharacters;
			UpdateAvailableCharacterIcons(availableCharacters);
		}

		public void SetSelectedCharacterIndexReversed(int newSelectedCharacterIndex)
		{
			if (newSelectedCharacterIndex < selectedCharacterIcons.Length && availableCharacters > 0)
			{
				int num = 0;
				if (newSelectedCharacterIndex > 0)
				{
					num = availableCharacters - newSelectedCharacterIndex;
				}
				if (num < selectedCharacterIcons.Length)
				{
					UpdateSelectedCharacter(num);
				}
			}
		}

		public void SetSelectedCharacterIndex(int newSelectedCharacterIndex)
		{
			if (newSelectedCharacterIndex < selectedCharacterIcons.Length)
			{
				UpdateSelectedCharacter(newSelectedCharacterIndex);
			}
		}

		public void SetCharacterUnavailable(int unavailableCharacterIndex)
		{
			if (unavailableCharacterIndex < selectedCharacterIcons.Length)
			{
				selectedCharacterIcons[unavailableCharacterIndex].sprite = unavailableCharacterIcon;
			}
		}

		public void SetCharacterInformation(Characters character)
		{
			MoveStyle moveStyle = saveManager.CurrentSaveSlot.GetCharacterProgress(character).moveStyle;
			SetCharacterOutfitsUnlocked(character);
			SetCharacterName(character);
			SetCharacterSelectUIMoveStyle(moveStyle);
		}

		protected override void HandlePressedBackButton()
		{
			base.HandlePressedBackButton();
			audioManager.PlaySfxUI(SfxCollectionID.MenuSfx, AudioClipID.cancel);
			uIManager.gameplay.gameObject.SetActive(value: true);
			uIManager.PopCurrentMenuEndOfFrame();
		}

		private void UpdateAvailableCharacterIcons(int availableCharacters)
		{
			for (int i = 0; i < selectedCharacterIcons.Length; i++)
			{
				if (i < availableCharacters)
				{
					selectedCharacterIcons[i].gameObject.SetActive(value: true);
				}
				else
				{
					selectedCharacterIcons[i].gameObject.SetActive(value: false);
				}
			}
		}

		private void UpdateSelectedCharacter(int newSelectedCharacterIndex)
		{
			if (currentSelectCharacterIndex > -1)
			{
				selectedCharacterIcons[currentSelectCharacterIndex].sprite = deselectedCharacterIcon;
			}
			selectedCharacterIcons[newSelectedCharacterIndex].sprite = selectedCharacterIcon;
			currentSelectCharacterIndex = newSelectedCharacterIndex;
		}

		private void SetMoveStyleIcon(Sprite moveStyle)
		{
			characterMoveStyle.sprite = moveStyle;
		}

		private void SetCharacterOutfitsUnlocked(Characters character)
		{
			int characterUnlockedOutfitsCount = GetCharacterUnlockedOutfitsCount(character);
			characterUnlockedOutfitCountLabel.text = FormatOutfitCount(characterUnlockedOutfitsCount, 4);
		}

		private int GetCharacterUnlockedOutfitsCount(Characters character)
		{
			int num = 0;
			for (int i = 0; i < outfitUnlockables.Length; i++)
			{
				bool flag = false;
				if (outfitUnlockables[i].character == character)
				{
					flag = IsOutfitUnlocked(outfitUnlockables[i]);
				}
				if (flag)
				{
					num++;
				}
			}
			return num;
		}

		private bool IsOutfitUnlocked(AUnlockable unlockable)
		{
			if (unlockable.IsDefault)
			{
				return true;
			}
			return saveManager.CurrentSaveSlot.GetUnlockableDataByUid(unlockable.Uid).IsUnlocked;
		}

		private string FormatOutfitCount(int unlockedCount, int totalCount)
		{
			return $"{unlockedCount}/{totalCount}";
		}

		private void SetCharacterName(Characters character)
		{
			characterNameLabel.text = Core.Instance.Localizer.GetCharacterName(character);
		}

		private void SetCharacterSelectUIMoveStyle(MoveStyle moveStyle)
		{
			switch (moveStyle)
			{
			case MoveStyle.BMX:
				SetMoveStyleIcon(moveStyleBMXIcon);
				break;
			case MoveStyle.SKATEBOARD:
				SetMoveStyleIcon(moveStyleSkateboardIcon);
				break;
			case MoveStyle.INLINE:
				SetMoveStyleIcon(moveStyleSkateIcon);
				break;
			}
		}
	}
}
