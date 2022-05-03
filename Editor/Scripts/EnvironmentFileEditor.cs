// Copyright (c) Scott Doxey. All Rights Reserved. Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CandyCoded.env.Editor
{

    public class EnvironmentFileEditor : EditorWindow
    {

        private Vector2 scrollPosition;

        private List<Tuple<string, string>> tempConfig = new List<Tuple<string, string>>();

        private List<string> persistedVariables = new List<string>();

        private List<bool> variablesToIncludeInBuild = new List<bool>();

        private void Update()
        {

            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {

                Repaint();

            }

        }

        private void OnEnable()
        {

            LoadEnvironmentFile();

        }

        private void OnGUI()
        {

            if (!File.Exists(env.editorFilePath))
            {

                if (GUILayout.Button("Create .env File"))
                {

                    try
                    {

                        File.WriteAllText(env.editorFilePath,
                            env.SerializeEnvironmentDictionary(
                                new Dictionary<string, string> { { "VARIABLE_NAME", "VALUE" } }));

                        LoadEnvironmentFile();

                    }
                    catch (Exception err)
                    {

                        EditorUtility.DisplayDialog("Error", err.Message, "Ok");

                    }

                }

                return;

            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            for (var i = 0; i < tempConfig.Count; i += 1)
            {

                GUILayout.BeginHorizontal();

                var includeInBuild = GUILayout.Toggle(variablesToIncludeInBuild[i], "", GUILayout.ExpandWidth(false));

                var key = GUILayout.TextField(tempConfig[i].Item1, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(120));

                var value = GUILayout.TextField(tempConfig[i].Item2, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(160));

                if (tempConfig.Count == 1)
                {

                    GUI.enabled = false;

                }

                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {

                    tempConfig.RemoveAt(i);
                    variablesToIncludeInBuild.RemoveAt(i);

                    continue;

                }

                if (tempConfig.Count == 1)
                {

                    GUI.enabled = true;

                }

                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {

                    tempConfig.Insert(i + 1, new Tuple<string, string>("", ""));
                    variablesToIncludeInBuild.Insert(i + 1, false);

                    continue;

                }

                GUILayout.EndHorizontal();

                if (!key.Equals(tempConfig[i].Item1) || !value.Equals(tempConfig[i].Item2))
                {

                    tempConfig[i] = new Tuple<string, string>(key, value);

                }

                if (includeInBuild != variablesToIncludeInBuild[i])
                {

                    variablesToIncludeInBuild[i] = includeInBuild;

                }

            }

            if (GUILayout.Button("Save Changes"))
            {

                try
                {

                    File.WriteAllText(env.editorFilePath, env.SerializeEnvironmentDictionary(
                        tempConfig.ToDictionary(item => item.Item1, item => item.Item2)));
                    File.WriteAllText(env.persistedFilePath, env.SerializeEnvironmentDictionary(
                        tempConfig
                            .Select((v, i) => variablesToIncludeInBuild[i] ? v : null)
                            .Where(v => v != null)
                            .ToDictionary(item => item.Item1, item => item.Item2)));

                }
                catch (Exception err)
                {

                    EditorUtility.DisplayDialog("Error", err.Message, "Ok");

                }

            }

            if (GUILayout.Button("Revert Changes"))
            {

                LoadEnvironmentFile();

            }

            if (GUILayout.Button("Delete .env File"))
            {

                if (EditorUtility.DisplayDialog("Confirm", $"Delete {env.editorFilePath}?", "Ok", "Cancel"))
                {

                    File.Delete(env.editorFilePath);
                    File.Delete(env.persistedFilePath);

                }

            }

            GUILayout.EndScrollView();

        }

        [MenuItem("Window/DevTools/Environment File Editor")]
        public static void ShowWindow()
        {

            GetWindow(typeof(EnvironmentFileEditor), false, "Environment File Editor", true);

        }

        private void LoadEnvironmentFile()
        {

            if (File.Exists(env.editorFilePath))
            {

                persistedVariables = env.ParseEnvironmentFile(File.ReadAllText(env.persistedFilePath, Encoding.UTF8)).Select(item => item.Key)
                        .ToList();
                tempConfig = env.ParseEnvironmentFile().Select(item => new Tuple<string, string>(item.Key, item.Value))
                    .ToList();
                for (int i = 0; i < tempConfig.Count; i++) {
                    variablesToIncludeInBuild.Add(persistedVariables.Contains(tempConfig[i].Item1));
                }

            }

        }

    }

}
#endif
