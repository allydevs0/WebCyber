using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class LoadingScreenUI : MonoBehaviour
	{
		private float discRotateSpeed = 180000f;

		[SerializeField]
		private RectTransform discRectTransform;

		[SerializeField]
		private Slider loadingBarSlider;

		private BaseModule baseModule;

		private void Awake()
		{
			baseModule = Core.Instance.BaseModule;
		}

		private void OnEnable()
		{
			loadingBarSlider.value = 0f;
		}

		private void Update()
		{
			UpdateDiskSpinning();
			UpdateLoadingBar();
		}

		private void UpdateDiskSpinning()
		{
			discRectTransform.Rotate(0f, 0f, Time.deltaTime * discRotateSpeed);
		}

		private void UpdateLoadingBar()
		{
			loadingBarSlider.value = baseModule.GetCurrentLoadingProgress();
		}
	}
}
