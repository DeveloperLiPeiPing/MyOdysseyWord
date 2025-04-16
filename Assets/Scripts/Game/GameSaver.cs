using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Odyssey.Script
{

     [AddComponentMenu("Game/Game Saver")]
    public class GameSaver : Singleton<GameSaver>
    {
        /// <summary>
        /// 数据保存模式枚举
        /// </summary>
        public enum Mode { Binary, JSON, PlayerPrefs }

        [Header("保存设置")]
        public Mode mode = Mode.Binary;          // 当前使用的保存模式
        public string fileName = "save";          // 保存文件的基础名称
        public string binaryFileExtension = "data"; // 二进制文件扩展名

        /// <summary>
        /// 存档槽位总数
        /// </summary>
        protected static readonly int TotalSlots = 5;

        /// <summary>
        /// 保存游戏数据到指定槽位
        /// </summary>
        /// <param name="data">要保存的游戏数据</param>
        /// <param name="index">槽位索引(0-4)</param>
        public virtual void Save(GameData data, int index)
        {
            switch (mode)
            {
                default:
                case Mode.Binary:
                    SaveBinary(data, index);  // 二进制格式保存
                    break;
                case Mode.JSON:
                    SaveJSON(data, index);    // JSON格式保存
                    break;
                case Mode.PlayerPrefs:
                    SavePlayerPrefs(data, index); // PlayerPrefs保存
                    break;
            }
        }

        /// <summary>
        /// 从指定槽位加载游戏数据
        /// </summary>
        /// <param name="index">槽位索引(0-4)</param>
        /// <returns>加载的游戏数据，如不存在返回null</returns>
        public virtual GameData Load(int index)
        {
            switch (mode)
            {
                default:
                case Mode.Binary:
                    return LoadBinary(index);
                case Mode.JSON:
                    return LoadJSON(index);
                case Mode.PlayerPrefs:
                    return LoadPlayerPrefs(index);
            }
        }

        /// <summary>
        /// 删除指定槽位的存档数据
        /// </summary>
        /// <param name="index">槽位索引(0-4)</param>
        public virtual void Delete(int index)
        {
            switch (mode)
            {
                default:
                case Mode.Binary:
                case Mode.JSON:
                    DeleteFile(index);    // 删除存档文件
                    break;
                case Mode.PlayerPrefs:
                    DeletePlayerPrefs(index); // 删除PlayerPrefs数据
                    break;
            }
        }

        /// <summary>
        /// 加载所有槽位的游戏数据
        /// </summary>
        /// <returns>包含所有槽位数据的数组</returns>
        public virtual GameData[] LoadList()
        {
            var list = new GameData[TotalSlots];

            for (int i = 0; i < TotalSlots; i++)
            {
                var data = Load(i);

                if (data != null)
                {
                    list[i] = data;
                }
            }

            return list;
        }

        /// <summary>
        /// 使用二进制格式保存数据
        /// </summary>
        protected virtual void SaveBinary(GameData data, int index)
        {
            var path = GetFilePath(index);
            var formatter = new BinaryFormatter();  // 二进制格式化器
            var stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);  // 序列化数据
            stream.Close();
        }

        /// <summary>
        /// 从二进制文件加载数据
        /// </summary>
        protected virtual GameData LoadBinary(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                var formatter = new BinaryFormatter();
                var stream = new FileStream(path, FileMode.Open);
                var data = formatter.Deserialize(stream);  // 反序列化数据
                stream.Close();
                return data as GameData;
            }

            return null;
        }

        /// <summary>
        /// 使用JSON格式保存数据
        /// </summary>
        protected virtual void SaveJSON(GameData data, int index)
        {
            var json = data.ToJson();  // 转换为JSON字符串
            var path = GetFilePath(index);
            File.WriteAllText(path, json);  // 写入文件
        }

        /// <summary>
        /// 从JSON文件加载数据
        /// </summary>
        protected virtual GameData LoadJSON(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);  // 读取JSON字符串
                return GameData.FromJson(json);     // 从JSON解析
            }

            return null;
        }

        /// <summary>
        /// 删除存档文件
        /// </summary>
        protected virtual void DeleteFile(int index)
        {
            var path = GetFilePath(index);

            if (File.Exists(path))
            {
                File.Delete(path);  // 删除文件
            }
        }

        /// <summary>
        /// 使用PlayerPrefs保存数据
        /// </summary>
        protected virtual void SavePlayerPrefs(GameData data, int index)
        {
            var json = data.ToJson();
            var key = index.ToString();
            PlayerPrefs.SetString(key, json);  // 保存到PlayerPrefs
        }

        /// <summary>
        /// 从PlayerPrefs加载数据
        /// </summary>
        protected virtual GameData LoadPlayerPrefs(int index)
        {
            var key = index.ToString();

            if (PlayerPrefs.HasKey(key))
            {
                var json = PlayerPrefs.GetString(key);  // 从PlayerPrefs读取
                return GameData.FromJson(json);
            }

            return null;
        }

        /// <summary>
        /// 删除PlayerPrefs中的存档数据
        /// </summary>
        protected virtual void DeletePlayerPrefs(int index)
        {
            var key = index.ToString();

            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);  // 删除PlayerPrefs键值
            }
        }

        /// <summary>
        /// 获取存档文件完整路径
        /// </summary>
        /// <param name="index">槽位索引</param>
        /// <returns>文件完整路径</returns>
        protected virtual string GetFilePath(int index)
        {
            var extension = mode == Mode.JSON ? "json" : binaryFileExtension;
            return Application.persistentDataPath + $"/{fileName}_{index}.{extension}";
        }
    }
}