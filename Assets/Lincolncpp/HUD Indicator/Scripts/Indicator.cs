using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LincolnCpp.HUDIndicator {

    [System.Serializable]
    public abstract class Indicator : MonoBehaviour {

        public bool visible = true;
        [SerializeField] public List<IndicatorRenderer> renderers;
        
        protected Dictionary<IndicatorRenderer, IndicatorCanvas> indicatorsCanvas = new Dictionary<IndicatorRenderer, IndicatorCanvas>();

		private void Start() {
            ResetCanvas();
        }

		private void Update() {
            foreach(KeyValuePair<IndicatorRenderer, IndicatorCanvas> element in indicatorsCanvas) {
                element.Value.Update();
            }
        }

        private void OnEnable() {
            foreach(KeyValuePair<IndicatorRenderer, IndicatorCanvas> element in indicatorsCanvas) {
                element.Value.OnEnable();
            }
        }

        private void OnDisable() {
            foreach(KeyValuePair<IndicatorRenderer, IndicatorCanvas> element in indicatorsCanvas) {
                element.Value.OnDisable();
            }
        }

		private void OnDestroy() {
            Clear();
		}

        public void Clear()
        {
            foreach (KeyValuePair<IndicatorRenderer, IndicatorCanvas> element in indicatorsCanvas)
            {
                DestroyIndicatorCanvas(element.Key);
            }
        }

        public void ResetCanvas()
        {
            Clear();

            foreach (IndicatorRenderer renderer in renderers)
            {
                CreateIndicatorCanvas(renderer);
            }
        }

        protected abstract void CreateIndicatorCanvas(IndicatorRenderer renderer);

        private void DestroyIndicatorCanvas(IndicatorRenderer renderer) {
            if(indicatorsCanvas.ContainsKey(renderer)) {
                indicatorsCanvas[renderer].Destroy();
			}
		}
    }
}
