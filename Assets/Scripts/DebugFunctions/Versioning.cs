/*
 * Tabletop Theatre
 * Copyright (C) 2020-2022 Robert van Kooten
 * Original source code: https://github.com/ThatRobVK/Tabletop-Theatre
 * License: https://github.com/ThatRobVK/Tabletop-Theatre/blob/main/LICENSE
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#if (UNITY_EDITOR)

using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace TT.DebugFunctions
{
    public class Versioning : IPreprocessBuildWithReport
    {
        private static string _currentBuildNumber;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("RUNNING VERSIONING CODE");
            var versionAsset = Resources.Load<CurrentVersion>("CurrentVersion");
            if (versionAsset == null)
            {
                Debug.LogError("No CurrentVersion asset found. Create one to auto-increment versions.");
                return;
            }

            var buildDate = DateTime.Parse(string.IsNullOrEmpty(versionAsset.BuildDate) ? DateTime.Today.AddDays(-1).ToString() : versionAsset.BuildDate);
            if (buildDate.Date.CompareTo(DateTime.Today) != 0)
            {
                // First build of this day
                versionAsset.BuildNumber = 0;
                buildDate = DateTime.Today;
                versionAsset.BuildDate = buildDate.ToString(CultureInfo.InvariantCulture);
            }

            versionAsset.BuildNumber++;
            string releaseType = versionAsset.ReleaseType == ReleaseType.Prod ? "" : string.Format(" {0}", versionAsset.ReleaseType.ToString());
            _currentBuildNumber = string.Format("{0}.{1}.{2}.{3}{4}", buildDate.Year, buildDate.Month, buildDate.Day, versionAsset.BuildNumber, releaseType);
            versionAsset.Version = _currentBuildNumber;
            PlayerSettings.bundleVersion = _currentBuildNumber;

            EditorUtility.SetDirty(versionAsset);
            AssetDatabase.SaveAssets();
        }

        [PostProcessBuild(0)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            var destinationFolder = new FileInfo(pathToBuiltProject).Directory;
            var versionFile = new FileInfo("version.txt");

            if (versionFile.Exists) versionFile.Delete();

            using var file = versionFile.OpenWrite();
            var content = Encoding.ASCII.GetBytes(PlayerSettings.bundleVersion);
            file.Write(content, 0, content.Length);
            file.Close();

            if (destinationFolder != null)
                versionFile.CopyTo(string.Concat(destinationFolder.FullName, "\\", versionFile.Name), true);
        }
    }
}
#endif