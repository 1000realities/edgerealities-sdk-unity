using UnityEngine;

namespace CloudSLAM
{
    /// <summary>
    /// Contains helper functions for handling the CloudSLAM coordinate system.
    /// </summary>
    public static class CloudSLAMCoordiantes
    {
        /// <summary>
        /// Applies the pose received from a CloudSLAM server to unity 
        /// a given Unity transform. The pose will be transformed from 
        /// CloudSLAM's coordinate system to Unity's coordinate system.
        /// </summary>
        /// <param name="cloudSLAMPoseMatrix4x4">The pose as received from
        /// a CloudSLAM server (i.e. an array of 16 floats that make
        /// up a 4x4 TRS matrix).</param>
        /// <param name="t">The transform to which the pose should be applied.</param>
        public static void ApplyPoseToUnityTransform(float[] cloudSLAMPoseMatrix4x4, Transform t)
        {
            CloudSLAMCoordiantes.CloudSLAMPoseToUnityPose(cloudSLAMPoseMatrix4x4, ref poseMatrix);

            t.position = poseMatrix.MultiplyPoint3x4(Vector3.zero);
            t.LookAt(poseMatrix.MultiplyPoint3x4(CloudSLAMCoordiantes.Forward), poseMatrix.MultiplyVector(CloudSLAMCoordiantes.Up));
        }

        /// <summary>
        /// Applies the pose received from a CloudSLAM server to unity 
        /// a given Unity transform. The pose will be transformed from 
        /// CloudSLAM's coordinate system to Unity's coordinate system.
        /// </summary>
        /// <param name="position">The position vector as received from a CloudSLAM server.</param>
        /// <param name="rotation">The rotation quaternion as received from a CloudSLAM server.</param>
        /// <param name="localScale">The local scale vector as received from a CloudSLAM server.</param>
        /// <param name="t">The transform to which the pose should be applied.</param>
        public static void ApplyPoseToUnityTransform(Vector3 position, Quaternion rotation, Vector3 localScale, Transform t)
        {
            var pose = cloudSlamToUnityWorld * Matrix4x4.TRS(position, rotation, Vector3.one);
            
            t.position = pose.MultiplyPoint3x4(Vector3.zero);
            t.LookAt(pose.MultiplyPoint3x4(CloudSLAMCoordiantes.Forward), pose.MultiplyVector(CloudSLAMCoordiantes.Up));
            t.localScale = localScale;
        }

        /// <summary>
        /// Transforms a position in the CloudSLAM coordinate system
        /// to a position in Unity's coordinate system.
        /// </summary>
        /// <returns>A vector with the position in Unity's coordinate system.</returns>
        /// <param name="x">The x position coordinate in CloudSLAM's coordiante system.</param>
        /// <param name="y">The y position coordinate in CloudSLAM's coordiante system.</param>
        /// <param name="z">The z position coordinate in CloudSLAM's coordiante system.</param>
        public static Vector3 CloudSlamPositionToUnity(float x, float y, float z)
        {
            return cloudSlamToUnityWorld.MultiplyPoint3x4(new Vector3(x,y,z));
        }

        /// <summary>
        /// The up vector (positive Y) in of the CloudSLAM coordinate system, 
        /// expressed in Unity's coordinate system.
        /// </summary>
        public static Vector3 Up { get { return Vector3.down; } }

        /// <summary>
        /// The forward vector (positive Z) in of the CloudSLAM coordinate 
        /// system, expressed in Unity's coordinate system.
        /// </summary>
        public static Vector3 Forward { get { return Vector3.forward; } }

        /// <summary>
        /// The right vector (positive X) in of the CloudSLAM coordinate 
        /// system, expressed in Unity's coordinate system.
        /// </summary>
        public static Vector3 Right { get { return Vector3.right; } }

        /// <summary>
        /// Transforms a pose received from CloudSLAM to Unity's coordinate
        /// system.
        /// </summary>
        /// <param name="cloudSLAMPoseMatrix4x4">The pose matrix as received
        /// from the CloudSLAM server (i.e. an array of 16 floats that make
        /// up a 4x4 TRS matrix).</param>
        /// <param name="unityPoseMatrix">The TRS pose matrix in Unity's 
        /// coordinate system.</param>
        public static void CloudSLAMPoseToUnityPose(float[] cloudSLAMPoseMatrix4x4, ref Matrix4x4 unityPoseMatrix)
        {
            unityPoseMatrix[0, 0] = cloudSLAMPoseMatrix4x4[0];
            unityPoseMatrix[0, 1] = cloudSLAMPoseMatrix4x4[1];
            unityPoseMatrix[0, 2] = cloudSLAMPoseMatrix4x4[2];
            unityPoseMatrix[0, 3] = cloudSLAMPoseMatrix4x4[3];

            unityPoseMatrix[1, 0] = cloudSLAMPoseMatrix4x4[4];
            unityPoseMatrix[1, 1] = cloudSLAMPoseMatrix4x4[5];
            unityPoseMatrix[1, 2] = cloudSLAMPoseMatrix4x4[6];
            unityPoseMatrix[1, 3] = cloudSLAMPoseMatrix4x4[7];

            unityPoseMatrix[2, 0] = cloudSLAMPoseMatrix4x4[8];
            unityPoseMatrix[2, 1] = cloudSLAMPoseMatrix4x4[9];
            unityPoseMatrix[2, 2] = cloudSLAMPoseMatrix4x4[10];
            unityPoseMatrix[2, 3] = cloudSLAMPoseMatrix4x4[11];

            unityPoseMatrix[3, 0] = cloudSLAMPoseMatrix4x4[12];
            unityPoseMatrix[3, 1] = cloudSLAMPoseMatrix4x4[13];
            unityPoseMatrix[3, 2] = cloudSLAMPoseMatrix4x4[14];
            unityPoseMatrix[3, 3] = cloudSLAMPoseMatrix4x4[15];

            unityPoseMatrix = cloudSlamToUnityWorld * unityPoseMatrix;
        }

        private static Matrix4x4 BuildCloudSLAMToUnityWorldMatrix()
        {
            // Since CloudSLAM's positive Y axis points DOWN, to need to mirror each pose through the XZ plane.
            Matrix4x4 mirrorY = Matrix4x4.identity;
            mirrorY[1, 1] = -1;
            return mirrorY;
        }

        private static Matrix4x4 cloudSlamToUnityWorld = BuildCloudSLAMToUnityWorldMatrix();
        private static Matrix4x4 poseMatrix;
    }
}
