using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Biaui;
using ICSharpCode.AvalonEdit.Rendering;
using JitPad.Core.Interface;

namespace JitPad
{
    public class SourceCodeTextEditorBackgroundRenderer : IBackgroundRenderer
    {
        public KnownLayer Layer => KnownLayer.Background;

        public CompileResult.Message[] BuildDetailMessages { get; set; } = Array.Empty<CompileResult.Message>();

        public void Draw(TextView textView, DrawingContext dc)
        {
            var messages = BuildDetailMessages.ToLookup(x => x.StartLine);

            var pen = Caches.GetPen(ByteColor.Tomato, 2);
            var lineNo = 0;

            foreach (var visualLine in textView.VisualLines)
            {
                if (messages.Contains(lineNo))
                {
                    foreach (var message in messages[lineNo])
                    {
                        var end = message.EndCharacter;
                        if (end == message.StartCharacter)
                            ++end;

                        var segments = BackgroundGeometryBuilder.GetRectsFromVisualSegment(
                            textView,
                            visualLine,
                            message.StartCharacter,
                            end);

                        foreach (var segment in segments)
                            dc.DrawLine(
                                pen,
                                new Point(segment.Left, segment.Bottom - 1),
                                new Point(segment.Right, segment.Bottom - 1));
                    }
                }

                ++lineNo;
            }
        }
    }
}