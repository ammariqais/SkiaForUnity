using NUnit.Framework;
using System.Diagnostics;
using System.IO;

using Unity.PlasticSCM.Editor.AssetUtils;

namespace Unity.PlasticSCM.Tests.Editor.UI
{
    [TestFixture]
    public class UnityPlasticDllVersionTests
    {
        [Test]
        public void TestUnityPlasticDllVersion()
        {
            string LibFolderFullPath = (AssetsPath.IsRunningAsUPMPackage()) ?
                "Packages/com.unity.collab-proxy/Lib/Editor/PlasticSCM/unityplastic.dll" :
                "Assets/Plugins/PlasticSCM/Lib/Editor/unityplastic.dll";

            FileVersionInfo fileVersionInfo = FileVersionInfo.
                GetVersionInfo(Path.GetFullPath(LibFolderFullPath));

            string[] version = fileVersionInfo.FileVersion.Split('.');
            
            Assert.IsTrue(version.Length==4,"Dll version should be in xx.0.16.xxxx format");
            Assert.IsTrue(version[1]=="0","Dll version second number should be 0");
            Assert.IsTrue(version[2]=="16", "Dll version third number should be 16");
            Assert.IsTrue(version[3].Length ==4, "Dll version fourth number should have 4 digits");
            int buildNumber;
            Assert.IsTrue(int.TryParse(version[3], out buildNumber), "Dll version fourth number should be an int number");
            Assert.IsTrue(buildNumber > 6000, "Dll version fourth number should be greater than 6000");
        }
    }
}
