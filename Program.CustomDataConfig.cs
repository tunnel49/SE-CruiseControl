using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class CustomDataConfig
        {
            private Dictionary<string, string> config = new Dictionary<string, string>();
            private List<ConfigOption> configOptions = new List<ConfigOption>();
            private IMyProgrammableBlock pb;

            public CustomDataConfig(IMyProgrammableBlock pb, List<ConfigOption> configOptions)
            {
                this.pb = pb;
                this.configOptions = configOptions;
                if (pb.CustomData == "")
                    InitializeConfig();
                else
                    ParseConfig();
            }

            public T Get<T>(string key)
            {
                string value;
                if (config.TryGetValue(key, out value))
                    return (T)Convert.ChangeType(value, typeof(T));
                return default(T);
            }

            private void ParseConfig()
            {
                string[] lines = pb.CustomData.Split('\n');

                foreach (string line in lines)
                {
                    if (line.Length == 0 || line[0] == '#')
                        continue;
                    var words = line.Split('=');
                    if (words.Length == 2)
                    {
                        string key = words[0].Trim();
                        string value = words[1].Trim();
                        if (key == "" || value == "")
                            throw new ConfigParserException("Unable to parse line: " + line);
                        config[key] = value;
                    }
                    else
                        throw new ConfigParserException("Unable to parse line: " + line);
                }

                ValidateConfig();
            }

            private void ValidateConfig()
            {
                foreach (ConfigOption configOption in configOptions)
                {
                    if (!configOption.required)
                        continue;

                    if (!config.ContainsKey(configOption.key))
                        throw new ConfigParserException("Missing value for required key: " + configOption.key);
                }
            }

            public void InitializeConfig()
            {
                config.Clear();
                StringBuilder configString = new StringBuilder();
                foreach (ConfigOption configOption in configOptions)
                {
                    config[configOption.key] = configOption.value;
                    configString.Append("# " + configOption.description + " " + (configOption.required ? "Required" : "Optional"));
                    configString.Append("\n" + configOption.key + "=" + config[configOption.key] + "\n\n");
                }
                pb.CustomData = configString.ToString();
            }
        }

    }
}
