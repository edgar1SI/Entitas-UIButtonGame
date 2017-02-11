using System;
using System.Collections.Generic;

namespace Entitas {

    public partial class Context {

        public event ContextChanged OnEntityCreated;
        public event ContextChanged OnEntityWillBeDestroyed;
        public event ContextChanged OnEntityDestroyed;
        public event GroupChanged OnGroupCreated;
        public event GroupChanged OnGroupCleared;

        public delegate void ContextChanged(Context context, Entity entity);
        public delegate void GroupChanged(Context context, Group group);

        public int totalComponents { get { return _totalComponents; } }

        public Stack<IComponent>[] componentPools {
            get { return _componentPools; }
        }

        public ContextInfo contextInfo { get { return _contextInfo; } }

        public int count { get { return _entities.Count; } }

        public int reusableEntitiesCount {
            get { return _reusableEntities.Count; }
        }

        public int retainedEntitiesCount {
            get { return _retainedEntities.Count; }
        }

        readonly int _totalComponents;
        int _creationIndex;

        readonly HashSet<Entity> _entities = new HashSet<Entity>(
            EntityEqualityComparer.comparer
        );

        readonly Stack<Entity> _reusableEntities = new Stack<Entity>();
        readonly HashSet<Entity> _retainedEntities = new HashSet<Entity>(
            EntityEqualityComparer.comparer
        );

        Entity[] _entitiesCache;

        readonly ContextInfo _contextInfo;

        readonly Dictionary<IMatcher, Group> _groups =
            new Dictionary<IMatcher, Group>();

        readonly List<Group>[] _groupsForIndex;

        readonly Stack<IComponent>[] _componentPools;
        readonly Dictionary<string, IEntityIndex> _entityIndices;

        // Cache delegates to avoid gc allocations
        Entity.EntityChanged _cachedEntityChanged;
        Entity.ComponentReplaced _cachedComponentReplaced;
        Entity.EntityReleased _cachedEntityReleased;

        public Context(int totalComponents) : this(totalComponents, 0, null) {
        }

        public Context(int totalComponents,
                    int startCreationIndex,
                    ContextInfo contextInfo) {
            _totalComponents = totalComponents;
            _creationIndex = startCreationIndex;

            if(contextInfo != null) {
                _contextInfo = contextInfo;

                if(contextInfo.componentNames.Length != totalComponents) {
                    throw new ContextInfoException(this, contextInfo);
                }
            } else {

                // If Contexts.CreateContext() was used to create the context,
                // we will never end up here.
                // This is a fallback when the context is created manually.

                var componentNames = new string[totalComponents];
                const string prefix = "Index ";
                for (int i = 0; i < componentNames.Length; i++) {
                    componentNames[i] = prefix + i;
                }
                _contextInfo = new ContextInfo(
                    "Unnamed Context", componentNames, null
                );
            }

            _groupsForIndex = new List<Group>[totalComponents];
            _componentPools = new Stack<IComponent>[totalComponents];
            _entityIndices = new Dictionary<string, IEntityIndex>();

            // Cache delegates to avoid gc allocations
            _cachedEntityChanged = updateGroupsComponentAddedOrRemoved;
            _cachedComponentReplaced = updateGroupsComponentReplaced;
            _cachedEntityReleased = onEntityReleased;
        }

        public virtual Entity CreateEntity() {
            var entity = _reusableEntities.Count > 0
                    ? _reusableEntities.Pop()
                    : new Entity( _totalComponents, _componentPools, _contextInfo);
            entity._isEnabled = true;
            entity._creationIndex = _creationIndex++;
            entity.Retain(this);
            _entities.Add(entity);
            _entitiesCache = null;
            entity.OnComponentAdded +=_cachedEntityChanged;
            entity.OnComponentRemoved += _cachedEntityChanged;
            entity.OnComponentReplaced += _cachedComponentReplaced;
            entity.OnEntityReleased += _cachedEntityReleased;

            if(OnEntityCreated != null) {
                OnEntityCreated(this, entity);
            }

            return entity;
        }

