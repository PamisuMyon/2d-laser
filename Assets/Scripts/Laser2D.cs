using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Laser
{
    [RequireComponent(typeof(LineRenderer))]
    public class Laser2D : MonoBehaviour
    {

        [Tooltip("光照半径")]
        public float lightRadius = .5f;
        public int circleVertices = 10;
        public GameObject startParticles;
        public GameObject endParticles;

        LineRenderer line;
        Light2D lit;

        void Awake()
        {
            line = GetComponent<LineRenderer>();
            lit = GetComponent<Light2D>();
            SetEnable(false);
        }

        public void SetEnable(bool b)
        {
            line.enabled = b;
            lit.enabled = b;
            var particles = startParticles.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in particles)
            {
                if (b)
                    item.Play();
                else
                    item.Stop();
            }
            particles = endParticles.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in particles)
            {
                if (b)
                    item.Play();
                else
                    item.Stop();
            }
        }

        public void SetPositions(Vector3 start, Vector3 end)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            
            var dir = (end - start).normalized;
            startParticles.transform.position = start;
            startParticles.transform.right = dir;
            endParticles.transform.position = end;
            endParticles.transform.right = dir;
            
            // 更改Light2D形状
            if (start != end)
            {
                var direction = end - start;
                var localUp = Vector3.Cross(Vector3.forward, direction).normalized;
                localUp = transform.InverseTransformDirection(localUp) * lightRadius;
                var localStart = transform.InverseTransformPoint(start);
                var localEnd = transform.InverseTransformPoint(end);
                // 构造形状路径
                Vector3[] path = new Vector3[circleVertices + 2];
                float deltaAngle = 2 * Mathf.PI / circleVertices;
                float axisAngleOffset = Vector2.SignedAngle(Vector2.right, direction);
                // 处理翻转情况
                if (transform.lossyScale.x < 0)
                {
                    deltaAngle = -deltaAngle;
                    axisAngleOffset = -axisAngleOffset;
                }
                // 当前圆上顶点对应角度
                float theta = Mathf.PI / 2 + Mathf.Deg2Rad * axisAngleOffset;
                int index = 0;
                // 起点处的半圆
                path[index] = localStart + localUp;
                for (int i = 0; i < circleVertices / 2; i++)
                {
                    theta += deltaAngle;
                    path[++index] = localStart + new Vector3(lightRadius * Mathf.Cos(theta), lightRadius * Mathf.Sin(theta), 0);
                }
                // 终点处的半圆
                path[++index] = localEnd - localUp;
                for (int i = 0; i < circleVertices / 2; i++)
                {
                    theta += deltaAngle;
                    path[++index] = localEnd + new Vector3(lightRadius * Mathf.Cos(theta), lightRadius * Mathf.Sin(theta), 0);
                }

                if (transform.lossyScale.x < 0)
                    System.Array.Reverse(path);

                SetShapePath(lit, path);
            }
        }

        void SetShapePath(Light2D light, Vector3[] path)
        {
            var field = light.GetType().GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(light, path);
            var method = light.GetType().GetMethod("UpdateMesh", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(light, null);
        }

    }

}
