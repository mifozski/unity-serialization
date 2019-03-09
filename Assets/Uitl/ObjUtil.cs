using UnityEngine;

namespace Utils
{
    public static class ObjUtil
    {
		public static T GetAsFromSource<T>(object obj) where T : class
		{
			if (obj == null) return null;
			if (obj is T) return obj as T;
			// if (obj is IComponent)
			// {
			// 	var c = (obj as IComponent).component;
			// 	if (c is T) return c as T;
			// }
			var go = /* GameObjectUtil. */GetGameObjectFromSource(obj);
			if (go is T) return go as T;

			//if (go != null && ComponentUtil.IsAcceptableComponentType(typeof(T))) return go.GetComponentAlt<T>();
			if (go != null)
			{
				// UNCOMMENT

				// var tp = typeof(T);
				// if (typeof(SPEntity).IsAssignableFrom(tp))
				// 	return SPEntity.Pool.GetFromSource(tp, go) as T;
				// else if (ComponentUtil.IsAcceptableComponentType(tp))
				// 	return go.GetComponent(tp) as T;
			}

			return null;
		}

		public static GameObject GetGameObjectFromSource(object obj, bool respectProxy = false)
        {
            if (obj == null) return null;

            if (obj is GameObject)
                return obj as GameObject;
            if (obj is Component)
                return /* ObjUtil.IsObjectAlive(obj as Component) ?  */(obj as Component).gameObject/*  : null */;
            // if (obj is IGameObjectSource)
            //     return obj.IsNullOrDestroyed() ? null : (obj as IGameObjectSource).gameObject;

            return null;
        }
	}
}