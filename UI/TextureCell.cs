using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CollabUtils2.UI {
    public class TextureCell : OuiJournalPage.Cell {
        public TextureCell(MTexture texture, float width = 0f) {
            this.texture = texture;
            this.width = width;
        }

        public override float Width() {
            return Math.Max(texture.Width, width);
        }

        public override void Render(Vector2 center, float columnWidth) {
            float scale = Math.Min(columnWidth / texture.Width, 1f);
            texture.DrawCentered(center, Color.White, scale);
        }

        private MTexture texture;

        private float width;
    }
}