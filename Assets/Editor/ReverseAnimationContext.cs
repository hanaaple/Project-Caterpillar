using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ReverseAnimationContext
    {
        [MenuItem("Assets/Create Reversed Clip", false, 14)]
        private static void ReverseClip()
        {
            var directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
            var fileName = Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
            var fileExtension = Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
            fileName = fileName.Split('.')[0];
            var copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + "_Reversed" + fileExtension;

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Selection.activeObject), copiedFilePath);

            var clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));

            if (clip == null)
                return;
            var clipLength = clip.length;
            var curveBindings = AnimationUtility.GetCurveBindings(clip);

            var curves = curveBindings.Select(editorCurveBinding => AnimationUtility.GetEditorCurve(clip, editorCurveBinding)).ToList();

            clip.ClearCurves();
            for (var index = 0; index < curveBindings.Length; index++)
            {
                var editorCurveBinding = curveBindings[index];
                var curve = curves[index];
                var keys = curve.keys;
                var keyCount = keys.Length;
                (curve.postWrapMode, curve.preWrapMode) = (curve.preWrapMode, curve.postWrapMode);
                for (var i = 0; i < keyCount; i++)
                {
                    var k = keys[i];
                    k.time = clipLength - k.time;
                    var tmp = -k.inTangent;
                    k.inTangent = -k.outTangent;
                    k.outTangent = tmp;
                    keys[i] = k;
                }

                curve.keys = keys;
                clip.SetCurve(editorCurveBinding.path, editorCurveBinding.type, editorCurveBinding.propertyName, curve);
            }

            var events = AnimationUtility.GetAnimationEvents(clip);
            if (events.Length > 0)
            {
                foreach (var t in events)
                {
                    t.time = clipLength - t.time;
                }

                AnimationUtility.SetAnimationEvents(clip, events);
            }

            Debug.Log("Animation reversed!");
        }

        [MenuItem("Assets/Create Reversed Clip", true)]
        static bool ReverseClipValidation()
        {
            return Selection.activeObject is AnimationClip;
        }
    }
}