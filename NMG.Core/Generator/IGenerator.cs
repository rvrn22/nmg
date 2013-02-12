namespace NMG.Core.Generator
{
    public interface IGenerator
    {
        void Generate(bool writeToFile = true);
    }
}