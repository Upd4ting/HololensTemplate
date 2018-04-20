using UnityEngine;

namespace HololensTemplate {
    public class Tile {
        public const float tilesize = .6f;
        /// <inheritdoc />
        public Tile(Vector3 center, bool obstacle = false) {
            Center   = center;
            Obstacle = obstacle;
        }

        public Vector3 Center   { get; } = Vector3.zero;
        public bool    Obstacle { get; }
        private GameObject _obj = null;
        public void Render() {
            GameObject scene = GameObject.Find("Scene");
            if (_obj == null) {
                _obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _obj.transform.localScale = new Vector3(tilesize / 2 - .01f, .001f, tilesize / 2 - .01f);
                var v = Center;
                v.y                    += .5f;
                _obj.transform.position =  v;
                if (scene != null)
                    _obj.transform.parent   =  scene.transform;
            }
        }
    }
}