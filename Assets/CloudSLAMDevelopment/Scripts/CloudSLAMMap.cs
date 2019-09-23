using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace CloudSLAM
{
    /// <summary>
    /// WARNING! This class is for development purposes only.
    /// It will not work with a release build of the CloudSLAM server.
    /// </summary>
    public class CloudSLAMMap : MonoBehaviour
    {
        public float MapPointScale = 0.0025f;
        public Mesh MapPointMesh;
        public Material MapPointMaterial;

        public IEnumerator LoadMap(string serverAddress)
        {
            UnityWebRequest www = BuildGetMapRequest(serverAddress);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                storage = JsonConvert.DeserializeObject<MapStorage>(www.downloadHandler.text);
                if (storage != null)
                {
                    var scaleV = new Vector3(MapPointScale, MapPointScale, MapPointScale);
                    mapPoints = storage.mapPoints.Values
                        .Select(p => Matrix4x4.TRS(CloudSLAMCoordiantes.CloudSlamPositionToUnity(p[0], p[1], p[2]), Quaternion.identity, scaleV))
                        .ToArray();
                    for (int i=0; i<Mathf.Ceil(mapPoints.Count() / 1000); ++i)
                    {
                        mapPointBatches.Add(mapPoints.Skip(i * 1000).Take(1000).ToArray());
                    }
                }
            }
        }

        private void Update()
        {
            if (mapPointBatches.Any())
            {
                mapPointBatches.ForEach(b => Graphics.DrawMeshInstanced(MapPointMesh, 0, MapPointMaterial, b));
            }
        }

        private class MapStorage
        {
            public Dictionary<string, float[]> mapPoints;
        }

        private static UnityWebRequest BuildGetMapRequest(string serverUrl)
        {
            return UnityWebRequest.Get("http://" + serverUrl + "/client/map");
        }

        private MapStorage storage;
        private Matrix4x4[] mapPoints = null;
        private List<Matrix4x4[]> mapPointBatches = new List<Matrix4x4[]>();
        // This shit has to be here so that Unity does not crash.
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private SphereCollider sphereCollider;
        private BoxCollider boxCollider;
    }
}
