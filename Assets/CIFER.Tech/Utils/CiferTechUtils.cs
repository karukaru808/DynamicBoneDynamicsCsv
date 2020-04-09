using System.Linq;
using UnityEngine;

namespace CIFER.Tech.Utils
{
    public static class CiferTechUtils
    {
        public static Transform FindSameNameTransformInChildren(string name, Transform searchRoot)
        {
            return searchRoot.GetComponentsInChildren<Transform>().FirstOrDefault(tf => tf.name == name);
        }

        public static T FindOrCreateT<T>(string searchName, Transform searchRoot) where T : Component
        {
            var convertGameObject = FindSameNameTransformInChildren(searchName, searchRoot)?.gameObject;
            if (convertGameObject == null)
                convertGameObject = new GameObject(searchName);

            var component = convertGameObject.GetComponent<T>();
            if (component == null)
                component = convertGameObject.AddComponent<T>();

            return component;
        }

        public static void DeleteExistSetting<T>(Transform target, bool isIncludeChildren) where T : Component
        {
            foreach (var targetClass in isIncludeChildren
                ? target.GetComponentsInChildren<T>()
                : target.GetComponents<T>())
                Object.DestroyImmediate(targetClass);
        }
    }
}