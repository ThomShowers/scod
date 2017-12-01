using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace scod.UnitTests
{
    public class XmlMsBuildProjectTests
    {
        [Fact]
        public void Constructor_Throws_For_Null_Content()
        {
            Assert.Throws<ArgumentNullException>(() => new XmlMsBuildProject(null));
        }

        [Fact]
        public void Default_OutputType_Is_Library()
        {
            var projectXml =
                "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                    "<PropertyGroup>" +
                        "<TargetFramework>netcoreapp2.0</TargetFramework>" +
                    "</PropertyGroup>" +
                "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);

            Assert.Equal("Library", project.OutputType);
        }

        [Theory]
        [InlineData("Library")]
        [InlineData("Exe")]
        public void OutputType_Is_Correct(string outputType)
        {
            var projectXml =
                "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                    "<PropertyGroup>" +
                        "<TargetFramework>netcoreapp2.0</TargetFramework>" +
                        $"<OutputType>{outputType}</OutputType>" +
                    "</PropertyGroup>" +
                "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);

            Assert.Equal(outputType, project.OutputType);
        }

        [Fact]
        public void Default_AssemblyName_Is_Null()
        {
            var projectXml =
                "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                    "<PropertyGroup>" +
                        "<TargetFramework>netcoreapp2.0</TargetFramework>" +
                    "</PropertyGroup>" +
                "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);

            Assert.Null(project.AssemblyName);
        }

        [Fact]
        public void AssemblyName_Is_Correct()
        {
            var projectXml =
                "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                    "<PropertyGroup>" +
                        "<TargetFramework>netcoreapp2.0</TargetFramework>" +
                        "<AssemblyName>MyAssembly</AssemblyName>" +
                    "</PropertyGroup>" +
                "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);

            Assert.Equal("MyAssembly", project.AssemblyName);
        }

        [Fact]
        public void TargetFrameworks_Is_Correct_For_TargetFramework()
        {
            var projectXml =
                "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                    "<PropertyGroup>" +
                        "<TargetFramework>netcoreapp2.0</TargetFramework>" +
                    "</PropertyGroup>" +
                "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);
            var targetFrameworks = project.TargetFrameworks.ToList();
            Assert.Equal(1, targetFrameworks.Count);
            Assert.Equal("netcoreapp2.0", targetFrameworks[0]);
        }

        [Fact]
        public void TargetFrameworks_Is_Correct_For_TargetFrameworks()
        {
            var projectXml =
                "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                    "<PropertyGroup>" +
                        "<TargetFramework>netcoreapp2.0;net45</TargetFramework>" +
                    "</PropertyGroup>" +
                "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);
            var targetFrameworks = project.TargetFrameworks.ToList();
            Assert.Equal(2, targetFrameworks.Count);
            Assert.True(targetFrameworks.Contains("netcoreapp2.0"));
            Assert.True(targetFrameworks.Contains("net45"));
        }

        [Fact]
        public void TargetFrameworks_Is_Correct_For_TargetFrameworkVersion()
        {
            var projectXml =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<Project " +
                        "ToolsVersion=\"14.0\" " +
                        "DefaultTargets=\"Build\" " +
                        "xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">" +
                    "<PropertyGroup>" +
                        "<TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>" +
                    "</PropertyGroup>" +
                "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);
            var targetFrameworks = project.TargetFrameworks.ToList();
            Assert.Equal(1, targetFrameworks.Count);
            Assert.True(targetFrameworks.Contains("v4.6.1"));
        }

        [Theory]
        [InlineData("AnyCPU")]
        [InlineData("x86")]
        [InlineData("x64")]
        public void Default_PlatformTarget_Is_Configuration_Platform(string platform)
        {
            var projectXml =
                   "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                       "<PropertyGroup>" +
                           "<TargetFramework>netcoreapp2.0</TargetFramework>" +
                       "</PropertyGroup>" +
                   "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);
            Assert.Equal(platform, project.GetPlatformTarget("Debug", platform));
        }

        [Theory]
        [InlineData("x86")]
        [InlineData("x64")]
        public void PlatformTarget_Is_Correct(string platform)
        {
            var projectXml =
                   "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                       "<PropertyGroup>" +
                           "<TargetFramework>netcoreapp2.0</TargetFramework>" +
                       "</PropertyGroup>" +
                       "<PropertyGroup " +
                                "Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">" +
                           $"<PlatformTarget>{platform}</PlatformTarget>" +
                       "</PropertyGroup>" +
                   "</Project>";

            var projectContent = XDocument.Load(new StringReader(projectXml));
            var project = new XmlMsBuildProject(projectContent);
            Assert.Equal(platform, project.GetPlatformTarget("Debug", "AnyCPU"));
        }

        
    }
}
