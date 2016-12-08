//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGenerator.ComponentExtensionsGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Entitas;

public class ButtonOffListenerComponent : IComponent {

    public ButtonOffListener value;
}

namespace Entitas {

    public partial class Entity {

        public ButtonOffListenerComponent buttonOffListener { get { return (ButtonOffListenerComponent)GetComponent(ComponentIds.ButtonOffListener); } }
        public bool hasButtonOffListener { get { return HasComponent(ComponentIds.ButtonOffListener); } }

        public Entity AddButtonOffListener(ButtonOffListener newValue) {
            var component = CreateComponent<ButtonOffListenerComponent>(ComponentIds.ButtonOffListener);
            component.value = newValue;
            return AddComponent(ComponentIds.ButtonOffListener, component);
        }

        public Entity ReplaceButtonOffListener(ButtonOffListener newValue) {
            var component = CreateComponent<ButtonOffListenerComponent>(ComponentIds.ButtonOffListener);
            component.value = newValue;
            ReplaceComponent(ComponentIds.ButtonOffListener, component);
            return this;
        }

        public Entity RemoveButtonOffListener() {
            return RemoveComponent(ComponentIds.ButtonOffListener);
        }
    }

    public partial class Matcher {

        static IMatcher _matcherButtonOffListener;

        public static IMatcher ButtonOffListener {
            get {
                if(_matcherButtonOffListener == null) {
                    var matcher = (Matcher)Matcher.AllOf(ComponentIds.ButtonOffListener);
                    matcher.componentNames = ComponentIds.componentNames;
                    _matcherButtonOffListener = matcher;
                }

                return _matcherButtonOffListener;
            }
        }
    }
}