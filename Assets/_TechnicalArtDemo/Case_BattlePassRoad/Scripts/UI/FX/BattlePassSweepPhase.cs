using UnityEngine;
using UnityEngine.UI;

namespace TechnicalArtDemo.BattlePass.UI.Effects
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class BattlePassSweepPhase : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Graphic targetGraphic;

        [Header("Timing Variation")]
        [SerializeField, Range(0f, 1f)] private float phaseOffset;

        public float PhaseOffset => phaseOffset;

        public void SetPhase(float phase)
        {
            phaseOffset = Mathf.Repeat(phase, 1f);
            Apply();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            CacheTarget();
            Apply();
        }

        private void OnValidate()
        {
            CacheTarget();
            Apply();
        }

        [ContextMenu("Apply Phase")]
        private void ApplyPhase()
        {
            CacheTarget();
            Apply();
        }
#endif

        private void CacheTarget()
        {
            if (targetGraphic == null)
            {
                targetGraphic = GetComponent<Graphic>();
            }
        }

        private void Apply()
        {
            if (targetGraphic == null)
            {
                return;
            }

            Color targetColor = new Color(phaseOffset, 1f, 1f, 1f);

            if (targetGraphic.color != targetColor)
            {
                targetGraphic.color = targetColor;
            }

            if (targetGraphic.raycastTarget)
            {
                targetGraphic.raycastTarget = false;
            }
        }
    }
}