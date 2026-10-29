using Rewired;

namespace Reptile
{
	public class TextMeshProButtonGlyphFormatter : ATextMeshProFormatter
	{
		public const string TEXT_MESH_PRO_SPRITE_TAG = "<sprite index=\"{0}\">";

		private RewiredMappingHandler rewiredMappingHandler;

		private TextMeshProButtonSpriteHandler textMeshProButtonSpriteHandler;

		private GameInput gameInput;

		public TextMeshProButtonGlyphFormatter(GameInput gameInput, TextMeshProButtonSpriteHandler textMeshProButtonSpriteHandler)
		{
			rewiredMappingHandler = gameInput.RewiredMappingHandler;
			this.textMeshProButtonSpriteHandler = textMeshProButtonSpriteHandler;
			this.gameInput = gameInput;
		}

		public string CreateTagForControllerType(int elementId, ControllerType controllerType)
		{
			return CreateSpriteButtonGlyphTag(elementId, controllerType);
		}

		protected override string CreateTag(string actionName)
		{
			ActionElementMap currentControllerActionElementMap = rewiredMappingHandler.GetCurrentControllerActionElementMap(actionName.ToLower());
			if (currentControllerActionElementMap == null)
			{
				return HandleNoActionElementMapFound(actionName);
			}
			ControllerType controllerType = currentControllerActionElementMap.controllerMap.controllerType;
			int spriteIndex = currentControllerActionElementMap.elementIdentifierId;
			if (controllerType == ControllerType.Mouse)
			{
				controllerType = ControllerType.Keyboard;
				spriteIndex = TextMeshProButtonSpriteHandler.ConvertMouseElementIdToKeyboardSpriteIndex(currentControllerActionElementMap.elementIdentifierId);
			}
			return CreateSpriteButtonGlyphTag(spriteIndex, controllerType);
		}

		private string CreateSpriteButtonGlyphTag(int spriteIndex, ControllerType controllerType)
		{
			if (!textMeshProButtonSpriteHandler.HasButtonGlyphSprite(spriteIndex, controllerType))
			{
				return string.Empty;
			}
			return CreateButtonTag(spriteIndex);
		}

		private string CreateButtonTag(int spriteIndex)
		{
			return $"<sprite index=\"{spriteIndex}\">";
		}

		private string HandleNoActionElementMapFound(string actionName)
		{
			return actionName;
		}
	}
}
