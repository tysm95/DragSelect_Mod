using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DragSelectMod
{
    // GameScreen의 Update 메서드를 패치하여 드래그 박스와 카드 선택 처리
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
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas == null) return;

                dragStartPos = Mouse.current.position.ReadValue() / canvas.scaleFactor;
                isDragging = true;

                // 박스 UI 생성
                boxObject = new GameObject("DragBox");
                boxObject.transform.SetParent(canvas.transform, false);

                boxImage = boxObject.AddComponent<Image>();
                boxImage.color = new Color(0.8f, 0.8f, 0.8f, 0.3f); // 반투명 회색

                boxRect = boxImage.rectTransform;
                boxRect.anchorMin = Vector2.zero;
                boxRect.anchorMax = Vector2.zero;
                boxRect.pivot = Vector2.zero;
                boxRect.anchoredPosition = dragStartPos;
            }

            // 📦 드래그 중 → 박스 크기 실시간 업데이트
            if (rightMouse.isPressed && isDragging && boxRect != null)
            {
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas == null) return;

                Vector2 currentPos = Mouse.current.position.ReadValue() / canvas.scaleFactor;

                Vector2 min = Vector2.Min(dragStartPos, currentPos);
                Vector2 max = Vector2.Max(dragStartPos, currentPos);

                boxRect.anchoredPosition = min;
                boxRect.sizeDelta = max - min;
            }

            // ✅ 우클릭 해제 → 드래그 종료 및 카드 감지
            if (rightMouse.wasReleasedThisFrame && isDragging)
            {
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas == null) return;
                //Debug.Log("Canvas is Not Null");

                isDragging = false;
                Vector2 dragEnd = Mouse.current.position.ReadValue() / canvas.scaleFactor;

                Vector2 min = Vector2.Min(dragStartPos, dragEnd);
                Vector2 max = Vector2.Max(dragStartPos, dragEnd);
                Rect screenRect = new Rect(min, max - min);

                foreach (var card in selectedCards)
                    card.transform.localScale = Vector3.one;

                selectedCards.Clear();

                foreach (GameCard card in WorldManager.instance.AllCards)
                {
                    if (!card || !card.MyBoard.IsCurrent) continue;

                    Vector2 screenPos = Camera.main.WorldToScreenPoint(card.transform.position) / canvas.scaleFactor;

                    if (screenRect.Contains(screenPos))
                        selectedCards.Add(card);
                }

                foreach (var card in selectedCards)
                    card.transform.localScale = Vector3.one * 1.2f;

                if (boxObject != null)
                {
                    Object.Destroy(boxObject);
                    boxObject = null;
                    boxImage = null;
                    boxRect = null;
                }
            }

            // 🔒 박스가 남아있는 경우 제거 (예외 방지용)
            if (!isDragging && boxObject != null)
            {
                Object.Destroy(boxObject);
                boxObject = null;
                boxImage = null;
                boxRect = null;
            }
        }
    }
}
