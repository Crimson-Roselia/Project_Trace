using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VisualNovel.Mechanics.Data
{
    public class DialogueLine
    {
        public string SpeakerName;
        public string DialogueText;
        public CommandDataContainer[] CommandsData;
        private readonly char _dialogueStartIdentifier = '“';
        private readonly char _dialogueEndIdentifier = '”';
        private readonly string _commandRegexPattern = @"\#[\w]+\([^)]*\)";
        /* 这个字符样式允许匹配以下类型的文本：
         * 以 # 为开头的方法名，后跟一个或多个字母、数字或下划线字符（[\w]+）。但不包括中文。
         * 后面跟一个英文左括号(
         * 接着可以包含零个或多个任意字符，包括空格，但不包括右括号 )，这是使用 [^)]* 表示的。
         * 最后以右括号 ) 结尾。
         * 这个模式可以用来匹配形如 #Abc_Function( parameter1, parameter2 ) 之类的字符串。*/

        public bool HasSpeaker()
        {
            return !string.IsNullOrWhiteSpace(SpeakerName);
        }

        public bool HasDialogue()
        {
            return !string.IsNullOrWhiteSpace(DialogueText);
        }

        public bool HasCommand()
        {
            return CommandsData != null && CommandsData.Length > 0;
        }

        public DialogueLine(string rawString)
        {
            (string speaker, string dialogue, string[] commands) = RipContent(rawString);
            SpeakerName = speaker;
            DialogueText = dialogue;
            if (commands != null)
            {
                CommandDataContainer[] commandsData = new CommandDataContainer[commands.Length];
                for (int i = 0; i < commands.Length; i++)
                {
                    commandsData[i] = new CommandDataContainer(commands[i]);
                }
                CommandsData = commandsData;
            }
        }

        private (string, string, string[]) RipContent(string rawLine)
        {
            string speaker = "", dialogue = "";
            string[] commands = null;

            // 先尝试读取文本当中的命令，读取完毕之后从原句中摘除
            Regex commandRegex = new Regex(_commandRegexPattern);
            MatchCollection matches = commandRegex.Matches(rawLine);
            commands = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                commands[i] = matches[i].Value;
            }
            for (int j = matches.Count -1; j >= 0; j--)
            {
                rawLine = rawLine.Remove(matches[j].Index, matches[j].Length);
            }

            // 再尝试读取台词，如果发现有中文双引号‘“”’，则直到上引号‘“’之前为说话者的姓名，从‘“’到‘”’之间为台词的内容
            int dialogueStart = -1;
            int dialogueEnd = -1;
            for (int i = 0; i < rawLine.Length; i++)
            {
                char current = rawLine[i];
                if (current == _dialogueStartIdentifier)
                {
                    dialogueStart = i;
                }
                else if (current == _dialogueEndIdentifier)
                {
                    dialogueEnd = i;
                }
            }

            if (dialogueStart > -1 && dialogueEnd > dialogueStart)
            {
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Trim();
            }

            // 返回读取结果，其中任意项可能是空值
            return (speaker, dialogue, commands);
        }
    }

    public class CommandDataContainer
    {
        public string Name;
        public string[] Arguments;
        private readonly char _argumentsStartIdentifier = '(';
        private readonly char _argumentsEndIdentifier = ')';
        private readonly char _argumentsSpliter = ',';

        public CommandDataContainer(string rawCommand)
        {
            (string name, string[] arguements) = ParseAndRipCommand(rawCommand);
            Name = name;
            Arguments = arguements;
        }

        private (string, string[]) ParseAndRipCommand(string rawCommand)
        {
            int argumentsStart = -1;
            int argumentsEnd = -1;
            for (int i = 0; i < rawCommand.Length; i++)
            {
                char current = rawCommand[i];
                if (current == _argumentsStartIdentifier)
                {
                    argumentsStart = i;
                }
                else if (current == _argumentsEndIdentifier)
                {
                    argumentsEnd = i;
                }
            }

            string name = rawCommand.Substring(0, argumentsStart).Trim('#', ' ');
            string[] rawArguments = rawCommand.Substring(argumentsStart, argumentsEnd - argumentsStart + 1).Split(_argumentsSpliter, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rawArguments.Length; i++)
            {
                rawArguments[i] = rawArguments[i].Trim(' ', _argumentsStartIdentifier, _argumentsEndIdentifier);
            }

            return (name, rawArguments);
        }

    }
}