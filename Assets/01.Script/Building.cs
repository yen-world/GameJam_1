using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Building
{
    //건물 이미지
    public BuildingType buildingImg;

    // 건물 이름
    public string buildingName;

    // 건물의 타입
    public int type;

    // 건물의 기본 통행료
    public int toll;
}
