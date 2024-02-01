# 플레이어 움직임
디자인패턴 - 전략패턴 (현재 상태의 스크립트(Update 등)만 실행으로 유한상태로 만듦)

Player : 유저가 조작하는 캐릭터

StateMachine : 상황에 따라 상태를 바꿔주고 각 상태의 Operate 함수를 실행시켜주는, 상태들을 총괄해주는 역할

StateBaseMovement, StateWallRun, StateClimb : 각 상태들은 동시에 실행 되지 않으며, 각 상태들에서 움직임들을 담당

<img src = "https://github.com/Box-In-Box/Unity-Movement-strategy-pattern/assets/79827366/787f618f-07ba-410d-b80d-98f03999b6ec">

https://github.com/Box-In-Box/Unity-Movement-strategy-pattern/assets/79827366/fef5bde6-864f-41a7-9d2b-e7b1dc8b9d32
