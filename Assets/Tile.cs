using UnityEngine;

namespace HololensTemplate {
    public class Tile {
        public const float tilesize = .6f;
        public const float FreeSize = .1f;

        /// <inheritdoc />
        public Tile(Vector3 center, bool obstacle = false) {
            Center   = center;
            Obstacle = obstacle;
        }

        public Vector3    Center   { get; } = Vector3.zero;
        public GameObject Obj      { get; private set; }
        public bool       Obstacle { get; set; }

        public void Render() {
            GameObject scene = GameObject.Find("Scene");
            if (Obj == null) {
                Obj                      = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Obj.transform.localScale = new Vector3(tilesize / 2 - .01f, 1f, tilesize / 2 - .01f);

                Vector3 v = Center;
                v.y                         += Obj.transform.localScale.y + FreeSize;
                Obj.transform.localPosition =  v;
                if (scene != null)
                    Obj.transform.parent = scene.transform;

                Obj.AddComponent<TileCollider>();

                Obj.GetComponent<BoxCollider>().isTrigger = true;

                Obj.AddComponent<Rigidbody>().useGravity = false;

                Obj.GetComponent<Renderer>().material.color = Color.blue;
            }
        }
    }
}