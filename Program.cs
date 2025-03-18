
using System.Text;
using Antlr4.Runtime;
using rem_frontend_generator.generators;
using rem_frontend_generator.language;

class Program
{
    public static languageParser.SourceFileContext get_parse_tree(string file_source)
    {
        ICharStream stream = CharStreams.fromString(file_source);
        ITokenSource lexer = new languageLexer(stream);
        ITokenStream tokens = new CommonTokenStream(lexer);
        languageParser parser = new languageParser(tokens);
        languageParser.SourceFileContext tree = parser.sourceFile();

        return tree;
    }

    static string read_all_files(string path)
    {
        string[] files = Directory.GetFiles(path);

        StringBuilder result = new StringBuilder();

        foreach (string file in files)
        {
            result.AppendLine(File.ReadAllText(file));
        }

        return result.ToString();
    }

    static void Main()
    {
        string source = read_all_files("/media/linvirt/partish/rem_frontend_generator/code/aarch64");

        languageParser.SourceFileContext parse = get_parse_tree(source);

        ast_builder builder = new ast_builder();

        source_file sf = builder.Visit(parse) as source_file;

        rem_generator generator = new rem_generator();

        generator.generate_files(sf);

        generator.store_to("/media/linvirt/partish/rem/src/emulator/aarch64/", "aarch64_impl");

        Console.WriteLine("Success");
    }
}