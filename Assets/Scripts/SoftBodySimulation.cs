﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SoftBodySimulation
{
    private List<SpringJoint> _joints;
    private List<PhysicsObject> _nodes;

    public SpringJoint[] Joints => _joints.ToArray();
    
    public bool ShowDebug = false;
    public bool ShowDisabled = false;
    public bool Enabled = false;

    public SoftBodySimulation()
    {
        _joints = new List<SpringJoint>();
        _nodes = new List<PhysicsObject>();
    }
    
    public void Update(int substep = 3)
    {
        if (!Enabled) return;
        // do the calculation in the substep times
        for (int i = 0; i < substep; i++)
        {
            // compute all the spring joint forces
            foreach (SpringJoint joint in _joints)
                ComputeJointForce(joint, substep, ShowDebug);
            
            // update all nodes' physics
            foreach (PhysicsObject node in _nodes)
                node.ComputePhysics(substep);
            
            // solve for each constraints
            foreach (SpringJoint joint in _joints)
                SolveDistanceConstraint(joint, substep);
            
            // compute move velocity
            foreach (PhysicsObject node in _nodes)
                ComputeMoveVelocity(node);
        }
    }

    public void AddJoint(SpringJoint joint)
    {
        if (_joints.Contains(joint)) return;
        _joints.Add(joint);
    }
    
    public void AddNode(PhysicsObject node)
    {
        if (_nodes.Contains(node)) return;
        _nodes.Add(node);
    }

    public static void ComputeJointForce(SpringJoint joint, int substep, bool showDebug)
    {
        PhysicsObject fst = joint.Fst;
        PhysicsObject snd = joint.Snd;
        if (showDebug && joint.Enabled)
        {
            Debug.DrawLine(fst.Position, snd.Position, Color.white);
        }
        if (!joint.Enabled) return;
        
        float k = joint.K;
        float restLength = joint.RestLength;
        float damp = joint.Damp;

        float dist = joint.Distance;
        if (dist > joint.BreakLength)
        {
            joint.Enabled = false;
            return;
        }
        
        Vector3 dir = (snd.Position - fst.Position).normalized;

        // contraction force of each end spring 
        float forceMag = k * (dist - restLength) * 0.5f;
        
        Vector3 f = forceMag * dir;
        
        // apply half of the total force to each end
        fst.AddForce((f - damp * fst.Velocity)/substep);
        snd.AddForce((-f - damp * snd.Velocity)/substep);
    }

    public static void SolveDistanceConstraint(SpringJoint joint, int substep)
    {
        if (!joint.Enabled) return;
        
        float maxDist = joint.MaxDist;
        float minDist = joint.MinDist;
        PhysicsObject fst = joint.Fst;
        PhysicsObject snd = joint.Snd;
        float dist = joint.Distance;

        // solve for max distance constraint
        if (dist > maxDist)
        {
            Vector3 dir = (snd.Position - fst.Position).normalized;
            float dD = dist - maxDist;
            Vector3 dx = dD * -.5f * dir;
            fst.Rigidbody.position += -dx / substep;
            snd.Rigidbody.position += dx / substep;
        }

        // solve for min distance constraint
        if (dist < minDist)
        {
            Vector3 dir = (snd.Position - fst.Position).normalized;
            float dD = dist - minDist;
            Vector3 dx = dD * -.5f * dir;
            fst.Rigidbody.position += dx / substep;
            snd.Rigidbody.position += -dx / substep;
        }
    }

    public static void ComputeMoveVelocity(PhysicsObject node)
    {
        node.Rigidbody.velocity = (node.Position - node.PrevPosition);
    }
}

public class SpringJoint
{
    public bool Enabled;
    public float K { get; private set; }
    public float RestLength { get; private set; }
    public float MaxDist { get; private set; }
    public float MinDist { get; private set; }
    public float Damp { get; private set; }
    public float BreakLength { get; private set; }
    public PhysicsObject Fst { get; private set; }
    public PhysicsObject Snd { get; private set; }

    public float Distance => Vector3.Distance(Fst.Position, Snd.Position);

    public SpringJoint(PhysicsObject fst, PhysicsObject snd, JointSettings settings, bool enabled)
        : this(fst, snd, settings.SpringConstant, settings.RestLength,
            settings.MaxLength, settings.MinLength, settings.Damp, settings.BreakLength , enabled) { }
    
    public SpringJoint(
        PhysicsObject fst, PhysicsObject snd, float k, float restLength,
        float maxDist, float minDist, float damp, float breakLength, bool enabled)
    {
        Assert.IsTrue(maxDist >= minDist);
        Assert.IsTrue(fst != null && snd != null);
        
        Fst = fst;
        Snd = snd;
        K = k;
        RestLength = restLength;
        MaxDist = maxDist;
        MinDist = minDist;
        Damp = damp;
        BreakLength = breakLength;
        Enabled = enabled;
    }
}

public class PhysicsObject
{
    public float Mass;
    public float InverseMass;
    public float Scale;
    public Vector3 PrevPosition;
    public Vector3 Position  => Rigidbody.position;
    public Vector3 Velocity => Rigidbody.velocity;
    public Vector3 Acceleration;
    public Rigidbody Rigidbody;
    
    public PhysicsObject(float mass, float scale, Vector3 position, Vector3 velocity, GameObject obj) {
        Mass = mass;
        InverseMass = 1f / mass;
        Scale = scale;
        PrevPosition = position;
        Rigidbody = obj.GetComponent<Rigidbody>();
        Rigidbody.position = position;
        Rigidbody.velocity = velocity;
    }

    public void ComputePhysics(int substep)
    {
        PrevPosition = Position;
        Rigidbody.velocity += Acceleration / substep;
        Rigidbody.position += Velocity  / substep;
        Acceleration = Vector3.zero;
    }

    public void AddForce(Vector3 f)
    {
        Acceleration += InverseMass * f;
    }
}