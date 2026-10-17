using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeyondTime.Memories
{
    /// <summary>
    /// Small health bar and name floating over an NPC. Faces the camera.
    /// Hostiles: shows when damaged, hides after a few seconds. Protected people: always visible.
    /// </summary>
    public class FighterHealthBar : MonoBehaviour
    {
        public Fighter fighter;
        public Image fill;
        public Image guardFill;
        public TMP_Text nameText;
        public CanvasGroup group;
        public bool alwaysVisible;
        public float showSeconds = 3f;
        public float height = 2.25f;

        Camera cam;
        float lastDamage = -99f;

        void Start()
        {
            if (!fighter) fighter = GetComponentInParent<Fighter>();
            if (fighter) fighter.Damaged += (a, d) => lastDamage = Time.time;
            if (nameText && fighter) nameText.text = fighter.displayName;
        }

        void LateUpdate()
        {
            if (!fighter) return;
            if (!cam) cam = Camera.main;

            transform.position = fighter.transform.position + Vector3.up * height;
            if (cam) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

            if (fill) fill.fillAmount = fighter.Health01;
            if (guardFill) guardFill.fillAmount = fighter.Guard01;

            bool show = !fighter.IsDefeated && (alwaysVisible || Time.time - lastDamage < showSeconds || fighter.Health01 < 0.999f && fighter.inStance);
            if (group) group.alpha = Mathf.MoveTowards(group.alpha, show ? 1f : 0f, Time.deltaTime * 4f);
        }
    }
}
