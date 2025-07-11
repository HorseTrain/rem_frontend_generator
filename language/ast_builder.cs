using System.Numerics;
using System.Reflection.Metadata;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static languageParser;

namespace rem_frontend_generator.language
{
    public class ast_builder : languageBaseVisitor<i_ast_object>
    {
        Stack<scope> scope_stack;

        static runtime_variable o8 = new runtime_variable(runtime_variable_size.int8);
        static runtime_variable o16 = new runtime_variable(runtime_variable_size.int16);
        static runtime_variable o32 = new runtime_variable(runtime_variable_size.int32);
        static runtime_variable o64 = new runtime_variable(runtime_variable_size.int64);
        static runtime_variable o128 = new runtime_variable(runtime_variable_size.int128);
        static compile_time_type nonvoid_type = new compile_time_type(false);
        static compile_time_type void_type = new compile_time_type(true);

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

        public override i_ast_object VisitFloatConversions([NotNull] FloatConversionsContext context)
        {
            floating_point_conversion result = new floating_point_conversion(context.GetChild(0).GetText() == "to_float", Visit(context.variableType()) as variable_type, Visit(context.expression()) as expression);

            result.is_signed = context.signedSign() != null;

            return result;
        }

        public override i_ast_object VisitParenthesis([NotNull] ParenthesisContext context)
        {
            return new unary_operation("()", Visit(context.expression()) as expression);
        }

        public override i_ast_object VisitFunctionType([NotNull] FunctionTypeContext context)
        {
            function_reference_type result = new function_reference_type();

            if (context.GetText() == "void_function")
            {
                return result;
            }

            result.return_type = Visit(context.variableType()) as variable_type;
            result.parameter_types = new List<variable_type>();

            if (context.typeParameters() != null)
            {
                var parameters = context.typeParameters();

                foreach (var i in parameters.variableType())
                {
                    result.parameter_types.Add(Visit(i) as variable_type);
                }
            }

            return result;
        }

        ulong create_mask(int size)
        {
            if (size >= 64)
                return ulong.MaxValue;

            return (1UL << size) - 1;
        }

        public override i_ast_object VisitConstants([NotNull] ConstantsContext context)
        {
            string src = context.GetText();

            bool signed = !src.StartsWith("U");
            bool is_max = src.EndsWith("_MAX");

            string type = src.Split('_')[0];

            if (!signed)
            {
                type = type.Substring(1);
            }

            int size = int.Parse(type.Substring(3, type.Length - 3));
            ulong result ;

            if (is_max)
            {
                result = create_mask(size);

                if (signed)
                    result >>= 1;

                return new number(result, nonvoid_type);
            }
            else
            {
                if (!signed)
                    return new number(0, nonvoid_type);

                result = ulong.MaxValue << (size - 1);

                result &= create_mask(size);

                return new number(result, nonvoid_type);
            }
        }

        public override i_ast_object VisitVectorZero([NotNull] VectorZeroContext context)
        {
            return new vector_default(o128, vector_default_type.zeros);
        }

        public override i_ast_object VisitVectorOne([NotNull] VectorOneContext context)
        {
            return new vector_default(o128, vector_default_type.ones);
        }

        public override i_ast_object VisitElementExtract([NotNull] ElementExtractContext context)
        {
            return new element_extract(Visit(context.expression(0)) as expression, Visit(context.expression(1)) as expression, Visit(context.expression(2)) as expression, o64, o128);
        }

        public override i_ast_object VisitElementInsert([NotNull] ElementInsertContext context)
        {
            return new element_insert(Visit(context.expression(0)) as expression, Visit(context.expression(1)) as expression, Visit(context.expression(2)) as expression, Visit(context.expression(3)) as expression, o128);
        }

        public override i_ast_object VisitNumber([NotNull] NumberContext context)
        {
            string text = context.GetText();

            number result;

            if (text.StartsWith("0b") || text.StartsWith("0B"))
            {
                text = text.Substring(2);

                BigInteger result_temp = 0;

                for (int i = 0; i < text.Length; ++i)
                {
                    int location = text.Length - i - 1;

                    if (text[i] == '1')
                    {
                        result_temp |= 1 << location;
                    }
                }

                result = new number(result_temp, nonvoid_type);
            }
            else
            {
                result = new number(BigInteger.Parse(context.GetText()), nonvoid_type);
            }
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

        public override i_ast_object VisitSignExtend([NotNull] SignExtendContext context)
        {
            variable_type result_type = Visit(context.variableType()) as variable_type;
            expression value = Visit(context.expression()) as expression;

            return new sign_extend(result_type, value);
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
                string type = context.GetText();

                expression left = Visit(context.expression(0)) as expression;
                expression right = Visit(context.expression(1)) as expression;

                return new binary_operation(left, right, context.GetChild(1).GetText());
            }
            else
            {
                throw new Exception();
            }
        }

        public override i_ast_object VisitHostMemoryRead([NotNull] HostMemoryReadContext context)
        {
            return new physical_read(Visit(context.variableType()) as variable_type,Visit(context.expression()) as expression);
        }

