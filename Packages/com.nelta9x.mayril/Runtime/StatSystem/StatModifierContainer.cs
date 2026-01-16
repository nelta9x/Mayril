using System.Collections.Generic;

namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯 모디파이어 컨테이너.
    /// 모든 종류의 모디파이어들을 관리합니다.
    /// </summary>
    public class StatModifierContainer
    {
        // 최적화를 위해 단일 리스트로 관리 (메모리 할당 감소)
        // 외부에서 직접 접근할 필요가 없으므로 private으로 변경 (캡슐화 강화)
        private readonly List<StatModifier> _modifiers = new();

        /// <summary>
        /// 기반 값에 모디파이어를 적용했을 때의 값을 계산합니다.
        /// 계산 순서: (Base + Additive) * (1 + Multiplicative) + Fixed
        /// </summary>
        public float Apply(float baseValue)
        {
            float additiveSum = 0f;
            float multiplicativeSum = 0f;
            float fixedSum = 0f;

            // 단일 루프로 모든 모디파이어 처리 (LINQ 제거 및 순회 최적화)
            foreach (var modifier in _modifiers)
            {
                switch (modifier.Type)
                {
                    case StatModifierType.Additive:
                        additiveSum += modifier.Value;
                        break;
                    case StatModifierType.Multiplicative:
                        multiplicativeSum += modifier.Value;
                        break;
                    case StatModifierType.Fixed:
                        fixedSum += modifier.Value;
                        break;
                }
            }

            return (baseValue + additiveSum) * (1f + multiplicativeSum) + fixedSum;
        }

        /// <summary>
        /// 모디파이어를 추가합니다.
        /// </summary>
        public void Add(StatModifier modifier)
        {
            _modifiers.Add(modifier);
        }

        /// <summary>
        /// 모디파이어를 제거합니다.
        /// </summary>
        public bool Remove(StatModifier modifier)
        {
            return _modifiers.Remove(modifier);
        }

        /// <summary>
        /// 모디파이어들을 제거합니다.
        /// </summary>
        public void Clear()
        {
            _modifiers.Clear();
        }
    }
}