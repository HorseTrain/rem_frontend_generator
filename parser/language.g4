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

elementInsert
    : 'elm' expression expression expression expression
    ;

elementExtract
    : 'elm' expression expression expression
    ;
    
line
    : scope
    | operandTypeDeclaration
    | variableDeclaration
    | returnStatement
    | lValueSet
    | runtimeSet
    | ifStatement
    | functionCallLine
    | switchStatement
    | loopStatement
    | hostMemoryWrite
    | semiColin
    | elementInsert
    ;

hostMemoryRead
    : 'physical_read' variableType expression
    ;

hostMemoryWrite
    : 'physical_write' expression expression
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
    : 'loop' expression identifier scope
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

runtimeSet
    : 'set' identifier expression
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

externalIdentifierExpression
    : 'external' identifierExpression
    ;

baseExpression
    : identifierExpression
    | externalIdentifierExpression
    | parenthesis
    | number
    | trueFalse
    | cast
    | constants
    ;

constants
    : 'UINT8_MIN'
    | 'INT8_MIN'
    | 'UINT16_MIN'
    | 'INT16_MIN'
    | 'UINT32_MIN'
    | 'INT32_MIN'
    | 'UINT64_MIN'
    | 'INT64_MIN'
    | 'UINT8_MAX'
    | 'INT8_MAX'
    | 'UINT16_MAX'
    | 'INT16_MAX'
    | 'UINT32_MAX'
    | 'INT32_MAX'
    | 'UINT64_MAX'
    | 'INT64_MAX'
    ;

cast
    : '(' variableType ')' expression
    ;

unaryExpression
    : ('-' | '~' | '!') baseExpression
    ;

signedSign
    : 'signed'
    ;

floatConversions
    : 'to_float' signedSign? variableType expression
    | 'to_int' signedSign? variableType expression
    ;

signExtend
    : 'extend' variableType expression
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

vectorZero
    : 'vector_zero'
    ;

expression
    : functionCall
    | hostMemoryRead
    | baseExpression
    | unaryExpression
    | floatConversions
    | signExtend
    | elementExtract
    | vectorZero
    | expression ('*' | '/' | 'sdiv' | 'umulh' | 'smulh' | '%') expression
    | expression ('+' | '-') expression
    | expression ('<<' | '>>' | 'sar' | 'ror') expression
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

LINE_COMMENT
    : '//' ~[\r\n]* -> skip
;

BLOB_COMMENT
    : '/*' .*? '*/' -> skip
    ;