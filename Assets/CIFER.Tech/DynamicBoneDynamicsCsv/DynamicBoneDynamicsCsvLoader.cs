using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CIFER.Tech.Utils;
using UnityEngine;

namespace CIFER.Tech.DynamicBoneDynamicsCsv
{
    public static class DynamicBoneDynamicsCsvLoader
    {
        private static readonly Regex FieldRegex = new Regex(@"\[.+\]", RegexOptions.Compiled);
        private static StreamReader _streamReader;

        public static void CsvLoad(DynamicBoneDynamicsCsv.Data data)
        {
            _streamReader = new StreamReader(data.FilePath, Encoding.UTF8);
            while (!_streamReader.EndOfStream)
            {
                var line = _streamReader.ReadLine();
                var isFieldLine = line != null && FieldRegex.IsMatch(line);

                if (!isFieldLine)
                    continue;

                DynamicBoneDynamicsCsv.FieldType lineFieldType;
                Enum.TryParse(line.Trim('[', ']'), out lineFieldType);

                switch (lineFieldType)
                {
                    case DynamicBoneDynamicsCsv.FieldType.DynamicBone:
                        if (data.IsDynamicBoneSaveLoad)
                            AttachDynamicBone(data.TargetRoot);
                        break;

                    case DynamicBoneDynamicsCsv.FieldType.m_DampingDistrib:
                    case DynamicBoneDynamicsCsv.FieldType.m_ElasticityDistrib:
                    case DynamicBoneDynamicsCsv.FieldType.m_StiffnessDistrib:
                    case DynamicBoneDynamicsCsv.FieldType.m_InertDistrib:
                    case DynamicBoneDynamicsCsv.FieldType.m_FrictionDistrib:
                    case DynamicBoneDynamicsCsv.FieldType.m_RadiusDistrib:
                        if (data.IsDynamicBoneSaveLoad)
                            AttachAnimationCurve(data.TargetRoot, lineFieldType);
                        break;

                    case DynamicBoneDynamicsCsv.FieldType.m_Colliders:
                        if (data.IsDynamicBoneSaveLoad)
                            AttachDynamicBoneFieldT<DynamicBoneColliderBase>(data.TargetRoot);
                        break;

                    case DynamicBoneDynamicsCsv.FieldType.m_Exclusions:
                        if (data.IsDynamicBoneSaveLoad)
                            AttachDynamicBoneFieldT<Transform>(data.TargetRoot);
                        break;

                    case DynamicBoneDynamicsCsv.FieldType.ColliderBase:
                        if (data.IsColliderSaveLoad)
                            CreateDynamicBoneColliders(data.TargetRoot);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _streamReader.Close();
            _streamReader = null;
            Debug.Log($"{typeof(DynamicBoneDynamicsCsvSaver).ToString().Split('.').Last()}: ロードしました！");
        }

        private static void AttachDynamicBone(Transform targetRoot)
        {
            var inline = _streamReader.ReadLine();
            while (!string.IsNullOrEmpty(inline))
            {
                var split = inline.Split(',');

                var isNUllOrEmpty = string.IsNullOrEmpty(split[0]);
                //この場合はエラー
                if (CiferTechUtils.FindSameNameTransformInChildren(split[0], targetRoot) == null && !isNUllOrEmpty)
                {
                    inline = _streamReader.ReadLine();
                    continue;
                }

                var target = isNUllOrEmpty
                    ? targetRoot.gameObject.AddComponent<DynamicBone>()
                    : CiferTechUtils.FindOrCreateT<DynamicBone>(split[1], targetRoot);

                SetParent(target.transform, targetRoot, split);

                target.m_Root =
                    CiferTechUtils.FindSameNameTransformInChildren(split[9], targetRoot);
                target.m_UpdateRate = Convert.ToSingle(split[10]);
                target.m_UpdateMode = (DynamicBone.UpdateMode) Convert.ToInt32(split[11]);
                target.m_Damping = Convert.ToSingle(split[12]);
                target.m_Elasticity = Convert.ToSingle(split[13]);
                target.m_Stiffness = Convert.ToSingle(split[14]);
                target.m_Inert = Convert.ToSingle(split[15]);
                target.m_Friction = Convert.ToSingle(split[16]);
                target.m_Radius = Convert.ToSingle(split[17]);
                target.m_EndLength = Convert.ToSingle(split[18]);
                target.m_EndOffset.Set(Convert.ToSingle(split[19]), Convert.ToSingle(split[20]),
                    Convert.ToSingle(split[21]));
                target.m_Gravity.Set(Convert.ToSingle(split[22]), Convert.ToSingle(split[23]),
                    Convert.ToSingle(split[24]));
                target.m_Force.Set(Convert.ToSingle(split[25]), Convert.ToSingle(split[26]),
                    Convert.ToSingle(split[27]));
                target.m_FreezeAxis = (DynamicBone.FreezeAxis) Convert.ToInt32(split[28]);
                target.m_DistantDisable = Convert.ToBoolean(split[29]);
                target.m_ReferenceObject =
                    CiferTechUtils.FindSameNameTransformInChildren(split[30], targetRoot);
                target.m_DistanceToObject = Convert.ToSingle(split[31]);

                inline = _streamReader.ReadLine();
            }
        }

        private static void AttachAnimationCurve(Transform targetRoot,
            DynamicBoneDynamicsCsv.FieldType type)
        {
            var inline = _streamReader.ReadLine();
            while (!string.IsNullOrEmpty(inline))
            {
                var split = inline.Split(',');

                //この場合はエラー
                if (CiferTechUtils.FindSameNameTransformInChildren(split[0], targetRoot) == null)
                {
                    inline = _streamReader.ReadLine();
                    continue;
                }

                var target = CiferTechUtils.FindOrCreateT<DynamicBone>(split[0], targetRoot);

                var field = typeof(DynamicBone).GetField(type.ToString());
                if (field != null)
                {
                    field.SetValue(target, new AnimationCurve
                    {
                        keys = MoldKeyFrames(split)
                    });
                }

                inline = _streamReader.ReadLine();
            }
        }

        private static Keyframe[] MoldKeyFrames(IReadOnlyList<string> split)
        {
            var keyFrameList = new List<Keyframe>();
            for (var i = 1; i < split.Count; i += 6)
            {
                keyFrameList.Add(new Keyframe()
                {
                    time = Convert.ToSingle(split[i]),
                    value = Convert.ToSingle(split[i + 1]),
                    inTangent = Convert.ToSingle(split[i + 2]),
                    inWeight = Convert.ToSingle(split[i + 3]),
                    outTangent = Convert.ToSingle(split[i + 4]),
                    outWeight = Convert.ToSingle(split[i + 5]),
                });
            }

            return keyFrameList.ToArray();
        }

        private static void AttachDynamicBoneFieldT<T>(Transform targetRoot) where T : Component
        {
            var inline = _streamReader.ReadLine();
            while (!string.IsNullOrEmpty(inline))
            {
                var split = inline.Split(',');

                //この場合はエラー
                if (CiferTechUtils.FindSameNameTransformInChildren(split[0], targetRoot) == null)
                {
                    inline = _streamReader.ReadLine();
                    continue;
                }

                var target = CiferTechUtils.FindOrCreateT<DynamicBone>(split[0], targetRoot);

                var list = new List<T>();
                for (var i = 1; i < split.Length; i++)
                {
                    list.Add(CiferTechUtils.FindSameNameTransformInChildren(split[i], targetRoot)?.GetComponent<T>());
                }

                if (typeof(T) == typeof(DynamicBoneColliderBase))
                {
                    target.m_Colliders = list as List<DynamicBoneColliderBase>;
                }
                else if (typeof(T) == typeof(Transform))
                {
                    target.m_Exclusions = list as List<Transform>;
                }

                inline = _streamReader.ReadLine();
            }
        }

        private static void CreateDynamicBoneColliders(Transform targetRoot)
        {
            var inline = _streamReader.ReadLine();
            while (!string.IsNullOrEmpty(inline))
            {
                var split = inline.Split(',');

                var isNUllOrEmpty = string.IsNullOrEmpty(split[0]);
                //この場合はエラー
                if (CiferTechUtils.FindSameNameTransformInChildren(split[0], targetRoot) == null && !isNUllOrEmpty)
                {
                    inline = _streamReader.ReadLine();
                    continue;
                }

                DynamicBoneColliderBase target;
                //TRUE: DynamicBoneCollider, FALSE: DynamicBonePlaneCollider
                if (split.Length > 14)
                {
                    target = isNUllOrEmpty
                        ? targetRoot.gameObject.AddComponent<DynamicBoneCollider>()
                        : CiferTechUtils.FindOrCreateT<DynamicBoneCollider>(split[1], targetRoot);

                    var dynamicBoneCollider = (DynamicBoneCollider) target;
                    dynamicBoneCollider.m_Radius = Convert.ToSingle(split[14]);
                    dynamicBoneCollider.m_Height = Convert.ToSingle(split[15]);
                }
                else
                {
                    target = isNUllOrEmpty
                        ? targetRoot.gameObject.AddComponent<DynamicBonePlaneCollider>()
                        : CiferTechUtils.FindOrCreateT<DynamicBonePlaneCollider>(split[1], targetRoot);
                }

                SetParent(target.transform, targetRoot, split);

                target.m_Direction = (DynamicBoneColliderBase.Direction) Convert.ToInt32(split[9]);
                target.m_Center.Set(Convert.ToSingle(split[10]), Convert.ToSingle(split[11]),
                    Convert.ToSingle(split[12]));
                target.m_Bound = (DynamicBoneColliderBase.Bound) Convert.ToInt32(split[13]);

                inline = _streamReader.ReadLine();
            }
        }

        private static void SetParent(Transform target, Transform targetRoot, IReadOnlyList<string> split)
        {
            if ((target.parent != null ? target.parent.name : null) != split[0])
                target.SetParent(CiferTechUtils.FindSameNameTransformInChildren(split[0], targetRoot));

            target.transform.localPosition = new Vector3(Convert.ToSingle(split[2]), Convert.ToSingle(split[3]),
                Convert.ToSingle(split[4]));
            target.transform.localRotation = new Quaternion(Convert.ToSingle(split[5]), Convert.ToSingle(split[6]),
                Convert.ToSingle(split[7]), Convert.ToSingle(split[8]));
        }
    }
}