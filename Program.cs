
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
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
        string source = read_all_files("/home/linvirt/code/rem_frontend_generator/code/aarch64");

        languageParser.SourceFileContext parse = get_parse_tree(source);

        ast_builder builder = new ast_builder();

        source_file sf = builder.Visit(parse) as source_file;

        cpp_interpreter_generator i_generator = new cpp_interpreter_generator();
        rem_jit_generator j_generator = new rem_jit_generator();

        i_generator.generate(sf);
        j_generator.generate(sf);

        File.WriteAllText("/home/linvirt/code/rem/src/emulator/aarch64/interpreter/interpreter.cpp",i_generator.cpp_file.ToString());
        File.WriteAllText("/home/linvirt/code/rem/src/emulator/aarch64/interpreter/interpreter.h",i_generator.header_file.ToString());

        File.WriteAllText("/home/linvirt/code/rem/src/emulator/aarch64/jit/jit.cpp",j_generator.cpp_file.ToString());
        File.WriteAllText("/home/linvirt/code/rem/src/emulator/aarch64/jit/jit.h",j_generator.header_file.ToString());


        //File.WriteAllText("/home/linvirt/code/rem/src/emulator/aarch64/jit.h",v.header_file.ToString());
        //File.WriteAllText("/home/linvirt/code/rem/src/emulator/aarch64/jit.cpp",v.cpp_file.ToString());
    }
}