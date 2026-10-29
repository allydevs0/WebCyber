using System;
using Rewired;
using TMPro;

namespace Reptile
{
	public class TextMeshProButtonSpriteHandler
	{
		public const int MOUSE_SPRITE_START_INDEX = 132;

		private ButtonSpriteData buttonSpriteData;

		private GameInput gameInput;

		private RewiredControllerHandler rewiredControllerHandler;

		public TextMeshProButtonSpriteHandler(ButtonSpriteData buttonSpriteData, GameInput gameInput)
		{
			this.buttonSpriteData = buttonSpriteData;
			this.gameInput = gameInput;
			rewiredControllerHandler = gameInput.RewiredControllerHandler;
		}

		internal static int ConvertMouseElementIdToKeyboardSpriteIndex(int elementId)
		{
			return elementId += 132;
		}

		public void SetCurrentControllerUITextButtonSpriteAsset(TextMeshProUGUI textMesh)
		{
			if (gameInput.IsPlayerControllerJoystick())
			{
				SetUITextButtonJoystickSpriteAsset(textMesh);
			}
			else
			{
				SetUITextButtonKeyboardSpriteAsset(textMesh);
			}
		}

		public void SetUITextButtonJoystickSpriteAsset(TextMeshProUGUI textMesh)
		{
			Guid playerCurrentControllerTypeGuid = rewiredControllerHandler.GetPlayerCurrentControllerTypeGuid(ControllerType.Joystick);
			TMP_SpriteAsset buttonSpriteForGUID = buttonSpriteData.GetButtonSpriteForGUID(playerCurrentControllerTypeGuid);
			SetSpriteAssetOnTextMesh(textMesh, buttonSpriteForGUID);
		}

		public void SetUITextButtonKeyboardSpriteAsset(TextMeshProUGUI textMesh)
		{
			SetSpriteAssetOnTextMesh(textMesh, buttonSpriteData.keyboardSpriteAsset);
		}

		public bool HasButtonGlyphSprite(int buttonGlyphSpriteIndex, ControllerType controllerType)
		{
			bool num = DetermineHasButtonGlyphSprite(buttonGlyphSpriteIndex, controllerType);
			if (!num)
			{
				HandleNoButtonGlyphFound(buttonGlyphSpriteIndex);
			}
			return num;
		}

		private bool DetermineHasButtonGlyphSprite(int buttonGlyphSpriteIndex, ControllerType controllerType)
		{
			if (buttonSpriteData == null)
			{
				return false;
			}
			if (buttonGlyphSpriteIndex < 0)
			{
				return false;
			}
			if (controllerType == ControllerType.Joystick)
			{
				return DetermineHasJoyStickButtonGlyphSprite(buttonGlyphSpriteIndex);
			}
			return buttonSpriteData.HasKeyboardButtonGlyph((uint)buttonGlyphSpriteIndex);
		}

		private bool DetermineHasJoyStickButtonGlyphSprite(int buttonGlyphSpriteIndex)
		{
			if (buttonGlyphSpriteIndex < 0)
			{
				return false;
			}
			Guid playerCurrentControllerTypeGuid = rewiredControllerHandler.GetPlayerCurrentControllerTypeGuid(ControllerType.Joystick);
			return buttonSpriteData.HasJoystickButtonGlyph((uint)buttonGlyphSpriteIndex, playerCurrentControllerTypeGuid);
		}

		private void HandleNoButtonGlyphFound(int buttonGlyphSpriteIndex)
		{
		}

		private void SetSpriteAssetOnTextMesh(TextMeshProUGUI textMesh, TMP_SpriteAsset buttonSpriteAsset)
		{
			if (textMesh.spriteAsset == null || !textMesh.spriteAsset.Equals(buttonSpriteAsset))
			{
				textMesh.spriteAsset = buttonSpriteAsset;
			}
		}
	}
}
