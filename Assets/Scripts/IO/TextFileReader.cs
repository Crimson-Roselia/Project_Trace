using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VisualNovel.IO
{
    public class TextFileReader
    {
        private readonly string _textsFolderInResources = "Texts";

        public List<string> ReadTextAsset(string fileName)
        {
            TextAsset asset = Resources.Load<TextAsset>(_textsFolderInResources + $"/{fileName}");
            if (asset == null)
            {
                Debug.LogError($"文本文件读取失败，错误文件名：{fileName}");
                return null;
            }

            List<string> result = new List<string>();

            StringReader reader = new StringReader(asset.text);
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    result.Add(line);
                }
            }
            reader.Close();

            return result;
        }
    }
}