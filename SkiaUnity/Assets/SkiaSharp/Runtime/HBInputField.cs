using System;
using Topten.RichTextKit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextAlignment = Topten.RichTextKit.TextAlignment;

namespace SkiaSharp.Unity.HB {

	[AddComponentMenu("Skia UI (Canvas)/HB InputField")]
	public class HBInputField : Selectable,
		IUpdateSelectedHandler, IPointerClickHandler, ISubmitHandler,
		IBeginDragHandler, IDragHandler, IEndDragHandler {

		public enum ContentType { Standard, Integer, Decimal, Alphanumeric, Name, EmailAddress, Password }
		public enum LineType { SingleLine, MultiLine }

		[SerializeField] HB_TEXTBlock textComponent;
		[SerializeField] HB_TEXTBlock placeholder;
		[SerializeField] RectTransform textViewport;
		[SerializeField] Image caretImage;

		[SerializeField, TextArea] string m_Text = "";
		[SerializeField] int characterLimit;
		[SerializeField] ContentType contentType = ContentType.Standard;
		[SerializeField] LineType lineType = LineType.SingleLine;
		[SerializeField] bool readOnly;
		[SerializeField] Color caretColor = new Color(0.2f, 0.2f, 0.2f, 1f);
		[SerializeField] float caretBlinkRate = 0.85f;
		[SerializeField] int caretWidth = 2;
		[SerializeField] bool hideCaret;
		[SerializeField] Color selectionColor = new Color(0.24f, 0.48f, 0.9f, 0.4f);
		[SerializeField] char asteriskChar = '*';

		// Text settings
		[SerializeField] UnityEngine.Object fontAsset;
		[SerializeField] int fontSize = 20;
		[SerializeField] Color fontColor = Color.black;
		[SerializeField] bool bold;
		[SerializeField] bool italic;
		[SerializeField] bool richText;
		[SerializeField] TextAlignment textAlignment = TextAlignment.Left;
		[SerializeField] string placeholderText = "Enter text...";
		[SerializeField] Color placeholderColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

		// Behavior
		[SerializeField] bool selectAllOnFocus;
		[SerializeField] bool tabNavigation = true;

		// Read-only appearance
		[SerializeField] Color readOnlyColor = new Color(0.85f, 0.85f, 0.85f, 1f);
		[SerializeField] Color readOnlyFontColor = new Color(0.5f, 0.5f, 0.5f, 1f);

		// Focus appearance
		[SerializeField] bool useFocusColor;
		[SerializeField] Color focusBackgroundColor = new Color(1f, 1f, 1f, 1f);

		// Mobile
		[SerializeField] bool hideMobileInput = true;

		// Auto resize
		[SerializeField] bool autoResize;
		[SerializeField] float minHeight = 40f;
		[SerializeField] float maxAutoHeight = 300f;

		[Serializable] public class SubmitEvent : UnityEvent<string> { }
		[Serializable] public class ValueChangedEvent : UnityEvent<string> { }

		public ValueChangedEvent onValueChanged = new ValueChangedEvent();
		public SubmitEvent onSubmit = new SubmitEvent();
		public SubmitEvent onEndEdit = new SubmitEvent();
		public UnityEvent onFocus = new UnityEvent();
		public UnityEvent onUnfocus = new UnityEvent();

		// Runtime state
		private int m_CaretPosition;
		private int m_SelectionAnchor;
		private bool m_HasFocus;
		private float m_BlinkTimer;
		private bool m_CaretVisible;
		private bool m_DragSelecting;
		private RectTransform m_CaretRect;
		private Image m_SelectionHighlight;
		private TouchScreenKeyboard m_Keyboard;
		private Image m_BackgroundImage;
		private Color m_OriginalBackgroundColor;
		private Vector2 m_ScrollOffset;
		private RectTransform m_TextRT;
		private RectTransform m_PlaceholderRT;

		// Public API
		public string text {
			get => m_Text;
			set {
				if (m_Text == value) return;
				m_Text = value ?? "";
				if (characterLimit > 0 && m_Text.Length > characterLimit)
					m_Text = m_Text.Substring(0, characterLimit);
				m_CaretPosition = Mathf.Min(m_CaretPosition, m_Text.Length);
				m_SelectionAnchor = m_CaretPosition;
				UpdateDisplay();
				onValueChanged?.Invoke(m_Text);
			}
		}

		public int caretPosition {
			get => m_CaretPosition;
			set {
				m_CaretPosition = Mathf.Clamp(value, 0, m_Text.Length);
				m_SelectionAnchor = m_CaretPosition;
				RefreshCaretView();
			}
		}

		public bool isFocused => m_HasFocus;

		public HB_TEXTBlock TextComponent => textComponent;
		public HB_TEXTBlock Placeholder => placeholder;

		// ===================== Lifecycle =====================

		protected override void Awake() {
			base.Awake();

			if (caretImage != null) {
				m_CaretRect = caretImage.GetComponent<RectTransform>();
				caretImage.color = caretColor;
				caretImage.gameObject.SetActive(false);
			}

			m_BackgroundImage = GetComponent<Image>();
			if (m_BackgroundImage != null)
				m_OriginalBackgroundColor = m_BackgroundImage.color;

			if (textComponent != null)
				m_TextRT = textComponent.GetComponent<RectTransform>();
			if (placeholder != null)
				m_PlaceholderRT = placeholder.GetComponent<RectTransform>();

			CreateSelectionHighlight();
			ApplyReadOnlyAppearance();
		}

		protected override void Start() {
			base.Start();
			SyncTextSettings();
			SyncMaxWidthToRect();
			UpdateDisplay();
		}

		protected override void OnEnable() {
			base.OnEnable();
			UpdateDisplay();
		}

		protected override void OnDisable() {
			base.OnDisable();
			Deactivate();
		}

#if UNITY_EDITOR
		private void OnValidate() {
			if (textComponent != null) {
				SyncTextSettings();
				UpdateDisplay();
			}
		}
#endif

		protected override void OnRectTransformDimensionsChange() {
			base.OnRectTransformDimensionsChange();
			SyncMaxWidthToRect();
		}

		private void SyncMaxWidthToRect() {
			if (textComponent == null) return;
			var rt = m_TextRT != null ? m_TextRT : textComponent.GetComponent<RectTransform>();
			if (rt == null) return;

			float viewportW = rt.rect.width;
			if (viewportW <= 0) return;

			if (lineType == LineType.SingleLine) {
				// Single-line: don't wrap — use large maxWidth so text extends freely
				float largeW = Mathf.Max(viewportW, 10000f);
				if (Mathf.Abs(textComponent.MaxWidth - largeW) > 0.5f)
					textComponent.MaxWidth = largeW;
			} else {
				// Multiline: wrap at viewport width
				if (Mathf.Abs(textComponent.MaxWidth - viewportW) > 0.5f)
					textComponent.MaxWidth = viewportW;
			}

			// Placeholder always wraps at viewport width
			if (placeholder != null) {
				var prt = m_PlaceholderRT != null ? m_PlaceholderRT : placeholder.GetComponent<RectTransform>();
				if (prt != null) {
					float pw = prt.rect.width;
					if (pw > 0 && Mathf.Abs(placeholder.MaxWidth - pw) > 0.5f)
						placeholder.MaxWidth = pw;
				}
			}
		}

		private void SyncTextSettings() {
			if (textComponent != null) {
				if (fontAsset != null) textComponent.Font = fontAsset;
				textComponent.FontSize = fontSize;
				textComponent.FontColor = readOnly ? readOnlyFontColor : fontColor;
				textComponent.Bold = bold;
				textComponent.Italic = italic;
				textComponent.TextAlignment = textAlignment;
				// richText mode is applied in UpdateDisplay via SetRichText
			}
			if (placeholder != null) {
				if (fontAsset != null) placeholder.Font = fontAsset;
				placeholder.FontSize = fontSize;
				placeholder.FontColor = placeholderColor;
				placeholder.Bold = false;
				placeholder.Italic = italic;
				placeholder.TextAlignment = textAlignment;
				placeholder.text = placeholderText;
			}
			ApplyReadOnlyAppearance();
		}

		private void ApplyReadOnlyAppearance() {
			if (m_BackgroundImage != null) {
				if (readOnly) {
					m_BackgroundImage.color = readOnlyColor;
				} else if (!m_HasFocus || !useFocusColor) {
					m_BackgroundImage.color = m_OriginalBackgroundColor;
				}
			}
		}

		private void AutoResizeHeight() {
			if (!autoResize || lineType != LineType.MultiLine) return;
			if (textComponent == null || textComponent.Info == null) return;

			var rootRT = GetComponent<RectTransform>();
			if (rootRT == null) return;

			// Get measured text height + viewport padding
			float textHeight = textComponent.Info.MeasuredHeight;
			// Account for viewport padding (offsetMin.y + abs(offsetMax.y))
			float padding = 12f; // default 6 top + 6 bottom from creation
			if (textViewport != null) {
				padding = textViewport.offsetMin.y + Mathf.Abs(textViewport.offsetMax.y);
			}

			float desiredHeight = Mathf.Clamp(textHeight + padding, minHeight, maxAutoHeight);
			if (Mathf.Abs(rootRT.sizeDelta.y - desiredHeight) > 0.5f) {
				rootRT.sizeDelta = new Vector2(rootRT.sizeDelta.x, desiredHeight);
			}
		}

		private void ScrollToCaret() {
			if (textComponent == null || textViewport == null) return;
			if (!textComponent.GetCaretLocalRect(m_CaretPosition, out Rect caretRect)) return;

			var textRT = m_TextRT != null ? m_TextRT : textComponent.GetComponent<RectTransform>();
			Rect viewRect = textViewport.rect;

			// Caret position in viewport space = caret local pos + current scroll offset
			float caretXInView = caretRect.x + m_ScrollOffset.x;
			float caretYInView = caretRect.y + m_ScrollOffset.y;
			float caretTop = caretYInView + caretRect.height;

			float margin = 4f;

			// Horizontal scrolling
			if (caretXInView < viewRect.xMin + margin) {
				m_ScrollOffset.x += (viewRect.xMin + margin) - caretXInView;
			} else if (caretXInView + caretWidth > viewRect.xMax - margin) {
				m_ScrollOffset.x -= (caretXInView + caretWidth) - (viewRect.xMax - margin);
			}

			// Vertical scrolling (multiline)
			if (lineType == LineType.MultiLine) {
				if (caretYInView < viewRect.yMin + margin) {
					m_ScrollOffset.y += (viewRect.yMin + margin) - caretYInView;
				} else if (caretTop > viewRect.yMax - margin) {
					m_ScrollOffset.y -= caretTop - (viewRect.yMax - margin);
				}
			}

			// Clamp: don't scroll past the beginning
			if (m_ScrollOffset.x > 0) m_ScrollOffset.x = 0;
			if (m_ScrollOffset.y < 0 && lineType == LineType.MultiLine) {
				// For multiline, don't scroll down past content
			}
			if (lineType == LineType.SingleLine) m_ScrollOffset.y = 0;

			ApplyScrollOffset();
		}

		private void ApplyScrollOffset() {
			if (m_TextRT == null) return;

			// The text RT anchors stretch to viewport. Offset via offsetMin/offsetMax.
			m_TextRT.offsetMin = new Vector2(m_ScrollOffset.x, m_TextRT.offsetMin.y);
			m_TextRT.offsetMax = new Vector2(m_ScrollOffset.x, m_TextRT.offsetMax.y);

			if (lineType == LineType.MultiLine) {
				m_TextRT.offsetMin = new Vector2(m_TextRT.offsetMin.x, m_ScrollOffset.y);
				m_TextRT.offsetMax = new Vector2(m_TextRT.offsetMax.x, m_ScrollOffset.y);
			}
		}

		private void ResetScroll() {
			m_ScrollOffset = Vector2.zero;
			if (m_TextRT != null) {
				m_TextRT.offsetMin = Vector2.zero;
				m_TextRT.offsetMax = Vector2.zero;
			}
		}

		// ===================== Focus =====================

		public override void OnSelect(BaseEventData eventData) {
			base.OnSelect(eventData);
			Activate();
		}

		public override void OnDeselect(BaseEventData eventData) {
			base.OnDeselect(eventData);
			Deactivate();
		}

		private void Activate() {
			if (m_HasFocus) return;
			m_HasFocus = true;
			m_BlinkTimer = 0f;
			m_CaretVisible = true;

			if (caretImage != null && !hideCaret)
				caretImage.gameObject.SetActive(true);

			// Focus background color
			if (useFocusColor && !readOnly && m_BackgroundImage != null)
				m_BackgroundImage.color = focusBackgroundColor;

			// Open native keyboard on mobile platforms
			if (TouchScreenKeyboard.isSupported && !readOnly) {
				if (hideMobileInput)
					TouchScreenKeyboard.hideInput = true;

				TouchScreenKeyboardType keyboardType;
				switch (contentType) {
					case ContentType.Integer: keyboardType = TouchScreenKeyboardType.NumberPad; break;
					case ContentType.Decimal: keyboardType = TouchScreenKeyboardType.DecimalPad; break;
					case ContentType.EmailAddress: keyboardType = TouchScreenKeyboardType.EmailAddress; break;
					default: keyboardType = TouchScreenKeyboardType.Default; break;
				}
				bool secure = contentType == ContentType.Password;
				bool multiline = lineType == LineType.MultiLine;
				m_Keyboard = TouchScreenKeyboard.Open(m_Text, keyboardType, true, multiline, secure);
			}

			if (selectAllOnFocus && m_Text.Length > 0) {
				m_SelectionAnchor = 0;
				m_CaretPosition = m_Text.Length;
			}

			RefreshCaretView();
			onFocus?.Invoke();
		}

		private void Deactivate() {
			if (!m_HasFocus) return;
			m_HasFocus = false;
			if (caretImage != null) caretImage.gameObject.SetActive(false);
			if (m_SelectionHighlight != null) m_SelectionHighlight.gameObject.SetActive(false);
			if (m_Keyboard != null) {
				m_Keyboard.active = false;
				m_Keyboard = null;
			}

			// Reset scroll and restore background
			ResetScroll();
			if (m_BackgroundImage != null) {
				m_BackgroundImage.color = readOnly ? readOnlyColor : m_OriginalBackgroundColor;
			}

			onEndEdit?.Invoke(m_Text);
			onUnfocus?.Invoke();
		}

		// ===================== Input Processing =====================

		public void OnUpdateSelected(BaseEventData eventData) {
			if (!m_HasFocus) return;

			// Blink caret (works for both desktop and mobile)
			if (!hideCaret) {
				m_BlinkTimer += Time.unscaledDeltaTime;
				if (caretBlinkRate > 0 && m_BlinkTimer >= caretBlinkRate) {
					m_BlinkTimer = 0f;
					m_CaretVisible = !m_CaretVisible;
					if (caretImage != null)
						caretImage.enabled = m_CaretVisible && !HasSelection;
				}
			}

			// Mobile keyboard
			if (m_Keyboard != null) {
				ProcessTouchKeyboard();
				eventData.Use();
				return;
			}

			// Process keyboard events using Event.PopEvent (works in EventSystem callbacks)
			bool changed = false;
			Event evt = new Event();
			while (Event.PopEvent(evt)) {
				if (evt.rawType == EventType.KeyDown) {
					changed |= ProcessKeyEvent(evt);

					// Handle character input from key events
					char c = evt.character;
					if (c != 0 && c != '\b' && c != 127 && c != '\t' && !readOnly) {
						if (c == '\n' || c == '\r') {
							if (lineType == LineType.MultiLine && !evt.control && !evt.command) {
								changed |= InsertChar('\n');
							}
						} else {
							changed |= InsertChar(c);
						}
					}
				}
			}

			if (changed) {
				UpdateDisplay();
				onValueChanged?.Invoke(m_Text);
			}

			eventData.Use();
		}

		private void ProcessTouchKeyboard() {
			if (m_Keyboard == null) return;

			string kbText = m_Keyboard.text;
			if (kbText != m_Text) {
				// Validate characters
				if (contentType != ContentType.Standard && contentType != ContentType.Password) {
					string validated = "";
					foreach (char c in kbText) {
						if (ValidateChar(c)) validated += c;
					}
					kbText = validated;
				}
				if (characterLimit > 0 && kbText.Length > characterLimit)
					kbText = kbText.Substring(0, characterLimit);

				// Sync validated text back to keyboard if it changed
				if (kbText != m_Keyboard.text)
					m_Keyboard.text = kbText;

				// Compute new caret position from what actually changed
				m_CaretPosition = ComputeCaretFromDiff(m_Text, kbText);
				m_Text = kbText;
				m_SelectionAnchor = m_CaretPosition;
				UpdateDisplay();
				onValueChanged?.Invoke(m_Text);

				// Keep native keyboard cursor in sync with our caret
				SyncKeyboardSelection();
			}

			if (m_Keyboard.status == TouchScreenKeyboard.Status.Done) {
				onSubmit?.Invoke(m_Text);
				Deactivate();
			} else if (m_Keyboard.status == TouchScreenKeyboard.Status.Canceled) {
				Deactivate();
			}
		}

		private int ComputeCaretFromDiff(string oldText, string newText) {
			int oldLen = oldText.Length;
			int newLen = newText.Length;

			// Find common prefix
			int prefixLen = 0;
			int minLen = Mathf.Min(oldLen, newLen);
			while (prefixLen < minLen && oldText[prefixLen] == newText[prefixLen])
				prefixLen++;

			// Find common suffix
			int suffixLen = 0;
			while (suffixLen < (minLen - prefixLen)
				   && oldText[oldLen - 1 - suffixLen] == newText[newLen - 1 - suffixLen])
				suffixLen++;

			// Caret goes right after the inserted/changed region
			int insertedLen = newLen - prefixLen - suffixLen;
			return Mathf.Clamp(prefixLen + Mathf.Max(0, insertedLen), 0, newLen);
		}

		private void SyncKeyboardSelection() {
			if (m_Keyboard == null || !m_Keyboard.active) return;

			if (HasSelection) {
				int start = Mathf.Min(m_CaretPosition, m_SelectionAnchor);
				int length = Mathf.Abs(m_CaretPosition - m_SelectionAnchor);
				m_Keyboard.selection = new RangeInt(start, length);
			} else {
				m_Keyboard.selection = new RangeInt(m_CaretPosition, 0);
			}
		}

		private bool ProcessKeyEvent(Event evt) {
			bool ctrl = evt.control || evt.command;
			bool shift = evt.shift;
			KeyCode kc = evt.keyCode;

			// Backspace (keyCode or character '\b')
			if (kc == KeyCode.Backspace || evt.character == '\b') {
				if (readOnly) return false;
				return Backspace();
			}

			// Delete (keyCode or character 127)
			if (kc == KeyCode.Delete || evt.character == 127) {
				if (readOnly) return false;
				return Delete();
			}

			// Navigation
			if (kc == KeyCode.LeftArrow) { MoveCaretLeft(shift, ctrl); return false; }
			if (kc == KeyCode.RightArrow) { MoveCaretRight(shift, ctrl); return false; }
			if (kc == KeyCode.UpArrow && lineType == LineType.MultiLine) { MoveCaretUp(shift); return false; }
			if (kc == KeyCode.DownArrow && lineType == LineType.MultiLine) { MoveCaretDown(shift); return false; }
			if (kc == KeyCode.Home) { MoveToStart(shift); return false; }
			if (kc == KeyCode.End) { MoveToEnd(shift); return false; }

			// Submit / newline
			if (kc == KeyCode.Return || kc == KeyCode.KeypadEnter) {
				// Handled by character input for multiline, submit for single line
				if (lineType != LineType.MultiLine || ctrl) {
					onSubmit?.Invoke(m_Text);
					Deactivate();
				}
				return false; // char input handles the actual '\n' insertion
			}

			if (kc == KeyCode.Escape) { Deactivate(); return false; }

			// Tab navigation
			if (kc == KeyCode.Tab && tabNavigation) {
				Selectable next = shift ? FindSelectableOnUp() ?? FindSelectableOnLeft()
					: FindSelectableOnDown() ?? FindSelectableOnRight();
				if (next != null) {
					Deactivate();
					EventSystem.current.SetSelectedGameObject(next.gameObject);
				}
				return false;
			}

			// Shortcuts
			if (ctrl) {
				if (kc == KeyCode.A) { SelectAll(); return false; }
				if (kc == KeyCode.C) { CopyToClipboard(); return false; }
				if (kc == KeyCode.V && !readOnly) return Paste();
				if (kc == KeyCode.X && !readOnly) { CopyToClipboard(); return DeleteSelection(); }
			}

			return false;
		}

		// ===================== Text Editing =====================

		private bool InsertChar(char c) {
			if (!ValidateChar(c)) return false;

			DeleteSelection();

			if (characterLimit > 0 && m_Text.Length >= characterLimit) return false;

			m_Text = m_Text.Insert(m_CaretPosition, c.ToString());
			m_CaretPosition++;
			m_SelectionAnchor = m_CaretPosition;
			ResetBlink();
			return true;
		}

		private bool Backspace() {
			if (HasSelection) return DeleteSelection();
			if (m_CaretPosition <= 0) return false;

			m_CaretPosition--;
			m_Text = m_Text.Remove(m_CaretPosition, 1);
			m_SelectionAnchor = m_CaretPosition;
			ResetBlink();
			return true;
		}

		private bool Delete() {
			if (HasSelection) return DeleteSelection();
			if (m_CaretPosition >= m_Text.Length) return false;

			m_Text = m_Text.Remove(m_CaretPosition, 1);
			ResetBlink();
			return true;
		}

		private bool DeleteSelection() {
			if (!HasSelection) return false;

			int start = Mathf.Min(m_CaretPosition, m_SelectionAnchor);
			int end = Mathf.Max(m_CaretPosition, m_SelectionAnchor);
			m_Text = m_Text.Remove(start, end - start);
			m_CaretPosition = start;
			m_SelectionAnchor = start;
			ResetBlink();
			return true;
		}

		private bool Paste() {
			string clipboard = GUIUtility.systemCopyBuffer;
			if (string.IsNullOrEmpty(clipboard)) return false;

			DeleteSelection();

			if (lineType == LineType.SingleLine)
				clipboard = clipboard.Replace("\n", "").Replace("\r", "");

			// Validate each character
			string validated = "";
			foreach (char c in clipboard) {
				if (ValidateChar(c)) validated += c;
			}

			if (characterLimit > 0) {
				int space = characterLimit - m_Text.Length;
				if (space <= 0) return false;
				if (validated.Length > space) validated = validated.Substring(0, space);
			}

			m_Text = m_Text.Insert(m_CaretPosition, validated);
			m_CaretPosition += validated.Length;
			m_SelectionAnchor = m_CaretPosition;
			ResetBlink();
			return true;
		}

		// ===================== Navigation =====================

		private void MoveCaretLeft(bool shift, bool ctrl) {
			int newPos = m_CaretPosition;
			if (ctrl) {
				newPos = FindPreviousWordStart(m_CaretPosition);
			} else {
				if (!shift && HasSelection) {
					newPos = Mathf.Min(m_CaretPosition, m_SelectionAnchor);
				} else {
					newPos = Mathf.Max(0, m_CaretPosition - 1);
				}
			}
			m_CaretPosition = newPos;
			if (!shift) m_SelectionAnchor = m_CaretPosition;
			ResetBlink();
			RefreshCaretView();
		}

		private void MoveCaretRight(bool shift, bool ctrl) {
			int newPos = m_CaretPosition;
			if (ctrl) {
				newPos = FindNextWordEnd(m_CaretPosition);
			} else {
				if (!shift && HasSelection) {
					newPos = Mathf.Max(m_CaretPosition, m_SelectionAnchor);
				} else {
					newPos = Mathf.Min(m_Text.Length, m_CaretPosition + 1);
				}
			}
			m_CaretPosition = newPos;
			if (!shift) m_SelectionAnchor = m_CaretPosition;
			ResetBlink();
			RefreshCaretView();
		}

		private void MoveCaretUp(bool shift) {
			if (textComponent == null || textComponent.Info == null) return;
			var info = textComponent.Info;
			var current = info.GetCaretInfo(new Topten.RichTextKit.CaretPosition(m_CaretPosition));
			if (current.LineIndex <= 0) {
				m_CaretPosition = 0;
			} else {
				var result = info.HitTestLine(current.LineIndex - 1, current.CaretXCoord);
				m_CaretPosition = Mathf.Clamp(result.ClosestCodePointIndex, 0, m_Text.Length);
			}
			if (!shift) m_SelectionAnchor = m_CaretPosition;
			ResetBlink();
			RefreshCaretView();
		}

		private void MoveCaretDown(bool shift) {
			if (textComponent == null || textComponent.Info == null) return;
			var info = textComponent.Info;
			var current = info.GetCaretInfo(new Topten.RichTextKit.CaretPosition(m_CaretPosition));
			if (current.LineIndex >= info.LineCount - 1) {
				m_CaretPosition = m_Text.Length;
			} else {
				var result = info.HitTestLine(current.LineIndex + 1, current.CaretXCoord);
				m_CaretPosition = Mathf.Clamp(result.ClosestCodePointIndex, 0, m_Text.Length);
			}
			if (!shift) m_SelectionAnchor = m_CaretPosition;
			ResetBlink();
			RefreshCaretView();
		}

		private void MoveToStart(bool shift) {
			m_CaretPosition = 0;
			if (!shift) m_SelectionAnchor = 0;
			ResetBlink();
			RefreshCaretView();
		}

		private void MoveToEnd(bool shift) {
			m_CaretPosition = m_Text.Length;
			if (!shift) m_SelectionAnchor = m_Text.Length;
			ResetBlink();
			RefreshCaretView();
		}

		private void SelectAll() {
			m_SelectionAnchor = 0;
			m_CaretPosition = m_Text.Length;
			RefreshCaretView();
		}

		// ===================== Selection =====================

		private bool HasSelection => m_CaretPosition != m_SelectionAnchor;

		private void CopyToClipboard() {
			if (!HasSelection) return;
			int start = Mathf.Min(m_CaretPosition, m_SelectionAnchor);
			int end = Mathf.Max(m_CaretPosition, m_SelectionAnchor);
			string selected = m_Text.Substring(start, end - start);
			if (contentType == ContentType.Password) return;
			GUIUtility.systemCopyBuffer = selected;
		}

		// ===================== Click / Drag =====================

		public void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) return;

