using AltV.Net.Elements.Entities;
using ChoiceVServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Base {
    public static class EntityExtensions {
        public static bool Exists(this IEntity entity) {
            try {
                if(entity.Exists) {
                    return true;
                } else {
                    return false;
                }
            } catch(EntityRemovedException) {
                return false;
            }
        }

        /// <summary>
        /// Adds a CollisionShape to the player CollisionShape List
        /// </summary>
        public static void addCurrentCollisionShape(this IEntity entity, CollisionShape colShape) {
            if(entity.hasData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE)) {
                List<CollisionShape> list = entity.getData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE);
                if(!list.Contains(colShape)) {
                    list.Add(colShape);
                    entity.setData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE, list);
                } else {
                    Logger.logWarning(LogCategory.System, LogActionType.Blocked, $"addCurrentCollisionShape: tried to add colshape which was already in list!");
                }
            } else {
                var newList = new List<CollisionShape>();
                newList.Add(colShape);
                entity.setData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE, newList);
            }
        }

        /// <summary>
        /// Removes a CollisionShape from the player CollisionShape List
        /// </summary>
        public static void removeCollisionShape(this IEntity entity, CollisionShape colShape) {
            if(entity.hasData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE)) {
                List<CollisionShape> list = entity.getData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE);
                try {
                    list.RemoveAll(c => c == colShape);
                } catch(Exception e) {
                    Logger.logException(e, "removeCurrentCollisionShape: Tried to remove checkpoint that is not in the List!");
                    return;
                }

                if(list.Count > 0) {
                    entity.setData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE, list);
                } else {
                    entity.resetData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE);
                }
            }
        }

        /// <summary>
        /// Gets the most inner CollisionShape the player is in. (Works if the player is in multiple CollisionShape at once)
        /// </summary>
        public static List<CollisionShape> getCurrentCollisionShapes(this IEntity entity) {
            if(entity.hasData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE)) {
                List<CollisionShape> list = entity.getData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE);

                //Update the CollisionShape if there was a port
                list.ForEach(c => {
                    if(c.IsInShape(entity.Position)) {
                        c.updateShapeForEntity(entity);
                    }
                });

                //Return the Last element of the List, which is the most inner CollisionShape the player is in (If he is in multiple)
                return list;
            } else {
                //Logger.logError($"getCurrentCollisionShape: Tried to get current CollisionShape from empty or not created CollisionShape List!");
                return new List<CollisionShape>();
            }
        }

        /// <summary>
        /// Gets if a entity is standing in a specific CollisionShape
        /// </summary>
        public static bool isInSpecificCollisionShape(this IEntity entity, Func<CollisionShape, bool> predicate) {
            return entity.getCurrentCollisionShapes().Any(predicate);
        }

        /// <summary>
        /// Gets if a entity is standing in a restricting collisionShape
        /// </summary>
        public static bool isRestricted(this IEntity entity) {
            if(entity.hasData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE)) {
                List<CollisionShape> list = entity.getData(Constants.DATA_ENTITY_CURRENT_COLLISIONSHAPE);

                foreach(var shape in list) {
                    if(shape.RestrictSpecificActions) {
                        return true;
                    }
                }

            }

            return false;
        }
    }
}
