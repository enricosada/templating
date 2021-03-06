﻿using System.Collections.Generic;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Core;
using Microsoft.TemplateEngine.Core.Contracts;
using Microsoft.TemplateEngine.Core.Expressions.Cpp;

namespace Microsoft.TemplateEngine.Orchestrator.RunnableProjects
{
    public class CreationPath : ICreationPath
    {
        public string Path { get; set; }

        public static IReadOnlyList<ICreationPath> ListFromModel(IReadOnlyList<ICreationPathModel> modelList, IVariableCollection rootVariableCollection)
        {
            List<ICreationPath> pathList = new List<ICreationPath>();

            if (rootVariableCollection == null)
            {
                rootVariableCollection = new VariableCollection();
            }

            foreach (ICreationPathModel model in modelList)
            {
                // Note: this check is probably superfluous. The Model has evaluation info.
                // OTOH: this is probaby a cleaner way to do it. 
                if (string.IsNullOrEmpty(model.Condition)
                    || CppStyleEvaluatorDefinition.EvaluateFromString(model.Condition, rootVariableCollection))
                {
                    ICreationPath path = new CreationPath()
                    {
                        Path = model.PathResolved
                    };
                    pathList.Add(path);
                }
            }

            return pathList;
        }
    }
}
