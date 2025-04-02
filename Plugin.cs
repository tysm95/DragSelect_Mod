using HarmonyLib;
using UnityEngine;

namespace DragSelectMod
{
    // DragSelect 모드의 메인 클래스
    public class Plugin : Mod
    {
        private Harmony harmony; // Harmony 인스턴스 (패치 관리용)

        // 모드 로딩 시 호출됨
        public override void Ready()
        {
            harmony = new Harmony("dragselectmod");
            harmony.PatchAll(); // 현재 어셈블리 내 모든 HarmonyPatch 적용
            Logger.Log("[✅] DragSelectMod Ready!"); // 모드 로딩 완료 로그
        }

        // 모드 언로드 시 호출됨
        private void OnDestroy()
        {
            harmony.UnpatchSelf(); // 적용한 패치만 해제 (권장 방식)
        }
    }
}
