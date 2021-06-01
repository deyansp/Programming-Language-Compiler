# Overview
A C# compiler for the fictional PAL programming language, based on an EBNF specification. 
It makes use of an Ardkit library, class inheritance and method overriding to meet the language requirements.

 Unit tests and a test mode are used to check for correct syntax and semantic analysis.

# Sample Output
![Sample Output](https://raw.githubusercontent.com/deyansp/Programming-Language-Compiler/main/compiler-output.PNG?token=AKMQMV6NVR7LKKOUJ7UZK5TAX7SCU)

This output was generated from the following code:
```
PROGRAM IncorrectTypesAndVars
WITH
i AS INTEGER
x, y, x AS REAL
i AS REAL
IN
	i = a + 32.4
	z = 2
	x = 5.5 * i + 1.0 / 5
	
	y := 23
END
```
# Implementation Details
The compiler is run from the `Main` driver method (line 11, `Program.cs`) which starts the analysis with the `runCompiler` method after checking that the command line argument is valid.

When the compiler is run with `TXT_FILES_TEST` as the command line argument, the program checks each ".txt" file in the project's "Tests" directory. This is done through the `runTestsFromTxtFiles` method (line 31, `Program.cs`).

The scanner implementation is a finite state machine, which was slightly modified to also allow for underscores to be included in identifier tokens as per the EBNF. The scanner performs lexical analysis which converts the raw string input in the program into a collection of tokens. These tokens contain a type (e.g. a PAL keyword, an identifier, a "+" sign etc.), the position of the token in the source file, and optionally, the value of the token (e.g. a numerical constant, or a variable name). 

The tokens generated by the scanner are individually passed through a syntax analyser i.e. a parser. Parsing is done through a `PALParser` class which extends Ardkit's `RecoveringRdParser` class, allowing the compiler to continue running despite encountering errors. Through recursive descent parsing, the order of tokens is checked with recogniser methods and any errors are reported. In some of the recogniser methods, the `PALParser` class utilizes an instance of a `PALSemantics` class to check things such as variable declarations, expressions, and type compatibility. 

Some of the semantic checking logic is contained in the methods of the `PALSemantics` class. An example is the `checkExpression` method, which first checks if both sides of the expression are invalid in which case it returns an undefined type instead of comparing the variables with checkTypesSame. If both variables are of the same type, it returns that as the overall expression type, if not the expression type is inferred to the larger type of the two (based on the integer value of the `LanguageType` class).

# EBNF Specification
```
// The scope of all declared identifiers is the entire program.
// PAL is strictly typed, with no implicit conversion between integers
// and reals (even in constants, i.e. "42" is not a valid REAL constant).

// The Identifier supplied as the program name is not used for any
// semantic purpose.
<Program> ::= PROGRAM Identifier
              WITH <VarDecls> 
              IN (<Statement>)+
              END ;

// All variable names in declarations must be unique within a scope.
// All variable names used in other PAL constructs must have been
// declared in the WITH block.
// Variables are implicitly initialised to 0 or 0.0.
<VarDecls> ::= (<IdentList> AS <Type>)* ;

<Type> ::= REAL | INTEGER ;

<Statement> ::= <Assignment> | <Loop> | <Conditional> | <I-o> ;

// Evaluate the <Expression> and store the result value in the
// existing variable named by Identifier. The variable and <Expression>
// must be of the same type.
<Assignment> ::= Identifier = <Expression> ;

// If <BooleanExpr> is true, skip to the statement following the
// ENDLOOP, otherwise execute the statements and repeat.
<Loop> ::= UNTIL <BooleanExpr> REPEAT (<Statement>)* ENDLOOP ;

// If <BooleanExpr> is true, execute the block after the THEN.
// If <BooleanExpr> is false, execute the block after the ELSE
// (or do nothing, if there is no ELSE clause).
<Conditional> ::= IF <BooleanExpr> THEN (<Statement>)*
                      ( ELSE (<Statement>)* )? 
                      ENDIF ;

// Actual physical I/O depends on the operation of the available
// target execution platform I/O instructions (e.g. INPUT might read
// from standard input, and OUTPUT write to standard output).
// INPUT's argument must be an existing variable.
// Items in the lists may be of different types (e.g. you can
// "OUTPUT 42, 3.7, 1234" or "INPUT someInt, someReal").
<I-o> ::= INPUT <IdentList> | 
          OUTPUT <Expression> ( , <Expression>)* ;

// Operator precedence is implicitly defined via this syntax.
// All elements of an expression must be of the same type.
<Expression> ::= <Term> ( (+|-) <Term>)* ;

<Term> ::= <Factor> ( (*|/) <Factor>)* ;

// Note optional unary operators.
<Factor> ::= (+|-)? ( <Value> | "(" <Expression> ")" ) ;

<Value> ::= Identifier | IntegerValue | RealValue ;

// Both sides of the Boolean expression must be of the same type.
// Evaluates to a true/false value.
<BooleanExpr> ::= <Expression> ("<" | "=" | ">") <Expression> ;

<IdentList> ::= Identifier ( , Identifier)* ;

microsyntax 
// Uses .NET regular expression syntax.

Identifier   <|[a-zA-Z]\w*
RealValue    <|\d+\.\d*
IntegerValue <|\d+
```