        public virtual void DestroyEntity(Entity entity) {
            var removed = _entities.Remove(entity);
            if(!removed) {
                throw new ContextDoesNotContainEntityException(
                    "'" + this + "' cannot destroy " + entity + "!",
                    "Did you call context.DestroyEntity() on a wrong context?"
                );
            }
            _entitiesCache = null;

            if(OnEntityWillBeDestroyed != null) {
                OnEntityWillBeDestroyed(this, entity);
            }

            entity.destroy();

            if(OnEntityDestroyed != null) {
                OnEntityDestroyed(this, entity);
            }

            if(entity.retainCount == 1) {
                // Can be released immediately without
                // adding to _retainedEntities
                entity.OnEntityReleased -= _cachedEntityReleased;
                _reusableEntities.Push(entity);
                entity.Release(this);
                entity.removeAllOnEntityReleasedHandlers();
            } else {
                _retainedEntities.Add(entity);
                entity.Release(this);
            }
        }

        public virtual void DestroyAllEntities() {
            var entities = GetEntities();
            for (int i = 0; i < entities.Length; i++) {
                DestroyEntity(entities[i]);
            }

            _entities.Clear();

            if(_retainedEntities.Count != 0) {
                throw new ContextStillHasRetainedEntitiesException(this);
            }
        }

        public virtual bool HasEntity(Entity entity) {
            return _entities.Contains(entity);
        }

        public virtual Entity[] GetEntities() {
            if(_entitiesCache == null) {
                _entitiesCache = new Entity[_entities.Count];
                _entities.CopyTo(_entitiesCache);
            }

            return _entitiesCache;
        }

        public virtual Group GetGroup(IMatcher matcher) {
            Group group;
            if(!_groups.TryGetValue(matcher, out group)) {
                group = new Group(matcher);
                var entities = GetEntities();
                for (int i = 0; i < entities.Length; i++) {
                    group.HandleEntitySilently(entities[i]);
                }
                _groups.Add(matcher, group);

                for (int i = 0; i < matcher.indices.Length; i++) {
                    var index = matcher.indices[i];
                    if(_groupsForIndex[index] == null) {
                        _groupsForIndex[index] = new List<Group>();
                    }
                    _groupsForIndex[index].Add(group);
                }

                if(OnGroupCreated != null) {
                    OnGroupCreated(this, group);
                }
            }

            return group;
        }

        public void ClearGroups() {
            foreach(var group in _groups.Values) {
                group.RemoveAllEventHandlers();
                var entities = group.GetEntities();
                for (int i = 0; i < entities.Length; i++) {
                    entities[i].Release(group);
                }

                if(OnGroupCleared != null) {
                    OnGroupCleared(this, group);
                }
            }
            _groups.Clear();

            for (int i = 0; i < _groupsForIndex.Length; i++) {
                _groupsForIndex[i] = null;
            }
        }

        public void AddEntityIndex(string name, IEntityIndex entityIndex) {
            if(_entityIndices.ContainsKey(name)) {
                throw new ContextEntityIndexDoesAlreadyExistException(this, name);
            }

            _entityIndices.Add(name, entityIndex);
        }

        public IEntityIndex GetEntityIndex(string name) {
            IEntityIndex entityIndex;
            if(!_entityIndices.TryGetValue(name, out entityIndex)) {
                throw new ContextEntityIndexDoesNotExistException(this, name);
            }

            return entityIndex;
        }

        public void DeactivateAndRemoveEntityIndices() {
            foreach(var entityIndex in _entityIndices.Values) {
                entityIndex.Deactivate();
            }

            _entityIndices.Clear();
        }

        public void ResetCreationIndex() {
            _creationIndex = 0;
        }

        public void ClearComponentPool(int index) {
            var componentPool = _componentPools[index];
            if(componentPool != null) {
                componentPool.Clear();
            }
        }

        public void ClearComponentPools() {
            for (int i = 0; i < _componentPools.Length; i++) {
                ClearComponentPool(i);
            }
        }

