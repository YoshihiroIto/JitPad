using System;
using System.Windows;
using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;

namespace JitPad.Behaviors
{
    public sealed class TextEditorBehavior : Behavior<TextEditor>
    {
        #region Text

        public string Text
        {
            get => _Text;
            set
            {
                if (value != _Text)
                    SetValue(TextProperty, value);
            }
        }

        private string _Text = "";

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(TextEditorBehavior),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        var self = (TextEditorBehavior) s;
                        self._Text = (string) e.NewValue;

                        var editor = self.AssociatedObject;
                        if (editor.Document.Text != self.Text)
                        {
                            var caretOffset = editor.CaretOffset;
                            editor.Document.Text = self.Text;
                            editor.CaretOffset = Math.Min(editor.Document.TextLength, caretOffset);
                        }
                    }
                ));

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            
            if (AssociatedObject.IsReadOnly)
                AssociatedObject.Document.UndoStack.SizeLimit = 0;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
        }

        private void AssociatedObjectOnTextChanged(object? sender, EventArgs eventArgs)
        {
            if (sender is TextEditor textEditor)
                if (textEditor.Document != null)
                    Text = textEditor.Document.Text;
        }
    }
}