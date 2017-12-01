using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using wimm.Guardian;

namespace scod
{
    /// <summary>
    /// An MSBuild project described by XML.
    /// </summary>
    public class XmlMsBuildProject
    {
        private const string CONFIGURATION_PROPERTYGROUP_PATTERN =
            @"'\$\(Configuration\)\|\$\(Platform\)'\s*==\s*'(?'configuration'[^']*)'";

        private readonly XDocument _projectContent;
        private readonly string _defaultXmlNamespace;

        /// <summary>
        /// Gets the OutputType property for the project.
        /// </summary>
        /// <remarks>
        /// It is assumed that the OutputType is defined in an unconditioned property group,
        /// that only one element exists, and that it is unconditioned. If any of these assumptions
        /// are untrue then the reported value may not match what MSBuild uses when the project is
        /// evaluated.
        /// </remarks>
        public string OutputType
        {
            get
            {
                return
                    GetPropertyValue("OutputType", GetUnconditionedPropertyGroups()) ?? "Library";
            }
        }

        /// <summary>
        /// Gets the AssemblyName property for the project.
        /// </summary>
        /// <remarks>
        /// It is assumed that the AssemblyName is defined in an unconditioned property group,
        /// that only one element exists, and that it is unconditioned. If any of these assumptions
        /// are untrue then the reported value may not match what MSBuild uses when the project is
        /// evaluated.
        /// </remarks>
        public string AssemblyName
        {
            get
            {
                return
                    GetPropertyValue("AssemblyName", GetUnconditionedPropertyGroups());
            }
        }

        /// <summary>
        /// Gets the TargetFrameworks, TargetFramework, or TargetFrameworkVersion property for the
        /// project.
        /// </summary>
        /// <remarks>
        /// It is assumed that only one of these properties is defined, that it is defined in an
        /// unconditioned property group, and that it is unconditioned. If any of these assumptions
        /// are untrue then the reported value may not match what MSBuild uses when the project is
        /// evaluated.
        /// </remarks>
        public IEnumerable<string> TargetFrameworks
        {
            get
            {
                return
                    (GetPropertyValue("TargetFrameworks", GetUnconditionedPropertyGroups()) ??
                    GetPropertyValue("TargetFramework", GetUnconditionedPropertyGroups()) ??
                    GetPropertyValue("TargetFrameworkVersion", GetUnconditionedPropertyGroups()))
                    ?.Split(";")
                    ?.Select(tf => tf.Trim())
                    ?? new List<string>();
            }
        }

        /// <summary>
        /// Initializes a new <see cref="XmlMsBuildProject"/> using
        /// <paramref name="projectContent"/> as the project content.
        /// </summary>
        /// <param name="projectContent"></param>
        public XmlMsBuildProject(XDocument projectContent)
        {
            projectContent.Require(nameof(projectContent)).Argument().IsNotNull();
            _projectContent = projectContent;
            _defaultXmlNamespace = projectContent.Root.GetDefaultNamespace().ToString();
        }

        /// <summary>
        /// Gets the PlatformTarget property for the project in the specified configuration.
        /// </summary>
        /// <remarks>
        /// It is assumed that this property is defined only in property groups whose conditions
        /// specify a complete and unique proejct configuration. If this assumptions is untrue then
        /// the reported value may not match what MSBuild uses when the project is evaluated.
        /// </remarks>
        public string GetPlatformTarget(string configuration, string platform)
        {
            return
                GetPropertyValue(
                    "PlatformTarget",
                    GetPropertyGroups($"{configuration}|{platform}")) 
                ?? "AnyCPU";
        }

        /// <summary>
        /// Gets the OutputPath property for the project in the specified configuration.
        /// </summary>
        /// <remarks>
        /// It is assumed that this property is defined only in property groups whose conditions
        /// specify a complete and unique proejct configuration. If this assumptions is untrue then
        /// the reported value may not match what MSBuild uses when the project is evaluated.
        /// </remarks>
        public string GetOutputPath(string configuration, string platform)
        {
            return
                   GetPropertyValue(
                       "OutputPath",
                       GetPropertyGroups($"{configuration}|{platform}"));
        }

        private string GetPropertyValue(string propertyName, IEnumerable<XElement> propertyGroups)
        {
            // This method is naive in that it will ignore conditioned property assignments and the
            // order of evaluation MSBuild uses.

            return
                propertyGroups
                .SelectMany(pg => pg.Elements(XName.Get(propertyName, _defaultXmlNamespace)))
                .FirstOrDefault()?.Value;
        }

        private IEnumerable<XElement> GetUnconditionedPropertyGroups()
        {
            return GetPropertyGroups(pg => pg.Attribute("Condition") == null);
        }

        private IEnumerable<XElement> GetPropertyGroups(string configuration)
        {
            return GetPropertyGroups(p =>
            {
                var condition = p.Attribute("Condition");
                if (condition == null) { return false; }
                var match = Regex.Match(condition.Value, CONFIGURATION_PROPERTYGROUP_PATTERN);
                if (!match.Success) { return false; }
                return match.Groups["configuration"].Value == configuration;
            });
        }

        private IEnumerable<XElement> GetPropertyGroups(Func<XElement, bool> filter)
        {
            var propertyGroupName = XName.Get("PropertyGroup", _defaultXmlNamespace);
            return _projectContent.Root.Elements(propertyGroupName).Where(filter);
        }
    }
}
