using UnityEngine;

public class FOVCtrl : MonoBehaviour
{
	public Transform FOVSlider;

	private Camera cam;

	private void Awake()
	{
		cam = GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		float horizontalFieldOfView = FOVSlider.localPosition.z * -1f;
		cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(horizontalFieldOfView, 1.7777778f);
	}
}
