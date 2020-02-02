using System;
using System.Collections;
using System.Diagnostics;
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
using JitPad.Core.Interface;
using JitPad.Core.Processor;
using Microsoft.Xaml.Behaviors;

// ReSharper disable RedundantUsingDirective

// fot Descendants
using System.Linq;
using Biaui.Internals;

// ReSharper restore RedundantUsingDirective

namespace JitPad.Behaviors
{
    public sealed class TextEditorCompletionBehavior : Behavior<TextEditor>
    {
        #region Compiler

        public ICompiler Compiler
        {
            get => _Compiler ?? throw new NullReferenceException();
            set
            {
                if (value != _Compiler)
                    SetValue(CompilerProperty, value);
            }
        }

        private ICompiler? _Compiler;

        public static readonly DependencyProperty CompilerProperty =
            DependencyProperty.Register(
                nameof(Compiler),
                typeof(ICompiler),
                typeof(TextEditorCompletionBehavior),
                new PropertyMetadata(
                    default,
                    (s, e) =>
                    {
                        var self = (TextEditorCompletionBehavior) s;
                        self._Compiler = (ICompiler) e.NewValue;

                        self._codeCompleter?.Dispose();
                        self._codeCompleter = new CodeCompleter(self._Compiler);
                    }));

        #endregion

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

        public void CloseCompletionWindow()
        {
            _completionWindow?.Close();
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

        private CodeCompleter? _codeCompleter;

        private void ShowCompletionWindow(char? completionChar)
        {
            if (_completionWindow != null)
                return;

            var text = AssociatedObject.Text;
            var offset = AssociatedObject.CaretOffset;

            Task.Run(async () =>
            {
                if (_codeCompleter == null)
                    return;

                var results = await _codeCompleter.CompleteAsync(text, offset, completionChar)
                    .ConfigureAwait(false);

                if (results.Length == 0)
                    return;

                App.UiDispatcher.Invoke(() =>
                {
                    _completionWindow =
                        new CompletionWindow(AssociatedObject.TextArea)
                        {
                            MinWidth = 300,
                            CloseWhenCaretAtBeginning = true,
                        };

                    if (ToolToolField?.GetValue(_completionWindow) is ToolTip toolTip)
                    {
                        toolTip.Placement = PlacementMode.Left;
                        toolTip.VerticalOffset = 0;
                        toolTip.HorizontalOffset = 4;
                    }

                    _completionWindow.Closed += (_, __) => _completionWindow = null;

                    _completionWindow.Loaded += (sender, __) =>
                    {
                        var listBox = ((DependencyObject) sender).Descendants().OfType<ListBox>().FirstOrDefault();
                        if (listBox != null)
                            listBox.Background = Brushes.Transparent;
                    };

                    if (_itemsSourceChangeNotifier != null)
                        _itemsSourceChangeNotifier.ValueChanged -= ItemsSourceChangeNotifierOnValueChanged;

                    _itemsSourceChangeNotifier = new PropertyChangeNotifier(_completionWindow.CompletionList.ListBox, ItemsControl.ItemsSourceProperty);
                    _itemsSourceChangeNotifier.ValueChanged += ItemsSourceChangeNotifierOnValueChanged;

                    if (completionChar != null && char.IsLetterOrDigit(completionChar.Value))
                        _completionWindow.StartOffset -= 1;

                    foreach (var item in results)
                        _completionWindow.CompletionList.CompletionData.Add(item: new CompletionData(item));

                    if (completionChar == null)
                        if (_completionWindow.CompletionList.CompletionData.Count > 0)
                            if (_completionWindow.CompletionList.SelectedItem == null)
                                _completionWindow.CompletionList.SelectedItem = _completionWindow.CompletionList.CompletionData[0];

                    _completionWindow.Show();
                });
            });
        }

        private void ItemsSourceChangeNotifierOnValueChanged(object? sender, EventArgs e)
        {
            if (!(_completionWindow?.CompletionList?.ListBox?.ItemsSource is ICollection collection))
                return;

            var visibility = collection.Count == 0 ? Visibility.Hidden : Visibility.Visible;

            _completionWindow.Visibility = visibility;

            if (ToolToolField?.GetValue(_completionWindow) is ToolTip toolTip)
                toolTip.Visibility = visibility;
        }

        private PropertyChangeNotifier? _itemsSourceChangeNotifier;

        private static readonly FieldInfo? ToolToolField = typeof(CompletionWindow).GetField("toolTip", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public class CompletionData : ICompletionData
    {
        public object Description
        {
            get
            {
                if (_description == null)
                {
                    _description = new Decorator();
                    _description.Loaded += DescriptionOnLoaded;
                }

                return _description;
            }
        }

        public object Content => _data.Item.DisplayText;
        public ImageSource? Image { get; } = null;
        public double Priority { get; } = 0.0;
        public string Text => _data.Item.DisplayText;

        private readonly CompleteData _data;
        private Decorator? _description;

        public CompletionData(CompleteData data)
        {
            _data = data;
        }

        private void DescriptionOnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(_description != null);

            _description.Loaded -= DescriptionOnLoaded;

            Task.Run(async () =>
            {
                var description = await _data.CompletionService.GetDescriptionAsync(_data.Document, _data.Item).ConfigureAwait(false);
                App.UiDispatcher.Invoke(() => _description.Child = description.TaggedParts.ToTextBlock());
            });
        }

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