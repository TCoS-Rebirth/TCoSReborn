using PackageExtractor;

public interface ISBSerializable
{
    void Deserialize(SBFileReader reader);
}