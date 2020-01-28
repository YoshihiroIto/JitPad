using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;

namespace JitPad.Behaviors
{
    public sealed class MainWindowBehavior : Behavior<MainWindow>
    {
        #region CodeEditor

        public TextEditor? CodeEditor
        {
            get => _CodeEditor;
            set
            {
                if (value != _CodeEditor)
                    SetValue(CodeEditorProperty, value);
            }
        }

        private TextEditor? _CodeEditor;

        public static readonly DependencyProperty CodeEditorProperty =
            DependencyProperty.Register(
                nameof(CodeEditor),
                typeof(TextEditor),
                typeof(MainWindowBehavior),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        var self = (MainWindowBehavior) s;
                        self._CodeEditor = (TextEditor) e.NewValue;
                    }));

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewKeyDown += AssociatedObjectOnPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewKeyDown -= AssociatedObjectOnPreviewKeyDown;
        }

        private void AssociatedObjectOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                CodeEditor?.Undo();
                e.Handled = true;
            }
            else if (e.Key == Key.Y && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                CodeEditor?.Redo();
                e.Handled = true;
            }
        }
    }
}