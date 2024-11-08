﻿using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.DAI;
using DA_Assets.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DA_Assets.UI;
using System.Threading.Tasks;

#if UITK_LINKER_EXISTS
using DA_Assets.UEL;
#endif

namespace DA_Assets.FCU
{
    [Serializable]
    public class ScriptGenerator : MonoBehaviourLinkerRuntime<FigmaConverterUnity>
    {
        public async Task Generate()
        {
            List<FObject> buttons = monoBeh.CanvasDrawer.ButtonDrawer.Buttons;
            List<FObject> inputFields = monoBeh.CanvasDrawer.InputFieldDrawer.InputFields;
            List<FObject> texts = monoBeh.CanvasDrawer.TextDrawer.Texts;

            List<FObject> allItems = new List<FObject>();

            if (!buttons.IsEmpty())
            {
                allItems.AddRange(buttons);
            }

            if (!inputFields.IsEmpty())
            {
                allItems.AddRange(inputFields);
            }

            if (!texts.IsEmpty())
            {
                texts = texts.Where(x => !x.Data.Parent.IsDefault() && !x.Data.Parent.ContainsTag(FcuTag.InputField)).ToList();
                allItems.AddRange(texts);
            }

            var grouped = allItems
                .GroupBy(item => item.Data.RootFrame)
                .Select(group => new
                {
                    RootFrame = group.Key,
                    FObjects = group.ToList()
                });

            string usings = "";

            if (monoBeh.IsUGUI())
            {
                usings = @"using UnityEngine.UI;";

                if (monoBeh.UsingTextMesh())
                {
                    usings += @"
#if TextMeshPro
using TMPro;
#endif";
                }
            }
            else
            {
                usings = @"using UnityEngine.UIElements;";
            }

            string baseClass = FcuConfig.Instance.BaseClass.text;

            foreach (var group in grouped)
            {
                SyncData rootFrame = group.RootFrame;

                string className = rootFrame.Names.ClassName;

                string fields = GetFields(group.FObjects);
                string script = "";

                script = string.Format(baseClass,
                   usings,
                   monoBeh.Settings.ScriptGeneratorSettings.Namespace,
                   className,
                   fields);

                string folderPath = monoBeh.Settings.ScriptGeneratorSettings.OutputPath;
                Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, $"{className}.cs");

                File.WriteAllText(filePath, script.ToString());
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            await Task.Yield();
        }

        private string GetFields(List<FObject> fobjects)
        {
            StringBuilder elemsSb = new StringBuilder();
            StringBuilder labelsSb = new StringBuilder();

            foreach (FObject fobject in fobjects)
            {
                string fieldName = fobject.Data.Names.FieldName;

                if (monoBeh.IsUGUI())
                {
                    if (fobject.Data.GameObject.TryGetComponentSafe(out Text c1))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(Text)} {fieldName};");
                    }
                    else if (fobject.Data.GameObject.TryGetComponentSafe(out TMP_Text c2))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(TMP_Text)} {fieldName};");
                    }
                    else if (fobject.Data.GameObject.TryGetComponentSafe(out Button c3))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(Button)} {fieldName};");
                    }
                    else if (fobject.Data.GameObject.TryGetComponentSafe(out FcuButton c4))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(FcuButton)} {fieldName};");
                    }
                    else if (fobject.Data.GameObject.TryGetComponentSafe(out InputField c6))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(InputField)} {fieldName};");
                    }
                    else if (fobject.Data.GameObject.TryGetComponentSafe(out TMP_InputField c7))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(TMP_InputField)} {fieldName};");
                    }
                    else
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(GameObject)} {fieldName};");
                    }
                }
                else
                {
#if UITK_LINKER_EXISTS && UNITY_2021_3_OR_NEWER
                    if (fobject.Data.GameObject.TryGetComponentSafe(out UitkLabel c1))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(UitkLabel)} {fieldName};");
                    }
                    else if (fobject.Data.GameObject.TryGetComponentSafe(out UitkButton c2))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(UitkButton)} {fieldName};");
                    }
                    else if (fobject.Data.GameObject.TryGetComponentSafe(out UitkVisualElement c7))
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(UitkVisualElement)} {fieldName};");
                    }
                    else
                    {
                        labelsSb.AppendLine($"        [SerializeField] {nameof(GameObject)} {fieldName};");
                    }
#endif
                }
            }

            return $"{elemsSb}\n{labelsSb}";
        }
    }
}