using UnityEngine;

[ExecuteInEditMode]
public class PrefabShuffle : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _list;

	private Transform _transform;

	private GameObject _instantiate;
}
