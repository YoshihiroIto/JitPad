using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using JitPad.Core;
using Microsoft.Xaml.Behaviors;

namespace JitPad.Behaviors
{
    public sealed class TextEditorCompletionBehavior : Behavior<TextEditor>
    {
        // ref: https://pierre3.hatenablog.com/entry/2016/07/28/001230

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.TextArea.TextEntered += TextAreaOnTextEntered;
            AssociatedObject.TextArea.TextEntering += TextAreaOnTextEntering;
            AssociatedObject.KeyDown += AssociatedObjectOnKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.TextArea.TextEntered -= TextAreaOnTextEntered;
            AssociatedObject.TextArea.TextEntering -= TextAreaOnTextEntering;
            AssociatedObject.KeyDown -= AssociatedObjectOnKeyDown;
        }

        private CompletionWindow? _CompletionWindow;

        private void TextAreaOnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length == 0 || _CompletionWindow == null)
                return;

            if (char.IsLetterOrDigit(e.Text[0]) == false)
                _CompletionWindow.CompletionList.RequestInsertion(e);
        }

        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
                ShowCompletionWindow();
        }

        private void AssociatedObjectOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                ShowCompletionWindow();
            }
        }

        private void ShowCompletionWindow()
        {
            var text = AssociatedObject.Text;
            var offset = AssociatedObject.TextArea.Caret.Offset;

            Task.Run(async () =>
            {
                var codeCompleter = new CodeCompleter();

                var items = await codeCompleter.CompleteAsync(text, offset)
                    .ConfigureAwait(false);

                await App.UiDispatcher.InvokeAsync(() =>
                {
                    _CompletionWindow = new CompletionWindow(AssociatedObject.TextArea);
                    _CompletionWindow.Closed += (_, __) => _CompletionWindow = null;

                    _CompletionWindow.Loaded += (sender, __) =>
                    {
                        var listBox = Descendants((DependencyObject)sender).OfType<ListBox>().FirstOrDefault();
                        if (listBox != null)
                            listBox.Background = Brushes.Transparent;
                    };

                    if (items.Length > 0)
                    {
                        foreach (var item in items)
                            _CompletionWindow.CompletionList.CompletionData.Add(item: new CompletionData(item));

                        _CompletionWindow.Show();
                    }
                });
            });
        }

        // Biaui
        private static IEnumerable<DependencyObject> Children(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj is Popup popup)
            {
                if (popup.Child != null)
                    yield return popup.Child;
            }

            var count = VisualTreeHelper.GetChildrenCount(obj);
            if (count == 0)
                yield break;

            for (var i = 0; i != count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                yield return child;
            }
        }

        private static IEnumerable<DependencyObject> Descendants(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            foreach (var child in Children(obj))
            {
                yield return child;

                foreach (var grandChild in Descendants(child))
                    yield return grandChild;
            }
        }
    }

    public class CompletionData : ICompletionData
    {
        public object Content { get; }

        public object Description { get; }

        public ImageSource? Image { get; }

        public double Priority { get; } = 0.0;

        public string Text { get; }

        public CompletionData(CompleteItem item)
        {
            Content = item.Content;
            Text = item.Text;
            Image = null;
            Description = item.Description;
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}