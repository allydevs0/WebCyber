using Reptile;
using UnityEngine;

public class Boostpack : MonoBehaviour
{
	public Transform BoostCore;

	public Transform Spark;

	public GameObject ParticleSpark;

	public Animation anim;

	public Renderer boostPackRenderer;

	public Player player;

	private string currentAnim;

	private void Start()
	{
		if (WorldHandler.instance == null)
		{
			StageManager.OnStageInitialized += StageManager_OnStageInitialized;
		}
		else
		{
			player = WorldHandler.instance.GetCurrentPlayer();
			if (player == null)
			{
				StageManager.OnStageInitialized += StageManager_OnStageInitialized;
			}
		}
		ParticleSpark.SetActive(value: false);
		Core.OnCoreUpdatePaused += OnCoreUpdatePaused;
		Core.OnCoreUpdateUnPaused += OnCoreUpdateUnPaused;
		PlayAnim("boostIdle");
	}

	private void StageManager_OnStageInitialized()
	{
		player = WorldHandler.instance.GetCurrentPlayer();
	}

	private void OnEnable()
	{
		if (!(player == null))
		{
			if (player.ability == player.wallrunAbility || (player.IsGrinding() && player.grindAbility.trickTimer <= 0f) || player.ability == player.slideAbility)
			{
				PlayAnim("boostSpark");
				ParticleSpark.SetActive(value: true);
			}
			else
			{
				PlayAnim("boostStart");
				ParticleSpark.SetActive(value: false);
			}
		}
	}

	private void OnCoreUpdatePaused()
	{
		if (base.gameObject.activeSelf)
		{
			anim[currentAnim].speed = 0f;
		}
	}

	private void OnCoreUpdateUnPaused()
	{
		if (base.gameObject.activeSelf)
		{
			anim[currentAnim].speed = 1f;
		}
	}

	public void PlayAnim(string animName)
	{
		currentAnim = animName;
		anim.Play(animName);
	}

	private void LateUpdate()
	{
		if (player != null)
		{
			Spark.rotation = player.cam.transform.rotation;
			if (!anim.isPlaying)
			{
				PlayAnim("boostIdle");
			}
			float z = BoostCore.lossyScale.z;
			if (boostPackRenderer != null && boostPackRenderer.sharedMaterial != null)
			{
				boostPackRenderer.sharedMaterial.SetFloat("_TexScale", 4f - z * z / 2f);
			}
		}
	}

	private void OnDestroy()
	{
		Core.OnCoreUpdatePaused -= OnCoreUpdatePaused;
		Core.OnCoreUpdateUnPaused -= OnCoreUpdateUnPaused;
	}
}