        public override i_ast_object VisitHostMemoryWrite([NotNull] HostMemoryWriteContext context)
        {
            return new physical_write(Visit(context.expression(0)) as expression, Visit(context.expression(1)) as expression);
        }

        public override i_ast_object VisitLoopStatement([NotNull] LoopStatementContext context)
        {
            expression count = Visit(context.expression()) as expression;
            string identifier = context.identifier().GetText();

            loop result = new loop(count, top_scope());

            push_scope(result);

            variable_declaration declaration = new variable_declaration(count.get_type(), identifier, false);

            result.loop_index = declaration;
            result.add_object_to_scope(declaration.variable_name, declaration);

            result.body = Visit(context.scope()) as scope;

            pop_scope();

            return result;
        }

        public override i_ast_object VisitUnaryExpression([NotNull] UnaryExpressionContext context)
        {
            return new unary_operation(context.GetChild(0).GetText(), Visit(context.baseExpression()) as expression);
        }

        public override i_ast_object VisitCast([NotNull] CastContext context)
        {
            return new cast(Visit(context.variableType()) as variable_type, Visit(context.expression()) as expression);
        }

        public override i_ast_object VisitRuntimeSet([NotNull] RuntimeSetContext context)
        {
            l_value_set result = new l_value_set();

            result.l_value = new object_reference(top_scope().get_scoped_object(context.identifier().GetText()));
            result.r_value = Visit(context.expression()) as expression;

            result.force_runtime = true;

            (result.l_value.reference as variable_declaration).force_non_constant = true;

            return result;
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

        public override i_ast_object VisitExternalIdentifierExpression([NotNull] ExternalIdentifierExpressionContext context)
        {
            identifier result = new identifier();

            result.data = context.identifierExpression().GetText();
            result.is_external = true;
            result.external_type = o64;

            return result;
        }

        public override i_ast_object VisitInternalIdentifierExpression([NotNull] InternalIdentifierExpressionContext context)
        {
            identifier result = new identifier();

            result.data = context.identifierExpression().GetText();
            result.is_internal = true;
            result.external_type = nonvoid_type;

            return result;
        }

        public override i_ast_object VisitTrueFalse([NotNull] TrueFalseContext context)
        {
            if (context.GetText() == "true")
            {
                return new number(1, nonvoid_type);
            }

            return new number(0, nonvoid_type);
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
                foreach (var i in generics.variableType())
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

            function explicit_function_reference;
            List<variable_type> parameter_types;

            try 
            {
                explicit_function_reference = top_scope().get_scoped_object(function_key) as function;

                parameter_types = new List<variable_type>();
                result.return_type = explicit_function_reference.return_type;
            }
            catch (Exception e)
            {
                i_ast_object dynamic_function_reference = top_scope().get_scoped_object(result.function_name);
                explicit_function_reference = null;

                switch (dynamic_function_reference)
                {
                    case variable_declaration vd:
                    {
                        if (vd.type is not function_reference_type fre)
                        {
                            throw new Exception();
                        }

                        result.is_reference_call = true;
                        parameter_types = fre.parameter_types;
                        result.return_type = fre.return_type;
                    }; break;

                    default: throw new Exception();
                }
            }

            if (!result.is_reference_call)
            {
                if (explicit_function_reference == null)
                {
                    throw new Exception();
                }

                result.function_reference = explicit_function_reference;

                for (int i = 0; i < result.function_reference.parameters.Count; ++i)
                {
                    parameter_types.Add(result.function_reference.parameters[i].type);
                }
            }

            Dictionary<string, variable_type> g_map = new Dictionary<string, variable_type>();
            
            for (int i = 0; i < result.generics.Count; ++i)
            {
                g_map.Add(explicit_function_reference.generics[i].name, result.generics[i]);
            }

            for (int i = 0; i < result.function_arguments.Count; ++i)
            {
                expression working = result.function_arguments[i];
                variable_type parameter_type = parameter_types[i];

                if (!parameter_type.is_runtime())
                    continue;

                if (parameter_type is generic_runtime_variable_type grvt)
                {
                    string type_name = grvt.name;

                    parameter_type = g_map[type_name];
                }

                if (result.function_arguments[i].get_type() == parameter_type)
                    continue;

                result.function_arguments[i] = new cast(parameter_type, working);
            }

            result.parameter_types = parameter_types;

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
            return context.GetText() == "void" ? void_type : nonvoid_type;
        }

        i_ast_object get_runtime_type(string name)
        {
            switch (name)
            {
                case "o8":      return o8;
                case "o16":     return o16;
                case "o32":     return o32;
                case "o64":     return o64;
                case "o128":    return o128;
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

                    if (i.encodingExtra() != null)
                    {
                        var extra = i.encodingExtra();

                        working_operand.extra_rule = extra.GetChild(0).GetText();
                        working_operand.extra_number = (int)(Visit(extra.number()) as number).value;                        
                    }
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
                    nonvoid_type,
                    name,
                    true
                );

                result.add_object_to_scope(name, parameter);
                result.parameters.Add(parameter);
            }

            result.return_type = void_type;
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
                foreach (var i in generic_data.variableType())
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