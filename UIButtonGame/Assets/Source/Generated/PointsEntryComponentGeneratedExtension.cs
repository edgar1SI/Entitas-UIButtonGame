//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGenerator.ComponentExtensionsGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Entitas {

    public partial class Entity {

        public PointsEntryComponent pointsEntry { get { return (PointsEntryComponent)GetComponent(ComponentIds.PointsEntry); } }
        public bool hasPointsEntry { get { return HasComponent(ComponentIds.PointsEntry); } }

        public Entity AddPointsEntry(int newPoints, int newButtonId) {
            var component = CreateComponent<PointsEntryComponent>(ComponentIds.PointsEntry);
            component.points = newPoints;
            component.buttonId = newButtonId;
            return AddComponent(ComponentIds.PointsEntry, component);
        }

        public Entity ReplacePointsEntry(int newPoints, int newButtonId) {
            var component = CreateComponent<PointsEntryComponent>(ComponentIds.PointsEntry);
            component.points = newPoints;
            component.buttonId = newButtonId;
            ReplaceComponent(ComponentIds.PointsEntry, component);
            return this;
        }

        public Entity RemovePointsEntry() {
            return RemoveComponent(ComponentIds.PointsEntry);
        }
    }

    public partial class Matcher {

        static IMatcher _matcherPointsEntry;

        public static IMatcher PointsEntry {
            get {
                if(_matcherPointsEntry == null) {
                    var matcher = (Matcher)Matcher.AllOf(ComponentIds.PointsEntry);
                    matcher.componentNames = ComponentIds.componentNames;
                    _matcherPointsEntry = matcher;
                }

                return _matcherPointsEntry;
            }
        }
    }
}
