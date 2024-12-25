using System.Numerics;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static languageParser;

namespace rem_frontend_generator.language
{
    public class ast_builder : languageBaseVisitor<i_ast_object>
    {
        Stack<scope> scope_stack;

        public ast_builder()
        {
            scope_stack = new Stack<scope>();
        }
        
        void push_scope(scope working_scope)
        {
            scope_stack.Push(working_scope);
        }

        void pop_scope()
        {
            scope_stack.Pop();
        }

        scope top_scope()
        {
            return scope_stack.First();
        }

        public override i_ast_object VisitIdentifierExpression([NotNull] IdentifierExpressionContext context)
        {
            i_ast_object reference = top_scope().get_scoped_object(context.GetText());

            return new object_reference(reference);
        }

        public override i_ast_object VisitParenthesis([NotNull] ParenthesisContext context)
        {
            return new unary_operation("()", Visit(context.expression()) as expression);
        }

        public override i_ast_object VisitNumber([NotNull] NumberContext context)
        {
            number result = new number(BigInteger.Parse(context.GetText()));

            return result;
        }

        public override i_ast_object VisitReturnStatement([NotNull] ReturnStatementContext context)
        {
            if (context.expression() == null)
            {
                return new return_statement(null, top_scope().get_working_function().return_type);
            }
            else
            {
                return new return_statement(Visit(context.expression()) as expression, top_scope().get_working_function().return_type);
            }
        }

        public override i_ast_object VisitExpression([NotNull] ExpressionContext context)
        {
            if (context.ChildCount == 1)
            {
                var result = Visit(context.GetChild(0));

                return result;
            }
            else if (context.ChildCount == 3)
            {
                expression left = Visit(context.expression(0)) as expression;
                expression right = Visit(context.expression(1)) as expression;

                return new binary_operation(left, right, context.GetChild(1).GetText());
            }
            else
            {
                throw new Exception();
            }
        }

        public override i_ast_object VisitUnaryExpression([NotNull] UnaryExpressionContext context)
        {
            return new unary_operation(context.GetChild(0).GetText(), Visit(context.baseExpression()) as expression);
        }

        public override i_ast_object VisitCast([NotNull] CastContext context)
        {
            return new cast(Visit(context.variableType()) as variable_type, Visit(context.expression()) as expression);
        }

        public override i_ast_object VisitLValueSet([NotNull] LValueSetContext context)
        {
            l_value_set result = new l_value_set();

            result.l_value = new object_reference(top_scope().get_scoped_object(context.identifier().GetText()));
            result.r_value = Visit(context.expression()) as expression;

            return result;
        }

        public override i_ast_object VisitIfStatement([NotNull] IfStatementContext context)
        {
            i_ast_object condition = Visit(context.parenthesis());

            i_ast_object yes = Visit(context.line());
            i_ast_object no = context.elseStatement() == null ? null : Visit(context.elseStatement());

            return new if_statment() {condition = condition as expression, yes = yes, no = no};
        }

        public override i_ast_object VisitRuntimeTypeSwitch([NotNull] RuntimeTypeSwitchContext context)
        {
            runtime_type_switch result = new runtime_type_switch();

            result.test = Visit(context.expression()) as expression;

            foreach (var i in context.runtimeTypeCase())
            {
                generic_declaration_case working_case = new generic_declaration_case();

                working_case.condition = Visit(i.expression()) as expression;
                working_case.type = Visit(i.runtimeTypeReference()) as variable_type;

                result.conditions.Add(working_case);
            }

            return result;
        }

        public override i_ast_object VisitTrueFalse([NotNull] TrueFalseContext context)
        {
            if (context.GetText() == "true")
            {
                return new number(1);
            }

            return new number(0);
        }

        public override i_ast_object VisitOperandTypeDeclaration([NotNull] OperandTypeDeclarationContext context)
        {
            string operand_name = context.identifier().GetText();

            runtime_type_switch types = Visit(context.runtimeTypeReference()) as runtime_type_switch;

            scope body = new scope(top_scope());

            generic_declaration result = new generic_declaration(top_scope(), operand_name, types, body);

            push_scope(body);

            body.add_object_to_scope(operand_name, result);

            parse_lines(body, context.line());

            pop_scope();

            return result;
        }

        public override i_ast_object VisitFunctionCall([NotNull] FunctionCallContext context)
        {
            function_call result = new function_call();

            var generics = context.genericImpl();

            if (generics != null)
            {
                foreach (var i in generics.identifier())
                {
                    variable_type generic_type = get_runtime_type(i.GetText()) as variable_type;

                    result.generics.Add(generic_type);
                }
            }

            foreach (var e in context.expression())
            {
                result.function_arguments.Add(Visit(e) as expression);
            }

            result.function_name = context.identifier().GetText();

            string function_key = source_file.get_function_key(result);

            function function_reference = top_scope().get_scoped_object(function_key) as function;

            if (function_reference == null)
            {
                throw new Exception();
            }

            result.function_reference = function_reference;

            return result;
        }

        public override i_ast_object VisitSourceFile([NotNull] languageParser.SourceFileContext context)
        {
            source_file result = new source_file();

            push_scope(result);

            foreach (var declaration in context.topLevelDeclarations())
            {
                i_ast_object object_to_add = Visit(declaration);

                if (object_to_add is function f)
                {
                    string function_key = source_file.get_function_key(f);

                    result.functions.Add(function_key, f);
                    result.add_object_to_scope(function_key, f);
                }
            }
            
            foreach (function f in result.functions.Values)
            {
                if (f.is_external)
                    continue;

                push_scope(f);

                f.function_body = Visit(f.function_source) as scope;

                pop_scope();
            }

            pop_scope();

            return result;
        }

