using System;

namespace Mayril.AbilitySystem
{
    /// <summary>
    /// 어빌리티의 행동자.
    /// </summary>
    [Flags]
    public enum AbilityBehavior
    {
        None,
        Hidden, // Hud 상에서 표현되지 않아야 함.
        NoTarget, // 타겟 없이 발동. (예: 캐스터 주변에 피해 주기)
        Target, // 캐스팅 하기 위해선 타겟 필요.
        Aoe, // 영역에 영향을 끼치는 어빌리티. Aoe 범위 관련 표현이 추가됩니다.
        Passive, // 수동으로 사용할 수 없음.
        Channeled, // 채널링 어빌리티 여부.
        Immediate, // 액션 큐에 들어가지 않고, 즉시 발동됨 여부.
        Item, // Item에 연결되어있는 어빌리티.
        Aura, // 어빌리티는 Aura입니다.
    }
}