using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudSLAM
{
    public class POIStorage : MonoBehaviour
    {
        public static IQueryable<CloudSLAM.PointOfInterest> FindPOIs()
        {
            return FindObjectsOfType<CloudSLAM.PointOfInterest>().AsQueryable();
        }

        public GameObject EmptyAnchor;

        public IEnumerator LoadPOIsFromServer(string serverAddress)
        {
            return LoadPOIsWSAsync(serverAddress).AsIEnumerator();
        }

        protected async Task LoadPOIsWSAsync(string serverAddress)
        {
            var ws = new ClientWebSocket();

            await ws.ConnectAsync(new Uri(string.Format("ws://{0}", serverAddress)), CancellationToken.None);

            var buffer = new byte[1024];

            WebSocketReceiveResult result;
            StringBuilder json = new StringBuilder();
            while (ws.State == WebSocketState.Open)
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                json.Append(Encoding.UTF8.GetString(buffer));
                if (result.EndOfMessage)
                {
                    break;
                }
            }

            await ws.CloseAsync(WebSocketCloseStatus.Empty, String.Empty, CancellationToken.None);
            
            GotPOIsFromServer(json.ToString());
        }

        protected virtual void GotPOIsFromServer(string json)
        {
            Debug.Log(json);
            LoadPOIsFromJson(json);
        }

        protected void LoadPOIsFromJson(string json)
        {
            storage = JsonConvert.DeserializeObject<POICollection>(json);
            if (storage != null)
            {
                ClearPOIsFromScene();
                PlacePOIsInScene(storage, null);
            }
        }

        protected enum POIType { Unknown, Sphere, Cuboid, Anchor }

        protected class PointOfInterest
        {
            public string GUID;
            public string type;
            public int imgId;
            public string checksum;

            public POITransform transform;
            public POIDetails details;

            public POIType Type
            { 
                get
                { 
                    switch (type)
                    {
                        case "cuboid":
                            return POIType.Cuboid;
                        case "sphere":
                            return POIType.Sphere;
                        case "anchor":
                            return POIType.Anchor;
                        default:
                            return POIType.Unknown;
                    }
                } 
            }
            public Vector3 Position { get { return new Vector3(transform.px, transform.py, transform.pz); } }

            public Quaternion Rotation
            {
                get { return new Quaternion(transform.qx, transform.qy, transform.qz, transform.qw); }
            }

            public Vector3 LocalScale
            { 
                get
                {
                    switch(Type)
                    {
                        case POIType.Cuboid:
                            return new Vector3(details.xSize, details.ySize, details.zSize);
                        case POIType.Sphere:
                            return Vector3.one * details.radius;
                        case POIType.Anchor:
                            return Vector3.one * details.size;
                        default:
                            return Vector3.one * transform.scale;
                    }
                } 
            }
        }

        protected class POICollection
        {
            public Dictionary<string, PointOfInterest> shapes;
        }

        protected class POITransform
        {
            public float px;
            public float py;
            public float pz;
            public float qx;
            public float qy;
            public float qz;
            public float qw;
            public float scale;
        }

        protected class POIDetails
        {
            public float size;
            public float xSize;
            public float ySize;
            public float zSize;
            public float radius;
            public bool triggerInsideOnly;
        }

        protected static UnityWebRequest BuildGetPOIsRequest(string serverUrl)
        {
            return UnityWebRequest.Get("http://" + serverUrl + "/client/pois");
        }

        protected void PlacePOIsInScene(POICollection pois, GameObject parent)
        {
            if (pois.shapes == null)
            {
                return;
            }
            Debug.LogError("Placing " + pois.shapes.Values.Count() + " POIs in scene.");
            pois.shapes.Values.ToList().ForEach(poi => PlaceInScene(poi, parent));
        }

        protected void ClearPOIsFromScene()
        {
            FindObjectsOfType<CloudSLAM.PointOfInterest>().ToList().ForEach(p => Destroy(p.gameObject));
        }

        protected void PlaceInScene(PointOfInterest poi, GameObject parent)
        {
            var actor = CreateActor(poi);
            if (actor == null)
            {
                return;
            }
            if (parent != null)
            {
                actor.transform.SetParent(parent.transform);
            }
            actor.GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.receiveShadows = false);
            CloudSLAMCoordiantes.ApplyPoseToUnityTransform(poi.Position, poi.Rotation, poi.LocalScale, actor.transform);
        }

        protected GameObject CreateActor(PointOfInterest poi)
        {
            if (poi == null)
            {
                return null;
            }
            switch (poi.Type)
            {
                case POIType.Cuboid:
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.AddComponent<CloudSLAM.PointOfInterest>();
                    return cube;
                case POIType.Sphere:
                    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.AddComponent<CloudSLAM.PointOfInterest>();
                    return sphere;
                case POIType.Anchor:
                    return CreateAnchorActor(poi);

                default:
                    return null;
            }
        }

        protected virtual GameObject CreateAnchorActor(PointOfInterest poi)
        {
            var actor = Instantiate(EmptyAnchor);
            actor.AddComponent<CloudSLAM.PointOfInterest>();
            return actor;
        }

        private POICollection storage;
        // This has to be here so that Unity does not crash.
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private SphereCollider sphereCollider;
        private BoxCollider boxCollider;
    }
    
    public static class TaskExtensions
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
 
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }
    }
}
