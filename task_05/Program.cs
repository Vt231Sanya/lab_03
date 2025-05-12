using System;
using System.Collections.Generic;
using System.Text;

abstract class LightNode
{
    public string OuterHTML => GenerateOuterHTML();
    public string InnerHTML => GenerateInnerHTML();

    protected abstract string GenerateOuterHTML();
    protected abstract string GenerateInnerHTML();

    //public abstract void Accept(IVisitor visitor); 
}

class LightTextNode : LightNode
{
    public string Text { get; }

    public LightTextNode(string text) => Text = text;

    protected override string GenerateOuterHTML() => Text;
    protected override string GenerateInnerHTML() => Text;

    //public override void Accept(IVisitor visitor) { } 
}

enum TagType { SelfClosing, Paired }

class LightElementNode : LightNode
{
    public string TagName { get; }
    public TagType TagKind { get; }
    public List<string> CssClasses { get; } = new List<string>();
    public List<LightNode> Children { get; } = new List<LightNode>();

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

    //public override void Accept(IVisitor visitor) { }
}

class Program
{
    static void Main()
    {
        var ul = new LightElementNode("ul", TagType.Paired);
        ul.AddClass("shopping-list");

        var li = new LightElementNode("li", TagType.Paired);
        li.AddChild(new LightTextNode("Яблука"));
        ul.AddChild(li);

        Console.WriteLine(ul.OuterHTML);
    }
}
