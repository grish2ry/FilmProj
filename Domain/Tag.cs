namespace Domain;
public class Tag
{
    public string TagCtx { get; private set; }
    public Tag(string tag)
    {
        TagCtx = tag;
    }
}