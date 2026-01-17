using System;
using UnityEngine;

namespace Mayril.TagSystem
{
    /// <summary>
    /// 게임 태그.
    /// 계층 구조를 가지는 경량화된 태그 구조체입니다. (int ID 기반)
    /// </summary>
    [System.Serializable]
    public struct GameTag : IEquatable<GameTag>, ISerializationCallbackReceiver
    {
        [SerializeField] private string tagName;
        private int _id;

        public string TagName => tagName;
        public int Id => _id;

        /// <summary>
        /// 태그가 유효한지(Id가 0이 아닌지) 확인합니다.
        /// </summary>
        public bool IsValid => _id != 0;

        public static implicit operator GameTag(string tagName) => new GameTag(tagName);
        public static implicit operator string(GameTag tag) => tag.tagName;

        /// <summary>
        /// 다른 태그와 같은지 비교합니다. (Hash 비교)
        /// </summary>
        public bool Equals(GameTag other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is GameTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            return tagName;
        }

        public static bool operator ==(GameTag left, GameTag right)
        {
            return left._id == right._id;
        }

        public static bool operator !=(GameTag left, GameTag right)
        {
            return left._id != right._id;
        }

        /// <summary>
        /// 이 태그가 other 태그와 매칭되는지 확인합니다.
        /// (자신이 other와 같거나, other의 자손인지 확인)
        /// </summary>
        /// <remarks>
        /// 런타임 성능을 위해 더 이상 문자열 연산(Split, StartsWith)을 하지 않습니다.
        /// <see cref="GameTagManager"/>에 미리 계산된 계층 정보를 사용합니다.
        /// </remarks>
        public bool MatchesTag(GameTag other)
        {
            if (_id == 0 || other._id == 0) return false;
            return GameTagManager.CheckTagMatch(_id, other._id);
        }

        public GameTag(string tagName)
        {
            this.tagName = tagName;
            _id = GameTagManager.GetTagId(tagName);
        }

        internal GameTag(int id)
        {
            _id = id;
            tagName = GameTagManager.GetTagName(id);
        }

        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// 역직렬화 후 Hash를 다시 계산합니다.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(tagName))
            {
                _id = GameTagManager.GetTagId(tagName);
            }
        }
    }
}
