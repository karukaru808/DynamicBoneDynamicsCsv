using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CIFER.Tech.DynamicBoneDynamicsCsv
{
    public static class DynamicBoneDynamicsCsvSaver
    {
        private static StreamWriter _streamWriter;

        public static void CsvSave(DynamicBoneDynamicsCsv.Data data)
        {
            _streamWriter = new StreamWriter(data.FilePath, false, Encoding.UTF8);

            if (data.IsColliderSaveLoad)
            {
                WriteFieldType(DynamicBoneDynamicsCsv.FieldType.ColliderBase);
                var collider = data.TargetRoot.GetComponentsInChildren<DynamicBoneColliderBase>();
                foreach (var cb in collider)
                {
                    WriteColliderBase(cb);
                }
            }

            if (data.IsDynamicBoneSaveLoad)
            {
                WriteFieldType(DynamicBoneDynamicsCsv.FieldType.DynamicBone);
                var dynamicBones = data.TargetRoot.GetComponentsInChildren<DynamicBone>();
                foreach (var db in dynamicBones)
                {
                    WriteDynamicBone(db);
                }

                for (var type = DynamicBoneDynamicsCsv.FieldType.m_DampingDistrib;
                    type <= DynamicBoneDynamicsCsv.FieldType.m_RadiusDistrib;
                    type++)
                {
                    WriteFieldType(type);
                    foreach (var db in dynamicBones)
                    {
                        WriteKeyFrames(db, type);
                    }
                }

                WriteFieldType(DynamicBoneDynamicsCsv.FieldType.m_Colliders);
                foreach (var db in dynamicBones)
                {
                    WriteDynamicBoneFieldT<DynamicBoneColliderBase>(db);
                }

                WriteFieldType(DynamicBoneDynamicsCsv.FieldType.m_Exclusions);
                foreach (var db in dynamicBones)
                {
                    WriteDynamicBoneFieldT<Transform>(db);
                }
            }

            _streamWriter.Close();
            _streamWriter = null;
            Debug.Log($"{typeof(DynamicBoneDynamicsCsvSaver).ToString().Split('.').Last()}: セーブしました！");
        }

        private static void WriteFieldType(DynamicBoneDynamicsCsv.FieldType st)
        {
            _streamWriter.Write(Environment.NewLine);
            _streamWriter.WriteLine($"[{st}]");
        }

        private static void WriteColliderBase(DynamicBoneColliderBase cb)
        {
            var tf = cb.transform;
            _streamWriter.Write(
                $"{(tf.parent != null ? tf.parent.name : "")},{cb.name},{tf.localPosition.x},{tf.localPosition.y},{tf.localPosition.z}," +
                $"{tf.localRotation.x},{tf.localRotation.y},{tf.localRotation.z},{tf.localRotation.w}," +
                $"{(int) cb.m_Direction},{cb.m_Center.x},{cb.m_Center.y},{cb.m_Center.z},{(int) cb.m_Bound}");

            if (!(cb is DynamicBoneCollider))
            {
                _streamWriter.Write(Environment.NewLine);
                return;
            }

            var dbc = (DynamicBoneCollider) cb;
            _streamWriter.WriteLine($",{dbc.m_Radius},{dbc.m_Height}");
        }

        private static void WriteDynamicBone(DynamicBone db)
        {
            var tf = db.transform;
            _streamWriter.WriteLine(
                $"{(tf.parent != null ? tf.parent.name : "")},{db.name},{tf.localPosition.x},{tf.localPosition.y},{tf.localPosition.z}," +
                $"{tf.localRotation.x},{tf.localRotation.y},{tf.localRotation.z},{tf.localRotation.w}," +
                $"{(db.m_Root != null ? db.m_Root.name : "")},{db.m_UpdateRate},{(int) db.m_UpdateMode},{db.m_Damping}," +
                $"{db.m_Elasticity},{db.m_Stiffness},{db.m_Inert},{db.m_Friction},{db.m_Radius},{db.m_EndLength}," +
                $"{db.m_EndOffset.x},{db.m_EndOffset.y},{db.m_EndOffset.z}," +
                $"{db.m_Gravity.x},{db.m_Gravity.y},{db.m_Gravity.z}," +
                $"{db.m_Force.x},{db.m_Force.y},{db.m_Force.z}," +
                $"{(int) db.m_FreezeAxis},{db.m_DistantDisable}," +
                $"{(db.m_ReferenceObject != null ? db.m_ReferenceObject.name : "")},{db.m_DistanceToObject}");
        }

        private static void WriteKeyFrames(DynamicBone db, DynamicBoneDynamicsCsv.FieldType type)
        {
            _streamWriter.Write($"{db.name}");

            var field = typeof(DynamicBone).GetField(type.ToString());
            if (field == null)
                return;

            var animationCurve = field.GetValue(db) as AnimationCurve;
            if (animationCurve != null)
            {
                foreach (var keyframe in animationCurve.keys)
                {
                    _streamWriter.Write(
                        $",{keyframe.time},{keyframe.value},{keyframe.inTangent},{keyframe.inWeight},{keyframe.outTangent},{keyframe.outWeight}");
                }
            }

            _streamWriter.Write(Environment.NewLine);
        }

        private static void WriteDynamicBoneFieldT<T>(DynamicBone db) where T : Component
        {
            _streamWriter.Write($"{db.name}");

            var list = new List<T>();
            if (typeof(T) == typeof(DynamicBoneColliderBase))
            {
                list = db.m_Colliders as List<T>;
            }
            else if (typeof(T) == typeof(Transform))
            {
                list = db.m_Exclusions as List<T>;
            }

            if (list != null)
            {
                foreach (var obj in list.Where(obj => obj != null))
                {
                    _streamWriter.Write($",{obj.name}");
                }
            }

            _streamWriter.Write(Environment.NewLine);
        }
    }
}