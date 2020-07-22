﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollider : MonoBehaviour
{
	public GameObject blocks;
	public GameObject startStairs;
	public GameObject stairs;
	public int chunkId;

	private int startStairsIter = 0;
	private int stairsIter = 0;

	public void ManualStart()
	{
		name = $"chunk-{chunkId}";
		for (int i = 0; i < blocks.transform.childCount; i++)
		{
			GameObject block = blocks.transform.GetChild(i).gameObject;
			block.name = $"block-{chunkId}-{i}";
		}
		while (PlayerPrefs.HasKey($"startStairs-{chunkId}-{startStairsIter}-X"))
		{
			Vector3 vector3 = Load.GetVec3($"startStairs-{chunkId}-{startStairsIter}");
			Instantiate(startStairs, vector3, Quaternion.identity, blocks.transform);
			startStairsIter++;
		}
		while (PlayerPrefs.HasKey($"stairs-{chunkId}-{stairsIter}-X"))
		{
			Vector3 vector3 = Load.GetVec3($"stairs-{chunkId}-{stairsIter}");
			Instantiate(stairs, vector3, Quaternion.identity, blocks.transform);
			stairsIter++;
		}
	}

	public void SaveStartStairs(GameObject startStairs)
	{
		Save.SetVec3($"startStairs-{chunkId}-{startStairsIter}", startStairs.transform.position);
		startStairsIter++;
	}
	
	public void SaveStairs(GameObject stairs)
	{
		Save.SetVec3($"stairs-{chunkId}-{stairsIter}", stairs.transform.position);
		stairsIter++;
	}
}
