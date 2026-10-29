namespace Reptile
{
	public interface IMenuAnimationTransitionController
	{
		bool IsTransitioning { get; }

		void ShowOpeningMenuTransitionAnimation(Menu fromMenu);

		void ShowClosingMenuTransitionAnimation(Menu toMenu);

		void PlayCustomMenuAnimation(int index);

		void PlayCustomMenuAnimationBackwards(int index);
	}
}
