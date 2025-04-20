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
        private readonly char _dialogueStartIdentifier = '��';
        private readonly char _dialogueEndIdentifier = '��';
        private readonly string _commandRegexPattern = @"\#[\w]+\([^)]*\)";
        /* ����ַ���ʽ����ƥ���������͵��ı���
         * �� # Ϊ��ͷ�ķ����������һ��������ĸ�����ֻ��»����ַ���[\w]+���������������ġ�
         * �����һ��Ӣ��������(
         * ���ſ��԰���������������ַ��������ո񣬵������������� )������ʹ�� [^)]* ��ʾ�ġ�
         * ����������� ) ��β��
         * ���ģʽ��������ƥ������ #Abc_Function( parameter1, parameter2 ) ֮����ַ�����*/

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

            // �ȳ��Զ�ȡ�ı����е������ȡ���֮���ԭ����ժ��
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

            // �ٳ��Զ�ȡ̨�ʣ��������������˫���š�����������ֱ�������š�����֮ǰΪ˵���ߵ��������ӡ�������������֮��Ϊ̨�ʵ�����
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

            // ���ض�ȡ�������������������ǿ�ֵ
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