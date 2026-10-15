using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.LegacyHash.Tests
{
    [TestClass]
    public class LegacyHashTests
    {
        // Observations from the installed CLR are not historical frozen-input vectors.
        [TestMethod]
        public void WhenInstalledFrameworkHashesStringsThenRealX86ClrEvidenceIsArchived()
        {
            string root = typeof(LegacyHashTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value;
            string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "legacy-clr-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(archive);
            MethodInfo hash = typeof(string).GetMethod("GetHashCode", Type.EmptyTypes);
            MethodInfo legacy = typeof(string).GetMethod("GetLegacyNonRandomizedHashCode", BindingFlags.Instance | BindingFlags.NonPublic);
            var report = new StringBuilder();
            report.AppendLine("runtime=" + Environment.Version);
            report.AppendLine("pointerSize=" + IntPtr.Size);
            report.AppendLine("mscorlib=" + typeof(string).Assembly.Location);
            report.AppendLine("fileVersion=" + FileVersionInfo.GetVersionInfo(typeof(string).Assembly.Location).FileVersion);
            report.AppendLine("hostConfig=" + AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            report.AppendLine("COMPlus_UseRandomizedStringHashAlgorithm=" + Environment.GetEnvironmentVariable("COMPlus_UseRandomizedStringHashAlgorithm"));
            report.AppendLine("legacyMethodReachable=" + (legacy != null));
            using (var sha = SHA256.Create()) report.AppendLine("mscorlibSHA256=" + BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(typeof(string).Assembly.Location))).Replace("-", ""));
            MethodBody body = hash.GetMethodBody();
            report.AppendLine("GetHashCodeIL=" + (body == null ? "RuntimeImplemented" : BitConverter.ToString(body.GetILAsByteArray())));
            foreach (string value in new[] { "", "a", "abc", "MilMatrix3x2D", "MILCMD_MATRIXRESOURCE", "S_11", "\uD83D\uDE00", "a\0b" })
            {
                report.AppendLine(Convert.ToBase64String(Encoding.Unicode.GetBytes(value)) + " public=" + value.GetHashCode() + " legacy=" + (legacy == null ? "Unavailable" : legacy.Invoke(value, null).ToString()));
            }
            if (File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)) File.Copy(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, Path.Combine(archive, "host.config"));
            File.WriteAllText(Path.Combine(archive, "report.txt"), report.ToString());
            Console.WriteLine(archive);
            Assert.AreEqual(4, IntPtr.Size);
        }
    }
}
