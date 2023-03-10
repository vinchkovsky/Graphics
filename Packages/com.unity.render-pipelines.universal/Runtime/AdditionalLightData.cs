using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine
{
    public class AdditionalLightData : MonoBehaviour
    {
        public float BacklightRange = 10;

        void OnDrawGizmosSelected()
        {
            if (BacklightRange > 0)
            {
                // Draw a yellow sphere at the transform's position
                var c = Color.yellow;
                c.a = 0.2f;

                Gizmos.color = c;

                Gizmos.DrawWireSphere(transform.position, this.BacklightRange);
            }
        }
    }
}
