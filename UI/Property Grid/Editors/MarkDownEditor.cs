using Grooper;
using System.ComponentModel;

namespace GrooperGit
{
    internal class MarkDownEditor : PgCodeEditor<GitProject>
    {
        public override CodeHighlighter Highlighter { get; } = new XshdHighlighter("MarkDown-Mode");
        public override bool? ShowControlChars { get; } = false;
        protected override void ConfigureEditor(CodeEditor Editor, GitProject Instance, PropertyDescriptor pd)
        {
            Editor.HighlightMode = CodeEditor.HighlightingMode.MarkDown;
        }
        public override CompletionProvider GetCompletionProvider(object instance, PropertyDescriptor pd)
        {
            // Uncomment and adjust as needed
            // return new CssCompletionProvider(instance);
            return null;
        }
    }
}
