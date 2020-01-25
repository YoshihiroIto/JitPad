using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using JitPad.Core.Processor;
using Microsoft.Xaml.Behaviors;

namespace JitPad.Behaviors
{
    public sealed class TextEditorCompletionBehavior : Behavior<TextEditor>
    {
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

        private CompletionWindow? _completionWindow;

        private void TextAreaOnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (_completionWindow == null || e.Text.Length == 0)
                return;

            if (IsCharIdentifier(e.Text[0]) == false)
                _completionWindow.CompletionList.RequestInsertion(e);

            static bool IsCharIdentifier(char c) => char.IsLetterOrDigit(c) || c == '_';
        }

        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs e)
        {
            var offset = AssociatedObject.CaretOffset;
            var c = AssociatedObject.Document.GetCharAt(offset - 1);

            ShowCompletionWindow(c);
        }

        private void AssociatedObjectOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                ShowCompletionWindow(null);
            }
        }

        private readonly CodeCompleter _CodeCompleter = new CodeCompleter();

        private void ShowCompletionWindow(char? completionChar)
        {
            if (_completionWindow != null)
                return;

            var text = AssociatedObject.Text;
            var offset = AssociatedObject.CaretOffset;

            Task.Run(async () =>
            {
                var results = await _CodeCompleter.CompleteAsync(text, offset, completionChar)
                    .ConfigureAwait(true);

                if (results.CompletionData.Length > 0)
                {
                    App.UiDispatcher.Invoke(() =>
                    {
                        _completionWindow =
                            new CompletionWindow(AssociatedObject.TextArea)
                            {
                                MinWidth = 300,
                                CloseWhenCaretAtBeginning = true,
                            };

                        if (ToolToolField != null)
                        {
                            var toolTip = (ToolTip) ToolToolField.GetValue(_completionWindow);
                            toolTip.Placement = PlacementMode.Left;
                            toolTip.VerticalOffset = 0;
                            toolTip.HorizontalOffset = 4;
                        }

                        _completionWindow.Closed += (_, __) => _completionWindow = null;

                        _completionWindow.Loaded += (sender, __) =>
                        {
                            var listBox = Descendants((DependencyObject) sender).OfType<ListBox>().FirstOrDefault();
                            if (listBox != null)
                                listBox.Background = Brushes.Transparent;
                        };

                        if (results.CompletionData.Length > 0)
                        {
                            if (completionChar != null && char.IsLetterOrDigit(completionChar.Value))
                                _completionWindow.StartOffset -= 1;

                            foreach (var item in results.CompletionData)
                                _completionWindow.CompletionList.CompletionData.Add(item: new CompletionData(item));

                            _completionWindow.Show();
                        }
                    });
                }
            });
        }

        private static readonly FieldInfo ToolToolField = typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance);

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
        public object Content => _data.Item.DisplayText;

        public object Description
        {
            get
            {
                if (_description == null)
                {
                    _description = new Decorator();
                    _description.Loaded += (o, e) =>
                    {
                        // ReSharper disable once UnusedVariable
                        var task = _descriptionTask.Value;
                    };
                }

                return _description;
            }
        }

        public ImageSource? Image { get; } = null;

        public double Priority { get; } = 0.0;

        public string Text => _data.Item.DisplayText;

        private readonly CompleteData _data;

        public CompletionData(CompleteData data)
        {
            _data = data;
            _descriptionTask = new Lazy<Task>(RetrieveDescriptionAsync);
        }

        private async Task RetrieveDescriptionAsync()
        {
            if (_description != null)
            {
                var description = await Task.Run(() => _data.CompletionService.GetDescriptionAsync(_data.Document, _data.Item)).ConfigureAwait(true);
                _description.Child = description.TaggedParts.ToTextBlock();
            }
        }

        private Decorator? _description;
        private readonly Lazy<Task> _descriptionTask;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // ReSharper disable once AsyncConverter.AsyncWait
            var changes = _data.CompletionService.GetChangeAsync(_data.Document, _data.Item, null).Result;

            var document = textArea.Document;

            using (document.RunUpdate())
            {
                var textChange = changes.TextChange;

                if (completionSegment.EndOffset > textChange.Span.End)
                {
                    document.Replace(
                        new TextSegment
                        {
                            StartOffset = textChange.Span.End,
                            EndOffset = completionSegment.EndOffset
                        },
                        string.Empty);
                }

                document.Replace(textChange.Span.Start, textChange.Span.Length, new StringTextSource(textChange.NewText));
            }

            if (changes.NewPosition != null)
                textArea.Caret.Offset = changes.NewPosition.Value;
        }
    }
}