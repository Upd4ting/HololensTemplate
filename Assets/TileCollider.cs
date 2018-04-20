using HololensTemplate.Utils;

using UnityEngine;

namespace HololensTemplate {
    public class TileCollider : MonoBehaviour {
        public bool Crossed { get; private set; }
        public bool IsCrossed = false;

        private void OnCollisionEnter(Collision collision) {
            Crossed   = true;
            IsCrossed = true;
        }

        private void OnCollisionStay(Collision collision) {
            Crossed   = true;
            IsCrossed = true;
        }

        private void OnCollisionExit(Collision collision) {
            Crossed   = false;
            IsCrossed = false;
        }

        private void OnTriggerEnter(Collider other) {
            Crossed   = true;
            IsCrossed = true;
        }

        private void OnTriggerExit(Collider other) {
            Crossed   = false;
            IsCrossed = false;
        }

        private void OnTriggerStay(Collider other) {
            Crossed   = true;
            IsCrossed = true;
        }
    }
}