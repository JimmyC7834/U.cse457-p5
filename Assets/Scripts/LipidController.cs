using System;
using UnityEngine;

public class LipidController : MonoBehaviour
{
    [SerializeField] private SimulationProfile _profile;
    
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _tail;
    [SerializeField] private Rigidbody _headRigidbody;
    [SerializeField] private Rigidbody _tailRigidbody;

    public Vector3 Position => (_head.position + _tail.position) / 2f;
    public Vector3 HeadPosition => _head.position;
    public Vector3 TailPosition => _tail.position;
    
    private void Awake()
    {
        _head = transform;
    }

    public void PhysicsUpdate()
    {
        float attractionRadius = _profile.AttractionRadius;

        Collider[] results = Physics.OverlapSphere(Position, attractionRadius);
        Vector3 totalHeadV = Vector3.zero;
        Vector3 totalTailV = Vector3.zero;

        foreach (Collider c in results)
        {
            LipidController other;
            if (!c.TryGetComponent<LipidController>(out other)) continue;
            if (other == this) continue;

            float headF = 
                _profile.HeadAttractionForce(Vector3.Distance(other.HeadPosition, HeadPosition));
            
            float tailF =
                _profile.TailAttractionForce(Vector3.Distance(other.TailPosition, TailPosition));
            
            totalHeadV += headF * (other.HeadPosition - Position).normalized;
            totalTailV += tailF * (other.TailPosition - Position).normalized;
        }

        _headRigidbody.velocity = totalHeadV;
        _tailRigidbody.velocity = totalTailV;
    }
}
