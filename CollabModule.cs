using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CollabUtils2 {
    public class CollabModule : EverestModule {

        public static CollabModule Instance;
        
        public CollabModule() {
            Instance = this;
        }

        public override void Load() 
        {
            On.Celeste.Strawberry.OnCollect += Strawberry_OnCollect;
            Everest.Events.Level.OnLoadEntity += Level_OnLoadEntity;
        }

        public override void Unload() 
        {
            On.Celeste.Strawberry.OnCollect -= Strawberry_OnCollect;
            Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
        }

        /// <summary>
        /// Gets the section's name (author) based on room name, assuming format Author_Number
        /// </summary>
        public static string GetSectionName(string roomName) {
            return roomName.Split('_')[0];
        }

        /// <summary>
        /// Checks if you're in the collab map based on the current map's SID
        /// </summary>
        public static bool IsInCollab() {
            if ((Engine.Scene as Level) != null) {
                if ((Engine.Scene as Level).Session.Area.LevelSet.Contains("SpringCollab2020"))
                    return true;
            }
            return false;
        }

        private bool Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            switch (entityData.Name) {
                case "CollabUtils2/strawberryCounter":
                    level.Add(new StrawberryCounterText(entityData, offset));
                    return true;
                default:
                    return false;
            }
        }

        public void Strawberry_OnCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self) {
            orig(self);
            if (IsInCollab()) {
                string section = GetSectionName(self.ID.Level);
                StrawberryCounterText.IncrementCounter(section);
            }
        }
    }
}
