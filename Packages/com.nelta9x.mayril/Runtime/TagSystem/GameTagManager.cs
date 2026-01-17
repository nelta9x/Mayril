using System.Collections.Generic;
using UnityEngine;

namespace Mayril.TagSystem
{
    /// <summary>
    /// 게임 태그 관리자.
    /// 해시와 태그 이름 간의 매핑을 관리합니다.
    /// </summary>
    public static class GameTagManager
    {
        private static readonly Dictionary<int, string> _hashToNameMap = new();
        // TagId -> Set of Parent TagIds (Self is included)
        private static readonly Dictionary<int, HashSet<int>> _parentTagsMap = new();

        /// <summary>
        /// 태그를 등록하고 해시를 반환합니다.
        /// </summary>
        public static int GetTagId(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return 0;

            int hash = StringToHash(tagName);
            if (!_hashToNameMap.ContainsKey(hash))
            {
                _hashToNameMap[hash] = tagName;
                RegisterParentTags(tagName, hash);
            }
            return hash;
        }

        /// <summary>
        /// 부모 태그 관계를 미리 계산하여 캐싱합니다.
        /// 예: "State.Debuff.Stun" -> ["State", "State.Debuff", "State.Debuff.Stun"]
        /// </summary>
        private static void RegisterParentTags(string tagName, int tagHash)
        {
            if (!_parentTagsMap.ContainsKey(tagHash))
            {
                _parentTagsMap[tagHash] = new HashSet<int>();
            }

            // 자기 자신도 포함 (MatchesTag는 자기 자신에 대해서도 true여야 함)
            _parentTagsMap[tagHash].Add(tagHash);

            // "." 단위로 쪼개서 부모 태그들 등록
            // 문자열 조작이 발생하지만, 이는 태그 '최초 등록 시'에만 1회 발생하므로 런타임 성능에 영향 없음.
            int lastDotIndex = tagName.LastIndexOf('.');
            while (lastDotIndex != -1)
            {
                string parentTagName = tagName.Substring(0, lastDotIndex);
                int parentHash = StringToHash(parentTagName);

                _parentTagsMap[tagHash].Add(parentHash);

                // 재귀적으로 부모의 부모도 등록할 필요 없이, 
                // 위 루프에서 문자열을 줄여가며 등록하므로 모든 조상이 등록됨.

                // 부모 태그 이름도 매핑에 등록 (디버깅용)
                if (!_hashToNameMap.ContainsKey(parentHash))
                {
                    _hashToNameMap[parentHash] = parentTagName;
                }

                lastDotIndex = tagName.LastIndexOf('.', lastDotIndex - 1);
            }
        }

        /// <summary>
        /// 해시로부터 태그 이름을 가져옵니다.
        /// </summary>
        public static string GetTagName(int tagId)
        {
            if (tagId == 0) return "";
            return _hashToNameMap.TryGetValue(tagId, out var name) ? name : $"UnknownTag({tagId})";
        }

        /// <summary>
        /// checkTag가 otherTag와 같거나, otherTag의 자손인지 확인합니다.
        /// (즉, checkTag가 otherTag를 포함하는지 확인)
        /// 예: checkTag="State.Debuff.Stun", otherTag="State" -> True
        /// </summary>
        public static bool CheckTagMatch(int checkTagId, int otherTagId)
        {
            if (checkTagId == 0 || otherTagId == 0) return false;

            // 정확히 일치
            if (checkTagId == otherTagId) return true;

            // 계층 구조 확인 (O(1) lookup)
            if (_parentTagsMap.TryGetValue(checkTagId, out var parents))
            {
                // checkTag의 부모 목록(조상들) 중에 otherTagId가 있는지 확인
                return parents.Contains(otherTagId);
            }

            return false;
        }

        /// <summary>
        /// 해당 태그의 모든 상위 태그(자신 포함) ID 목록을 반환합니다.
        /// </summary>
        public static IEnumerable<int> GetParentTagIds(int tagId)
        {
            if (tagId == 0) return System.Array.Empty<int>();

            if (_parentTagsMap.TryGetValue(tagId, out var parents))
            {
                return parents;
            }
            return System.Array.Empty<int>();
        }

        // --- Hashing (FNV-1a implementation) ---

        private const uint FnvPrime = 16777619;
        private const uint OffsetBasis = 2166136261;

        /// <summary>
        /// 문자열에 대해 FNV-1a(32-bit) 해시를 계산합니다.
        /// </summary>
        public static int StringToHash(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            uint hash = OffsetBasis;
            for (int i = 0; i < str.Length; i++)
            {
                hash ^= str[i];
                hash *= FnvPrime;
            }

            return unchecked((int)hash);
        }
    }
}
