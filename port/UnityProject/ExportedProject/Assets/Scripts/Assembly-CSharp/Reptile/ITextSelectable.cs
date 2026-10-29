namespace Reptile
{
	public interface ITextSelectable
	{
		void ResetSelectableState();

		void DisableSelectableAsRaycastTarget();

		void EnableSelectableAsRaycastTarget();
	}
}
