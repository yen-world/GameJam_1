# 🐓후다닭! : 치킨이 될 순 없어!
![후다닭Title](https://github.com/LKM0222/GameJam_1/blob/main/Assets/04.Image/Title/main.png?raw=true)

## 목차
0. [다운로드](#다운로드)
1. [프로젝트 소개](#-프로젝트-소개)
2. [개발기간](#-개발기간)
3. [개발자 소개](#%EF%B8%8F-개발자-소개)
4. [게임 설명](#게임-설명)
5. [게임 내부 컨텐츠](#게임-내부-컨텐츠)
6. [게임 제작에 사용한 API](#게임-제작에-사용한-api)



## 다운로드
 <center><a href="https://www.google.com/" target="_blank">후다닭! : 치킨이 될 순 없어!</a></center>



📜 프로젝트 소개
----
2023 경북 게임잼에 참여해서 4인 팀을 구성하여 기획 및 제작한 게임입니다.

3일간의 제작기간동안 게임의 기초 틀을 만들었고, 이후 기간동안 시간을 내서 추가로 개발하였습니다.


📆 개발기간
----
+ 2023.11. ~ 2024.03.31
+ 2023 경북 게임잼 참여


🙋‍♂️ 개발자 소개
---
+ 💻 이준호 : 메인 시스템 개발자, Server개발자
+ 💻 박준희 : 서브 시스템 개발자, Server개발자
+ 🖌️ 김가영 : 메인 디자이너, UI 및 게임 캐릭터, 배경 디자이너
+ 📒 이기창 : 메인 기획자, 레벨 디자이너

----
## 게임 설명

게임은 부루마블 형식의 보드게임입니다.

자신의 턴이 되면 자신의 펫말을 아래로 당겨 주사위를 굴리세요!

여러가지 건물을 짓고 상대방의 알을 모두 소모시키세요!

----
## 게임 내부 컨텐츠
#### **1. 건물**

건물은 빈 땅에 플레이어가 도착했을 시 건설 할 수 있습니다.

빈 땅과 건물을 구매하는 가격은 50알입니다.

건물은 총 4개로 각각 다음과 같은 특징을 가집니다.
+ 농    장 : 플레이어가 소지한 양계장에 도착하면 200알을 받습니다. 또한, 양계장에 지나치치 않고 정확히 도착하면 기본금 + (농장 x 100알)을 추가로 받습니다.
+ 재    단 : 플레이어가 소지한 재단에 도착할 때 마다 땅의 가격이 2배 상승합니다.
+ 상    점 : 플레이어가 소지한 상점에 도착하면 소지한 플레이어가 카드 2장을 받습니다.
+ 랜드마크 : 상대방 플레이어가 도착하면 상대방이 200알을 지불합니다.
-> 건물이 지어진 땅에 상대방이 걸린다면 상대방은 100알을 지불하게 됩니다. (랜드마크는 200알입니다.)

#### **2. 특수타일**

특수타일은 맵 각 모서리에 4개가 존재합니다.
+ 양계장 : 플레이어가 지나칠땐 100알, 정확히 도착할땐 100알 + (플레이어가 소지한 농장 수 x 100알)을 획득하게 됩니다.
+ 텔레포트 : 플레이어가 원하는 위치로 텔레포트 할 수 있게 됩니다. 타일을 클릭 후 다음턴에 플레이어가 바로 이동하게 됩니다.
+ 올림픽 : 플레이어가 올림픽에 도착하면 플레이어가 소지한 모든 건물이 있는 타일의 가격이 2배가 됩니다.
+ 건물강탈 : 상대 플레이어가 소유한 땅 중, 하나를 자신의 땅으로 만듭니다.

**3. 카드**

각 줄의 세번째 타일은 카드 타일입니다.

카드는 총 7장으로 구성되어 있고 자신의 턴이 시작되었을 때, 주사위를 굴리기 전에 사용 할 수 있습니다.


----
## 게임 제작에 사용한 API
게임에서 통신을 구현하기 위해 뒤끝 API를 사용하였습니다. <https://www.thebackend.io/>

뒤끝 API중 뒤끝매치 기능을 사용하였고, 뒤끝매치는 리그오브레전드 게임의 매칭 시스템을 따 와서 개발한 서버이기 때문에 따로 방을 만드는 작업은 사용할 수 없습니다.

리그오브레전드의 2인 랭크게임과 같이 자신의 친구를 초대해서 게임 매칭을 할 수 있는 시스템이 존재하지만 이 게임에서는 초대 기능이 구현되지 않았습니다.

<!--
제목 : # 제목 (#의 갯수만큼 크기가 작아진다 1~6개의 #을 쓸 수 있다.)
      # 제목 1
      ## 제목 2
      ### 제목 3
      #### 제목 4
      ##### 제목 5
      ###### 제목 6
이미지를 추가할때 : ![이미지 이름](이미지 링크)
줄바꿈 : 엔터두번
글 강조 (Bold) : **강조할 텍스트**
구분선 : ---(3개이상)
불릿 : 문단 맨 앞에 까만 점을 불릿이라고 한다.
        + 를 쓰거나
        - 를 써도 되고
        * 를 써도된다.
          * tap을 하면 들여쓰기로 빈 불릿이 생성된다.
인용문 : > 인용할 말
하이퍼 링크 : <링크 주소> <>로 묶으면 하이퍼링크


-->
