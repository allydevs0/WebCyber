using UnityEngine;

namespace Reptile
{
	public class ExplosionMaterialData : MonoBehaviour
	{
		private ParticleSystem ps;

		private void Awake()
		{
			ps = GetComponent<ParticleSystem>();
		}

		private void Start()
		{
		}

		private void Update()
		{
			ParticleSystem.MinMaxGradient color = ps.customData.GetColor(ParticleSystemCustomData.Custom1);
			base.gameObject.GetComponent<Renderer>().material.SetColor("_Color", color.Evaluate(ps.time / ps.main.startLifetimeMultiplier));
			base.gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", color.Evaluate(ps.time / ps.main.startLifetimeMultiplier));
		}
	}
}
