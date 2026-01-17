using System.Collections.Generic;
using System.Linq;

namespace Mayril.TagSystem
{
    /// <summary>
    /// 게임 태그 컨테이너.
    /// 태그들의 집합을 관리하고, 태그 검사 기능을 제공합니다.
    /// </summary>
    public class GameTagContainer
    {
        // TagId -> Count
        private readonly Dictionary<int, int> _tagCounts = new();

        /// <summary>
        /// 태그 개수 (고유 태그 수).
        /// 암시적으로 포함된 부모 태그들도 포함됩니다.
        /// </summary>
        public int Count => _tagCounts.Count;

        /// <summary>
        /// 태그 목록.
        /// 암시적으로 포함된 부모 태그들도 포함됩니다.
        /// </summary>
        public IEnumerable<GameTag> Tags => _tagCounts.Keys.Select(id => new GameTag(id));

        /// <summary>
        /// 기본 생성자.
        /// </summary>
        public GameTagContainer()
        {
        }

        /// <summary>
        /// 초기 태그 목록으로 생성합니다.
        /// </summary>
        public GameTagContainer(IEnumerable<GameTag> tags)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    AddTag(tag);
                }
            }
        }

        /// <summary>
        /// 태그를 추가합니다.
        /// 해당 태그와 모든 상위 태그의 카운트를 증가시킵니다.
        /// </summary>
        public void AddTag(GameTag tag)
        {
            if (!tag.IsValid) return;

            // 계층 구조 확장 (자신 포함 모든 부모)
            foreach (int id in GameTagManager.GetParentTagIds(tag.Id))
            {
                if (_tagCounts.TryGetValue(id, out int count))
                {
                    _tagCounts[id] = count + 1;
                }
                else
                {
                    _tagCounts[id] = 1;
                }
            }
        }

        /// <summary>
        /// 태그를 제거합니다.
        /// 해당 태그와 모든 상위 태그의 카운트를 감소시킵니다.
        /// </summary>
        public void RemoveTag(GameTag tag)
        {
            if (!tag.IsValid) return;

            foreach (int id in GameTagManager.GetParentTagIds(tag.Id))
            {
                if (_tagCounts.TryGetValue(id, out int count))
                {
                    if (count > 1)
                    {
                        _tagCounts[id] = count - 1;
                    }
                    else
                    {
                        _tagCounts.Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// 태그 목록을 추가합니다.
        /// </summary>
        public void AddTags(IEnumerable<GameTag> tags)
        {
            if (tags == null) return;
            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        /// <summary>
        /// 태그 목록을 제거합니다.
        /// </summary>
        public void RemoveTags(IEnumerable<GameTag> tags)
        {
            if (tags == null) return;
            foreach (var tag in tags)
            {
                RemoveTag(tag);
            }
        }

        /// <summary>
        /// 특정 태그를 포함하고 있는지 확인합니다.
        /// 상위 태그를 검사해도 하위 태그가 있다면 True를 반환합니다.
        /// </summary>
        public bool HasTag(GameTag tag)
        {
            if (!tag.IsValid) return false;
            return _tagCounts.ContainsKey(tag.Id);
        }

        /// <summary>
        /// 주어진 태그들 중 하나라도 포함하고 있는지 확인합니다.
        /// </summary>
        public bool HasAny(IEnumerable<GameTag> otherTags)
        {
            if (otherTags == null) return false;
            foreach (var tag in otherTags)
            {
                if (HasTag(tag))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 주어진 태그들을 모두 포함하고 있는지 확인합니다.
        /// </summary>
        public bool HasAll(IEnumerable<GameTag> otherTags)
        {
            if (otherTags == null) return true;
            foreach (var tag in otherTags)
            {
                if (!HasTag(tag))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 컨테이너를 비웁니다.
        /// </summary>
        public void Clear()
        {
            _tagCounts.Clear();
        }
    }
}
