using UnityEngine;
using UnityEngine.UI;

namespace Blackhole
{
    /// <summary>Vertical or horizontal vertex-colour gradient for uGUI graphics.</summary>
    [RequireComponent(typeof(Graphic))]
    public class UIGradient : BaseMeshEffect
    {
        [SerializeField] private Color top = Color.white;
        [SerializeField] private Color bottom = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private bool horizontal;

        private static readonly System.Collections.Generic.List<UIVertex> vertices = new System.Collections.Generic.List<UIVertex>();

        public void SetColors(Color topColor, Color bottomColor, bool isHorizontal = false)
        {
            top = topColor;
            bottom = bottomColor;
            horizontal = isHorizontal;
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }

            vertices.Clear();
            var vertex = new UIVertex();
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                vertices.Add(vertex);
                var value = horizontal ? vertex.position.x : vertex.position.y;
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            var span = Mathf.Max(0.0001f, max - min);
            for (var i = 0; i < vertices.Count; i++)
            {
                var current = vertices[i];
                var value = horizontal ? current.position.x : current.position.y;
                var t = Mathf.Clamp01((value - min) / span);
                var original = current.color;
                var tint = Color.Lerp(bottom, top, t);
                current.color = new Color32(
                    (byte)(original.r * tint.r),
                    (byte)(original.g * tint.g),
                    (byte)(original.b * tint.b),
                    (byte)(original.a * tint.a));
                vh.SetUIVertex(current, i);
            }
        }
    }
}
