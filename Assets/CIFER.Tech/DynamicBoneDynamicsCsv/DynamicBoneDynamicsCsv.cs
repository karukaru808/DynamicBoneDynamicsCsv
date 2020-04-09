using UnityEngine;

namespace CIFER.Tech.DynamicBoneDynamicsCsv
{
    public static class DynamicBoneDynamicsCsv
    {
        public enum FieldType
        {
            DynamicBone,
            m_DampingDistrib,
            m_ElasticityDistrib,
            m_StiffnessDistrib,
            m_InertDistrib,
            m_FrictionDistrib,
            m_RadiusDistrib,
            m_Colliders,
            m_Exclusions,
            ColliderBase,
        }

        public struct Data
        {
            public Transform TargetRoot;
            public bool IsDynamicBoneSaveLoad, IsColliderSaveLoad;
            public string FilePath;
        }
    }
}