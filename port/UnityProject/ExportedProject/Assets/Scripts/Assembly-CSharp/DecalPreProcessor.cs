using UnityEngine;
using ch.sycoforge.Decal;

[RequireComponent(typeof(EasyDecal))]
public class DecalPreProcessor : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<EasyDecal>().Distance = Random.Range(0.0001f, 0.001f);
	}
}
