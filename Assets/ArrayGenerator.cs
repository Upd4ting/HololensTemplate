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
                yield return new WaitForSeconds(SpatialMappingManager.Instance.SurfaceObserver.TimeBetweenUpdates + 1f);
                SpatialMappingManager.Instance.StopObserver();

                array = CreateArray();
            } while (array == null);

            yield return new WaitForFixedUpdate();

            FindCrossing(array);
        }

        private Tile[,] CreateArray() {
            Logs.Log("CreateArray");
            List<Mesh> listMesh = SpatialMappingManager.Instance.GetMeshes();
            Logs.Log($"Nb of meshs: {listMesh.Count}");

            if (listMesh.Count == 0) return null;

            tts.StartSpeaking("Start analysing");
            Vector3 min = Vector3.zero, max = Vector3.zero;
            foreach (Mesh mesh in listMesh) {
                Bounds bounds = mesh.bounds;
                min = CompareReturn(bounds.min, min);
                max = CompareReturn(bounds.max, max, false);
            }

            Logs.Log($"Min: {min}");
            Logs.Log($"Max: {max}");

            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.transform.position                      = min;
            o.transform.localScale                    = Vector3.one * Tile.tilesize;
            o.GetComponent<Renderer>().material.color = Color.red;

            o                                         = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.transform.position                      = max;
            o.transform.localScale                    = Vector3.one * Tile.tilesize;
            o.GetComponent<Renderer>().material.color = Color.yellow;

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

        private void FindCrossing(Tile[,] array) {
            Logs.Log("Findcrossing");
            foreach (Tile tile in array) {
                TileCollider collider = tile.Obj.GetComponent<TileCollider>();
                Logs.Log($"Collider.Crossed {collider.Crossed}");
                if (collider.Crossed)
                    tile.Obj.GetComponent<Renderer>().material.color = Color.red;
                else
                    tile.Obj.GetComponent<Renderer>().material.SetColor("_color", Color.white);
            }

            Logs.Log("End findcrossing");
        }

        private static Vector3 CompareReturn(Vector3 a, Vector3 b, bool lower = true) {
            Vector3 retour = new Vector3();
            if (a.x < b.x) { retour.x = lower ? a.x : b.x; } else { retour.x = lower ? b.x : a.x; }

            if (a.y < b.y) { retour.y = lower ? a.y : b.y; } else { retour.y = lower ? b.y : a.y; }

            if (a.z < b.z) { retour.z = lower ? a.z : b.z; } else { retour.z = lower ? b.z : a.z; }

            return retour;
        }
    }
}