        public override i_ast_object VisitSemiColin([NotNull] SemiColinContext context)
        {
            return new no_operation();
        }

        void parse_lines(scope result, LineContext[] lines)
        {
            foreach (var line in lines)
            {
                var to_add = Visit(line);

                if (to_add is no_operation)
                {
                    continue;
                }

                if (to_add == null)
                {
                    Console.WriteLine(line.GetText());

                    throw new Exception();
                }

                result.commands.Add(to_add);
            }
        }

        public override i_ast_object VisitScope([NotNull] ScopeContext context)
        {
            scope result = new scope(top_scope());

            push_scope(result);

            parse_lines(result, context.line());

            pop_scope();

            return result;
        }

        public override i_ast_object VisitUndefinedVariableDeclaration([NotNull] UndefinedVariableDeclarationContext context)
        {
            variable_type declaration_type = Visit(context.variableType()) as variable_type;
            string variable_name = context.identifier().GetText();

            variable_declaration result = new variable_declaration(declaration_type, variable_name, false);

            top_scope().add_object_to_scope(variable_name, result);

            return result;
        }

        public override i_ast_object VisitDefinedVariableDeclaration([NotNull] DefinedVariableDeclarationContext context)
        {
            variable_type declaration_type = Visit(context.variableType()) as variable_type;
            string variable_name = context.identifier().GetText();

            expression default_value = Visit(context.expression()) as expression;

            variable_declaration result = new variable_declaration(declaration_type, variable_name, false) { default_value = default_value };

            top_scope().add_object_to_scope(variable_name, result);

            return result;
        }

        public override i_ast_object VisitCompileTimeIntegerType([NotNull] CompileTimeIntegerTypeContext context)
        {
            return new compile_time_type(context.GetText() == "void");
        }

        i_ast_object get_runtime_type(string name)
        {
            switch (name)
            {
                case "o8": return new runtime_variable(runtime_variable_size.int8);
                case "o16": return new runtime_variable(runtime_variable_size.int16);
                case "o32": return new runtime_variable(runtime_variable_size.int32);
                case "o64": return new runtime_variable(runtime_variable_size.int64);
                case "o128": return new runtime_variable(runtime_variable_size.int128);
                default: 
                {
                    if (top_scope().object_exists(name, out i_ast_object result))
                    {
                        return result;
                    }

                    throw new Exception();
                };
            }
        }

        public override i_ast_object VisitRuntimeOperandType([NotNull] RuntimeOperandTypeContext context)
        {
            return get_runtime_type(context.GetText());
        }

        public override i_ast_object VisitFixedLengthInstruction([NotNull] FixedLengthInstructionContext context)
        {
            function result = new function(top_scope());

            string instruction_name = context.identifier().GetText();

            var instruction_operands = context.fixedLengthInstructionOperands();

            foreach (var i in instruction_operands.fixedLengthInstructionOperand())
            {
                instruction_operand working_operand = new instruction_operand();
                
                if (i.identifier() != null)
                {
                    working_operand.is_encoding = false;

                    string[] parts = i.identifier().GetText().Split('_');

                    working_operand.data = parts[0];
                    working_operand.size = int.Parse(parts[1]);
                }
                else
                {
                    working_operand.is_encoding = true;

                    working_operand.data = i.GetText();
                    working_operand.size = working_operand.data.Length;
                }

                result.fixed_length_operand_data.Add(working_operand);

                if (i.identifier() == null)
                    continue;

                string name = i.identifier().GetText().Split('_')[0];

                variable_declaration parameter = new variable_declaration(
                    new compile_time_type(false),
                    name,
                    true
                );

                result.add_object_to_scope(name, parameter);
                result.parameters.Add(parameter);
            }

            result.return_type = new compile_time_type(true);
            result.function_name = instruction_name;

            result.function_source = context.scope();

            return result;
        }

        public override i_ast_object VisitFunctionDeclaration([NotNull] languageParser.FunctionDeclarationContext context)
        {
            function result = new function(top_scope());

            result.function_name = context.identifier().GetText();

            var generic_data = context.genericImpl();

            if (generic_data != null)
            {
                foreach (var i in generic_data.identifier())
                {
                    string generic_name = i.GetText();

                    var type = new generic_runtime_variable_type(generic_name);

                    result.generics.Add(type);
                    result.add_object_to_scope(generic_name,type );
                }
            }

            push_scope(result);

            result.return_type = Visit(context.variableType()) as variable_type;

            var parameters = context.functionParameters();

            if (parameters != null)
            {
                foreach (var parameter in parameters.functionParameter())
                {
                    variable_type type = Visit(parameter.variableType()) as variable_type;
                    string name = parameter.identifier().GetText();

                    variable_declaration result_parameter = new variable_declaration(type, name, true);

                    result.parameters.Add(result_parameter);
                    result.add_object_to_scope(name, result_parameter);
                }
            }

            pop_scope();
            
            if (context.functionBody().GetText() == "external")
            {
                result.is_external = true;
            }
            else
            {
                result.function_source = context.functionBody().scope();
            }

            return result;
        }
    }
}