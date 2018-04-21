using System.Collections;
using System.Collections.Generic;

using HololensTemplate.Utils;

using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

using UnityEngine;

namespace HololensTemplate {
    public class ArrayGenerator : MonoBehaviour {
        private TextToSpeech tts;

        private void Start() {
            tts = GameObject.Find("tts").GetComponent<TextToSpeech>();
            StartCoroutine(Map());
        }

        private IEnumerator Map() {
            Tile[,] array;
            do {
                SpatialMappingManager.Instance.StartObserver();
                Logs.Log($"Observer state: {SpatialMappingManager.Instance.SurfaceObserver.ObserverState}");
                yield return new WaitForSeconds(SpatialMappingManager.Instance.SurfaceObserver.TimeBetweenUpdates * 3.1f);
                SpatialMappingManager.Instance.StopObserver();
                yield return new WaitForFixedUpdate();
                array = CreateArray();
            } while (array == null);

            yield return new WaitForFixedUpdate();
            StartCoroutine(FindCrossing(array));
        }

        private Tile[,] CreateArray() {
            Logs.Log("CreateArray");
            List<Mesh> listMesh = SpatialMappingManager.Instance.GetMeshes();
            Logs.Log($"Nb of meshs: {listMesh.Count}");

            if (listMesh.Count == 0) return null;

            tts.StartSpeaking("Start analysing");
            var b = SpatialMappingManager.Instance.gameObject.transform.GetColliderBounds();
            Vector3 min = b.min, max = b.max;

            Logs.Log($"Min: {min}");
            Logs.Log($"Max: {max}");
            SpawnSphere(max,Color.gray);
            SpawnSphere(min, Color.gray);

            float x = max.x - min.x;
            float z = max.z - min.z;

            float lon, lar;

            if (x >= z) {
                Logs.Log("x >= z");
                lon = x;
                lar = z;
            } else {
                Logs.Log("x < z");
                lon = z;
                lar = x;
            }

            Logs.Log($"Long: {lon}");
            Logs.Log($"Larg: {lar}");

            int tilex = Mathf.CeilToInt(lon / Tile.tilesize) * 2;
            int tiley = Mathf.CeilToInt(lar / Tile.tilesize) * 2;
            Logs.Log($"Array size: [{tilex},{tiley}]");

            Vector3 vect = min;
            Logs.Log($"Min: {min}");
            Logs.Log($"Vect start: {vect}");
            Tile[,] tiles = new Tile[tilex, tiley];
            for (int i = 0; i < tilex; i++) {
                vect.z += Tile.tilesize / 2;
                for (int j = 0; j < tiley; j++) {
                    vect.x      += Tile.tilesize / 2;
                    tiles[i, j] =  new Tile(vect);
                }

                vect.x = min.x;
            }

            Logs.Log($"Vect end: {vect}");
            Logs.Log($"Max: {max}");

            foreach (Tile tile in tiles) tile.Render();

            Logs.Log("End CreateArray");
            return tiles;
        }

        private IEnumerator FindCrossing(Tile[,] array) {
            Logs.Log("Findcrossing");
            int v = array.Length / 60;
            int i = 0;
            foreach (Tile tile in array) {
                TileCollider collider = tile.Obj.GetComponent<TileCollider>();
                Logs.Log($"Collider.Crossed {collider.Crossed}");
                if (collider.Crossed)
                    tile.Obj.GetComponent<Renderer>().material.color = Color.red;
                else
                    tile.Obj.GetComponent<Renderer>().material.color = Color.white;

                if (++i % v == 0)
                    yield return null;
            }

            Logs.Log("End findcrossing");
        }

        private void SpawnSphere(Vector3 pos,Color color = default(Color)) {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.transform.position                      = pos;
            o.transform.localScale                    = Vector3.one * Tile.tilesize;
            o.GetComponent<Renderer>().material.color = color;
        }
    }
}