//*********************************************************  
//  
// Copyright (c) Microsoft. All rights reserved.  
// This code is licensed under the MIT License (MIT).  
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY  
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR  
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.  
//  
//*********************************************************  

using System.Collections.Generic;
using System.Diagnostics;

namespace TestCmdLine
{
    public enum ParsedCommandType
    {
        Unknown,
        SelectItem,
        LoadConfig,
        LoadFile,
    }

    public class ParsedCommand
    {
        public ParsedCommandType Type { get; set; }
        public string Payload { get; set; }
    }

    public class ParsedCommands : List<ParsedCommand>
    {
    }

    public class CommandLineParser
    {
        public static List<KeyValuePair<string, string>> ParsedArgs { get; private set; }

        public static void Parse(string argString = null)
        {
            string[] args = argString.Split(' ');
            if (ParsedArgs == null)
            {
                ParsedArgs = new List<KeyValuePair<string, string>>();
            }
            else
            {
                ParsedArgs.Clear();
            }

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-") || args[i].StartsWith("/"))
                    {
                        var data = ParseData(args, i);
                        if (data.Key != null)
                        {
                            for (int j = 0; j < ParsedArgs.Count; j++)
                            {
                                if (ParsedArgs[j].Key == data.Key)
                                {
                                    ParsedArgs.RemoveAt(j);
                                }
                            }
                            ParsedArgs.Add(data);
                        }
                    }
                }
            }
        }

        private static KeyValuePair<string, string> ParseData(string[] args, int index)
        {
            string key = null;
            string val = null;
            if (args[index].StartsWith("-") || args[index].StartsWith("/"))
            {
                if (args[index].Contains(":"))
                {
                    string argument = args[index];
                    int endIndex = argument.IndexOf(':');
                    key = argument.Substring(1, endIndex -1);   // trim the '/' and the ':'.
                    int valueStart = endIndex + 1;
                    val = valueStart < argument.Length ? argument.Substring(
                        valueStart, argument.Length - valueStart) : null;
                }
                else
                {
                    key = args[index];
                    int argIndex = 1 + index;
                    if (argIndex < args.Length && !(args[argIndex].StartsWith("-") || args[argIndex].StartsWith("/")))
                    {
                        val = args[argIndex];
                    }
                    else
                    {
                        val = null;
                    }
                }
            }

            return key != null ? new KeyValuePair<string, string>(key, val) : default(KeyValuePair<string, string>);
        }

        public static bool IsFiniteOperation;

        public static ParsedCommands ParseUntrustedArgs(string cmdLineString)
        {
            ParsedCommands commands = new ParsedCommands();
            Parse(cmdLineString);
            foreach (KeyValuePair<string, string> kvp in ParsedArgs)
            {
                Debug.WriteLine("arg {0} = {1}", kvp.Key, kvp.Value);
                ParsedCommand command = new ParsedCommand();
                switch (kvp.Key)
                {
                    case "SelectItem":
                        command.Type = ParsedCommandType.SelectItem;
                        break;
                    case "LoadConfig":
                        command.Type = ParsedCommandType.LoadConfig;
                        break;
                    case "LoadFile":
                        command.Type = ParsedCommandType.LoadFile;
                        break;
                    default:
                        command.Type = ParsedCommandType.Unknown;
                        break;
                }
                command.Payload = kvp.Value;
                commands.Add(command);
            }
            return commands;
        }

    }
}
