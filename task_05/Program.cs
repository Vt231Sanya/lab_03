using System;
using System.Collections.Generic;
using System.Text;

abstract class LightNode
{
    public abstract string OuterHTML { get; }
    public abstract string InnerHTML { get; }
}

class LightTextNode : LightNode
{
    public string Text { get; }

    public LightTextNode(string text)
    {
        Text = text;
    }

    public override string OuterHTML => Text;
    public override string InnerHTML => Text;
}

enum DisplayType
{
    Block,
    Inline
}

enum TagType
{
    SelfClosing,
    Paired
}

class LightElementNode : LightNode
{
    public string TagName { get; }
    public DisplayType Display { get; }
    public TagType TagKind { get; }
    public List<string> CssClasses { get; } = new List<string>();
    public List<LightNode> Children { get; } = new List<LightNode>();

    public LightElementNode(string tagName, DisplayType display, TagType tagKind)
    {
        TagName = tagName;
        Display = display;
        TagKind = tagKind;
    }

    public void AddClass(string className)
    {
        CssClasses.Add(className);
    }

    public void AddChild(LightNode child)
    {
        if (TagKind == TagType.SelfClosing)
            throw new InvalidOperationException("Self-closing tags can't have children.");
        Children.Add(child);
    }

    public int ChildCount => Children.Count;

    public override string InnerHTML
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var child in Children)
            {
                sb.Append(child.OuterHTML);
            }
            return sb.ToString();
        }
    }

    public override string OuterHTML
    {
        get
        {
            var classAttr = CssClasses.Count > 0 ? $" class=\"{string.Join(" ", CssClasses)}\"" : "";
            if (TagKind == TagType.SelfClosing)
            {
                return $"<{TagName}{classAttr} />";
            }
            else
            {
                return $"<{TagName}{classAttr}>{InnerHTML}</{TagName}>";
            }
        }
    }
}

class Program
{
    static void Main()
    {
        var ul = new LightElementNode("ul", DisplayType.Block, TagType.Paired);
        ul.AddClass("shopping-list");

        var li1 = new LightElementNode("li", DisplayType.Block, TagType.Paired);
        li1.AddChild(new LightTextNode("Яблука"));

        var li2 = new LightElementNode("li", DisplayType.Block, TagType.Paired);
        li2.AddChild(new LightTextNode("Хліб"));

        var li3 = new LightElementNode("li", DisplayType.Block, TagType.Paired);
        li3.AddChild(new LightTextNode("Молоко"));

        ul.AddChild(li1);
        ul.AddChild(li2);
        ul.AddChild(li3);

        Console.WriteLine("OuterHTML:");
        Console.WriteLine(ul.OuterHTML);
        Console.WriteLine("\nInnerHTML:");
        Console.WriteLine(ul.InnerHTML);
    }
}
