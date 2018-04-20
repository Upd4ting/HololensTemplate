using System;
using System.Collections;
using System.Collections.Generic;

using HololensTemplate.Utils;

using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

using UnityEngine;

using Vuforia;

namespace HololensTemplate {
    public class ArrayGenerator : MonoBehaviour {
        private       TextToSpeech tts;

        private void Start() {
            tts = GameObject.Find("tts").GetComponent<TextToSpeech>();
            StartCoroutine(Map());
        }

        private IEnumerator Map() {
            SpatialMappingManager.Instance.StartObserver();
            yield return new WaitForSeconds(3);
            ScanFinished();
        }

        private void ScanFinished() {
            SpatialMappingManager.Instance.StopObserver();
            tts.StartSpeaking("Start analysing");
            List<MeshFilter> filters = SpatialMappingManager.Instance.GetMeshFilters();
            List<Mesh>       mesh    = SpatialMappingManager.Instance.GetMeshes();

            int    triangle = mesh[0].triangles[0];
            Bounds bounds   = mesh[0].bounds;

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            var o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.transform.position = min;
            o.transform.localScale = Vector3.one * Tile.tilesize;
            o.GetComponent<Renderer>().material.color = Color.red;
            
            o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.transform.position   = max;
            o.transform.localScale = Vector3.one * Tile.tilesize;
            o.GetComponent<Renderer>().material.color = Color.yellow;

            Logs.Log($"Min: {min}");
            Logs.Log($"Max: {max}");

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

            foreach (Tile tile in tiles) {
                tile.Render();
            }
        }
    }
}