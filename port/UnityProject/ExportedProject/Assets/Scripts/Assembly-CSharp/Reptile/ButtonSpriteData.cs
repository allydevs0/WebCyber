using System;
using TMPro;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "ButtonSpriteData", menuName = "Reptile/Platform/Button sprite data", order = 1)]
	public class ButtonSpriteData : ScriptableObject
	{
		[Tooltip("This is the default button sprite that is used in the game")]
		public TMP_SpriteAsset defaultButtonSpriteAsset;

		[Tooltip("This the keyboard asset that can be used if it is supported")]
		public TMP_SpriteAsset keyboardSpriteAsset;

		[SerializeField]
		[Tooltip("These are the assets that can be used if there is need of supporting multiple types of controllers")]
		private ButtonSpriteDictionary buttonSpriteAssets = new ButtonSpriteDictionary();

		public TMP_SpriteAsset GetButtonSpriteForGUID(Guid controllerGUID)
		{
			TMP_SpriteAsset value = null;
			if (!buttonSpriteAssets.TryGetValue(controllerGUID.ToString(), out value))
			{
				value = defaultButtonSpriteAsset;
			}
			return value;
		}

		public bool HasKeyboardButtonGlyph(uint spriteIndex)
		{
			return HasButtonGlyph(spriteIndex, keyboardSpriteAsset);
		}

		public bool HasJoystickButtonGlyph(uint spriteIndex, Guid controllerGUID)
		{
			TMP_SpriteAsset buttonSpriteForGUID = GetButtonSpriteForGUID(controllerGUID);
			return HasButtonGlyph(spriteIndex, buttonSpriteForGUID);
		}

		private bool HasButtonGlyph(uint spriteIndex, TMP_SpriteAsset buttonSpriteAsset)
		{
			if (buttonSpriteAsset == null)
			{
				return false;
			}
			return spriteIndex < buttonSpriteAsset.spriteCharacterTable.Count;
		}
	}
}
