namespace HIA.Core.HTML;

public enum NodeType
{
    Element = 1,
    Attribute = 2,
    Text = 3,
    Comment = 8,
}

public enum TagType
{
    Opening,
    Closing,
    SelfClosed,
    Text
}
