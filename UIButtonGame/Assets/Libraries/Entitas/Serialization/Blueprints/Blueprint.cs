﻿using System;

namespace Entitas.Serialization.Blueprints {

    [Serializable]
    public class Blueprint {

        public string contextIdentifier;
        public string name;
        public ComponentBlueprint[] components;

        public Blueprint() {
        }

        public Blueprint(string contextIdentifier, string name, Entity entity) {
            this.contextIdentifier = contextIdentifier;
            this.name = name;

            var allComponents = entity.GetComponents();
            var componentIndices = entity.GetComponentIndices();
            components = new ComponentBlueprint[allComponents.Length];
            for (int i = 0; i < allComponents.Length; i++) {
                components[i] = new ComponentBlueprint(
                    componentIndices[i], allComponents[i]
                );
            }
        }
    }
}
