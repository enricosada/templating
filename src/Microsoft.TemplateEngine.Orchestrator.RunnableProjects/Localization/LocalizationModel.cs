﻿using System;
using System.Collections.Generic;
using Microsoft.TemplateEngine.Abstractions;

namespace Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Localization
{
    public class LocalizationModel : ILocalizationModel
    {
        public string Author { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        // Identifies the langpack association with an actual template. They'll both have the same identity.
        // This is not localized info, more like a key
        public string Identity { get; set; }

        // symbol name -> localized description
        public IReadOnlyDictionary<string, string> SymbolDescriptions { get; set; }

        public IReadOnlyDictionary<Guid, IPostActionLocalizationModel> PostActions { get; set; }

        public IReadOnlyList<IFileLocalizationModel> FileLocalizations { get; set; }
    }
}
