using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Vec3Util
{
    public static Vector3 FindReciprocalVec3(Vector3 vec3)
    {
        Vector3 rVec3 = Vector3.zero;

        rVec3.x = vec3.x == 0 ? 0 : 1 / rVec3.x;
        rVec3.y = vec3.y == 0 ? 0 : 1 / rVec3.y;
        rVec3.z = vec3.z == 0 ? 0 : 1 / rVec3.z;

        return rVec3;
    }
}
