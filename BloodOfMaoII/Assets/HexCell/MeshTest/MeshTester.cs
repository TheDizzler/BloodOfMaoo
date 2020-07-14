using UnityEngine;

public class MeshTester : MonoBehaviour
{
	public float hexWidth;
	public float hexHeight;
	public bool[] cornerEnabled = new bool[6];

	public int configuration;



	private void OnDrawGizmos()
	{
		configuration = 0;

		Vector3 topLeftCorner = Vector3.zero;
		Vector3 topRightCorner = topLeftCorner + Vector3.right * .5f * hexWidth;
		Vector3 rightCorner = topRightCorner + Vector3.right * .25f * hexWidth - Vector3.up * .5f * hexHeight;
		Vector3 bottomRightCorner = rightCorner - Vector3.right * .25f * hexWidth - Vector3.up * .5f * hexHeight;
		Vector3 bottomLeftCorner = bottomRightCorner - Vector3.right * .5f * hexWidth;
		Vector3 leftCorner = bottomLeftCorner - Vector3.right * .25f * hexWidth + Vector3.up * .5f * hexHeight;

		Vector3 right = Vector3.right * .25f * hexWidth;
		Vector3 rightDown = Vector3.right * .125f * hexWidth - Vector3.up * .25f * hexHeight;
		Vector3 rightUp = Vector3.right * .125f * hexWidth + Vector3.up * .25f * hexHeight;


		Vector3 topNode = topLeftCorner + right;
		Vector3 rightTopNode = topRightCorner + rightDown;
		Vector3 rightBottomNode = bottomRightCorner + rightUp;
		Vector3 bottomNode = bottomLeftCorner + right;
		Vector3 leftBottomNode = leftCorner + rightDown;
		Vector3 leftTopNode = leftCorner + rightUp;

		Gizmos.color = Color.white;
		if (cornerEnabled[0])
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(topNode, leftTopNode);
			configuration += 32;
		}
		Gizmos.DrawSphere(topLeftCorner, .4f);


		Gizmos.color = Color.white;
		if (cornerEnabled[1])
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(topNode, rightTopNode);
			configuration += 16;
		}
		Gizmos.DrawSphere(topRightCorner, .4f);

		Gizmos.color = Color.white;
		if (cornerEnabled[2])
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(rightTopNode, rightBottomNode);
			configuration += 8;
		}
		Gizmos.DrawSphere(rightCorner, .4f);

		Gizmos.color = Color.white;
		if (cornerEnabled[3])
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(rightBottomNode, bottomNode);
			configuration += 4;
		}
		Gizmos.DrawSphere(bottomRightCorner, .4f);

		Gizmos.color = Color.white;
		if (cornerEnabled[4])
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(bottomNode, leftBottomNode);
			configuration += 2;
		}
		Gizmos.DrawSphere(bottomLeftCorner, .4f);

		Gizmos.color = Color.white;
		if (cornerEnabled[5])
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(leftBottomNode, leftTopNode);
			configuration += 1;
		}
		Gizmos.DrawSphere(leftCorner, .4f);

		Gizmos.color = Color.magenta;
		switch (configuration)
		{
			case 48:
				Gizmos.DrawLine(rightTopNode, leftTopNode);
				Gizmos.DrawLine(leftTopNode, topRightCorner);
				break;
			case 56:
				Gizmos.DrawLine(leftTopNode, topRightCorner);
				Gizmos.DrawLine(leftTopNode, rightCorner);
				Gizmos.DrawLine(leftTopNode, rightBottomNode);
				break;
		}

		Gizmos.color = Color.white;
		Gizmos.DrawLine(topLeftCorner, topRightCorner);
		Gizmos.DrawLine(topRightCorner, rightCorner);
		Gizmos.DrawLine(rightCorner, bottomRightCorner);
		Gizmos.DrawLine(bottomRightCorner, bottomLeftCorner);
		Gizmos.DrawLine(bottomLeftCorner, leftCorner);
		Gizmos.DrawLine(leftCorner, topLeftCorner);

		Gizmos.DrawCube(topNode, Vector3.one * .25f);
		Gizmos.DrawCube(rightTopNode, Vector3.one * .25f);
		Gizmos.DrawCube(rightBottomNode, Vector3.one * .25f);
		Gizmos.DrawCube(bottomNode, Vector3.one * .25f);
		Gizmos.DrawCube(leftBottomNode, Vector3.one * .25f);
		Gizmos.DrawCube(leftTopNode, Vector3.one * .25f);

	}

}
