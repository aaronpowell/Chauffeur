using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Chauffeur.Services
{
    //This to work around the bug http://issues.umbraco.org/issue/U4-4629
    //Most of the stuff is just delegated through to the real packaging service
    //so it's only working around the Macro bug.
    class OverridingPackagingService : IPackagingService
    {
        private readonly IPackagingService realPackagingService;
        private readonly IMacroService macroService;
        private readonly IDataTypeService dataTypeService;
        private readonly IContentTypeService contentTypeService;

        public OverridingPackagingService(
            IPackagingService realPackagingService,
            IMacroService macroService,
            IDataTypeService dataTypeService,
            IContentTypeService contentTypeService)
        {
            this.realPackagingService = realPackagingService;
            this.macroService = macroService;
            this.dataTypeService = dataTypeService;
            this.contentTypeService = contentTypeService;
        }

        public XElement Export(IMacro macro, bool raiseEvents = true)
        {
            return realPackagingService.Export(macro, raiseEvents);
        }

        public XElement Export(IEnumerable<IMacro> macros, bool raiseEvents = true)
        {
            return realPackagingService.Export(macros, raiseEvents);
        }

        public XElement Export(ITemplate template, bool raiseEvents = true)
        {
            return realPackagingService.Export(template, raiseEvents);
        }

        public XElement Export(IEnumerable<ITemplate> templates, bool raiseEvents = true)
        {
            return realPackagingService.Export(templates, raiseEvents);
        }

        public XElement Export(IDataTypeDefinition dataTypeDefinition, bool raiseEvents = true)
        {
            return realPackagingService.Export(dataTypeDefinition, raiseEvents);
        }

        public XElement Export(IEnumerable<IDataTypeDefinition> dataTypeDefinitions, bool raiseEvents = true)
        {
            return realPackagingService.Export(dataTypeDefinitions, raiseEvents);
        }

        public XElement Export(IDictionaryItem dictionaryItem, bool includeChildren, bool raiseEvents = true)
        {
            return realPackagingService.Export(dictionaryItem, includeChildren, raiseEvents);
        }

        public XElement Export(IEnumerable<IDictionaryItem> dictionaryItem, bool includeChildren = true, bool raiseEvents = true)
        {
            return realPackagingService.Export(dictionaryItem, includeChildren, raiseEvents);
        }

        public XElement Export(ILanguage language, bool raiseEvents = true)
        {
            return realPackagingService.Export(language, raiseEvents);
        }

        public XElement Export(IEnumerable<ILanguage> languages, bool raiseEvents = true)
        {
            return realPackagingService.Export(languages, raiseEvents);
        }

        public XElement Export(IMedia media, bool deep = false, bool raiseEvents = true)
        {
            return realPackagingService.Export(media, deep, raiseEvents);
        }

        public XElement Export(IContent content, bool deep = false, bool raiseEvents = true)
        {
            return realPackagingService.Export(content, deep, raiseEvents);
        }

        public XElement Export(IContentType contentType, bool raiseEvents = true)
        {
            return realPackagingService.Export(contentType, raiseEvents);
        }

        public IEnumerable<IContent> ImportContent(XElement element, int parentId = -1, int userId = 0, bool raiseEvents = true)
        {
            return realPackagingService.ImportContent(element, parentId, userId, raiseEvents);
        }

        public IEnumerable<IContentType> ImportContentTypes(XElement element, bool importStructure, int userId = 0, bool raiseEvents = true)
        {
            return realPackagingService.ImportContentTypes(element, importStructure, userId, raiseEvents);
        }

        public IEnumerable<IContentType> ImportContentTypes(XElement element, int userId = 0, bool raiseEvents = true)
        {
            var contentTypes = ImportContentTypes(element, true, userId, raiseEvents);

            PatchContentTypes(contentTypes, element);

            return contentTypes;
        }

        public IEnumerable<IDataTypeDefinition> ImportDataTypeDefinitions(XElement element, int userId = 0, bool raiseEvents = true)
        {
            return realPackagingService.ImportDataTypeDefinitions(element, userId, raiseEvents);
        }

        public IEnumerable<IDictionaryItem> ImportDictionaryItems(XElement dictionaryItemElementList, bool raiseEvents = true)
        {
            return realPackagingService.ImportDictionaryItems(dictionaryItemElementList, raiseEvents);
        }

        public IEnumerable<ILanguage> ImportLanguages(XElement languageElementList, int userId = 0, bool raiseEvents = true)
        {
            return realPackagingService.ImportLanguages(languageElementList, userId, raiseEvents);
        }

        public IEnumerable<ITemplate> ImportTemplates(XElement element, int userId = 0, bool raiseEvents = true)
        {
            return realPackagingService.ImportTemplates(element, userId, raiseEvents);
        }

        public IEnumerable<IMacro> ImportMacros(XElement element, int userId = 0, bool raiseEvents = true)
        {
            //This is basically a copy of the code from Umbraco itself including the patch to fix
            //the service not detecting existing macros

            if (raiseEvents)
            {
                //if (PackagingService.ImportingMacro.IsRaisedEventCancelled(new ImportEventArgs<IMacro>(element), this))
                //    return Enumerable.Empty<IMacro>();
            }

            var name = element.Name.LocalName;
            if (name.Equals("Macros") == false && name.Equals("macro") == false)
            {
                throw new ArgumentException("The passed in XElement is not valid! It does not contain a root element called 'Macros' for multiple imports or 'macro' for a single import.");
            }

            var macroElements = name.Equals("Macros")
                                       ? (from doc in element.Elements("macro") select doc).ToList()
                                       : new List<XElement> { element };

            var macros = macroElements.Select(ParseMacroElement).ToList();

            foreach (var macro in macros)
            {
                var existing = macroService.GetByAlias(macro.Alias);

                if (existing != null)
                    macro.Id = existing.Id;

                macroService.Save(macro, userId);
            }

            //if (raiseEvents)
            //    PackagingService.ImportedMacro.RaiseEvent(new ImportEventArgs<IMacro>(macros, element, false), this);

            return macros;
        }

        private IMacro ParseMacroElement(XElement macroElement)
        {
            var macroName = macroElement.Element("name").Value;
            var macroAlias = macroElement.Element("alias").Value;
            var controlType = macroElement.Element("scriptType").Value;
            var controlAssembly = macroElement.Element("scriptAssembly").Value;
            var xsltPath = macroElement.Element("xslt").Value;
            var scriptPath = macroElement.Element("scriptingFile").Value;

            //Following xml elements are treated as nullable properties
            var useInEditorElement = macroElement.Element("useInEditor");
            var useInEditor = false;
            if (useInEditorElement != null && string.IsNullOrEmpty(useInEditorElement.Value) == false)
            {
                useInEditor = bool.Parse(useInEditorElement.Value);
            }
            var cacheDurationElement = macroElement.Element("refreshRate");
            var cacheDuration = 0;
            if (cacheDurationElement != null && string.IsNullOrEmpty(cacheDurationElement.Value) == false)
            {
                cacheDuration = int.Parse(cacheDurationElement.Value);
            }
            var cacheByMemberElement = macroElement.Element("cacheByMember");
            var cacheByMember = false;
            if (cacheByMemberElement != null && string.IsNullOrEmpty(cacheByMemberElement.Value) == false)
            {
                cacheByMember = bool.Parse(cacheByMemberElement.Value);
            }
            var cacheByPageElement = macroElement.Element("cacheByPage");
            var cacheByPage = false;
            if (cacheByPageElement != null && string.IsNullOrEmpty(cacheByPageElement.Value) == false)
            {
                cacheByPage = bool.Parse(cacheByPageElement.Value);
            }
            var dontRenderElement = macroElement.Element("dontRender");
            var dontRender = true;
            if (dontRenderElement != null && string.IsNullOrEmpty(dontRenderElement.Value) == false)
            {
                dontRender = bool.Parse(dontRenderElement.Value);
            }

            var macro = CreateMacro(macroAlias, macroName, controlType, controlAssembly, xsltPath, scriptPath,
                cacheByPage, cacheByMember, dontRender, useInEditor, cacheDuration);

            var properties = macroElement.Element("properties");
            if (properties != null)
            {
                int sortOrder = 0;
                foreach (var property in properties.Elements())
                {
                    var propertyName = property.Attribute("name").Value;
                    var propertyAlias = property.Attribute("alias").Value;
                    var editorAlias = property.Attribute("propertyType").Value;
                    var sortOrderAttribute = property.Attribute("sortOrder");
                    if (sortOrderAttribute != null)
                    {
                        sortOrder = int.Parse(sortOrderAttribute.Value);
                    }

                    macro.Properties.Add(new MacroProperty(propertyAlias, propertyName, sortOrder, editorAlias));
                    sortOrder++;
                }
            }
            return macro;
        }

        private static IMacro CreateMacro(params object[] arguments)
        {
            //Since Macro is an internal type in at least 7.0.x we need to find it with reflection
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.FullName.Contains("Umbraco.Core"))
                .GetTypes()
                .First(t => t.FullName == "Umbraco.Core.Models.Macro");

            var ctor = type.GetConstructors().Last();

            var instance = ctor.Invoke(arguments);

            return instance as IMacro;
        }

        public string FetchPackageFile(Guid packageId, Version umbracoVersion, int userId)
        {
            return realPackagingService.FetchPackageFile(packageId, umbracoVersion, userId);
        }

        private void PatchContentTypes(IEnumerable<IContentType> contentTypes, XElement element)
        {
            var name = element.Name;

            var docTypeXmlElements = name == "DocumentType" ? new[] { element } : element.Elements("DocumentType");

            foreach (var xml in docTypeXmlElements)
            {
                var infoElement = xml.Element("Info");
                var contentType = contentTypes.First(x => x.Alias == infoElement.Element("Alias").Value);

                var propertiesXml = xml.Element("GenericProperties").Elements("GenericProperty");

                foreach (var property in propertiesXml)
                {
                    var sortOrder = 0;
                    var sortOrderElement = property.Element("SortOrder");
                    if (sortOrderElement != null)
                        int.TryParse(sortOrderElement.Value, out sortOrder);

                    var propertyType = contentType.PropertyTypes.First(pt => pt.Alias == property.Element("Alias").Value);
                    var dataTypeDefinition = GetDataTypeDefinition(property);
                    propertyType.DataTypeDefinitionId = dataTypeDefinition.Id;

                    propertyType.Name = property.Element("Name").Value;
                    propertyType.Description = (string)property.Element("Description");
                    propertyType.Mandatory = (property.Element("Mandatory") != null && property.Element("Mandatory").Value.ToLowerInvariant().Equals("true"));
                    propertyType.ValidationRegExp = (string)property.Element("Validation");
                    propertyType.SortOrder = sortOrder;
                }

                contentTypeService.Save(contentType);
            }
        }

        private IDataTypeDefinition GetDataTypeDefinition(XElement property)
        {
            var dataTypeDefinitionId = new Guid(property.Element("Definition").Value);
            var dataTypeDefinition = dataTypeService.GetDataTypeDefinitionById(dataTypeDefinitionId);

            var legacyPropertyEditorId = Guid.Empty;
            Guid.TryParse(property.Element("Type").Value, out legacyPropertyEditorId);
            var propertyEditorAlias = property.Element("Type").Value.Trim();
            if (dataTypeDefinition == null)
            {
                var dataTypeDefinitions = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(propertyEditorAlias);
                if (dataTypeDefinitions != null && dataTypeDefinitions.Any())
                    dataTypeDefinition = dataTypeDefinitions.FirstOrDefault();
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (legacyPropertyEditorId != Guid.Empty && dataTypeDefinition.ControlId != legacyPropertyEditorId)
#pragma warning restore CS0618 // Type or member is obsolete
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    var dataTypeDefinitions2 = dataTypeService.GetDataTypeDefinitionByControlId(legacyPropertyEditorId);
#pragma warning restore CS0618 // Type or member is obsolete
                    if (dataTypeDefinitions2 != null && dataTypeDefinitions2.Any())
                    {
                        dataTypeDefinition = dataTypeDefinitions2.FirstOrDefault();
                    }
                }
                else
                {
                    if (dataTypeDefinition.PropertyEditorAlias != propertyEditorAlias)
                    {
                        var dataTypeDefinitions3 = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias(propertyEditorAlias);
                        if (dataTypeDefinitions3 != null && dataTypeDefinitions3.Any())
                            dataTypeDefinition = dataTypeDefinitions3.FirstOrDefault();
                    }
                }
            }

            if (dataTypeDefinition == null)
                dataTypeDefinition = dataTypeService.GetDataTypeDefinitionByPropertyEditorAlias("Umbraco.NoEdit").FirstOrDefault();
            return dataTypeDefinition;
        }
    }
}
