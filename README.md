# Movement
디자인패턴 - 전략패턴

Player : 유저가 조작하는 캐릭터

StateMachine : 상황에 따라 상태를 바꿔주고 각 상태의 Operate 함수를 실행시켜주는, 상태들을 총괄해주는 역할

StateBaseMovement, StateWallRun, StateClimb : 각 상태들은 동시에 실행 되지 않으며, 각 상태들에서 움직임들을 담당

<img src = "https://github.com/Box-In-Box/Lastman_2D/assets/79827366/b6106cb3-0693-411c-ace9-7a35fe6de4ae">
