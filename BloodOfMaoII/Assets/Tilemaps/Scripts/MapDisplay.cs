using System;
using AtomosZ.BoMII.Terrain.Generators;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain.Generators
{
	public class MapDisplay : MonoBehaviour
	{
		[SerializeField] private MeshFilter meshFilter = null;
		[SerializeField] private MeshRenderer meshRenderer = null;
		[SerializeField] private Renderer textureRenderer = null;


		public void DrawTexture(Texture2D texture)
		{
			textureRenderer.sharedMaterial.mainTexture = texture;
			textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
		}

		public void DrawMesh(MeshData meshData, Texture2D texture)
		{
			meshFilter.sharedMesh = meshData.CreateMesh();
			meshRenderer.sharedMaterial.mainTexture = texture;
		}
	}
}