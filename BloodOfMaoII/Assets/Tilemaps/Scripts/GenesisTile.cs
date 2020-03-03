﻿using System;
using UnityEngine;

namespace AtomosZ.BoMII.Terrain
{
	public class GenesisTile : MonoBehaviour
	{
		private SpriteRenderer sprite;
		private Color NGColor = Color.red;
		private Color normalColor = Color.white;

		public void Start()
		{
			sprite = GetComponent<SpriteRenderer>();
		}


		public void SetEnabled(bool isEnabled)
		{
			if (isEnabled)
			{
				sprite.color = normalColor;
			}
			else
			{
				sprite.color = NGColor;
			}
		}
	}
}