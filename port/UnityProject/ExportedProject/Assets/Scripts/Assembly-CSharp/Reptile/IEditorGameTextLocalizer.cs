using System;

namespace Reptile
{
	public interface IEditorGameTextLocalizer : IDisposable
	{
		LocalizationGroup GetDialogueLocalizationGroup(string sceneName);

		LocalizationGroup GetTextLocalizationGroup();

		LocalizationGroup GetLocalizationGroup(string localizationGroupName);
	}
}
