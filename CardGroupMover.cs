using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DragSelectMod
{
    // WorldManager.Update 패치 → 선택된 카드 그룹을 마우스로 이동
    [HarmonyPatch(typeof(WorldManager), "Update")]
    public static class CardGroupMover
    {
        static bool isDraggingGroup = false;       // 그룹 드래그 중 여부
        static Vector3 dragOffset;                 // 마우스와 기준 카드 간 거리
        static GameCard baseCard = null;           // 기준 카드 (마우스로 클릭한 카드)

        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Mouse.current == null || DragBoxPatch.selectedCards.Count == 0) return;

            var leftMouse = Mouse.current.leftButton;
            Vector3 mouseWorld = WorldManager.instance.mouseWorldPosition;

            // ✅ 좌클릭 시작 → 마우스 근처 카드 감지
            if (leftMouse.wasPressedThisFrame)
            {
                GameCard clickedCard = null;
                float minDistance = 0.75f; // 기존 0.5보다 조금 여유 있게

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

                // ✅ 선택된 카드 중 하나라면 그룹 이동 시작
                if (clickedCard != null && DragBoxPatch.selectedCards.Contains(clickedCard))
                {
                    isDraggingGroup = true;
                    baseCard = clickedCard;
                    dragOffset = mouseWorld - baseCard.transform.position;
                }
            }

            // ✅ 마우스 드래그 중 → 모든 카드 TargetPosition 갱신
            if (leftMouse.isPressed && isDraggingGroup && baseCard != null)
            {
                Vector3 groupTargetPos = mouseWorld - dragOffset;

                foreach (GameCard card in DragBoxPatch.selectedCards)
                {
                    Vector3 offset = card.transform.position - baseCard.transform.position;
                    card.TargetPosition = groupTargetPos + offset;

                    // Debug.Log($"[🧲] 이동: {card.name} | Target: {card.TargetPosition} | Offset: {offset}");
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
