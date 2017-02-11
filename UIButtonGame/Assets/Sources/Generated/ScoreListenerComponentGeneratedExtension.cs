//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGenerator.ComponentExtensionsGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Entitas;

public class ScoreListenerComponent : IComponent {

    public ScoreListener value;
}

namespace Entitas {

    public partial class Entity {

        public ScoreListenerComponent scoreListener { get { return (ScoreListenerComponent)GetComponent(GameComponentIds.ScoreListener); } }
        public bool hasScoreListener { get { return HasComponent(GameComponentIds.ScoreListener); } }

        public Entity AddScoreListener(ScoreListener newValue) {
            var component = CreateComponent<ScoreListenerComponent>(GameComponentIds.ScoreListener);
            component.value = newValue;
            return AddComponent(GameComponentIds.ScoreListener, component);
        }

        public Entity ReplaceScoreListener(ScoreListener newValue) {
            var component = CreateComponent<ScoreListenerComponent>(GameComponentIds.ScoreListener);
            component.value = newValue;
            ReplaceComponent(GameComponentIds.ScoreListener, component);
            return this;
        }

        public Entity RemoveScoreListener() {
            return RemoveComponent(GameComponentIds.ScoreListener);
        }
    }
}

    public partial class GameMatcher {

        static IMatcher _matcherScoreListener;

        public static IMatcher ScoreListener {
            get {
                if(_matcherScoreListener == null) {
                    var matcher = (Matcher)Matcher.AllOf(GameComponentIds.ScoreListener);
                    matcher.componentNames = GameComponentIds.componentNames;
                    _matcherScoreListener = matcher;
                }

                return _matcherScoreListener;
            }
        }
    }