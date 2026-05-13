using Domain;
namespace Infrostruture.Readers;
public class TagCodeReader{
    public Dictionary<int, Tag> TagCode {get; private set; }

    public TagCodeReader()
    {
        TagCode = new Dictionary<int, Tag>();
    }

    public void ReadTags(string path = "BD\\ml-latest\\TagCodes_MovieLens.csv")
    {
        foreach(var l in File.ReadLines(path).Skip(1))
        {
            var parts = l.Split(',');
            if (parts.Length < 2) continue;
            var tagCtx = parts[1];
            if (!int.TryParse(parts[0], out var tagId)) continue;
            if (!TagCode.ContainsKey(tagId))
                TagCode.Add(tagId, new Tag(tagCtx));
        }
    }
}
