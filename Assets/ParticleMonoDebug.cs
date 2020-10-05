/*
 * This script is only use to calculate the force between 2 monobehavior
 * It's the same formula than tyhe SPH system but on monobehaviour 
 * I needed it when my formula was broken to search the reason why.
 * I leave it here for further use (not-uniform fluid formula)
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ParticleMonoDebug : MonoBehaviour
{
    [SerializeField] ParticleMonoDebug other;
    [SerializeField] float smoothingRadius;
    [SerializeField] float smoothingRadiusSquare;
    [SerializeField] float mass;
    [SerializeField] float density;
    [SerializeField] float viscosity;
    [SerializeField] float3 initialVelocity = new float3(0,0,0);
    [SerializeField] float GAS_CONST = 1000.0f;

    public float resultDensity = 0.0f;
    public float resultPressure = 0.0f;
    public float3 resultForcePressure = new float3(0, 0, 0);
    public float3 resultForceViscosity = new float3(0, 0, 0);
    public float3 resultForce = new float3(0, 0, 0);
    public float3 resultVelocity = new float3(0, 0, 0);

    // Update is called once per frame
    void Update()
    {
        resultForcePressure = new float3(0, 0, 0);
        resultForceViscosity = new float3(0, 0, 0);
        resultForce = new float3(0, 0, 0);
        resultVelocity = new float3(0, 0, 0);
        resultDensity = 0.0f;
        resultPressure = 0.0f;

        Vector3 separation = (other.transform.position - transform.position) * 1000;
        float distance = math.lengthsq(separation);

        if (distance < smoothingRadiusSquare)
        {
            var radius      = smoothingRadius;
            var surface     = math.PI * radius * radius;
            var volume      = radius * surface * 4.0f / 3.0f ;
            resultDensity += mass * volume;
        }

        resultPressure = GAS_CONST * (resultDensity - density);

        float minDistance = math.sqrt(distance);

        if (minDistance < smoothingRadius)
        {

            if (separation == Vector3.zero) separation = new Vector3(0, 0, 1.0f);

            var penetration         = smoothingRadius - minDistance;
            var squarePenetration   = penetration * penetration;
            var surfacePenetration  = math.PI * squarePenetration;
            var normalRedirection   = -math.normalize(separation);
            var deltaVelocity       = other.initialVelocity - initialVelocity;

            resultForcePressure += mass * resultPressure / resultDensity * normalRedirection * 45 * surfacePenetration;

            resultForceViscosity += viscosity * mass * deltaVelocity / resultDensity * 45 * surfacePenetration;
        }

        resultForce = resultForcePressure + resultForceViscosity;

        if(resultDensity > 0.0000001f) resultVelocity = Time.deltaTime * resultForce * 1000 / resultDensity;

    }
}
