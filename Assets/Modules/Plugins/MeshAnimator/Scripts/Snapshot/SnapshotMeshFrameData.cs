//----------------------------------------------
// Mesh Animator
// Flick Shot Games
// http://www.flickshotgames.com
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace FSG.MeshAnimator.Snapshot
{
    [System.Serializable]
    public class SnapshotMeshFrameData : MeshFrameDataBase
    {
        public Vector3[] verts { get { return decompressed; } }
        [HideInInspector]
        public Vector3[] normals;
        [System.NonSerialized]
        private Vector3[] decompressed = null;
        public void SetVerts(Vector3[] v)
        {
            decompressed = v;
        }
        public override bool Equals(object obj)
        {
            if (obj is SnapshotMeshFrameData)
            {
                SnapshotMeshFrameData other = (SnapshotMeshFrameData)obj;
                if (other.verts.Length != verts.Length)
                    return false;
                for (int i = 0; i < other.verts.Length; i++)
                {
                    if (verts[i] != other.verts[i])
                        return false;
                }
                return true;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    [System.Serializable]
    public class MeshTriangleData
    {
        public int submesh;
        public int[] triangles;
    }
    [System.Serializable]
    public class DeltaCompressedFrameData
    {
        public static float compressionAccuracy = 1000;

        public float accuracy = 1000;
        public int sizeOffset = 1;
        public int vertLength;
        public int exposedLength;

        [HideInInspector]
        public short[] positionsUShort;
        [HideInInspector]
        public int[] positions;
        [HideInInspector]
        public int[] frameIndexes;
        [HideInInspector]
        public Matrix4x4[] exposedTransforms;
        [HideInInspector]
        public Vector3[] rootMotionPositions;
        [HideInInspector]
        public Quaternion[] rootMotionRotations;
        [HideInInspector]
        public Vector3[] normals;

        public static implicit operator SnapshotMeshFrameData[] (DeltaCompressedFrameData s)
        {
            if (s.frameIndexes == null)
                return new SnapshotMeshFrameData[0];
            SnapshotMeshFrameData[] frames = new SnapshotMeshFrameData[s.frameIndexes.Length / s.vertLength];
            bool usingShortCompression = s.positionsUShort != null && s.positionsUShort.Length > 0;
            for (int i = 0; i < frames.Length; i++)
            {
                Vector3[] verts = new Vector3[s.vertLength];
                for (int j = 0; j < verts.Length; j++)
                {
                    Vector3 v = Vector3.zero;
                    int index = s.frameIndexes[i * s.vertLength + j] * 3;
                    int index0 = index + 0;
                    int index1 = index + 1;
                    int index2 = index + 2;

                    if (usingShortCompression)
                    {
                        v.x = s.positionsUShort[index0] / s.accuracy;
                        v.y = s.positionsUShort[index1] / s.accuracy;
                        v.z = s.positionsUShort[index2] / s.accuracy;
                    }
                    else
                    {
                        v.x = s.positions[index0] / s.accuracy;
                        v.y = s.positions[index1] / s.accuracy;
                        v.z = s.positions[index2] / s.accuracy;
                    }
                    v.x -= s.sizeOffset;
                    v.y -= s.sizeOffset;
                    v.z -= s.sizeOffset;
                    verts[j] = v;
                }
                frames[i] = new SnapshotMeshFrameData();
                frames[i].exposedTransforms = new Matrix4x4[s.exposedLength];
                for (int j = 0; j < s.exposedLength; j++)
                {
                    frames[i].exposedTransforms[j] = s.exposedTransforms[j * frames.Length + i];
                }
                frames[i].SetVerts(verts);
                if (s.rootMotionPositions != null && s.rootMotionPositions.Length > i)
                    frames[i].rootMotionPosition = s.rootMotionPositions[i];
                if (s.rootMotionRotations != null && s.rootMotionRotations.Length > i)
                    frames[i].rootMotionRotation = s.rootMotionRotations[i];
                int normalLength = s.normals != null ? s.normals.Length / frames.Length : 0;
                if (s.normals != null && s.normals.Length >= ((i * normalLength) + normalLength))
                {
                    frames[i].normals = new Vector3[normalLength];
                    System.Array.Copy(s.normals, i * normalLength, frames[i].normals, 0, normalLength);
                }
            }
            return frames;
        }
        public static implicit operator DeltaCompressedFrameData(SnapshotMeshFrameData[] frames)
        {
            if (frames.Length == 0)
                return new DeltaCompressedFrameData();
            bool hasRootMotion = false;
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].rootMotionPosition.x != 0 ||
                    frames[i].rootMotionPosition.y != 0 ||
                    frames[i].rootMotionPosition.z != 0 ||
                    frames[i].rootMotionRotation.x != 0 ||
                    frames[i].rootMotionRotation.y != 0 ||
                    frames[i].rootMotionRotation.z != 0 ||
                    frames[i].rootMotionRotation.w != 0)
                    hasRootMotion = true;
            }
            int vertLength = frames[0].verts != null ? frames[0].verts.Length : 0;
            int transformLength = frames[0].exposedTransforms != null ? frames[0].exposedTransforms.Length : 0;
            int normalLength = frames[0].normals != null ? frames[0].normals.Length * frames.Length : 0;
            DeltaCompressedFrameData output = new DeltaCompressedFrameData()
            {
                vertLength = vertLength,
                frameIndexes = new int[vertLength * frames.Length],
                accuracy = compressionAccuracy,
                exposedLength = transformLength,
                exposedTransforms = new Matrix4x4[frames.Length * transformLength],
                rootMotionPositions = hasRootMotion ? new Vector3[frames.Length] : null,
                rootMotionRotations = hasRootMotion ? new Quaternion[frames.Length] : null,
                normals = new Vector3[normalLength]
            };
            List<Vector3> allPositions = new List<Vector3>();
            Dictionary<Vector3, int> indexRemaps = new Dictionary<Vector3, int>();
            int sizeOffset = 1;
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].verts != null)
                {
                    for (int j = 0; j < frames[i].verts.Length; j++)
                    {
                        if (indexRemaps.ContainsKey(frames[i].verts[j]) == false)
                        {
                            indexRemaps.Add(frames[i].verts[j], allPositions.Count);
                            allPositions.Add(frames[i].verts[j]);
                            // credit to user jbooth for finding this bug!
                            while (Mathf.Abs(frames[i].verts[j].x) > sizeOffset)
                                sizeOffset *= 10;
                            while (Mathf.Abs(frames[i].verts[j].y) > sizeOffset)
                                sizeOffset *= 10;
                            while (Mathf.Abs(frames[i].verts[j].z) > sizeOffset)
                                sizeOffset *= 10;
                        }
                        output.frameIndexes[i * output.vertLength + j] = indexRemaps[frames[i].verts[j]];
                    }
                }
                if (frames[i].exposedTransforms != null)
                {
                    for (int j = 0; j < frames[i].exposedTransforms.Length; j++)
                    {
                        output.exposedTransforms[frames.Length * j + i] = frames[i].exposedTransforms[j];
                    }
                }
                if (output.rootMotionPositions != null)
                    output.rootMotionPositions[i] = frames[i].rootMotionPosition;
                if (output.rootMotionRotations != null)
                    output.rootMotionRotations[i] = frames[i].rootMotionRotation;
                if (frames[i].normals != null)
                {
                    System.Array.Copy(frames[i].normals, 0, output.normals, i * frames[i].normals.Length, frames[i].normals.Length);
                }
            }
            output.sizeOffset = sizeOffset;
            bool canUseShort = true;
            output.positions = new int[allPositions.Count * 3];
            for (int i = 0; i < allPositions.Count; i++)
            {
                output.positions[i * 3 + 0] = (int)((allPositions[i].x + output.sizeOffset) * output.accuracy);
                output.positions[i * 3 + 1] = (int)((allPositions[i].y + output.sizeOffset) * output.accuracy);
                output.positions[i * 3 + 2] = (int)((allPositions[i].z + output.sizeOffset) * output.accuracy);
                if (canUseShort)
                {
                    if (output.positions[i * 3 + 0] > ushort.MaxValue)
                        canUseShort = false;
                    else if (output.positions[i * 3 + 1] > ushort.MaxValue)
                        canUseShort = false;
                    else if (output.positions[i * 3 + 2] > ushort.MaxValue)
                        canUseShort = false;
                }
            }
            if (canUseShort)
            {
                output.positionsUShort = new short[output.positions.Length];
                for (int i = 0; i < output.positions.Length; i++)
                    output.positionsUShort[i] = (short)output.positions[i];
                output.positions = null;
            }
            return output;
        }
    }
}