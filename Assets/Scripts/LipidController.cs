using System;
using UnityEngine;

public class LipidController : MonoBehaviour
{
    public Transform Head;
    public Transform Tail;
    public Rigidbody HeadRigidbody;
    public Rigidbody TailRigidbody;

    public Vector3 Position => (Head.position + Tail.position) / 2f;
    public Vector3 HeadPosition => Head.position;
    public Vector3 TailPosition => Tail.position;
    
    private void Awake()
    {
        Head = transform;
    }
}
