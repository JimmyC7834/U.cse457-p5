using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class SimpleLipid : MonoBehaviour
{
    private Transform _transform;
    public Rigidbody Rigidbody;

    public Vector3 Position => _transform.position;
    public List<int> NeighborIndices;

    private void Awake()
    {
        _transform = transform;
        NeighborIndices = new List<int>();
    }
}

public struct LipidInfo
{
    public readonly Rigidbody Rigidbody;

    public Vector3 Position => Rigidbody.position;
    public List<int> NeighborIndices;

    public LipidInfo(Rigidbody rigidbody)
    {
        Rigidbody = rigidbody;
        NeighborIndices = new List<int>();
    }
}

public readonly struct LipidBond
{
    private readonly (int, int) _pair;
    public readonly float MinDist;
    public readonly float MaxDist;

    public int Fst => _pair.Item1;
    public int Snd => _pair.Item2;

    public LipidBond(int fst, int snd, float minDist, float maxDist)
    {
        _pair = (fst, snd);
        MinDist = minDist;
        MaxDist = maxDist;
    }
}