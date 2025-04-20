using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VisualNovel.Mechanics
{
    public class CommandManager : MonoBehaviour
    {
        private Dictionary<string, Delegate> _registeredCommands = new Dictionary<string, Delegate>();

        public static CommandManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterCommand(string commandName, Delegate command)
        {
            commandName = commandName.ToLower();

            if (_registeredCommands.ContainsKey(commandName))
            {
                Debug.LogWarning("���ݿ��Ѿ�������ָͬ������ظ�������" + commandName);
                return;
            }
            else
            {
                _registeredCommands.Add(commandName, command);
            }
        }

        public bool HasCommand(string commandName)
        {
            return _registeredCommands.ContainsKey(commandName.ToLower());
        }

        public Delegate GetCommand(string commandName)
        {
            commandName= commandName.ToLower();

            if (!_registeredCommands.ContainsKey(commandName))
            {
                Debug.LogWarning(commandName + "��ָ�������ݿ��в������ڣ�");
                return null;
            }
            else
            {
                return _registeredCommands[commandName];
            }

        }

        public Coroutine ExecuteCommand(string commandName, params string[] args)
        {
            Delegate command = GetCommand(commandName);
            if (command == null)
            {
                Debug.LogError("����Ϊ�գ�" + commandName);
                return null;
            }
            Coroutine commandRunningProcess = null;

            if (command is Action)
            {
                command.DynamicInvoke();
            }
            else if (command is Action<string>)
            {
                command.DynamicInvoke(args[0]);
            }
            else if (command is Action<float>)
            {
                float[] parsedArgs = new float[args.Length];
                if (TryCastParameters(out parsedArgs, args))
                {
                    command.DynamicInvoke(parsedArgs[0]);
                }
            }
            else if (command is Func<Coroutine>)
            {
                 commandRunningProcess = (Coroutine)command.DynamicInvoke();
            }
            else if (command is Func<float, float, Coroutine>)
            {
                float[] parsedArgs = new float[args.Length];
                if (TryCastParameters(out parsedArgs, args))
                {
                    commandRunningProcess = (Coroutine)command.DynamicInvoke(parsedArgs[0], parsedArgs[1]);
                }
            }

            return commandRunningProcess;
        }

        private bool TryCastParameters<T>(out T[] valueArray, params string[] rawArgs)
        {
            bool isParsingSuccess = true;
            valueArray = default;// �����out�������Ĳ�����ֵ
            if (typeof(T) == typeof(float))
            {
                float[] floatArray = new float[rawArgs.Length];
                for (int i = 0; i < rawArgs.Length; i++)
                {
                    float fValue;
                    if (float.TryParse(rawArgs[i], out fValue))
                    {
                        floatArray[i] = fValue;
                    }
                    else
                    {
                        Debug.LogError("��һ������Ĳ���Parseʧ�ܣ����ͣ�Float");
                        isParsingSuccess = false;
                        break;
                    }
                }
                valueArray = floatArray as T[];
            }

            return isParsingSuccess;
        }
    }
}