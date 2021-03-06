//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGenerator.ComponentContextGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Entitas;

public partial class GameContext {

    public GameEntity pointsEntity { get { return GetGroup(GameMatcher.Points).GetSingleEntity(); } }
    public PointsComponent points { get { return pointsEntity.points; } }
    public bool hasPoints { get { return pointsEntity != null; } }

    public GameEntity SetPoints(int newCurrPoints, int newTargetPoints) {
        if(hasPoints) {
            throw new EntitasException("Could not set points!\n" + this + " already has an entity with PointsComponent!",
                "You should check if the context already has a pointsEntity before setting it or use context.ReplacePoints().");
        }
        var entity = CreateEntity();
        entity.AddPoints(newCurrPoints, newTargetPoints);
        return entity;
    }

    public void ReplacePoints(int newCurrPoints, int newTargetPoints) {
        var entity = pointsEntity;
        if(entity == null) {
            entity = SetPoints(newCurrPoints, newTargetPoints);
        } else {
            entity.ReplacePoints(newCurrPoints, newTargetPoints);
        }
    }

    public void RemovePoints() {
        DestroyEntity(pointsEntity);
    }
}
