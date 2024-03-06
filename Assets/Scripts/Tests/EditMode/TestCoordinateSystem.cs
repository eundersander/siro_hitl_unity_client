using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Habitat.Tests.EditMode
{
    public class TestCoordinateSystem
    {
        private static Vector3 vec(int x, int y, int z)
        {
            return new Vector3(x, y, z);
        }
        private static float[] vec_arr(int x, int y, int z)
        {
            return new float[]{x, y, z};
        }
        private static Quaternion quat(int x, int y, int z, int w)
        {
            return new Quaternion(x, y, z, w);
        }
        private static float[] quat_arr(int w, int x, int y, int z)
        {
            return new float[]{w, x, y, z};
        }

        [Test]
        public void TestTransformVectors()
        {
            // Habitat: X-Right, Y-Up, Z-Back
            // Unity:   X-Right, Y-Up, Z-Forward

            // Habitat -> Unity
            Assert.AreEqual(CoordinateSystem.ToUnityVector(vec_arr(1, 0, 0)), vec(1, 0,  0));
            Assert.AreEqual(CoordinateSystem.ToUnityVector(vec_arr(0, 1, 0)), vec(0, 1,  0));
            Assert.AreEqual(CoordinateSystem.ToUnityVector(vec_arr(0, 0, 1)), vec(0, 0, -1));

            // Unity -> Habitat
            Assert.AreEqual(CoordinateSystem.ToHabitatVector(vec(1, 0, 0)), vec_arr(1, 0,  0));
            Assert.AreEqual(CoordinateSystem.ToHabitatVector(vec(0, 1, 0)), vec_arr(0, 1,  0));
            Assert.AreEqual(CoordinateSystem.ToHabitatVector(vec(0, 0, 1)), vec_arr(0, 0, -1));

            // Test round-trip.
            Assert.AreEqual(CoordinateSystem.ToUnityVector(CoordinateSystem.ToHabitatVector(vec(1, 0, 0))), vec(1, 0, 0));
            Assert.AreEqual(CoordinateSystem.ToUnityVector(CoordinateSystem.ToHabitatVector(vec(0, 1, 0))), vec(0, 1, 0));
            Assert.AreEqual(CoordinateSystem.ToUnityVector(CoordinateSystem.ToHabitatVector(vec(0, 0, 1))), vec(0, 0, 1));
        }

        [Test]
        public void TestTransformQuaternions()
        {
            // Habitat: X-Right, Y-Up, Z-Back,    W First
            // Unity:   X-Right, Y-Up, Z-Forward, W Last

            // Habitat -> Unity
            Assert.AreEqual(CoordinateSystem.ToUnityQuaternion(quat_arr(w:1, 1, 0, 0)), quat(1, 0,  0, w:-1));
            Assert.AreEqual(CoordinateSystem.ToUnityQuaternion(quat_arr(w:1, 0, 1, 0)), quat(0, 1,  0, w:-1));
            Assert.AreEqual(CoordinateSystem.ToUnityQuaternion(quat_arr(w:1, 0, 0, 1)), quat(0, 0, -1, w:-1));

            // Unity -> Habitat
            Assert.AreEqual(CoordinateSystem.ToHabitatQuaternion(quat(1, 0, 0, w:1)), quat_arr(w:-1, 1, 0,  0));
            Assert.AreEqual(CoordinateSystem.ToHabitatQuaternion(quat(0, 1, 0, w:1)), quat_arr(w:-1, 0, 1,  0));
            Assert.AreEqual(CoordinateSystem.ToHabitatQuaternion(quat(0, 0, 1, w:1)), quat_arr(w:-1, 0, 0, -1));

            // Test round-trip
            Assert.AreEqual(CoordinateSystem.ToUnityQuaternion(CoordinateSystem.ToHabitatQuaternion(quat(1, 0, 0, w:1))), quat(1, 0, 0, w:1));
            Assert.AreEqual(CoordinateSystem.ToUnityQuaternion(CoordinateSystem.ToHabitatQuaternion(quat(0, 1, 0, w:1))), quat(0, 1, 0, w:1));
            Assert.AreEqual(CoordinateSystem.ToUnityQuaternion(CoordinateSystem.ToHabitatQuaternion(quat(0, 0, 1, w:1))), quat(0, 0, 1, w:1));
        }

        [Test]
        public void TestComputeFrameRotationOffset()
        {
            // Rotate unit vectors with `CoordinateSystem.ComputeFrameRotationOffset` and check that they align with frame.
            {
                Frame frame = new Frame
                {
                    up = vec_arr(0, 1, 0),
                    front = vec_arr(0, 0, 1)
                };
                Quaternion rotationOffset = CoordinateSystem.ComputeFrameRotationOffset(frame);
                var unityUp = rotationOffset * Vector3.up;
                var unityFront = rotationOffset * Vector3.forward;
                Assert.AreEqual(unityFront.ToArray(), frame.front);
                Assert.AreEqual(unityUp.ToArray(), frame.up);
            }
            {
                Frame frame = new Frame
                {
                    up = vec_arr(-1, 0, 0),
                    front = vec_arr(0, -1, 0)
                };
                Quaternion rotationOffset = CoordinateSystem.ComputeFrameRotationOffset(frame);
                var unityUp = rotationOffset * Vector3.up;
                var unityFront = rotationOffset * Vector3.forward;
                Assert.AreEqual(unityFront.ToArray(), frame.front);
                Assert.AreEqual(unityUp.ToArray(), frame.up);
            }
        }
    }
}
