using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DragSelectMod
{
    // WorldManager.Update 메서드를 패치해서 카드 그룹 이동 기능 추가
    [HarmonyPatch(typeof(WorldManager), "Update")]
    public static class CardGroupMover
    {
        static bool isDraggingGroup = false;       // 그룹 드래그 중인지 여부
        static Vector3 dragOffset;                 // 기준 카드와 마우스 간 거리
        static GameCard baseCard = null;           // 드래그의 기준이 되는 카드

        [HarmonyPostfix]
        public static void Postfix()
        {
            // 마우스 또는 선택 카드가 없으면 리턴
            if (Mouse.current == null || DragBoxPatch.selectedCards.Count == 0) return;

            var leftMouse = Mouse.current.leftButton;
            Vector3 mouseWorld = WorldManager.instance.mouseWorldPosition;

            // ✅ 좌클릭 시작 → 가장 가까운 카드가 선택된 카드인지 확인
            if (leftMouse.wasPressedThisFrame)
            {
                GameCard clickedCard = null;
                float minDistance = 0.5f;

                // 모든 카드 중 마우스와 가장 가까운 카드 탐색
                foreach (var card in WorldManager.instance.AllCards)
                {
                    if (!card || !card.MyBoard.IsCurrent) continue;

                    float dist = Vector3.Distance(mouseWorld, card.transform.position);
                    if (dist < minDistance)
                    {
                        clickedCard = card;
                        minDistance = dist;
                    }
                }

                // 클릭된 카드가 선택된 카드 중 하나라면 이동 시작
                if (clickedCard != null && DragBoxPatch.selectedCards.Contains(clickedCard))
                {
                    isDraggingGroup = true;
                    baseCard = clickedCard;
                    dragOffset = mouseWorld - baseCard.transform.position;
                }
            }

            // ✅ 드래그 중 → 모든 선택 카드 위치 갱신
            if (leftMouse.isPressed && isDraggingGroup && baseCard != null)
            {
                Vector3 groupTargetPos = mouseWorld - dragOffset;

                foreach (GameCard card in DragBoxPatch.selectedCards)
                {
                    Vector3 offset = card.transform.position - baseCard.transform.position;
                    card.TargetPosition = groupTargetPos + offset;

                    // 디버깅용 로그
                    Debug.Log($"[🧲] 이동: {card.name} | Target: {card.TargetPosition} | Offset: {offset}");
                }
            }

            // ✅ 드래그 종료 → 상태 초기화
            if (leftMouse.wasReleasedThisFrame)
            {
                isDraggingGroup = false;
                baseCard = null;
            }
        }
    }
}
