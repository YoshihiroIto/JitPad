using System;
using System.Windows;
using ICSharpCode.AvalonEdit;
using JitPad.Core.Interface;
using Microsoft.Xaml.Behaviors;

namespace JitPad.Behaviors
{
    public sealed class SourceCodeTextEditorBehavior : Behavior<TextEditor>
    {
        #region BuildDetailMessages

        public CompileResult.Message[] BuildDetailMessages
        {
            get => _BuildDetailMessages;
            set
            {
                if (value != _BuildDetailMessages)
                    SetValue(BuildDetailMessagesProperty, value);
            }
        }

        private CompileResult.Message[] _BuildDetailMessages = Array.Empty<CompileResult.Message>();

        public static readonly DependencyProperty BuildDetailMessagesProperty =
            DependencyProperty.Register(
                nameof(BuildDetailMessages),
                typeof(CompileResult.Message[]),
                typeof(SourceCodeTextEditorBehavior),
                new PropertyMetadata(
                    Array.Empty<CompileResult.Message>(),
                    (s, e) =>
                    {
                        var self = (SourceCodeTextEditorBehavior) s;
                        self._BuildDetailMessages = (CompileResult.Message[]) e.NewValue;

                        self._backgroundRenderer.BuildDetailMessages = self._BuildDetailMessages;
                        self.AssociatedObject.TextArea.TextView.InvalidateVisual();
                    }));

        #endregion
        
        private readonly SourceCodeTextEditorBackgroundRenderer _backgroundRenderer = new SourceCodeTextEditorBackgroundRenderer();

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.TextArea.TextView.BackgroundRenderers.Add(_backgroundRenderer);
        }
    }
}