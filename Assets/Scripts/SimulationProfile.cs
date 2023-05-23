using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SimulationProfile")]
public class SimulationProfile : ScriptableObject
{
    [SerializeField] private float _attractionRadius;
    [SerializeField] private AnimationCurve _headAttractionForceCurve;
    [SerializeField] private AnimationCurve _tailAttractionForceCurve;
    [SerializeField] private float _attractionForce;

    public float AttractionRadius => _attractionRadius;
    public float AttractionForce => _attractionForce;
    
    public float HeadAttractionForce(float dist) => _attractionForce * _headAttractionForceCurve.Evaluate(dist);
    public float TailAttractionForce(float dist) => _attractionForce * _tailAttractionForceCurve.Evaluate(dist);
}
