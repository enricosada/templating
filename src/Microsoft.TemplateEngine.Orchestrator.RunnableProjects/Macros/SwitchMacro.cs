﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Core.Contracts;
using Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Macros.Config;
using Microsoft.TemplateEngine.Core.Operations;
using Newtonsoft.Json.Linq;

namespace Microsoft.TemplateEngine.Orchestrator.RunnableProjects.Macros
{
    internal class SwitchMacro : IMacro, IDeferredMacro
    {
        public Guid Id => new Guid("B57D64E0-9B4F-4ABE-9366-711170FD5294");

        public string Type => "switch";

        public static readonly string DefaultEvaluator = "C++";

        public void EvaluateConfig(IVariableCollection vars, IMacroConfig rawConfig, IParameterSet parameters, ParameterSetter setter)
        {
            SwitchMacroConfig config = rawConfig as SwitchMacroConfig;

            if (config == null)
            {
                throw new InvalidCastException("Couldn't cast the rawConfig as SwitchMacroConfig");
            }

            ConditionEvaluator evaluator = EvaluatorSelector.Select(config.Evaluator);
            string result = string.Empty;   // default if no condition assigns a value

            foreach (KeyValuePair<string, string> switchInfo in config.Switches)
            {
                string condition = switchInfo.Key;
                string value = switchInfo.Value;

                if (string.IsNullOrEmpty(condition))
                {   // no condition, this is the default.
                    result = value;
                    break;
                }
                else
                {
                    byte[] conditionBytes = Encoding.UTF8.GetBytes(condition);
                    int length = conditionBytes.Length;
                    int position = 0;
                    IProcessorState state = new GlobalRunSpec.ProcessorState(vars, conditionBytes, Encoding.UTF8);
                    bool faulted;

                    if (evaluator(state, ref length, ref position, out faulted))
                    {
                        result = value;
                        break;
                    }
                }
            }

            Parameter p = new Parameter
            {
                IsVariable = true,
                Name = config.VariableName,
                DataType = config.DataType
            };
            setter(p, result.ToString());
        }

        public void EvaluateDeferredConfig(IVariableCollection vars, IMacroConfig rawConfig, IParameterSet parameters, ParameterSetter setter)
        {
            GeneratedSymbolDeferredMacroConfig deferredConfig = rawConfig as GeneratedSymbolDeferredMacroConfig;

            if (deferredConfig == null)
            {
                throw new InvalidCastException("Couldn't cast the rawConfig as a SwitchMacroConfig");
            }

            JToken evaluatorToken;
            string evaluator = null;
            if (deferredConfig.Parameters.TryGetValue("evaluator", out evaluatorToken))
            {
                evaluator = evaluatorToken.ToString();
            }

            JToken dataTypeToken;
            string dataType = null;
            if (deferredConfig.Parameters.TryGetValue("datatype", out dataTypeToken))
            {
                dataType = dataTypeToken.ToString();
            }

            JToken switchListToken;
            List<KeyValuePair<string, string>> switchList = new List<KeyValuePair<string, string>>();
            if (deferredConfig.Parameters.TryGetValue("cases", out switchListToken))
            {
                JArray switchJArray = (JArray)switchListToken;
                foreach (JToken switchInfo in switchJArray)
                {
                    JObject map = (JObject)switchInfo;
                    string condition = map.ToString("condition");
                    string value = map.ToString("value");
                    switchList.Add(new KeyValuePair<string, string>(condition, value));
                }
            }

            IMacroConfig realConfig = new SwitchMacroConfig(deferredConfig.VariableName, evaluator, dataType, switchList);
            EvaluateConfig(vars, realConfig, parameters, setter);
        }
    }
}
