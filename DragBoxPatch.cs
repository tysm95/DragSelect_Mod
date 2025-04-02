using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DragSelectMod
{
    // WorldManager가 아닌 GameScreen의 Update를 패치하여 UI 갱신 중 처리
    [HarmonyPatch(typeof(GameScreen), "Update")]
    public static class DragBoxPatch
    {
        public static List<GameCard> selectedCards = new(); // 선택된 카드 리스트

        static GameObject boxObject;         // 드래그 박스 오브젝트
        static Image boxImage;               // 드래그 박스 이미지
        static RectTransform boxRect;        // 드래그 박스 RectTransform
        static Vector2 dragStartPos;         // 드래그 시작 위치
        static bool isDragging = false;      // 드래그 중 여부

        static void Postfix(GameScreen __instance)
        {
            if (Mouse.current == null) return;
            var rightMouse = Mouse.current.rightButton;

            // 🖱️ 우클릭 드래그 시작
            if (rightMouse.wasPressedThisFrame)
            {
                dragStartPos = Mouse.current.position.ReadValue();
                isDragging = true;

                // 박스 UI 생성
                boxObject = new GameObject("DragBox");
                boxObject.transform.SetParent(__instance.transform, false);

                boxImage = boxObject.AddComponent<Image>();
                boxImage.color = new Color(0.8f, 0.8f, 0.8f, 0.3f); // 반투명 회색

                boxRect = boxImage.rectTransform;
                boxRect.pivot = Vector2.zero;

                Debug.Log("[🟦] Drag start at: " + dragStartPos);
            }

            // 📦 드래그 중 → 박스 크기 실시간 업데이트
            if (rightMouse.isPressed && isDragging && boxRect != null)
            {
                Vector2 currentPos = Mouse.current.position.ReadValue();

                float minX = Mathf.Min(currentPos.x, dragStartPos.x);
                float minY = Mathf.Min(currentPos.y, dragStartPos.y);
                float sizeX = Mathf.Abs(currentPos.x - dragStartPos.x);
                float sizeY = Mathf.Abs(currentPos.y - dragStartPos.y);

                boxObject.transform.position = new Vector3(minX, minY, 0f);
                boxRect.sizeDelta = new Vector2(sizeX, sizeY);
            }

            // ✅ 우클릭 해제 → 드래그 종료 및 카드 감지
            if (rightMouse.wasReleasedThisFrame && isDragging)
            {
                isDragging = false;
                Vector2 dragEnd = Mouse.current.position.ReadValue();

                Debug.Log("[🟦] Drag end at: " + dragEnd);

                // 드래그 사각형 계산
                Vector2 min = Vector2.Min(dragStartPos, dragEnd);
                Vector2 max = Vector2.Max(dragStartPos, dragEnd);
                Rect screenRect = new Rect(min, max - min);

                // 이전 선택 카드 크기 초기화
                foreach (var card in selectedCards)
                    card.transform.localScale = Vector3.one;

                selectedCards.Clear();

                // 카드 감지
                foreach (GameCard card in WorldManager.instance.AllCards)
                {
                    if (!card || !card.MyBoard.IsCurrent) continue;

                    Vector2 screenPos = Camera.main.WorldToScreenPoint(card.transform.position);
                    if (screenRect.Contains(screenPos))
                    {
                        selectedCards.Add(card);
                        Debug.Log($"[🎴] 카드 감지됨: {card.name}");
                    }
                }

                // 새로 선택된 카드 크기 확대
                foreach (var card in selectedCards)
                    card.transform.localScale = Vector3.one * 1.2f;

                Debug.Log($"[✅] 총 감지된 카드 수: {selectedCards.Count}");

                // 박스 오브젝트 제거
                if (boxObject != null) Object.Destroy(boxObject);
            }
        }
    }
}
