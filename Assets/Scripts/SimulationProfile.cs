using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "SimulationProfile")]
public class SimulationProfile : ScriptableObject
{
    [SerializeField] private float _lipidRadius;
    [SerializeField] private float _drag;
    
    [SerializeField] private float _attractionRadius;
    [SerializeField] private AnimationCurve _headAttractionForceCurve;
    [SerializeField] private AnimationCurve _tailAttractionForceCurve;
    
    [SerializeField] private float _attractionForce;
    [SerializeField] private float _eqmDist;
    [SerializeField] private float _maxDist;

    [FormerlySerializedAs("_restitution")]
    [Range(0f, 1f)]
    [SerializeField] private float _velocityRestitution;
    [SerializeField] private Vector3 _forceRestitution;

    public float LipidRadius => _lipidRadius;
    public float EqmDist => _eqmDist;
    public float AttractionRadius => _attractionRadius;
    public float AttractionForce => _attractionForce;
    public float VelocityRestitution => _velocityRestitution;
    public Vector3 ForceRestitution => _forceRestitution;
    public float MaxDist => _maxDist;
    
    public float HeadAttractionForce(float dist) => _attractionForce * _headAttractionForceCurve.Evaluate(dist);
    public float TailAttractionForce(float dist) => _attractionForce * _tailAttractionForceCurve.Evaluate(dist);
}
