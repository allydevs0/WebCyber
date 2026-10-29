using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TilableBoxGeneratorScript : MonoBehaviour
{
	private int[][] triDex = new int[4][];

	[Range(2f, 0.1f)]
	public float textureSizeFront = 1f;

	[Range(2f, 0.1f)]
	public float textureSizeSide = 1f;

	[Range(2f, 0.1f)]
	public float textureSizeTop = 1f;

	[Range(2f, 0.1f)]
	public float textureSizeBottom = 1f;

	private Mesh s;

	private void Start()
	{
	}

	private void Update()
	{
	}
}
