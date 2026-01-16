using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Mayril.StatSystem
{
    /// <summary>
    /// 스탯 값 하나를 나타내는 클래스입니다.
    /// </summary>
    public class StatValue : NetworkVariableBase
    {
        private float _baseValue;
        private float _currentValue;
        private IStatSet _stats;
        private readonly StatModifierContainer _modifiers = new();

        /// <summary>
        /// 연결된 스탯.
        /// </summary>
        public IStatSet Stats
        {
            get => _stats;
            set => _stats = value;
        }

        public StatValue(float baseValue)
        {
            Reset(baseValue);
        }

        /// <summary>
        /// 기반 값.
        /// </summary>
        public float BaseValue
        {
            get => _baseValue;
            set
            {
                if (_baseValue == value)
                {
                    return;
                }
                
                _baseValue = value;
                SetDirty(true);
                RecalculateValue();
            }
        }

        /// <summary>
        /// 최종 값.
        /// 기반 값에 모든 모다피아어들을 반영했을 때의 값.
        /// </summary>
        public float CurrentValue => _currentValue;

        /// <summary>
        /// 모든 모디파이어들.
        /// </summary>
        public StatModifierContainer Modifiers => _modifiers;

        /// <summary>
        /// 초기화 시점에 호출됩니다.
        /// </summary>
        public override void OnInitialize()
        {
            _stats = GetBehaviour() as IStatSet;
        }

        /// <summary>
        /// 현재 값을 초기화합니다.
        /// 초기화 시에는 값 변경 관련 이벤트가 호출되지 않습니다.
        /// </summary>
        /// <param name="newBaseValue">새 기반 값.</param>
        public void Reset(float newBaseValue)
        {
            Reset(newBaseValue, Enumerable.Empty<StatModifier>());
        }

        /// <summary>
        /// 현재 값을 초기화합니다.
        /// 초기화 시에는 값 변경 관련 이벤트가 호출되지 않습니다.
        /// </summary>
        /// <param name="newBaseValue">새 기반 값.</param>
        /// /// <param name="modifiers">모디파이어들.</param>
        public void Reset(float newBaseValue, IEnumerable<StatModifier> modifiers)
        {
            SetDirty(false);
            _modifiers.Clear();
            _baseValue = newBaseValue;
            _currentValue = newBaseValue;
            foreach (var modifier in modifiers)
            {
                _modifiers.Add(modifier);
            }

            _currentValue = _modifiers.Apply(_baseValue);
        }

        /// <summary>
        /// 모디파이어를 추가합니다.
        /// </summary>
        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            RecalculateValue();
        }

        /// <summary>
        /// 모디파이어를 제거합니다.
        /// </summary>
        public bool RemoveModifier(StatModifier modifier)
        {
            if (!_modifiers.Remove(modifier))
            {
                return false;
            }
            
            RecalculateValue();
            return true;
        }

        /// <summary>
        /// 값을 재계산합니다.
        /// </summary>
        private void RecalculateValue()
        {
            float oldCurrentValue = _currentValue;
            float newCurrentValue = _modifiers.Apply(_baseValue);
            bool isDirty = oldCurrentValue != newCurrentValue; 
            if (isDirty)
            {
                if (_stats != null)
                {
                    // 값이 보정된 이후에도 변경이 되었는지 확인.
                    _stats.OnStatValueChanging(this, ref newCurrentValue);
                    isDirty = oldCurrentValue != newCurrentValue;
                }
            }

            if (!isDirty)
            {
                return;
            }
            
            SetDirty(true);
            _currentValue = newCurrentValue;
            _stats?.OnStatValueChanged(this);
        }

        /// <summary>
        /// 변경된 부분만 직렬화합니다. (델타 동기화용)
        /// 이 메소드 호출 이후, Netcode가 SetDirty를 false로 자동 처리합니다.
        /// </summary>
        public sealed override void WriteDelta(FastBufferWriter writer)
        {
            WriteField(writer);
        }

        /// <summary>
        /// 전체 상태를 직렬화합니다. (초기 동기화용)
        /// </summary>
        public sealed override void WriteField(FastBufferWriter writer)
        {
            writer.WriteValueSafe(_baseValue);
            writer.WriteValueSafe(_currentValue);
        }

        /// <summary>
        /// 전체 상태를 역직렬화합니다. (권한자에서는 호출되지 않음)
        /// </summary>
        public sealed override void ReadField(FastBufferReader reader)
        {
            float oldCurrentValue = _currentValue;
            reader.ReadValueSafe(out _baseValue);
            reader.ReadValueSafe(out _currentValue);
            if (_stats != null && oldCurrentValue != _currentValue)
            {
                _stats.OnStatValueChanged(this);
            }
        }

        /// <summary>
        /// 변경된 부분을 역직렬화합니다. (권한자에서는 호출되지 않음)
        /// </summary>
        public sealed override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            ReadField(reader);
        }
    }
}