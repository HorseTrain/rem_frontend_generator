grammar language;

options
{
    language=CSharp;
}

sourceFile
    : topLevelDeclarations* EOF
    ;

topLevelDeclarations
    : functionDeclaration
    | instructionDeclaration
    ;
    
line
    : scope
    | operandTypeDeclaration
    | variableDeclaration
    | returnStatement
    | lValueSet
    | ifStatement
    | functionCallLine
    | switchStatement
    | loopStatement
    | semiColin
    ;

semiColin
    : ';'
    ;

functionCallLine
    : functionCall
    ;

caseStatement
    : 'case' expression ':' line ';'?
    ;

loopStatement
    : 'loop' expression identifier? scope
    ;

switchStatement
    : 'switch' expression '{' caseStatement* '}'
    ;

returnStatement
    : 'return' expression?
    ;

lValueSet
    : identifier '=' expression
    ;

ifStatement
    : 'if' parenthesis line elseStatement?
    ;

elseStatement
    : 'else' line elseStatement?
    ;

scope
    : '{' line* '}'
    ;

identifierExpression
    : identifier
    ;

baseExpression
    : identifierExpression
    | parenthesis
    | number
    | trueFalse
    | cast
    ;

cast
    : '(' variableType ')' expression
    ;

unaryExpression
    : ('-' | '~' | '!') baseExpression
    ;

parenthesis
    : '(' expression ')'
    ;

genericImpl
    : '<' identifier (',' identifier)* '>'
    ;

functionCall
    : identifier genericImpl? '(' (expression (',' expression)*)? ')'
    ;

expression
    : baseExpression
    | unaryExpression
    | functionCall
    | expression ('*' | '/' | 'sdiv' | '%') expression
    | expression ('+' | '-') expression
    | expression ('<<' | '>>' | 'sar') expression
    | expression ('clt' | 'cgt' | 'clte' | 'cgte' | '<' | '<=' | '>' | '>=') expression
    | expression ('==' | '!=') expression
    | expression '&' expression
    | expression '^' expression
    | expression '|' expression
    | expression '&&' expression
    | expression '||' expression
    | expression '?' expression ':' expression
    ;

fixedLengthInstructionOperand
    : identifier
    | number
    ;

fixedLengthInstructionOperands
    : '(' fixedLengthInstructionOperand* ')'
    ;

fixedLengthInstruction
    : 'fl_instruction' number identifier fixedLengthInstructionOperands scope
    ;

instructionDeclaration
    : fixedLengthInstruction 
    ;

functionParameter
    : variableType identifier
    ;

functionParameters
    : functionParameter (',' functionParameter)*
    ;

functionBody
    : scope
    | 'external'
    ;

variableType
    : runtimeOperandType
    | compileTimeIntegerType
    ;

functionDeclaration
    : variableType identifier genericImpl? '(' functionParameters? ')' functionBody
    ;

identifier
    : IDENTIFIER
    ;

trueFalse
    : 'true'
    | 'false'
    ;

number
    : DECIMAL
    | BINARY
    | HEX
    ;

DECIMAL
    : [0-9]+
    ;

runtimeOperandType
    : 'o8'
    | 'o16'
    | 'o32'
    | 'o64'
    | 'o128'
    | identifier
    ;

compileTimeIntegerType
    : 'i64'
    | 'integer'
    | 'int'
    | 'void'
    ;

runtimeTypeCase
    : 'case' expression ':' runtimeTypeReference
    ;

runtimeTypeSwitch
    : 'switch' expression '{' runtimeTypeCase (',' runtimeTypeCase)* '}'
    ;

runtimeTypeReference
    : runtimeOperandType
    | runtimeTypeSwitch
    ;

operandTypeDeclaration
    : 'o_type' identifier '=' runtimeTypeReference line*
    ;

undefinedVariableDeclaration
    : variableType identifier
    ;

definedVariableDeclaration
    : variableType identifier '=' expression
    ;

variableDeclaration
    : undefinedVariableDeclaration
    | definedVariableDeclaration
    ;

BINARY
    : ('0b' | '0B') [0-1]+
    ;

HEX
    : ('0x' | '0X') [0-9a-fA-F]
    ;

IDENTIFIER
    : [a-zA-Z_]([a-zA-Z0-9_]*)
    ;

WHITE_SPACE
    : [ \n\t\r] -> skip
    ;