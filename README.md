# RhythmGamers-Unity
Unity기반 패턴 연습 프로그램</br>
</br>
이 프로젝트는 LHEALP님의 [유니티엔진 리듬게임2](https://github.com/LHEALP/UnityRhythmGame) 프로젝트를 기반으로 제작되었습니다.</br>
</br>
Unity Version: 2021.3.1f1</br>
## 프리뷰
### 인게임 화면
속도와 판정선 위치를 조절할 수 있어요. 조작키는 고정이지만, 웹에서는 설정을 통해 바꿀 수 있어요.

![2025-09-27 06-20-28](https://github.com/user-attachments/assets/8cb0e4d3-99a7-4028-90e7-11732434ea78)

### 결과 화면
세부 판정과 판정 타임라인을 볼 수 있고, 내 판정 평균과 분산도를 알 수 있어요.

![2025-09-27 06-20-28 (1)](https://github.com/user-attachments/assets/5aca4059-87d3-453a-ac2d-9c161ff1c59e)

### 에디터 화면 (수작업)
노래를 들으며, 하나씩 노트를 추가하거나 삭제할 수 있어요.

![2025-09-27 06-51-55](https://github.com/user-attachments/assets/45f9ced5-a258-4334-9ff0-d32f043118c5)

### 에디터 화면 (타건)
노래에 맞춰 키보드를 누르면, 현재 스냅에 맞춰 가장 가까운 선에 노트가 생성돼요.

![2025-09-27 06-51-55 (1)](https://github.com/user-attachments/assets/3de9db89-c4e4-4920-8961-e96a13df0168)

</br>
</br>

## OST - Smilegate RPG
* 화려한 서커스(Splendid Circus)</br>
* 영혼을 데우는 스프(Consolation)</br>
</br>
음원의 저작권은 아래를 따르고 있으며 지켜져야 합니다.</br>
https://creativecommons.org/licenses/by-nc-sa/4.0/deed.ko
</br></br>

## 조작법
엔터 - 게임 시작 ( Enter - Start Game )</br>
ESC - 게임 일시정지 ( ESC - Pause Game )</br>
1, 2 - 배속 변경 ( 1, 2 - Change note speed )</br>
3, 4 - 판정선 변경 ( 3, 4 - Change judge position )</br>
a, s, ;, ' - 4키 조작키 (a, s, ;, ' - 4key control key)</br>
a, s, d, l, ;, ' - 5, 6키 조작키 (a, s, d, l, ;, ' - 5, 6key control key)<br/>
</br>

## 게임/에디터 모드 전환
곡 선택화면의 왼쪽 상단에 Game/Edit mode 전환 버튼이 있습니다.</br>
</br>

## Editor Hotkey
스페이스 - 재생/일시정지 ( Space - Play/Puase )<br/>
마우스좌클릭 - 노트 배치 ( Mouse leftBtn - Dispose note )<br/>
마우스우클릭 - 노트 삭제 ( Mouse rightBtn - Cancel note )<br/>
마우스휠 - 음악 및 그리드 위치 이동 ( Mouse wheel - Move music and grids pos )<br/>
컨트롤 + 마우스휠 - 4비트 ~ 64비트 그리드 스냅 변경 ( Ctrl + Mouse wheel - Change snap of grids )<br/>)
컨트롤 + S - 로컬에 임시 저장 (Ctrl + S - Save File at local)
0 - 게임모드로 플레이 중일 때, 플레이 강제 종료 (Stop game in game mode)
1, 2 - 그리드 오프셋 변경 (1, 2 - Change grid offset)
</br>

## 노트 생성 방식에 대해
게임</br>
유니티 자체 ObjectPool을 활용하여 노트를 해당 시점에 필요한만큼 생성/해제.</br>
생성은 마디단위로 이루어지고, 해제는 노트가 화면 밖으로 이탈(노트의 y좌표가 0보다 작으면)하면 해제.</br>
생성되어있는 노트는 자신의 위치를 현재 노래 시간과 비교하여 지속적으로 재조정.
</br>
에디터</br>
게임에서 사용하던 ObjectPool 시스템을 그대로 활용하나, 실행 시 노트를 한번에 전부 생성.</br>
노트를 배치하면, ObjectPool이 현재 사용하지 않는 오브젝트를 재활용(ObjectPool.Get())하거나 새로 생성.</br>
노트를 삭제하면, 해당 오브젝트를 바로 회수하지는 않고 비활성화(gameObject.SetActive(false)).</br>
</br>

