using UnityEngine;
using UnityEngine.UIElements;

namespace Yueby.QuickActions.UIElements
{
    public class ActionButton : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<ActionButton>
        {
        }

        private Button _button;
        private VisualElement _checkMark;
        private Label _label;

        public Button Button => _button;

        public ActionButton()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("UIDocuments/ActionButton");
            visualTree.CloneTree(this);

            _button = this.Q<Button>();
            _checkMark = this.Q<VisualElement>("check-mark");
            _label = this.Q<Label>();

            // Hide checkmark by default
            SetChecked(false);
            SetShowCheckmark(false);
        }

        public void SetChecked(bool isChecked)
        {
            if (_checkMark != null)
            {
                if (isChecked)
                {
                    // Checked state: blue, fully opaque
                    _checkMark.style.backgroundColor = new StyleColor(new Color(64f / 255f, 160f / 255f, 254f / 255f, 1f));
                    _checkMark.style.opacity = 1f;
                }
                else
                {
                    // Unchecked state: gray, semi-transparent
                    _checkMark.style.backgroundColor = new StyleColor(new Color(149f / 255f, 149f / 255f, 149f / 255f, 1f));
                    _checkMark.style.opacity = 0.6f;
                }
            }
        }

        public void SetShowCheckmark(bool show)
        {
            if (_checkMark != null)
            {
                _checkMark.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public void SetText(string text)
        {
            if (_label != null)
            {
                _label.text = text;
            }
        }

    }
}