using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.CodeAnalysis;

namespace JitPad.Behaviors
{
    // ref: RoslynPad -- https://github.com/aelij/RoslynPad

    internal static class SymbolDisplayPartExtensions
    {
        private const string LeftToRightMarkerPrefix = "\u200e";

        public static string ToVisibleDisplayString(this TaggedText part, bool includeLeftToRightMarker)
        {
            var text = part.ToString();

            if (includeLeftToRightMarker)
            {
                if (part.Tag == TextTags.Punctuation ||
                    part.Tag == TextTags.Space ||
                    part.Tag == TextTags.LineBreak)
                {
                    text = LeftToRightMarkerPrefix + text;
                }
            }

            return text;
        }

        public static Run ToRun(this TaggedText text, bool isBold = false)
        {
            var s = text.ToVisibleDisplayString(includeLeftToRightMarker: true);

            var run = new Run(s);

            if (isBold)
            {
                run.FontWeight = FontWeights.Bold;
            }

            run.Foreground = text.Tag switch
            {
                TextTags.Keyword => Brushes.Blue,
                TextTags.Struct => Brushes.Teal,
                TextTags.Enum => Brushes.Teal,
                TextTags.TypeParameter => Brushes.Teal,
                TextTags.Class => Brushes.Teal,
                TextTags.Delegate => Brushes.Teal,
                TextTags.Interface => Brushes.Teal,
                _ => run.Foreground
            };

            return run;
        }

        public static TextBlock ToTextBlock(this IEnumerable<TaggedText> text, bool isBold = false)
        {
            var result = new TextBlock {TextWrapping = TextWrapping.Wrap};

            foreach (var part in text)
            {
                result.Inlines.Add(part.ToRun(isBold));
            }

            return result;
        }
    }
}