        public void Reset() {
            ClearGroups();
            DestroyAllEntities();
            ResetCreationIndex();

            OnEntityCreated = null;
            OnEntityWillBeDestroyed = null;
            OnEntityDestroyed = null;
            OnGroupCreated = null;
            OnGroupCleared = null;
        }

        public override string ToString() {
            return _contextInfo.name;
        }

        void updateGroupsComponentAddedOrRemoved(
            Entity entity, int index, IComponent component) {
            var groups = _groupsForIndex[index];
            if(groups != null) {
                var events = EntitasCache.GetGroupChangedList();

                    for(int i = 0; i < groups.Count; i++) {
                        events.Add(groups[i].handleEntity(entity));
                    }

                    for(int i = 0; i < events.Count; i++) {
                        var groupChangedEvent = events[i];
                        if(groupChangedEvent != null) {
                            groupChangedEvent(
                                groups[i], entity, index, component
                            );
                        }
                    }

                EntitasCache.PushGroupChangedList(events);
            }
        }

        void updateGroupsComponentReplaced(Entity entity,
                                           int index,
                                           IComponent previousComponent,
                                           IComponent newComponent) {
            var groups = _groupsForIndex[index];
            if(groups != null) {
                for (int i = 0; i < groups.Count; i++) {
                    groups[i].UpdateEntity(
                        entity, index, previousComponent, newComponent
                    );
                }
            }
        }

        void onEntityReleased(Entity entity) {
            if(entity._isEnabled) {
                throw new EntityIsNotDestroyedException(
                    "Cannot release " + entity + "!"
                );
            }
            entity.removeAllOnEntityReleasedHandlers();
            _retainedEntities.Remove(entity);
            _reusableEntities.Push(entity);
        }
    }

    public class ContextDoesNotContainEntityException : EntitasException {
        public ContextDoesNotContainEntityException(string message, string hint) :
            base(message + "\nContext does not contain entity!", hint) {
        }
    }

    public class EntityIsNotDestroyedException : EntitasException {
        public EntityIsNotDestroyedException(string message) :
            base(message + "\nEntity is not destroyed yet!",
                "Did you manually call entity.Release(context) yourself? " +
                "If so, please don't :)") {
        }
    }

    public class ContextStillHasRetainedEntitiesException : EntitasException {
        public ContextStillHasRetainedEntitiesException(Context context) : base(
            "'" + context + "' detected retained entities " +
            "although all entities got destroyed!",
            "Did you release all entities? Try calling context.ClearGroups() " +
            "and systems.ClearReactiveSystems() before calling " +
            "context.DestroyAllEntities() to avoid memory leaks.") {
        }
    }

    public class ContextInfoException : EntitasException {
        public ContextInfoException(Context context, ContextInfo contextInfo) :
            base("Invalid ContextInfo for '" + context + "'!\nExpected " +
                 context.totalComponents + " componentName(s) but got " +
                 contextInfo.componentNames.Length + ":",
                 string.Join("\n", contextInfo.componentNames)) {
        }
    }

    public class ContextEntityIndexDoesNotExistException : EntitasException {
        public ContextEntityIndexDoesNotExistException(Context context, string name) :
            base("Cannot get EntityIndex '" + name + "' from context '" +
                 context + "'!", "No EntityIndex with this name has been added.") {
        }
    }

    public class ContextEntityIndexDoesAlreadyExistException : EntitasException {
        public ContextEntityIndexDoesAlreadyExistException(Context context, string name) :
            base("Cannot add EntityIndex '" + name + "' to context '" + context + "'!",
                 "An EntityIndex with this name has already been added.") {
        }
    }

    public class ContextInfo {

        public readonly string name;
        public readonly string[] componentNames;
        public readonly Type[] componentTypes;

        public ContextInfo(string name,
                            string[] componentNames,
                            Type[] componentTypes) {
            this.name = name;
            this.componentNames = componentNames;
            this.componentTypes = componentTypes;
        }
    }
}
