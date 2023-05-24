using System.Collections.Generic;
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

public readonly struct Pair
{
    public readonly int Fst;
    public readonly int Snd;

    public Pair(int fst, int snd)
    {
        Fst = fst;
        Snd = snd;
    }

    public override int GetHashCode()
    {
        return Fst ^ Snd;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(Pair)) return false;
        Pair other = (Pair) obj;
        return (other.Fst == Snd && other.Snd == Fst) || (other.Fst == Fst && other.Snd == Snd);
    }
}