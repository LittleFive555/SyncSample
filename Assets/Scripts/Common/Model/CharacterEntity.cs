using System;

namespace SyncSample.Common.Model
{
    /// <summary>
    /// 角色实体（服务器权威状态）；字段名与 JsonUtility 序列化一致。
    /// </summary>
    [Serializable]
    public class CharacterEntity
    {
        public string id;
        public string name;
        public float x;
        public float y;
        public float dx;
        public float dy;
    }
}