using System;
using System.Collections.Generic;
using System.Text;

abstract class LightNode
{
    public string OuterHTML => GenerateOuterHTML();
    public string InnerHTML => GenerateInnerHTML();

    protected abstract string GenerateOuterHTML();
    protected abstract string GenerateInnerHTML();

   public abstract void Accept(IVisitor visitor); 
}

class LightTextNode : LightNode
{
    public string Text { get; }

    public LightTextNode(string text) => Text = text;

    protected override string GenerateOuterHTML() => Text;
    protected override string GenerateInnerHTML() => Text;

    public override void Accept(IVisitor visitor) => visitor.VisitText(this);
}

enum TagType { SelfClosing, Paired }

class LightElementNode : LightNode
{
    public string TagName { get; }
    public TagType TagKind { get; }
    public List<string> CssClasses { get; } = new List<string>();
    public LightNodeCollection Children { get; } = new LightNodeCollection();

    private IDisplayState _displayState = new BlockState();
    public void SetDisplayState(IDisplayState state) => _displayState = state;
    public string Render() => _displayState.Render(this);

    public LightElementNode(string tagName, TagType tagKind)
    {
        TagName = tagName;
        TagKind = tagKind;
    }

    public void AddClass(string className) => CssClasses.Add(className);

    public void AddChild(LightNode child)
    {
        if (TagKind == TagType.SelfClosing)
            throw new InvalidOperationException("Self-closing tags can't have children.");
        Children.Add(child);
    }

    protected override string GenerateInnerHTML()
    {
        var sb = new StringBuilder();
        foreach (var child in Children)
            sb.Append(child.OuterHTML);
        return sb.ToString();
    }

    protected override string GenerateOuterHTML()
    {
        var classAttr = CssClasses.Count > 0 ? $" class=\"{string.Join(" ", CssClasses)}\"" : "";
        if (TagKind == TagType.SelfClosing)
            return $"<{TagName}{classAttr} />";
        else
            return $"<{TagName}{classAttr}>{InnerHTML}</{TagName}>";
    }

    public override void Accept(IVisitor visitor) => visitor.VisitElement(this);
}

class LightNodeIterator : IEnumerator<LightNode>
{
    private readonly List<LightNode> _nodes;
    private int _position = -1;

    public LightNodeIterator(List<LightNode> nodes) => _nodes = nodes;

    public LightNode Current => _nodes[_position];
    object System.Collections.IEnumerator.Current => Current;
    public bool MoveNext() => ++_position < _nodes.Count;
    public void Reset() => _position = -1;
    public void Dispose() { }
}

class LightNodeCollection : IEnumerable<LightNode>
{
    private readonly List<LightNode> _nodes = new List<LightNode>();
    public void Add(LightNode node) => _nodes.Add(node);
    public IEnumerator<LightNode> GetEnumerator() => new LightNodeIterator(_nodes);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => _nodes.Count;
}

interface ICommand
{
    void Execute();
}

class AddClassCommand : ICommand
{
    private readonly LightElementNode _element;
    private readonly string _className;

    public AddClassCommand(LightElementNode element, string className)
    {
        _element = element;
        _className = className;
    }

    public void Execute() => _element.AddClass(_className);
}

interface IDisplayState
{
    string Render(LightElementNode node);
}

class BlockState : IDisplayState
{
    public string Render(LightElementNode node) => $"<div>{node.InnerHTML}</div>";
}

class InlineState : IDisplayState
{
    public string Render(LightElementNode node) => $"<span>{node.InnerHTML}</span>";
}

interface IVisitor
{
    void VisitText(LightTextNode textNode);
    void VisitElement(LightElementNode elementNode);
}

class TagCountVisitor : IVisitor
{
    public int Count { get; private set; } = 0;

    public void VisitText(LightTextNode textNode) { }

    public void VisitElement(LightElementNode elementNode)
    {
        Count++;
        foreach (var child in elementNode.Children)
            child.Accept(this);
    }
}


class Program
{
    static void Main()
    {

        var ul = new LightElementNode("ul", TagType.Paired);
        ul.AddClass("shopping-list");

        ul.SetDisplayState(new InlineState());

        var li = new LightElementNode("li", TagType.Paired);
        li.AddChild(new LightTextNode("Яблука"));
        ul.AddChild(li);

        var command = new AddClassCommand(ul, "highlighted");
        command.Execute();

        var visitor = new TagCountVisitor();
        ul.Accept(visitor);

        Console.WriteLine("== OuterHTML ==");
        Console.WriteLine(ul.OuterHTML);

        Console.WriteLine("\n== InnerHTML ==");
        Console.WriteLine(ul.InnerHTML);

        Console.WriteLine("\n== Render() ==");
        Console.WriteLine(ul.Render());

        Console.WriteLine("\n== Теги (Visitor) ==");
        Console.WriteLine(visitor.Count);

    }
}
