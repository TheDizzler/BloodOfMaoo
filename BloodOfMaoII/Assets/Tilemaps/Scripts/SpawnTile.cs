using UnityEngine;
using UnityEngine.Tilemaps;


namespace AtomosZ.BoMII.Terrain
{
	public class SpawnTile : MonoBehaviour
	{
		private TerrainTile parent;
		private float timeCreated;

		public void OnEnable()
		{
			parent = GetComponentInParent<TerrainTile>();
			timeCreated = Time.timeSinceLevelLoad;
		}


		public bool IsInBounds(Tilemap tilemap)
		{
			//BoundsInt bounds = tilemap.cellBounds;
			//Vector3 worldmin = tilemap.transform.TransformPoint(bounds.min);
			//Vector3 worldmax = tilemap.transform.TransformPoint(bounds.max);
			//if (transform.position.x < worldmin.x)
			//{
			//	Debug.Log("OoB! too small x!");
			//	return false;
			//}
			//else if (transform.position.x > worldmax.x)
			//{
			//	Debug.Log("OoB! too big x!");
			//	return false;
			//}
			//else if (transform.position.y < worldmin.y)
			//{
			//	Debug.Log("OoB! too small y!");
			//	return false;
			//}
			//else if (transform.position.y > worldmax.y)
			//{
			//	Debug.Log("OoB! too big y!");
			//	return false;
			//}

			return true;
		}

		public void SpawnSelf(Tilemap tilemap, TerrainTileBase tilebaseSO)
		{
			tilemap.SetTile(tilemap.LocalToCell(this.transform.position), tilebaseSO);
			Destroy(this.gameObject);
		}

		/// <summary>
		/// Checks if this spawner is overlapping another spawner.
		/// </summary>
		/// <returns></returns>
		public bool Overlaps()
		{
			ContactFilter2D filter = new ContactFilter2D();
			filter.SetLayerMask(~LayerMask.NameToLayer(Layers.SpawnPoints));
			filter.useTriggers = true;
			Collider2D[] results = new Collider2D[2];
			if (GetComponent<Collider2D>().OverlapCollider(filter, results) > 0)
			{
				return true;
			}

			return false;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			Debug.Log(collision.transform.name + " has hit me, " + transform.name);

			if (collision.CompareTag(Tags.Genesis))
			{
				// ignore
			}
			else if (collision.CompareTag(Tags.TerrainTile))
			{
				Destroy(this.gameObject);
			}
			else if (collision.CompareTag(Tags.TerrainTilemap))
			{
				Destroy(this.gameObject);
			}
			else if (collision.CompareTag(Tags.TileSpawnPoint))
			{
				if (timeCreated > collision.GetComponent<SpawnTile>().timeCreated)
					Destroy(this.gameObject);
			}
		}
	}
}