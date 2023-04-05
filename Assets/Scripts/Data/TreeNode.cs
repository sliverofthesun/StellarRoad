using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreeNode
{
    [SerializeField] public PlanetType planetType;
    [SerializeField] public float probability;
    [SerializeField] public int minDistanceFromStarToSpawnInKelvin;
    [SerializeField] public int maxDistanceFromStarToSpawnInKelvin;
    [SerializeField] public int probabilityOfAtmospherePercent;
    [SerializeField] public bool isGasGiant;
    [SerializeField] public List<TreeNode> children = new List<TreeNode>();
}