			if (!m_HasFocus) {
				EventSystem.current.SetSelectedGameObject(gameObject);
			}

			// Position caret at click location
			PositionCaretFromPointer(eventData);

			if (eventData.clickCount == 2) {
				SelectWordAtCaret();
			} else if (eventData.clickCount == 3) {
				SelectAll();
			}
		}

		public void OnBeginDrag(PointerEventData eventData) {
			if (!m_HasFocus) return;
			m_DragSelecting = true;
			PositionCaretFromPointer(eventData);
			m_SelectionAnchor = m_CaretPosition;
		}

		public void OnDrag(PointerEventData eventData) {
			if (!m_DragSelecting) return;
			PositionCaretFromPointer(eventData, keepAnchor: true);
		}

		public void OnEndDrag(PointerEventData eventData) {
			m_DragSelecting = false;
		}

		public void OnSubmit(BaseEventData eventData) {
			if (!m_HasFocus) {
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
		}

		private void PositionCaretFromPointer(PointerEventData eventData, bool keepAnchor = false) {
			if (textComponent == null) return;

			var rt = m_TextRT != null ? m_TextRT : textComponent.GetComponent<RectTransform>();
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint)) {
				int index = textComponent.HitTestLocal(localPoint);
				m_CaretPosition = Mathf.Clamp(index, 0, m_Text.Length);
				if (!keepAnchor) m_SelectionAnchor = m_CaretPosition;
				ResetBlink();
				RefreshCaretView();
			}
		}

		private void SelectWordAtCaret() {
			if (m_Text.Length == 0) return;

			int start = m_CaretPosition;
			int end = m_CaretPosition;

			// Expand backward
			while (start > 0 && !char.IsWhiteSpace(m_Text[start - 1])) start--;
			// Expand forward
			while (end < m_Text.Length && !char.IsWhiteSpace(m_Text[end])) end++;

			m_SelectionAnchor = start;
			m_CaretPosition = end;
			RefreshCaretView();
		}

		// ===================== Display =====================

		private void UpdateDisplay() {
			string displayText = m_Text;
			if (contentType == ContentType.Password && !string.IsNullOrEmpty(displayText)) {
				displayText = new string(asteriskChar, displayText.Length);
			}

			if (textComponent != null) {
				if (richText)
					textComponent.SetRichText(displayText);
				else
					textComponent.text = displayText;
			}

			if (placeholder != null) {
				placeholder.gameObject.SetActive(string.IsNullOrEmpty(m_Text));
			}

			AutoResizeHeight();
			UpdateCaretVisual();
			ScrollToCaret();
			UpdateSelectionVisual();
		}

		private void UpdateCaretVisual() {
			if (caretImage == null || m_CaretRect == null || textComponent == null) return;
			if (!m_HasFocus || hideCaret) return;

			var textRT = textComponent.GetComponent<RectTransform>();
			Rect parentRect = textRT.rect;

			if (textComponent.GetCaretLocalRect(m_CaretPosition, out Rect localRect)) {
				m_CaretRect.SetParent(textRT, false);
				m_CaretRect.anchorMin = new Vector2(0, 0);
				m_CaretRect.anchorMax = new Vector2(0, 0);
				m_CaretRect.pivot = new Vector2(0, 0);
				// Convert from pivot-relative local coords to bottom-left anchor coords
				m_CaretRect.anchoredPosition = new Vector2(
					localRect.x - parentRect.xMin,
					localRect.y - parentRect.yMin
				);
				m_CaretRect.sizeDelta = new Vector2(caretWidth, localRect.height);
				caretImage.color = caretColor;
			} else if (m_Text.Length == 0) {
				m_CaretRect.SetParent(textRT, false);
				m_CaretRect.anchorMin = new Vector2(0, 0);
				m_CaretRect.anchorMax = new Vector2(0, 0);
				m_CaretRect.pivot = new Vector2(0, 0);
				// Place at top-left of text area
				m_CaretRect.anchoredPosition = new Vector2(4f, parentRect.height - 4f - textComponent.FontSize);
				m_CaretRect.sizeDelta = new Vector2(caretWidth, textComponent.FontSize);
				caretImage.color = caretColor;
			}
		}

		private void CreateSelectionHighlight() {
			if (m_SelectionHighlight != null) return;
			if (textComponent == null) return;

			var go = new GameObject("Selection", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			go.transform.SetParent(textComponent.transform, false);
			go.transform.SetAsFirstSibling();
			m_SelectionHighlight = go.GetComponent<Image>();
			m_SelectionHighlight.color = selectionColor;
			m_SelectionHighlight.raycastTarget = false;
			m_SelectionHighlight.gameObject.SetActive(false);
		}

		private void UpdateSelectionVisual() {
			if (m_SelectionHighlight == null) return;

			if (!HasSelection || !m_HasFocus) {
				m_SelectionHighlight.gameObject.SetActive(false);
				return;
			}

			int start = Mathf.Min(m_CaretPosition, m_SelectionAnchor);
			int end = Mathf.Max(m_CaretPosition, m_SelectionAnchor);

			var textRT = textComponent.GetComponent<RectTransform>();
			Rect parentRect = textRT.rect;

			if (textComponent.GetCaretLocalRect(start, out Rect startRect) &&
				textComponent.GetCaretLocalRect(end, out Rect endRect)) {
				m_SelectionHighlight.gameObject.SetActive(true);
				var rt = m_SelectionHighlight.GetComponent<RectTransform>();
				rt.anchorMin = new Vector2(0, 0);
				rt.anchorMax = new Vector2(0, 0);
				rt.pivot = new Vector2(0, 0);
				// Convert from pivot-relative local coords to bottom-left anchor coords
				rt.anchoredPosition = new Vector2(
					startRect.x - parentRect.xMin,
					startRect.y - parentRect.yMin
				);
				rt.sizeDelta = new Vector2(endRect.x - startRect.x, startRect.height);
			} else {
				m_SelectionHighlight.gameObject.SetActive(false);
			}
		}

		private void RefreshCaretView() {
			UpdateCaretVisual();
			ScrollToCaret();
			UpdateSelectionVisual();
			SyncKeyboardSelection();
		}

		private void ResetBlink() {
			m_BlinkTimer = 0f;
			m_CaretVisible = true;
			if (caretImage != null)
				caretImage.enabled = true;
		}

		// ===================== Validation =====================

		private bool ValidateChar(char c) {
			if (c < 32 && c != '\n') return false;

			switch (contentType) {
				case ContentType.Integer:
					return char.IsDigit(c) || (c == '-' && m_CaretPosition == 0 && !m_Text.Contains("-"));
				case ContentType.Decimal:
					return char.IsDigit(c) || (c == '.' && !m_Text.Contains("."))
						|| (c == '-' && m_CaretPosition == 0 && !m_Text.Contains("-"));
				case ContentType.Alphanumeric:
					return char.IsLetterOrDigit(c);
				case ContentType.Name:
					return char.IsLetter(c) || c == ' ' || c == '\'' || c == '-';
				case ContentType.EmailAddress:
					return char.IsLetterOrDigit(c) || c == '@' || c == '.' || c == '_' || c == '-' || c == '+';
				case ContentType.Password:
				case ContentType.Standard:
				default:
					return true;
			}
		}

		// ===================== Word Navigation =====================

		private int FindPreviousWordStart(int from) {
			if (from <= 0) return 0;
			int i = from - 1;
			// Skip whitespace
			while (i > 0 && char.IsWhiteSpace(m_Text[i])) i--;
			// Skip word
			while (i > 0 && !char.IsWhiteSpace(m_Text[i - 1])) i--;
			return i;
		}

		private int FindNextWordEnd(int from) {
			if (from >= m_Text.Length) return m_Text.Length;
			int i = from;
			// Skip word
			while (i < m_Text.Length && !char.IsWhiteSpace(m_Text[i])) i++;
			// Skip whitespace
			while (i < m_Text.Length && char.IsWhiteSpace(m_Text[i])) i++;
			return i;
		}
	}
}
