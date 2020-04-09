using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CIFER.Tech.DynamicBoneDynamicsCsv
{
    public class DynamicBoneDynamicsCsvSaveLoaderWindow : EditorWindow
    {
        private static Transform _targetRoot;
        private bool _isDynamicBoneSaveLoad = true, _isColliderSaveLoad = true;

        [MenuItem("CIFER.Tech/DynamicBoneDynamicsCsvSaveLoader")]
        private static void Open()
        {
            var window = GetWindow<DynamicBoneDynamicsCsvSaveLoaderWindow>("DB CSV SL");
            window.minSize = new Vector2(350f, 200f);

            _targetRoot = GetRootBone();
        }

        private void OnGUI()
        {
            _targetRoot =
                EditorGUILayout.ObjectField("DynamicBoneのルート", _targetRoot, typeof(Transform), true) as Transform;

            if (GUILayout.Button("選択からルートを取得"))
            {
                _targetRoot = GetRootBone();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("設定", new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter
            });

            _isDynamicBoneSaveLoad = EditorGUILayout.ToggleLeft("DynamicBone", _isDynamicBoneSaveLoad);
            _isColliderSaveLoad = EditorGUILayout.ToggleLeft("コライダー", _isColliderSaveLoad);

            GUILayout.FlexibleSpace();

            //エラー、警告判定
            if (_targetRoot == null)
            {
                EditorGUILayout.HelpBox("ルートオブジェクトを選択してください。", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                //セーブ
                if (GUILayout.Button("セーブ"))
                {
                    var filePath = EditorUtility.SaveFilePanel(
                        "DynamicBoneセットアップを保存", "", $"{_targetRoot.name}_DynamicBone_Dynamics.csv", "csv");
                    if (filePath.Length <= 0)
                        return;

                    var copyData = new DynamicBoneDynamicsCsv.Data
                    {
                        TargetRoot = _targetRoot,
                        IsDynamicBoneSaveLoad = _isDynamicBoneSaveLoad,
                        IsColliderSaveLoad = _isColliderSaveLoad,
                        FilePath = filePath
                    };
                    DynamicBoneDynamicsCsvSaver.CsvSave(copyData);
                }

                //ロード
                if (GUILayout.Button("ロード"))
                {
                    var filePath = EditorUtility.OpenFilePanelWithFilters(
                        "DynamicBoneセットアップを読み込む", "", new[] {"CSVファイル", "csv", "テキストファイル", "txt"});
                    if (filePath.Length <= 0)
                        return;

                    var copyData = new DynamicBoneDynamicsCsv.Data()
                    {
                        TargetRoot = _targetRoot,
                        IsDynamicBoneSaveLoad = _isDynamicBoneSaveLoad,
                        IsColliderSaveLoad = _isColliderSaveLoad,
                        FilePath = filePath,
                    };
                    DynamicBoneDynamicsCsvLoader.CsvLoad(copyData);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static Transform GetRootBone()
        {
            return Selection.transforms.Length <= 0 ? null : Selection.transforms.First();
        }
    }
}